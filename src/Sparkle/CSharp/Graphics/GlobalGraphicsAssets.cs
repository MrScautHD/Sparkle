using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Graphics.Pipelines.Textures;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Windowing;
using Sparkle.CSharp.Graphics.VertexTypes;
using Veldrid;

namespace Sparkle.CSharp.Graphics;

public static class GlobalGraphicsAssets {
    
    /// <summary>
    /// The graphics device used for rendering.
    /// </summary>
    public static GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// The window used for rendering.
    /// </summary>
    public static IWindow Window { get; private set; }

    /// <summary>
    /// A list of all registered buffer layouts.
    /// </summary>
    public static List<SimpleBufferLayout> BufferLayouts { get; private set; }
    
    /// <summary>
    /// A list of all registered texture layouts.
    /// </summary>
    public static List<SimpleTextureLayout> TextureLayouts { get; private set; }
    
    /// <summary>
    /// The shader effect used for rendering the skybox.
    /// </summary>
    public static Effect SkyboxEffect { get; private set; }

    /// <summary>
    /// The shader effect used for rendering physics debug visuals.
    /// </summary>
    public static Effect PhysicsDebugEffect { get; private set; }

    /// <summary>
    /// Initializes global graphics resources.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create rendering resources.</param>
    /// <param name="window">The rendering window associated with the graphics context.</param>
    internal static void Init(GraphicsDevice graphicsDevice, IWindow window) {
        GraphicsDevice = graphicsDevice;
        Window = window;
        BufferLayouts = new List<SimpleBufferLayout>();
        TextureLayouts = new List<SimpleTextureLayout>();
        
        // Skybox effect.
        SkyboxEffect = new Effect(graphicsDevice, CubemapVertex3D.VertexLayout, "content/shaders/skybox.vert", "content/shaders/skybox.frag");
        SkyboxEffect.AddBufferLayout(CreateBufferLayout("ProjectionViewBuffer", SimpleBufferType.Uniform, ShaderStages.Vertex));
        SkyboxEffect.AddTextureLayout(CreateTextureLayout("fCubemap"));
        
        // Physics debug effect.
        PhysicsDebugEffect = new Effect(graphicsDevice, PhysicsDebugVertex3D.VertexLayout, "content/shaders/physics_debug_drawer.vert", "content/shaders/physics_debug_drawer.frag");
        PhysicsDebugEffect.AddBufferLayout(CreateBufferLayout("ProjectionViewBuffer", SimpleBufferType.Uniform, ShaderStages.Vertex));
    }
    
    /// <summary>
    /// Creates and registers a new buffer layout used by shader effects.
    /// </summary>
    /// <param name="name">The name of the buffer used in the shader.</param>
    /// <param name="bufferType">The type of buffer (e.g., uniform, storage).</param>
    /// <param name="stages">The shader stages where the buffer will be active.</param>
    /// <returns>The created buffer layout.</returns>
    public static SimpleBufferLayout CreateBufferLayout(string name, SimpleBufferType bufferType, ShaderStages stages) {
        SimpleBufferLayout bufferLayout = new SimpleBufferLayout(GraphicsDevice, name, bufferType, stages);
        BufferLayouts.Add(bufferLayout);
        return bufferLayout;
    }
    
    /// <summary>
    /// Creates and registers a new texture layout for use with shader effects.
    /// </summary>
    /// <param name="name">The name of the texture resource in the shader.</param>
    /// <returns>The created texture layout.</returns>
    public static SimpleTextureLayout CreateTextureLayout(string name) {
        SimpleTextureLayout textureLayout = new SimpleTextureLayout(GraphicsDevice, name);
        TextureLayouts.Add(textureLayout);
        return textureLayout;
    }

    /// <summary>
    /// Releases and disposes of all global graphics assets.
    /// </summary>
    internal static void Destroy() {
        SkyboxEffect.Dispose();
        PhysicsDebugEffect.Dispose();
        
        foreach (SimpleBufferLayout bufferLayout in BufferLayouts) {
            bufferLayout.Dispose();
        }
        
        BufferLayouts.Clear();
        
        foreach (SimpleTextureLayout textureLayout in TextureLayouts) {
            textureLayout.Dispose();
        }
        
        TextureLayouts.Clear();
    }
}