using Raylib_cs;

namespace Sparkle.csharp.content.type; 

public class WaveContent : IContentType<Wave> {
    
    public string Path { get; }
    
    /// <summary>
    /// Initializes a new instance of the WaveContent class with the specified audio wave file path.
    /// </summary>
    /// <param name="path">The path to the audio wave file.</param>
    public WaveContent(string path) {
        this.Path = path;
    }
}