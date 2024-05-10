using Raylib_CSharp.Materials;

namespace Sparkle.CSharp.Content.Types;

public class MaterialContent : IContentType<Material[]> {
    
    public string Path { get; }

    /// <summary>
    /// Initializes a new instance of the MaterialContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the material content.</param>
    public MaterialContent(string path) {
        this.Path = path;
    }
}