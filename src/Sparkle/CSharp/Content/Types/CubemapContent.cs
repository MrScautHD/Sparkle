using Bliss.CSharp.Textures.Cubemaps;

namespace Sparkle.CSharp.Content.Types;

public class CubemapContent : IContentType<Cubemap> {
    
    /// <summary>
    /// The file path of the cubemap.
    /// </summary>
    public string Path { get; }
    
    /// <summary>
    /// An action that is invoked when the content is loaded.
    /// </summary>
    public Action<object>? OnLoaded { get; set; }
    
    /// <summary>
    /// The layout configuration of the cubemap.
    /// </summary>
    public CubemapLayout Layout { get; }
    
    /// <summary>
    /// Indicates whether to generate mipmaps for the cubemap.
    /// </summary>
    public bool Mipmap { get; }
    
    /// <summary>
    /// Indicates whether to use the sRGB color format for the cubemap.
    /// </summary>
    public bool UseSrgbFormat { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CubemapContent"/> class.
    /// </summary>
    /// <param name="path">The file path of the cubemap.</param>
    /// <param name="layout">The layout configuration of the cubemap (default is AutoDetect).</param>
    /// <param name="mipmap">Whether to generate mipmaps for the cubemap (default is true).</param>
    /// <param name="srgb">Whether to use the sRGB format for the cubemap (default is false).</param>
    public CubemapContent(string path, CubemapLayout layout = CubemapLayout.AutoDetect, bool mipmap = true, bool srgb = false) {
        this.Path = path;
        this.Layout = layout;
        this.Mipmap = mipmap;
        this.UseSrgbFormat = srgb;
    }
}