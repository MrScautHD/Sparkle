using Raylib_cs;

namespace Sparkle.CSharp.Content.Types;

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