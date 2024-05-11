using Raylib_CSharp.Geometry;
using Raylib_CSharp.Unsafe.Spans.Data;

namespace Sparkle.CSharp.Content.Types;

public class ModelAnimationContent : IContentType<ReadOnlySpanData<ModelAnimation>> {
    
    public string Path { get; }

    /// <summary>
    /// Initializes a new instance of the ModelAnimationContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the image content.</param>
    public ModelAnimationContent(string path) {
        this.Path = path;
    }
}