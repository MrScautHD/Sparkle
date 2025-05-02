using Bliss.CSharp.Colors;
using Veldrid;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies;

public class SoftBodyRenderInfo {

    /// <summary>
    /// The sampler used for rendering the soft body mesh. Can be null if not required.
    /// </summary>
    public Sampler? Sampler;

    /// <summary>
    /// Defines the depth and stencil test settings used during rendering.
    /// </summary>
    public DepthStencilStateDescription DepthStencilState;
    
    /// <summary>
    /// Defines the rasterization behavior, including fill mode and face culling.
    /// </summary>
    public RasterizerStateDescription RasterizerState;
    
    /// <summary>
    /// The color used to render the soft body mesh.
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftBodyRenderInfo"/> class with default rendering settings.
    /// </summary>
    public SoftBodyRenderInfo() {
        this.Sampler = null;
        this.DepthStencilState = DepthStencilStateDescription.DEPTH_ONLY_LESS_EQUAL;
        this.RasterizerState = RasterizerStateDescription.DEFAULT;
        this.Color = Color.White;
    }
}