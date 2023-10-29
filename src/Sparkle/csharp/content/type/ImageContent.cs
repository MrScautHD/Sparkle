namespace Sparkle.csharp.content.type; 

public class ImageContent : IContentType {
    
    public string Path;
    
    public bool AllowDuplicates { get; }
    
    /// <summary>
    /// Initializes a new instance of the ImageContent class with the specified path and disallows duplicate items.
    /// </summary>
    /// <param name="path">The path to the image content.</param>
    public ImageContent(string path) {
        this.Path = path;
        this.AllowDuplicates = false;
    }
}