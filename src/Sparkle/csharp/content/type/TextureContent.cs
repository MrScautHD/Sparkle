using Raylib_cs;

namespace Sparkle.csharp.content.type; 

public class TextureContent : IContentType<Texture2D> {
    
    public string Path { get; }
    
    /// <summary>
    /// Initializes a new instance of the TextureContent class with the specified texture image file path.
    /// </summary>
    /// <param name="path">The path to the texture image file.</param>
    public TextureContent(string path) {
        this.Path = path;
    }
}