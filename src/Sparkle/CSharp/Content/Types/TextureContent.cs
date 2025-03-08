using Bliss.CSharp.Textures;

namespace Sparkle.CSharp.Content.Types;

public class TextureContent : IContentType<Texture2D> {
    
    /// <summary>
    /// The file path of the texture content.
    /// </summary>
    public string Path { get; }
    
    /// <summary>
    /// Indicates whether to generate mipmaps for the texture.
    /// </summary>
    public bool Mipmap { get; }
    
    /// <summary>
    /// Indicates whether to use the sRGB color format for the texture.
    /// </summary>
    public bool UseSrgbFormat { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureContent"/> class.
    /// </summary>
    /// <param name="path">The file path of the texture.</param>
    /// <param name="mipmap">Whether to generate mipmaps for the texture (default is true).</param>
    /// <param name="srgb">Whether to use the sRGB format for the texture (default is false).</param>
    public TextureContent(string path, bool mipmap = true, bool srgb = false) {
        this.Path = path;
        this.Mipmap = mipmap;
        this.UseSrgbFormat = srgb;
    }
}