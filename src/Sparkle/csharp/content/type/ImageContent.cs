using Raylib_cs;

namespace Sparkle.csharp.content.type; 

public class ImageContent : IContentType<Image> {
    
    public string Path { get; }
    
    /// <summary>
    /// Initializes a new instance of the ImageContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the image content.</param>
    public ImageContent(string path) {
        this.Path = path;
    }
}