using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Windowing;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Effects.Filters;
using Sparkle.CSharp.Effects.Posts;
using Sparkle.CSharp.Graphics.VertexTypes;
using Veldrid;
using Veldrid.SPIRV;

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
    /// The shader effect used for rendering the skybox.
    /// </summary>
    public static Effect SkyboxEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used for rendering physics debug visuals.
    /// </summary>
    public static Effect PhysicsDebugEffect { get; private set; }
    
    /// <summary>
    /// The FXAA (Fast Approximate Anti-Aliasing) effect used for post-processing rendering.
    /// </summary>
    public static FxaaEffect FxaaEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a grayscale filter.
    /// </summary>
    public static Effect GrayScaleEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a bloom filter.
    /// </summary>
    public static BloomEffect BloomEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a blur filter.
    /// </summary>
    public static BlurEffect BlurEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a sobel filter.
    /// </summary>
    public static SobelEffect SobelEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a predator filter.
    /// </summary>
    public static PredatorEffect PredatorEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a posterization filter.
    /// </summary>
    public static PosterizationEffect PosterizationEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a pixelizer filter.
    /// </summary>
    public static PixelizerEffect PixelizerEffect { get; private set; }
    
    /// <summary>
    /// Initializes the global graphics assets with the provided graphics device and window.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device to be used for rendering operations.</param>
    /// <param name="window">The window instance associated with the graphics rendering context.</param>
    internal static void Init(GraphicsDevice graphicsDevice, IWindow window) {
        GraphicsDevice = graphicsDevice;
        Window = window;
    }
    
    /// <summary>
    /// Loads and initializes all global graphics assets.
    /// </summary>
    /// <param name="content">The content manager responsible for loading assets.</param>
    internal static void Load(ContentManager content) {
        
        // Skybox effect.
        SkyboxEffect = new Effect(GraphicsDevice, CubemapVertex3D.VertexLayout, "content/sparkle/shaders/skybox.vert", "content/sparkle/shaders/skybox.frag", new CrossCompileOptions());
        SkyboxEffect.AddBufferLayout("ProjectionViewBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkyboxEffect.AddTextureLayout("fCubemap", 1);
        
        // Physics debug effect.
        PhysicsDebugEffect = new Effect(GraphicsDevice, PhysicsDebugVertex3D.VertexLayout, "content/sparkle/shaders/physics_debug_drawer.vert", "content/sparkle/shaders/physics_debug_drawer.frag", new CrossCompileOptions());
        PhysicsDebugEffect.AddBufferLayout("ProjectionViewBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        
        // FXAA post-processing effect.
        FxaaEffect = new FxaaEffect(GraphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        FxaaEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        FxaaEffect.AddTextureLayout("fTexture", 1);
        
        // Gray scale filter effect.
        GrayScaleEffect = new Effect(GraphicsDevice, SpriteVertex2D.VertexLayout, "content/bliss/shaders/full_screen_render_pass.vert", "content/sparkle/shaders/filters/gray_scale.frag", new CrossCompileOptions());
        GrayScaleEffect.AddTextureLayout("fTexture", 0);
        
        // Bloom filter effect.
        BloomEffect = new BloomEffect(GraphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        BloomEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        BloomEffect.AddTextureLayout("fTexture", 1);
        
        // Blur filter effect.
        BlurEffect = new BlurEffect(GraphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        BlurEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        BlurEffect.AddTextureLayout("fTexture", 1);
        
        // Sobel filter effect.
        SobelEffect = new SobelEffect(GraphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        SobelEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SobelEffect.AddTextureLayout("fTexture", 1);
        
        // Predator filter effect.
        PredatorEffect = new PredatorEffect(GraphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        PredatorEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PredatorEffect.AddTextureLayout("fTexture", 1);
        
        // Posterization filter effect.
        PosterizationEffect = new PosterizationEffect(GraphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        PosterizationEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PosterizationEffect.AddTextureLayout("fTexture", 1);
        
        // Pixelizer filter effect.
        PixelizerEffect = new PixelizerEffect(GraphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        PixelizerEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PixelizerEffect.AddTextureLayout("fTexture", 1);
    }
    
    /// <summary>
    /// Releases and disposes of all global graphics assets.
    /// </summary>
    internal static void Destroy() {
        SkyboxEffect.Dispose();
        PhysicsDebugEffect.Dispose();
        FxaaEffect.Dispose();
        GrayScaleEffect.Dispose();
        BloomEffect.Dispose();
        BlurEffect.Dispose();
    }
}