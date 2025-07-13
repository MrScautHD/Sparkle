using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Graphics.Pipelines.Textures;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Windowing;
using Sparkle.CSharp.Effects.Filters;
using Sparkle.CSharp.Effects.Posts;
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
        SkyboxEffect = new Effect(graphicsDevice, CubemapVertex3D.VertexLayout, "content/shaders/skybox.vert", "content/shaders/skybox.frag");
        SkyboxEffect.AddBufferLayout(new SimpleBufferLayout(graphicsDevice, "ProjectionViewBuffer", SimpleBufferType.Uniform, ShaderStages.Vertex));
        SkyboxEffect.AddTextureLayout(new SimpleTextureLayout(graphicsDevice, "fCubemap"));
        
        // Physics debug effect.
        PhysicsDebugEffect = new Effect(graphicsDevice, PhysicsDebugVertex3D.VertexLayout, "content/shaders/physics_debug_drawer.vert", "content/shaders/physics_debug_drawer.frag");
        PhysicsDebugEffect.AddBufferLayout(new SimpleBufferLayout(graphicsDevice, "ProjectionViewBuffer", SimpleBufferType.Uniform, ShaderStages.Vertex));
        
        // FXAA post-processing effect.
        FxaaEffect = new FxaaEffect(graphicsDevice, SpriteVertex2D.VertexLayout);
        FxaaEffect.AddBufferLayout(new SimpleBufferLayout(graphicsDevice, "ParameterBuffer", SimpleBufferType.Uniform, ShaderStages.Fragment));
        FxaaEffect.AddTextureLayout(new SimpleTextureLayout(graphicsDevice, "fTexture"));
        
        // Gray scale filter effect.
        GrayScaleEffect = new Effect(graphicsDevice, SpriteVertex2D.VertexLayout, "content/shaders/full_screen_render_pass.vert", "content/shaders/filters/gray_scale.frag");
        GrayScaleEffect.AddTextureLayout(new SimpleTextureLayout(graphicsDevice, "fTexture"));
        
        // Bloom filter effect.
        BloomEffect = new BloomEffect(graphicsDevice, SpriteVertex2D.VertexLayout);
        BloomEffect.AddBufferLayout(new SimpleBufferLayout(graphicsDevice, "ParameterBuffer", SimpleBufferType.Uniform, ShaderStages.Fragment));
        BloomEffect.AddTextureLayout(new SimpleTextureLayout(graphicsDevice, "fTexture"));
        
        // Blur filter effect.
        BlurEffect = new BlurEffect(graphicsDevice, SpriteVertex2D.VertexLayout);
        BlurEffect.AddBufferLayout(new SimpleBufferLayout(graphicsDevice, "ParameterBuffer", SimpleBufferType.Uniform, ShaderStages.Fragment));
        BlurEffect.AddTextureLayout(new SimpleTextureLayout(graphicsDevice, "fTexture"));
        
        // Sobel filter effect.
        SobelEffect = new SobelEffect(graphicsDevice, SpriteVertex2D.VertexLayout);
        SobelEffect.AddBufferLayout(new SimpleBufferLayout(graphicsDevice, "ParameterBuffer", SimpleBufferType.Uniform, ShaderStages.Fragment));
        SobelEffect.AddTextureLayout(new SimpleTextureLayout(graphicsDevice, "fTexture"));
        
        // Predator filter effect.
        PredatorEffect = new PredatorEffect(graphicsDevice, SpriteVertex2D.VertexLayout);
        PredatorEffect.AddBufferLayout(new SimpleBufferLayout(graphicsDevice, "ParameterBuffer", SimpleBufferType.Uniform, ShaderStages.Fragment));
        PredatorEffect.AddTextureLayout(new SimpleTextureLayout(graphicsDevice, "fTexture"));
        
        // Posterization filter effect.
        PosterizationEffect = new PosterizationEffect(graphicsDevice, SpriteVertex2D.VertexLayout);
        PosterizationEffect.AddBufferLayout(new SimpleBufferLayout(graphicsDevice, "ParameterBuffer", SimpleBufferType.Uniform, ShaderStages.Fragment));
        PosterizationEffect.AddTextureLayout(new SimpleTextureLayout(graphicsDevice, "fTexture"));
        
        // Pixelizer filter effect.
        PixelizerEffect = new PixelizerEffect(graphicsDevice, SpriteVertex2D.VertexLayout);
        PixelizerEffect.AddBufferLayout(new SimpleBufferLayout(graphicsDevice, "ParameterBuffer", SimpleBufferType.Uniform, ShaderStages.Fragment));
        PixelizerEffect.AddTextureLayout(new SimpleTextureLayout(graphicsDevice, "fTexture"));
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