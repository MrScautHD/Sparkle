namespace Sparkle.csharp.content.type; 

public class ShaderContent : IContentType {
    
    public string Path { get; set; }
    public string FragPath { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the ShaderContent class with the specified vertex and fragment shader paths.
    /// </summary>
    /// <param name="vertPath">The path to the vertex shader source.</param>
    /// <param name="fragPath">The path to the fragment shader source.</param>
    public ShaderContent(string vertPath, string fragPath) {
        this.Path = vertPath;
        this.FragPath = fragPath;
    }
}