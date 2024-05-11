using Raylib_CSharp.Materials;
using Raylib_CSharp.Unsafe.Spans.Data;

namespace Sparkle.CSharp.Content.Types;

public class MaterialContent : IContentType<ReadOnlySpanData<Material>> {
    
    public string Path { get; }

    /// <summary>
    /// Initializes a new instance of the MaterialContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the material content.</param>
    public MaterialContent(string path) {
        this.Path = path;
    }
}