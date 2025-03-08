using Bliss.CSharp.Geometry;

namespace Sparkle.CSharp.Content.Types;

public class ModelContent : IContentType<Model> {
    
    /// <summary>
    /// The file path of the model content.
    /// </summary>
    public string Path { get; }
    
    /// <summary>
    /// Indicates whether to load the material with the model.
    /// </summary>
    public bool LoadMaterial { get; }
    
    /// <summary>
    /// Indicates whether to flip the model's UV mapping.
    /// </summary>
    public bool FlipUv { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelContent"/> class.
    /// </summary>
    /// <param name="path">The file path of the model.</param>
    /// <param name="loadMaterial">Whether to load the model's material (default is true).</param>
    /// <param name="flipUv">Whether to flip the model's UV mapping (default is false).</param>
    public ModelContent(string path, bool loadMaterial = true, bool flipUv = false) {
        this.Path = path;
        this.LoadMaterial = loadMaterial;
        this.FlipUv = flipUv;
    }
}