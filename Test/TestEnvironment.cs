using System.Linq.Expressions;
using Javascriptifier;

namespace Js {


    public interface Entity {
        int EntityId { get; }
    }

    public interface Block: Entity {
        bool Bool { get; }
        public int Int { get; }
        public double Width { get; }
        public double Height { get; }
        public double Execute(Expression<Func<int, int>> expression);

        public int this[string key] { get; }

        public Color Color { get; }

        [JavascriptPropertyGetFormat("Extension({0})")]
        double ExtensionProperty { get; }


    }

    public interface Paragraph : Block {
        double FontSize { get; }

    }

    public static class Animation {

        [JavascriptOnlyMember]
        [Stateful]
        public static double Duration(double duration, double target) => throw new JavascriptOnlyException();
    }



    [JavascriptClass("window")]
    public static class Window {
        public static double DevicePixelRatio => throw new JavascriptOnlyException();

        [JavascriptOnlyMember]
        public static double GetValue() => throw new JavascriptOnlyException();


        [JavascriptOnlyMember]
        [JavascriptMethodFormat("nativeMethod({0},{1})")]
        public static double MethodWithOtherName(int a, params int[] b) => throw new JavascriptOnlyException();

        [JavascriptOnlyMember]
        [JavascriptMethodFormat("nativeMethod({1},{0})")]
        public static double MethodWithOtherNameParametersOrder(int a, params int[] b) => throw new JavascriptOnlyException();

        [JavascriptOnlyMember]
        public static double Width => throw new JavascriptOnlyException();

        [JavascriptOnlyMember]
        [JavascriptPropertyName("nativeProperty")]
        public static double PropertyWithOtherName => throw new JavascriptOnlyException();

        

    }

    [JavascriptClass("")]
    public static class Global {

        public static double? UndefinedValue = null;

        [JavascriptOnlyMember]
        public static double Property => throw new JavascriptOnlyException();

        [JavascriptOnlyMember]
        public static double Method() => throw new JavascriptOnlyException();


        public static double First(params double[] value) => throw new JavascriptOnlyException();

        public static double Sum(params double[] value) => throw new JavascriptOnlyException();

        public static double Min(params double[] value) => throw new JavascriptOnlyException();

        [JavascriptOnlyMember]
        public static double TupleParameter((double, double) pair) => throw new JavascriptOnlyException();

        [JavascriptOnlyMember]
        public static double TupleArrayParameter(params (double, double)[] keyframes) => throw new JavascriptOnlyException();


    }



    public struct Color : IStringifiable {

        public string ToJavascriptString() {

            if (double.IsNaN(A))
                return $"new Color({R},{G},{B})";
            else
                return $"new Color({R},{G},{B},{A})";
        }


        public double R { get; init; } = 0;
        public double G { get; init; } = 0;
        public double B { get; init; } = 0;

        public double A { get; init; } = double.NaN;

        public Color(string value) : this() {
            double hexToDouble255(string c) {
                return Convert.ToInt32(c, 16) / 255d;
            }
            double hexToDouble15(string c) {
                return Convert.ToInt32(c, 16) / 15d;
            }

            if (value.StartsWith('#'))
                value = value.Substring(1);

            if (value.Length == 8) {
                A = hexToDouble255(value[0..2]);
                value = value.Substring(2);
            }

            if (value.Length == 6) {
                R = hexToDouble255(value[0..2]);
                G = hexToDouble255(value[2..4]);
                B = hexToDouble255(value[4..6]);
                return;
            }

            if (value.Length == 4) {
                A = hexToDouble15(value[0..1]);
                value = value.Substring(1);
            }

            if (value.Length == 3) {
                R = hexToDouble15(value[0..1]);
                G = hexToDouble15(value[1..2]);
                B = hexToDouble15(value[2..3]);
            }
        }


        public Color(double r, double g, double b, double a = double.NaN) {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        private static double firstNotNaN(params double[] values) {
            foreach (var i in values)
                if (!double.IsNaN(i))
                    return i;
            return double.NaN;
        }

        [JavascriptMethodFormat("CplusC({0},{1})")]
        public static Color operator +(Color a, Color b) {
            return new Color(a.R + b.R, a.G + b.G, a.B + b.B, firstNotNaN(a.A + b.A, a.A, b.A));
        }

        [JavascriptMethodFormat("CplusN({0},{1})")]
        public static Color operator +(Color a, double b) {
            return new Color(a.R + b, a.G + b, a.B + b, a.A + b);
        }

        [JavascriptMethodFormat("CplusN({1},{0})")]
        public static Color operator +(double a, Color b) {
            return new Color(a + b.R, a + b.G, a + b.B, a + b.A);
        }
    }




}