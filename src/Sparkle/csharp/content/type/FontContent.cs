using Raylib_cs;

namespace Sparkle.csharp.content.type; 

public class FontContent : IContentType<Font> {

    public string Path { get; }
    
    /// <summary>
    /// Initializes a new instance of the FontContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the font content.</param>
    public FontContent(string path) {
        this.Path = path;
    }
}