namespace Sparkle.csharp.content.type; 

public class ImageContent : IContentType {
    
    public string Path { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the ImageContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the image content.</param>
    public ImageContent(string path) {
        this.Path = path;
    }
}