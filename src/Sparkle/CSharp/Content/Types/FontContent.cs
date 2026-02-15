using Bliss.CSharp.Fonts;
using FontStashSharp;

namespace Sparkle.CSharp.Content.Types;

public class FontContent : IContentType<Font> {
    
    /// <summary>
    /// The file path of the font.
    /// </summary>
    public string Path { get; }
    
    /// <summary>
    /// An action that is invoked when the content is loaded.
    /// </summary>
    public Action<object>? OnLoaded { get; set; }
    
    /// <summary>
    /// The settings used to configure the font system when loading this font.
    /// </summary>
    public FontSystemSettings? Settings;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FontContent"/> class with the specified path and optional settings.
    /// </summary>
    /// <param name="path">The file path of the font to load.</param>
    /// <param name="settings">Optional font system settings; if not provided, default settings are used.</param>
    public FontContent(string path, FontSystemSettings? settings = null) {
        this.Path = path;
        this.Settings = settings ?? new FontSystemSettings();
    }
}