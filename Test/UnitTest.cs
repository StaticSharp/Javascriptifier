using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Javascriptifier;
using Js;

namespace Test;

[TestClass]
public class UnitTest {

    private void A<T>(Expression<Func<Js.Block, T>> expression, string reference) {
        var actual = ExpressionScriptifier.Scriptify(expression).ToString();
        Assert.AreEqual(
            reference,
            actual,
            $"\n{reference}\n{actual}"
            );
    }

    [TestMethod]
    public void Simple() {
        A(
            e => e.Width * e.Height,
            "(e)=>(e.Width * e.Height)"
            );
    }

    [TestMethod]
    public void NumericConstants() {
        A(
            e => 20 + 2 + e.Width * e.Height,
            "(e)=>(22 + (e.Width * e.Height))"
            );

    }

    int TestInt => 65;
    double TestDouble => 0.25;



    [TestMethod]
    public void BackProperty() {
        A(
            e => TestInt + TestDouble + e.Width,
            "(e)=>(65.25 + e.Width)"
            );
    }

    int TestMethodInt(int a) => 2 * a;
    [TestMethod]
    public void BackMethod() {
        A(
            e => TestMethodInt(32),
            "(e)=>64"
            );
    }

    [TestMethod]
    public void BackMethodFrontParams() {
        //It is imposible to call this method
        //neither on Back (because e.Int is not calculable)
        //nor on Front (because "this" cannot be serialized)
        Expression<Func<Js.Block, double>> expression = e => TestMethodInt(e.Int);

        Assert.ThrowsException<NotImplementedException>(() => {
            ExpressionScriptifier.Scriptify(expression);
        });
    }

    static int TestStaticMethodInt(int a) => 2 * a;
    [TestMethod]
    public void BackStaticMethodFrontParams() {
        A(
            e => TestStaticMethodInt(e.Int),
            $"(e)=>{GetType().Name}.TestStaticMethodInt(e.Int)"
            );
    }


    int Sum(params int[] args) {
        var result = 0;
        foreach (var arg in args) {
            result += arg;
        }
        return result;
    }
    [TestMethod]
    public void BackMethodParams() {
        A(
            e => Sum(1, 2, 3, 4),
            "(e)=>10"
            );
    }

    [TestMethod]
    public void JavascriptOnlyMethod() {
        A(
            e => Js.Window.GetValue(),
            "(e)=>window.GetValue()"
            );
    }

    [TestMethod]
    public void JavascriptMethodRename() {
        A(
            e => Js.Window.MethodWithOtherName(0, 1, 2),
            "(e)=>window.nativeMethod(0,1,2)"
            );
    }

    [TestMethod]
    public void JavascriptMethodRenameParametersOrder() {
        A(
            e => Js.Window.MethodWithOtherNameParametersOrder(0, 1, 2),
            "(e)=>window.nativeMethod(1,2,0)"
            );
    }

    [TestMethod]
    public void JavascriptOnlyProperty() {
        A(
            e => Js.Window.Width,
            "(e)=>window.Width"
            );
    }

    [TestMethod]
    public void JavascriptOnlyPropertyRename() {
        A(
            e => Js.Window.PropertyWithOtherName,
            "(e)=>window.nativeProperty"
            );
    }

    [TestMethod]
    public void UnaryOperations() {
        var a = 3;
        A(e => -a, "(e)=>-3");
        A(e => +a, "(e)=>3");
        A(e => e.Width - -a, "(e)=>(e.Width - -3)");
        A(e => e.Width - ~a, "(e)=>(e.Width - -4)");
        A(e => ~e.Int, "(e)=>~e.Int");
        A(e => !e.Bool ? 0 : 1, "(e)=>(!e.Bool?0:1)");

    }


    [TestMethod]
    public void Lambda() {

        A(
            e => e.Execute(x => x + (2 * 5) + e.Int),
            "(e)=>e.Execute((x)=>((x + 10) + e.Int))"
            );

    }


    [TestMethod]
    public void Indexers() {

        A(
            e => e["One"],
            "(e)=>e[\"One\"]"
            );

    }

    [TestMethod]
    public void Cast() {

        A(
            e => (e as Entity).EntityId,
            "(e)=>e.EntityId"
            );

        A(
            e => (double)e.Int,
            "(e)=>e.Int"
            );

        A(
            e => (e as Js.Paragraph).FontSize,
            "(e)=>as(e,\"Paragraph\").FontSize"
            );

        A(
            e => ((Js.Paragraph)e).FontSize,
            "(e)=>convert(e,\"Paragraph\").FontSize"
            );
    }


    [TestMethod]
    public void OperatorOverloading() {

        A(
            e => (e.Color + new Js.Color("ff0000")).R,
            "(e)=>Color.CplusC(e.Color,new Color(1,0,0)).R"
            );

    }

    [TestMethod]
    public void GlobalMethod() {

        A(
            e => Js.Global.Method(),
            "(e)=>Method()"
            );

    }
    
    [TestMethod]
    public void UndefinedValue() {

        A(
            e => Js.Global.UndefinedValue,
            "(e)=>undefined"
            );

    }

    [TestMethod]
    public void BackendCalculation() {
        var words = new[] { "hello", "world" };
        A(
            e => "Text "+string.Join(" ",words),
            """
            (e)=>"Text hello world"
            """
            );

    }

    [TestMethod]
    public void ExtensionProperty() {
        A(
            e => e.ExtensionProperty,
            """
            (e)=>Extension(e)
            """
            );
    }

    [TestMethod]
    public void StatefulLambda() {
        /*
        (() => {
            let v0 = {}
            return e => Animation.Duration(v0)(1000, () => as(e, "Hover").Value ? 100 : 0)
        })()
        */
        var state = ExpressionScriptifier.StateVariablePrefix;

        A(
            e => Js.Animation.Duration(1,10),
            $$"""
            (()=>{const {{state}}0={};return (e)=>Animation.Duration.bind({{state}}0)(1,10)})()
            """
            );
    }



}
