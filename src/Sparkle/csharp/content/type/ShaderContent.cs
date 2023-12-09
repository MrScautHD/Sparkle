using Raylib_cs;

namespace Sparkle.csharp.content.type; 

public class ShaderContent : IContentType<Shader> {
    
    public string Path { get; }
    public string FragPath { get; }
    
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