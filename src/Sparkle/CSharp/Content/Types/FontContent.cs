using Bliss.CSharp.Fonts;

namespace Sparkle.CSharp.Content.Types;

public class FontContent : IContentType<Font> {
    
    /// <summary>
    /// The file path of the font.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontContent"/> class.
    /// </summary>
    /// <param name="path">The file path of the font.</param>
    public FontContent(string path) {
        this.Path = path;
    }
}