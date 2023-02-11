using System.Linq.Expressions;
using Javascriptifier;

namespace Js {


    public interface Block {
        bool Bool { get; }
        public int Int { get; }
        public double Width { get; }
        public double Height { get; }
        public double Execute(Expression<Func<int, int>> expression);

        public int this[string key] {get;}
    }

    public interface Paragraph : Block {
        double FontSize { get; }
    
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
    public static class Math {

        public static double First(params double[] value) => throw new JavascriptOnlyException();

        public static double Sum(params double[] value) => throw new JavascriptOnlyException();

        public static double Min(params double[] value) => throw new JavascriptOnlyException();

        public static double Max(params double[] value) => throw new JavascriptOnlyException();
    }


}