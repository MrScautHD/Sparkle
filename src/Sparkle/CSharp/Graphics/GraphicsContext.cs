using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.ImGUI.CSharp;
using Veldrith;

namespace Sparkle.CSharp.Graphics;

public class GraphicsContext {
    
    /// <summary>
    /// The graphics device used for rendering.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// The command list for submitting rendering commands.
    /// </summary>
    public CommandList CommandList { get; private set; }
    
    /// <summary>
    /// The full-screen render pass used for post-processing.
    /// </summary>
    public FullScreenRenderer FullScreenRenderer { get; private set; }
    
    /// <summary>
    /// The sprite batch used for efficient 2D sprite rendering.
    /// </summary>
    public SpriteBatch SpriteBatch { get; private set; }
    
    /// <summary>
    /// The primitive batch for rendering basic geometric shapes.
    /// </summary>
    public PrimitiveBatch PrimitiveBatch { get; private set; }
    
    /// <summary>
    /// The ImGui controller used for rendering immediate-mode GUI elements.
    /// </summary>
    public ImGuiController ImGuiController { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsContext"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device to use.</param>
    /// <param name="commandList">The command list for issuing rendering commands.</param>
    /// <param name="fullScreenRenderer">The render pass used for full-screen rendering operations.</param>
    /// <param name="spriteBatch">The sprite batch for rendering sprites.</param>
    /// <param name="primitiveBatch">The primitive batch for rendering primitive shapes.</param>
    /// <param name="imGuiController">The ImGui controller for rendering immediate-mode GUI.</param>
    public GraphicsContext(GraphicsDevice graphicsDevice, CommandList commandList, FullScreenRenderer fullScreenRenderer, SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch, ImGuiController imGuiController) {
        this.GraphicsDevice = graphicsDevice;
        this.CommandList = commandList;
        this.FullScreenRenderer = fullScreenRenderer;
        this.SpriteBatch = spriteBatch;
        this.PrimitiveBatch = primitiveBatch;
        this.ImGuiController = imGuiController;
    }
}