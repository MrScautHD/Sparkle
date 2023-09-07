namespace Sparkle.csharp.attribute; 

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class HeadlessAttribute : Attribute {

    private bool _headless;
    
    public HeadlessAttribute(bool headless) {
        this._headless = headless;
    }
}