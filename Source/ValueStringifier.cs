using System;
using System.Globalization;

namespace Javascriptifier;

public interface IStringifiable {
    string ToJavascriptString();
}

public partial class ValueStringifier {

    public static string Stringify(object? value) {
        if (value == null)
            return "undefined";
        if (value is bool valueAsBool) {
            return valueAsBool.ToString().ToLower();
        }
        if (value is string valueAsString) {
            return "\"" + valueAsString + "\"";
        }

        if (value.GetType().IsEnum) {
            return "\"" + value.ToString() + "\"";
        }

        if (value is IStringifiable stringifiable) {
            return stringifiable.ToJavascriptString();
        }
#warning Add .ToString(format)
        if (value is decimal valueAsDecimal) {
            return valueAsDecimal.ToString();
        }

        if (value is double valueAsDouble) {
            return valueAsDouble.ToString();
        }
            
        if (value is float valueAsFloat) {
            return valueAsFloat.ToString();
        }

        if (value.GetType().IsPrimitive) {
            return value.ToString()!;
        }

        throw new NotImplementedException($"Convertion to Js value not implemented for type {value.GetType()}");
    }
}