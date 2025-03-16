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
    /// The output description that defines the rendering target format.
    /// </summary>
    public OutputDescription Output { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsContext"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device to use.</param>
    /// <param name="commandList">The command list for issuing rendering commands.</param>
    /// <param name="spriteBatch">The sprite batch for rendering sprites.</param>
    /// <param name="primitiveBatch">The primitive batch for rendering shapes.</param>
    /// <param name="immediateRenderer">The immediate renderer for low-level drawing.</param>
    /// <param name="output">The output description specifying rendering output details.</param>
    public GraphicsContext(GraphicsDevice graphicsDevice, CommandList commandList, SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch, ImmediateRenderer immediateRenderer, OutputDescription output) {
        this.GraphicsDevice = graphicsDevice;
        this.CommandList = commandList;
        this.SpriteBatch = spriteBatch;
        this.PrimitiveBatch = primitiveBatch;
        this.ImmediateRenderer = immediateRenderer;
        this.Output = output;
    }
}