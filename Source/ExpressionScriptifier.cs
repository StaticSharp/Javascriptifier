using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Javascriptifier;

public class ExpressionScriptifier {


    public class Result {
        public bool Calculable => Expression != null;

        private string? Script { get; set; } = null;
        private Expression? Expression { get; set; } = null;

        public Result(string script) {
            Script = script;
        }
        public Result(Expression expression) {
            Expression = expression;
        }
        public static implicit operator bool(Result result) {
            return result.Calculable;
        }
        public override string ToString() {
            if (Script != null)
                return Script;

            if (Expression == null)
                throw new InvalidOperationException();

            var compiledExpression = Expression.Lambda(Expression).Compile();
            var value = compiledExpression.DynamicInvoke();

            var script = ValueStringifier.Stringify(value);
            return script;
        }
    }


    public static Result Scriptify(LambdaExpression expression) {
        return EvalLambdaExpression(expression);
    }

    public static Result EvalLambdaExpression(LambdaExpression expression) {

        var bodyResult = Eval(expression.Body);

        var script = $"({string.Join(',', expression.Parameters.Select(x => x.Name))})=>{bodyResult}";

        return new Result(script);
    }

    public static Result Eval(Expression expression) {
        switch (expression) {
            case LambdaExpression lambdaExpression: return EvalLambdaExpression(lambdaExpression);
            case BinaryExpression binaryExpression: return EvalBinaryExpression(binaryExpression);
            case UnaryExpression unaryExpression: return EvalUnaryExpression(unaryExpression);
            case MemberExpression memberExpression: return EvalMemberExpression(memberExpression);
            case ParameterExpression parameterExpression: return new Result(parameterExpression.Name);
            case ConstantExpression constantExpression: return new Result(constantExpression);
            case MethodCallExpression methodCallExpression: return EvalMethodCallExpression(methodCallExpression);
            case ConditionalExpression conditionalExpression: return EvalConditionalExpression(conditionalExpression);
            case NewExpression newExpression: return new Result(newExpression);
        }
        throw NotImplemented(expression);
    }

    private static Result EvalConditionalExpression(ConditionalExpression expression) {
        var testResult = Eval(expression.Test);
        var trueResult = Eval(expression.IfTrue);
        var falseResult = Eval(expression.IfFalse);

        if (testResult && falseResult && trueResult) {
            return new Result(expression);
        }

        return new Result($"({testResult}?{trueResult}:{falseResult})");

    }

    private static Result EvalMethodCallExpression(MethodCallExpression expression) {        
        var parameters = expression.Method.GetParameters();
        var arguments = expression.Arguments.ToArray();
        List<Result> argumentsResults = new();

        for (int i = 0; i < arguments.Length; i++) {
            var isParams = parameters[i].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
            if (isParams) {
                if (arguments[i] is NewArrayExpression newArrayExpression) {
                    var paramsExpressions = newArrayExpression.Expressions;
                    foreach (var p in paramsExpressions) {
                        argumentsResults.Add(Eval(p));
                    }
                } else {
                    throw NotImplemented(arguments[i]);
                }
            } else {
                argumentsResults.Add(Eval(arguments[i]));
            }
        }


        Result? objectResult = expression.Object != null ? Eval(expression.Object) : null;
        bool objectIsCalculable = objectResult != null ? objectResult.Calculable : true;

        bool methodIsCalculable = expression.Method.GetCustomAttribute<JavascriptOnlyMemberAttribute>() == null;

        if (objectIsCalculable && methodIsCalculable && argumentsResults.All(x => x)) {
            return new Result(expression);
        }


        string call = "";
        string delimiter = ".";
        var formant = expression.Method.GetCustomAttribute<JavascriptMethodFormatAttribute>()?.Format;
        if (formant != null) {
            List<string> argumentsStrings = new();
            //This is made for Params arguments
            argumentsStrings.AddRange(argumentsResults.Take(arguments.Length - 1).Select(x => x.ToString()));
            argumentsStrings.Add(string.Join(",", argumentsResults.Skip(arguments.Length - 1)));
            call = string.Format(formant, argumentsStrings.ToArray());
        } else {
            var argumentList = string.Join(",", argumentsResults.Select(x => x.ToString()));
            if (expression.Method.IsSpecialName) {
                if (expression.Method.Name == "get_Item") {
                    call = $"[{argumentList}]";
                    delimiter = "";
                } else {
                    throw NotImplemented(expression);
                }
            } else {
                call = $"{expression.Method.Name}({argumentList})";
            }            
        }

        string objectOrType = "";
        if ((objectResult != null)) {
            objectOrType = objectResult.ToString();
        } else {
            Type declaringType = expression.Method.DeclaringType!; //Null for Module methods (wtf) https://learn.microsoft.com/en-us/dotnet/api/system.reflection.module.getmethods?view=net-7.0
            objectOrType = GetTypeName(declaringType);
            if (string.IsNullOrEmpty(objectOrType))
                delimiter= "";
        }

        var script = $"{objectOrType}{delimiter}{call}";

        return new Result(script);
    }


