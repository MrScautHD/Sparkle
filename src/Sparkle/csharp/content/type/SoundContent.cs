namespace Sparkle.csharp.content.type; 

public class SoundContent : IContentType {
    
    public string Path;
    
    public bool AllowDuplicates { get; }
    
    /// <summary>
    /// Initializes a new instance of the SoundContent class with the specified sound file path and disallows duplicate items.
    /// </summary>
    /// <param name="path">The path to the sound file.</param>
    public SoundContent(string path) {
        this.Path = path;
        this.AllowDuplicates = false;
    }
}