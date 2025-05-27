using Bliss.CSharp.Fonts;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content.Processors;

public class FontProcessor : IContentProcessor {

    /// <summary>
    /// Loads a font resource from the specified content type.
    /// </summary>
    /// <typeparam name="T">The type of content being loaded.</typeparam>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="type">The content type containing the font path.</param>
    /// <returns>A new <see cref="Font"/> instance loaded from the specified path.</returns>
    public object Load<T>(GraphicsDevice graphicsDevice, IContentType<T> type) {
        FontContent contentType = (FontContent) type;
        return new Font(contentType.Path, contentType.Settings);
    }

    /// <summary>
    /// Unloads the specified content item and releases any resources associated with it.
    /// </summary>
    /// <param name="item">The content item to unload, typically of a resource type such as a font.</param>
    public void Unload(object item) {
        ((Font) item).Dispose();
    }
}