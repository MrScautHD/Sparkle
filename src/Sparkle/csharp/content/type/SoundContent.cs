namespace Sparkle.csharp.content.type; 

public class SoundContent : IContentType {
    
    public string Path { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the SoundContent class with the specified sound file path.
    /// </summary>
    /// <param name="path">The path to the sound file.</param>
    public SoundContent(string path) {
        this.Path = path;
    }
}