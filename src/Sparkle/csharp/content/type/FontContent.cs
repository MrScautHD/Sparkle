namespace Sparkle.csharp.content.type; 

public class FontContent : IContentType {

    public string Path;
    
    public bool AllowDuplicates { get; }
    
    /// <summary>
    /// Initializes a new instance of the FontContent class with the specified path and disallows duplicate items.
    /// </summary>
    /// <param name="path">The path to the font content.</param>
    public FontContent(string path) {
        this.Path = path;
        this.AllowDuplicates = false;
    }
}