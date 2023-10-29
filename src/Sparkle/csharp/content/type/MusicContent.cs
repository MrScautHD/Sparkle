namespace Sparkle.csharp.content.type; 

public class MusicContent : IContentType {
    
    public string Path;
    
    public bool AllowDuplicates { get; }
    
    /// <summary>
    /// Initializes a new instance of the MusicContent class with the specified path and disallows duplicate items.
    /// </summary>
    /// <param name="path">The path to the music content.</param>
    public MusicContent(string path) {
        this.Path = path;
        this.AllowDuplicates = false;
    }
}