    private static Result EvalMemberExpression(MemberExpression expression) {

        Result? objectResult = null;
        bool objectCalculable = true;
        if (expression.Expression != null) {
            objectResult = Eval(expression.Expression);
            objectCalculable = objectResult.Calculable;
        }

        bool memberCalculable = expression.Member.GetCustomAttribute<JavascriptOnlyMemberAttribute>() == null;
        
        if (objectCalculable && memberCalculable) {
            return new Result(expression);
        }

        string objectOrType = "";
        if (expression.Expression == null) {//Static class
            objectOrType = GetTypeName(expression.Member.DeclaringType!);
        } else {
            objectOrType = objectResult!.ToString();
        }

        var memberName = expression.Member.GetCustomAttribute<JavascriptPropertyNameAttribute>()?.Name ?? expression.Member.Name;
        
        if (!string.IsNullOrEmpty(objectOrType))
            objectOrType = objectOrType + '.';

        return new Result(objectOrType + memberName);
    }

    private static string GetTypeName(Type type) {
        var typeName = type.Name;
        typeName = type.GetCustomAttribute<JavascriptClassAttribute>()?.Name
            ?? typeName;
        return typeName;
    }

    private static Result EvalUnaryExpression(UnaryExpression expression) {

        var operandResult = Eval(expression.Operand);
        if (operandResult)
            return new Result(expression);

        switch (expression.NodeType) {
            case ExpressionType.Quote:
            case ExpressionType.Convert:
            case ExpressionType.TypeAs:
            return operandResult;
        }

        var Op = expression.NodeType switch {
            ExpressionType.UnaryPlus => "+",
            ExpressionType.Negate => "-",
            ExpressionType.Not => expression.Operand.Type == typeof(bool) ? "!" : "~",
            _ => throw NotImplemented(expression)
        };

        var script = $"{Op}{operandResult.ToString()}";

        return new Result(script);

    }

    public static Result EvalBinaryExpression(BinaryExpression binaryExpression) {

        var left = Eval(binaryExpression.Left);
        var right = Eval(binaryExpression.Right);

        if (left && right)
            return new Result(binaryExpression);


        if (binaryExpression.Method != null) {
            
            var customConverter = binaryExpression.Method.GetCustomAttribute<JavascriptMethodFormatAttribute>()?.Format;
            if (customConverter != null) {
                var typeName = GetTypeName(binaryExpression.Method.DeclaringType!);
                if (!string.IsNullOrEmpty(typeName))
                    typeName = typeName + '.';
                var script = typeName + string.Format(customConverter, left.ToString(), right.ToString());
                return new Result(script);
            }
        }

        var Op = binaryExpression.NodeType switch {
            ExpressionType.Add => "+",
            ExpressionType.Subtract => "-",
            ExpressionType.Multiply => "*",
            ExpressionType.Divide => "/",
            ExpressionType.Modulo => "%",

            ExpressionType.And => "&",
            ExpressionType.AndAlso => "&&",

            ExpressionType.Or => "|",
            ExpressionType.OrElse => "||",

            ExpressionType.ExclusiveOr => "^",

            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",


            ExpressionType.Equal => "==",
            ExpressionType.NotEqual => "!=",
            ExpressionType.Not => "!",

            _ => throw NotImplemented(binaryExpression)
        };

        {
            //Why are spaces included on either side of 'Op'? Right for 1--1, left for readability
            var script = $"({left.ToString()} {Op} {right.ToString()})";
            return new Result(script);
        }
    }

    private static Exception NotImplemented(Expression expression) {
        return new NotImplementedException($"Expression Type: {expression.GetType().FullName} NodeType: {expression.NodeType}");
    }
}