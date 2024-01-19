using Raylib_cs;

namespace Sparkle.CSharp.Content.Types; 

public class MusicContent : IContentType<Music> {
    
    public string Path { get; }
    
    /// <summary>
    /// Initializes a new instance of the MusicContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the music content.</param>
    public MusicContent(string path) {
        this.Path = path;
    }
}