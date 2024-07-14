using Sparkle.CSharp.Gifs;

namespace Sparkle.CSharp.Content.Types;

public class GifContent : IContentType<Gif> {
    
    public string Path { get; }

    /// <summary>
    /// Initializes a new instance of the GifContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the gif content.</param>
    public GifContent(string path) {
        this.Path = path;
    }
}