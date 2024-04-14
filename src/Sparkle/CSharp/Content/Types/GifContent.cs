using Sparkle.CSharp.Rendering.Gifs;

namespace Sparkle.CSharp.Content.Types;

public class GifContent : IContentType<Gif> {
    
    public string Path { get; }
    public int FrameDelay { get; }

    /// <summary>
    /// Initializes a new instance of the GifContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the gif content.</param>
    /// <param name="frameDelay">Delay between frames in milliseconds.</param>
    public GifContent(string path, int frameDelay) {
        this.Path = path;
        this.FrameDelay = frameDelay;
    }
}