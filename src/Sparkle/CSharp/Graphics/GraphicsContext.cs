using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
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
    /// Initializes a new instance of the <see cref="GraphicsContext"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> used for rendering.</param>
    /// <param name="commandList">The <see cref="CommandList"/> for submitting draw commands.</param>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> for rendering sprites.</param>
    /// <param name="primitiveBatch">The <see cref="PrimitiveBatch"/> for drawing geometric shapes.</param>
    /// <param name="immediateRenderer">The <see cref="ImmediateRenderer"/> for direct rendering operations.</param>
    public GraphicsContext(GraphicsDevice graphicsDevice, CommandList commandList, SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch, ImmediateRenderer immediateRenderer) {
        this.GraphicsDevice = graphicsDevice;
        this.CommandList = commandList;
        this.SpriteBatch = spriteBatch;
        this.PrimitiveBatch = primitiveBatch;
        this.ImmediateRenderer = immediateRenderer;
    }
}