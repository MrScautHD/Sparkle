using Raylib_CSharp.Audio;

namespace Sparkle.CSharp.Content.Types;

public class SoundContent : IContentType<Sound> {
    
    public string Path { get; }
    
    /// <summary>
    /// Initializes a new instance of the SoundContent class with the specified sound file path.
    /// </summary>
    /// <param name="path">The path to the sound file.</param>
    public SoundContent(string path) {
        this.Path = path;
    }
}