using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Windowing;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Effects;
using Sparkle.CSharp.Effects.Filters;
using Sparkle.CSharp.Effects.Posts;
using Veldrith;
using Veldrith.SPIRV;

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
    /// The shader effect used for rendering physics debug visuals.
    /// </summary>
    public static Effect PhysicsDebugEffect { get; private set; }

    /// <summary>
    /// The effect used for rendering 3D models with PBR.
    /// </summary>
    public static Effect PbrModelEffect { get; private set; }
    
    /// <summary>
    /// The effect used for skinned rendering 3D models with PBR.
    /// </summary>
    public static Effect SkinnedPbrModelEffect { get; private set; }

    /// <summary>
    /// The effect used for shadow-based visual elements in the graphics pipeline.
    /// </summary>
    public static Effect ShadowEffect { get; private set; }
    
    /// <summary>
    /// The effect used for skinned shadow-based visual elements in the graphics pipeline.
    /// </summary>
    public static Effect SkinnedShadowEffect { get; private set; }
    
    /// <summary>
    /// The effect used by the PBR normal/depth prepass.
    /// </summary>
    public static Effect PbrNormalPrepassEffect { get; private set; }
    
    /// <summary>
    /// The effect used by the PBR skinned normal/depth prepass.
    /// </summary>
    public static Effect SkinnedPbrNormalPrepassEffect { get; private set; }
    
    /// <summary>
    /// The effect used by the PBR screen-space reflection pass.
    /// </summary>
    public static Effect SsrEffect { get; private set; }
    
    /// <summary>
    /// The effect used to composite the scaled PBR screen-space reflection result.
    /// </summary>
    public static Effect SsrCompositeEffect { get; private set; }
    
    /// <summary>
    /// The effect used to bake the diffuse irradiance cubemap from an environment cubemap.
    /// </summary>
    public static Effect IrradianceEffect { get; private set; }
    
    /// <summary>
    /// The effect used to bake the prefiltered specular environment cubemap, one roughness level per mip.
    /// </summary>
    public static Effect PrefilterEffect { get; private set; }
    
    /// <summary>
    /// The effect used to generate the split-sum BRDF lookup table (x = NdotV, y = roughness, rg = scale and bias).
    /// </summary>
    public static Effect BrdfLut { get; private set; }
    
    /// <summary>
    /// The shader effect used for rendering tile-based terrain graphics.
    /// </summary>
    public static TileTerrainEffect TileTerrainEffect { get; private set; }
    
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
        
        // Physics debug effect.
        PhysicsDebugEffect = new Effect(GraphicsDevice, "content/sparkle/shaders/visualization/physics_debug_drawer.vert", "content/sparkle/shaders/visualization/physics_debug_drawer.frag", new CrossCompileOptions());
        PhysicsDebugEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        
        // PBR model effect.
        PbrModelEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/model/pbr_model.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/model/pbr_model.frag"), new CrossCompileOptions(), []);
        PbrModelEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        PbrModelEffect.AddBufferLayout("TransformBuffer", 1, SimpleBufferType.Uniform, ShaderStages.Vertex);
        PbrModelEffect.AddBufferLayout("MaterialBuffer", 2, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PbrModelEffect.AddBufferLayout("LightBuffer", 3, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PbrModelEffect.AddBufferLayout("ShadowBuffer", 4, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PbrModelEffect.AddBufferLayout("EnvironmentBuffer", 5, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PbrModelEffect.AddTextureLayout(MaterialMapType.Albedo.GetName(), 6);
        PbrModelEffect.AddTextureLayout(MaterialMapType.Normal.GetName(), 7);
        PbrModelEffect.AddTextureLayout(MaterialMapType.Metallic.GetName(), 8);
        PbrModelEffect.AddTextureLayout(MaterialMapType.Emission.GetName(), 9);
        PbrModelEffect.AddTextureLayout("fShadowMap", 10);
        PbrModelEffect.AddTextureLayout("fIrradianceMap", 11);
        PbrModelEffect.AddTextureLayout("fPrefilterMap", 12);
        PbrModelEffect.AddTextureLayout("fBrdfLut", 13);
        
        // PBR skinned model effect.
        SkinnedPbrModelEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/model/pbr_skinned_model.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/model/pbr_skinned_model.frag"), new CrossCompileOptions(), []);
        SkinnedPbrModelEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkinnedPbrModelEffect.AddBufferLayout("TransformBuffer", 1, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkinnedPbrModelEffect.AddBufferLayout("BoneBuffer", 2, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkinnedPbrModelEffect.AddBufferLayout("MaterialBuffer", 3, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SkinnedPbrModelEffect.AddBufferLayout("LightBuffer", 4, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SkinnedPbrModelEffect.AddBufferLayout("ShadowBuffer", 5, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SkinnedPbrModelEffect.AddBufferLayout("EnvironmentBuffer", 6, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SkinnedPbrModelEffect.AddTextureLayout(MaterialMapType.Albedo.GetName(), 7);
        SkinnedPbrModelEffect.AddTextureLayout(MaterialMapType.Normal.GetName(), 8);
        SkinnedPbrModelEffect.AddTextureLayout(MaterialMapType.Metallic.GetName(), 9);
        SkinnedPbrModelEffect.AddTextureLayout(MaterialMapType.Emission.GetName(), 10);
        SkinnedPbrModelEffect.AddTextureLayout("fShadowMap", 11);
        SkinnedPbrModelEffect.AddTextureLayout("fIrradianceMap", 12);
        SkinnedPbrModelEffect.AddTextureLayout("fPrefilterMap", 13);
        SkinnedPbrModelEffect.AddTextureLayout("fBrdfLut", 14);
        
        // Shadow model effect.
        ShadowEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/shadows/shadow_map.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/shadows/shadow_map.frag"), new CrossCompileOptions(), []);
        ShadowEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        ShadowEffect.AddBufferLayout("TransformBuffer", 1, SimpleBufferType.Uniform, ShaderStages.Vertex);
        ShadowEffect.AddBufferLayout("MaterialBuffer", 2, SimpleBufferType.Uniform, ShaderStages.Fragment);
        ShadowEffect.AddTextureLayout(MaterialMapType.Albedo.GetName(), 3);
        
        // Shadow skinned model effect.
        SkinnedShadowEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/shadows/skinned_shadow_map.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/shadows/skinned_shadow_map.frag"), new CrossCompileOptions(), []);
        SkinnedShadowEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkinnedShadowEffect.AddBufferLayout("TransformBuffer", 1, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkinnedShadowEffect.AddBufferLayout("BoneBuffer", 2, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkinnedShadowEffect.AddBufferLayout("MaterialBuffer", 3, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SkinnedShadowEffect.AddTextureLayout(MaterialMapType.Albedo.GetName(), 4);
        
        // PBR normal/depth prepass effect.
        PbrNormalPrepassEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/prepass/normal_depth_prepass.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/prepass/normal_depth_prepass.frag"), new CrossCompileOptions(), []);
        PbrNormalPrepassEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        PbrNormalPrepassEffect.AddBufferLayout("TransformBuffer", 1, SimpleBufferType.Uniform, ShaderStages.Vertex);
        
        // PBR skinned normal/depth prepass effect.
        SkinnedPbrNormalPrepassEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/prepass/skinned_normal_depth_prepass.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/prepass/normal_depth_prepass.frag"), new CrossCompileOptions(), []);
        SkinnedPbrNormalPrepassEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkinnedPbrNormalPrepassEffect.AddBufferLayout("TransformBuffer", 1, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkinnedPbrNormalPrepassEffect.AddBufferLayout("BoneBuffer", 2, SimpleBufferType.Uniform, ShaderStages.Vertex);
        
        // Screen-space reflections effect.
        SsrEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/fullscreen_triangle.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/post/ssr.frag"), new CrossCompileOptions(), []);
        SsrEffect.AddBufferLayout("SsrBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SsrEffect.AddTextureLayout("fSceneColor", 1);
        SsrEffect.AddTextureLayout("fNormal", 2);
        SsrEffect.AddTextureLayout("fDepth", 3);
        SsrEffect.AddTextureLayout("fSsrMask", 4);
        
        // Screen-space reflections composite effect.
        SsrCompositeEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/fullscreen_triangle.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/post/ssr_composite.frag"), new CrossCompileOptions(), []);
        SsrCompositeEffect.AddTextureLayout("fSceneColor", 0);
        SsrCompositeEffect.AddTextureLayout("fSsrColor", 1);
        SsrCompositeEffect.AddTextureLayout("fSsrMask", 2);
        
        // Irradiance effect.
        IrradianceEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/environment/cubemap_capture.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/environment/irradiance_convolution.frag"), new CrossCompileOptions(), []);
        IrradianceEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        IrradianceEffect.AddTextureLayout("fEnvironmentMap", 1);
        
        // Prefilter effect.
        PrefilterEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/environment/cubemap_capture.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/environment/specular_prefilter.frag"), new CrossCompileOptions(), []);
        PrefilterEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        PrefilterEffect.AddBufferLayout("PrefilterBuffer", 1, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PrefilterEffect.AddTextureLayout("fEnvironmentMap", 2);
        
        // BRDF Lut effect.
        BrdfLut = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/fullscreen_triangle.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/environment/brdf_lut.frag"), new CrossCompileOptions(), []);
        
        // Tile terrain effect.
        TileTerrainEffect = new TileTerrainEffect(GraphicsDevice, new CrossCompileOptions());
        TileTerrainEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        TileTerrainEffect.AddBufferLayout("TransformBuffer", 1, SimpleBufferType.Uniform, ShaderStages.Vertex);
        TileTerrainEffect.AddBufferLayout("MaterialBuffer", 2, SimpleBufferType.Uniform, ShaderStages.Fragment);
        TileTerrainEffect.AddBufferLayout("BrushBuffer", 3, SimpleBufferType.Uniform, ShaderStages.Fragment);
        TileTerrainEffect.AddTextureLayout("fSources", 4);
        TileTerrainEffect.AddTextureLayout("fTiles", 5);
        
        // FXAA post-processing effect.
        FxaaEffect = new FxaaEffect(GraphicsDevice, new CrossCompileOptions());
        FxaaEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        FxaaEffect.AddTextureLayout("fTexture", 1);
        
        // Gray scale filter effect.
        GrayScaleEffect = new Effect(GraphicsDevice, "content/bliss/shaders/full_screen_render_pass.vert", "content/sparkle/shaders/post/filters/gray_scale.frag", new CrossCompileOptions());
        GrayScaleEffect.AddTextureLayout("fTexture", 0);
        
        // Bloom filter effect.
        BloomEffect = new BloomEffect(GraphicsDevice, new CrossCompileOptions());
        BloomEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        BloomEffect.AddTextureLayout("fTexture", 1);
        
        // Blur filter effect.
        BlurEffect = new BlurEffect(GraphicsDevice, new CrossCompileOptions());
        BlurEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        BlurEffect.AddTextureLayout("fTexture", 1);
        
        // Sobel filter effect.
        SobelEffect = new SobelEffect(GraphicsDevice, new CrossCompileOptions());
        SobelEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SobelEffect.AddTextureLayout("fTexture", 1);
        
        // Predator filter effect.
        PredatorEffect = new PredatorEffect(GraphicsDevice, new CrossCompileOptions());
        PredatorEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PredatorEffect.AddTextureLayout("fTexture", 1);
        
        // Posterization filter effect.
        PosterizationEffect = new PosterizationEffect(GraphicsDevice, new CrossCompileOptions());
        PosterizationEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PosterizationEffect.AddTextureLayout("fTexture", 1);
        
        // Pixelizer filter effect.
        PixelizerEffect = new PixelizerEffect(GraphicsDevice, new CrossCompileOptions());
        PixelizerEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PixelizerEffect.AddTextureLayout("fTexture", 1);
    }
    
    /// <summary>
    /// Releases and disposes of all global graphics assets.
    /// </summary>
    internal static void Destroy() {
        PhysicsDebugEffect.Dispose();
        PbrModelEffect.Dispose();
        SkinnedPbrModelEffect.Dispose();
        ShadowEffect.Dispose();
        SkinnedShadowEffect.Dispose();
        PbrNormalPrepassEffect.Dispose();
        SkinnedPbrNormalPrepassEffect.Dispose();
        SsrEffect.Dispose();
        SsrCompositeEffect.Dispose();
        IrradianceEffect.Dispose();
        PrefilterEffect.Dispose();
        BrdfLut.Dispose();
        TileTerrainEffect.Dispose();
        FxaaEffect.Dispose();
        GrayScaleEffect.Dispose();
        BloomEffect.Dispose();
        BlurEffect.Dispose();
        SobelEffect.Dispose();
        PredatorEffect.Dispose();
        PosterizationEffect.Dispose();
        PixelizerEffect.Dispose();
    }
}