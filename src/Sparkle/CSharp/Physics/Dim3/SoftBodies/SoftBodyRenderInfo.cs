using Bliss.CSharp.Colors;
using Veldrid;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies;

public class SoftBodyRenderInfo {

    /// <summary>
    /// The sampler used for rendering the soft body mesh. Can be null if not required.
    /// </summary>
    public Sampler? Sampler;

    /// <summary>
    /// Whether to render the mesh in wireframe mode.
    /// </summary>
    public bool DrawWires;
    
    /// <summary>
    /// The color used to render the soft body mesh.
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftBodyRenderInfo"/> class with default rendering settings.
    /// </summary>
    public SoftBodyRenderInfo() {
        this.Sampler = null;
        this.DrawWires = false;
        this.Color = Color.White;
    }
}