namespace Sparkle.csharp.content.type; 

public class MusicContent : IContentType {
    
    public string Path { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the MusicContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the music content.</param>
    public MusicContent(string path) {
        this.Path = path;
    }
}