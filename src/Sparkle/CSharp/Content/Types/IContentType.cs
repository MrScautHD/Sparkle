namespace Sparkle.CSharp.Content.Types;

public interface IContentType<T> {
    
    /// <summary>
    /// The file path of the content.
    /// </summary>
    string Path { get; }
}