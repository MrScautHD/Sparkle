namespace Sparkle.csharp.content.type; 

public class WaveContent : IContentType {
    
    public string Path;
    
    public bool AllowDuplicates { get; }
    
    /// <summary>
    /// Initializes a new instance of the WaveContent class with the specified audio wave file path and disallows duplicate items.
    /// </summary>
    /// <param name="path">The path to the audio wave file.</param>
    public WaveContent(string path) {
        this.Path = path;
        this.AllowDuplicates = false;
    }
}