using MiniAudioEx;

namespace Sparkle.CSharp.Content.Types;

public class AudioContent : IContentType<AudioClip> {
    
    public string Path { get; }
    
    /// <summary>
    /// Initializes a new instance of the MusicContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the music content.</param>
    public AudioContent(string path) {
        this.Path = path;
    }
}