namespace Sparkle.csharp.content.type; 

public class ShaderContent : IContentType {
    
    public string VertPath;
    public string FragPath;
    
    public bool AllowDuplicates { get; }
    
    /// <summary>
    /// Initializes a new instance of the ShaderContent class with the specified vertex and fragment shader paths and disallows duplicate items.
    /// </summary>
    /// <param name="vertPath">The path to the vertex shader source.</param>
    /// <param name="fragPath">The path to the fragment shader source.</param>
    public ShaderContent(string vertPath, string fragPath) {
        this.VertPath = vertPath;
        this.FragPath = fragPath;
        this.AllowDuplicates = false;
    }
}