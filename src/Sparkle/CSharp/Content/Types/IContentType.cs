namespace Sparkle.CSharp.Content.Types;

public interface IContentType {
    
    /// <summary>
    /// The file path of the content.
    /// </summary>
    string Path { get; }
    
    /// <summary>
    /// The type of the content.
    /// </summary>
    Type ContentType { get; }
    
    /// <summary>
    /// An action that is invoked when the content is loaded.
    /// </summary>
    Action<object>? OnLoaded { get; set; }
}

public interface IContentType<T> : IContentType {
    
    /// <summary>
    /// The type of the content represented by the instance.
    /// </summary>
    Type IContentType.ContentType => typeof(T);
}

public static class ContentTypeExtensions {
    
    /// <summary>
    /// Attaches a specified action to the <see cref="IContentType{T}"/> instance, which is invoked when the content is loaded.
    /// </summary>
    /// <typeparam name="T">The type of the content being processed.</typeparam>
    /// <param name="contentType">The <see cref="IContentType{T}"/> instance to which the action will be attached.</param>
    /// <param name="action">The action to perform on the loaded content of type <typeparamref name="T"/>.</param>
    /// <returns>The same instance of <see cref="IContentType{T}"/> with the action attached.</returns>
    public static IContentType<T> Do<T>(this IContentType<T> contentType, Action<T> action) {
        contentType.OnLoaded = (item) => action((T) item);
        return contentType;
    }
}