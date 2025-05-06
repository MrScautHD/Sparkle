using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Passes;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Sparkle.CSharp.Graphics.Rendering;
using Veldrid;

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
    public FullScreenRenderPass FullScreenRenderPass { get; private set; }
    
    /// <summary>
    /// The sprite batch used for efficient 2D sprite rendering.
    /// </summary>
    public SpriteBatch SpriteBatch { get; private set; }
    
    /// <summary>
    /// The primitive batch for rendering basic geometric shapes.
    /// </summary>
    public PrimitiveBatch PrimitiveBatch { get; private set; }
    
    /// <summary>
    /// The immediate renderer for low-level rendering operations.
    /// </summary>
    public ImmediateRenderer ImmediateRenderer { get; private set; }

    /// <summary>
    /// The 3D physics debug drawer used for rendering debug visualization of physics objects.
    /// </summary>
    public Physics3DDebugDrawer Physics3DDebugDrawer { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsContext"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device to use.</param>
    /// <param name="commandList">The command list for issuing rendering commands.</param>
    /// <param name="fullScreenRenderPass">The render pass used for full-screen rendering operations.</param>
    /// <param name="spriteBatch">The sprite batch for rendering sprites.</param>
    /// <param name="primitiveBatch">The primitive batch for rendering primitive shapes.</param>
    /// <param name="immediateRenderer">The immediate renderer for performing low-level custom rendering.</param>
    /// <param name="physics3DDebugDrawer">The 3D physics debug drawer used for rendering debug visualization of physics objects.</param>
    public GraphicsContext(GraphicsDevice graphicsDevice, CommandList commandList, FullScreenRenderPass fullScreenRenderPass, SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch, ImmediateRenderer immediateRenderer, Physics3DDebugDrawer physics3DDebugDrawer) {
        this.GraphicsDevice = graphicsDevice;
        this.CommandList = commandList;
        this.FullScreenRenderPass = fullScreenRenderPass;
        this.SpriteBatch = spriteBatch;
        this.PrimitiveBatch = primitiveBatch;
        this.ImmediateRenderer = immediateRenderer;
        this.Physics3DDebugDrawer = physics3DDebugDrawer;
    }
}