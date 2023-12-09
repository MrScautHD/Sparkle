using Raylib_cs;

namespace Sparkle.csharp.content.type; 

public class ModelAnimationContent : IContentType<ModelAnimation[]> {
    
    public string Path { get; }

    /// <summary>
    /// Initializes a new instance of the ModelAnimationContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the image content.</param>
    public ModelAnimationContent(string path) {
        this.Path = path;
    }
}