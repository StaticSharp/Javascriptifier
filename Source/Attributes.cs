using System;
namespace Javascriptifier;

[AttributeUsage(AttributeTargets.Class)]
public class JavascriptClassAttribute : Attribute {
    public string Name { get; }
    public JavascriptClassAttribute(string name) {
        Name = name ?? "";
    }
}



[AttributeUsage(AttributeTargets.Method)]
public class JavascriptMethodFormatAttribute : Attribute {

    public string Format { get; }
    public JavascriptMethodFormatAttribute(string format) {
        Format = format;
    }    
}


[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class JavascriptPropertyNameAttribute : Attribute {
    public string Name { get; }
    public JavascriptPropertyNameAttribute(string name) {
        Name = name;
    }
}


[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class JavascriptOnlyMemberAttribute : Attribute {
}


