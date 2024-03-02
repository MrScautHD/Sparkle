namespace Sparkle.CSharp.Content.Types;

public interface IContentType<T> {
    
    /// <summary>
    /// Gets the path of the content type.
    /// </summary>
    /// <returns>The path of the content type.</returns>
    string Path { get; }
}