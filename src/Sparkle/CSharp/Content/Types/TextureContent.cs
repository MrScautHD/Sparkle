using Bliss.CSharp.Textures;

namespace Sparkle.CSharp.Content.Types;

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