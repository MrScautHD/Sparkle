using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Windowing;
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
    /// Initializes global graphics resources.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create rendering resources.</param>
    /// <param name="window">The rendering window associated with the graphics context.</param>
    internal static void Init(GraphicsDevice graphicsDevice, IWindow window) {
        GraphicsDevice = graphicsDevice;
        Window = window;
        
        // Skybox effect.
        SkyboxEffect = new Effect(graphicsDevice, CubemapVertex3D.VertexLayout, "content/sparkle/shaders/skybox.vert", "content/sparkle/shaders/skybox.frag", new CrossCompileOptions());
        SkyboxEffect.AddBufferLayout("ProjectionViewBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkyboxEffect.AddTextureLayout("fCubemap", 1);
        
        // Physics debug effect.
        PhysicsDebugEffect = new Effect(graphicsDevice, PhysicsDebugVertex3D.VertexLayout, "content/sparkle/shaders/physics_debug_drawer.vert", "content/sparkle/shaders/physics_debug_drawer.frag", new CrossCompileOptions());
        PhysicsDebugEffect.AddBufferLayout("ProjectionViewBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        
        // FXAA post-processing effect.
        FxaaEffect = new FxaaEffect(graphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        FxaaEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        FxaaEffect.AddTextureLayout("fTexture", 1);
        
        // Gray scale filter effect.
        GrayScaleEffect = new Effect(graphicsDevice, SpriteVertex2D.VertexLayout, "content/bliss/shaders/full_screen_render_pass.vert", "content/sparkle/shaders/filters/gray_scale.frag", new CrossCompileOptions());
        GrayScaleEffect.AddTextureLayout("fTexture", 1);
        
        // Bloom filter effect.
        BloomEffect = new BloomEffect(graphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        BloomEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        BloomEffect.AddTextureLayout("fTexture", 1);
        
        // Blur filter effect.
        BlurEffect = new BlurEffect(graphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        BlurEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        BlurEffect.AddTextureLayout("fTexture", 1);
        
        // Sobel filter effect.
        SobelEffect = new SobelEffect(graphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        SobelEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SobelEffect.AddTextureLayout("fTexture", 1);
        
        // Predator filter effect.
        PredatorEffect = new PredatorEffect(graphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        PredatorEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PredatorEffect.AddTextureLayout("fTexture", 1);
        
        // Posterization filter effect.
        PosterizationEffect = new PosterizationEffect(graphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
        PosterizationEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PosterizationEffect.AddTextureLayout("fTexture", 1);
        
        // Pixelizer filter effect.
        PixelizerEffect = new PixelizerEffect(graphicsDevice, SpriteVertex2D.VertexLayout, new CrossCompileOptions());
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