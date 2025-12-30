using MiniAudioEx.Core.StandardAPI;

namespace Sparkle.CSharp.Content.Types;

public class AudioClipContent : IContentType<AudioClip> {
    
    /// <summary>
    /// The file path of the audio clip.
    /// </summary>
    public string Path { get; }
    
    /// <summary>
    /// Determines whether the audio clip should be streamed from disk instead of loading into memory.
    /// </summary>
    public bool StreamFromDisk { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioClipContent"/> class.
    /// </summary>
    /// <param name="path">The file path of the audio clip.</param>
    /// <param name="streamFromDisk">Whether to stream from disk (default: true).</param>
    public AudioClipContent(string path, bool streamFromDisk = true) {
        this.Path = path;
        this.StreamFromDisk = streamFromDisk;
    }
}