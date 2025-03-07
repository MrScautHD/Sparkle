using Bliss.CSharp.Geometry;
using Sparkle.CSharp.Models;

namespace Sparkle.CSharp.Content.Types;

public class ModelContent : IContentType<Model> {
    
    public string Path { get; }
    
    public MaterialManipulator? Manipulator { get; }
    
    /// <summary>
    /// Initializes a new instance of the ModelContent class with the specified path.
    /// </summary>
    /// <param name="path">The path to the model content.</param>
    /// <param name="manipulator">Optional material manipulator for the model.</param>
    public ModelContent(string path, MaterialManipulator? manipulator = null) {
        this.Path = path;
        this.Manipulator = manipulator;
    }
}