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
    /// The compute effect that assigns the lights to the clusters of the view frustum.
    /// </summary>
    public static ComputeEffect ClusterLightsEffect { get; private set; }

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
    /// The effect used by the fullscreen exponential height fog pass.
    /// </summary>
    public static Effect ExponentialHeightFogEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a bloom filter.
    /// </summary>
    public static Effect BloomEffect { get; private set; }
    
    /// <summary>
    /// The effect used to composite bloom over the final scene color.
    /// </summary>
    public static Effect BloomCompositeEffect { get; private set; }
    
    /// <summary>
    /// The effect used by the PBR screen-space reflection pass.
    /// </summary>
    public static Effect SsrEffect { get; private set; }
    
    /// <summary>
    /// The effect used to composite the scaled PBR screen-space reflection result.
    /// </summary>
    public static Effect SsrCompositeEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used for rendering tile-based terrain graphics.
    /// </summary>
    public static TileTerrainEffect TileTerrainEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used for rendering pbr-tile-based terrain graphics.
    /// </summary>
    public static TileTerrainEffect PbrTileTerrainEffect { get; private set; }
    
    /// <summary>
    /// The FXAA (Fast Approximate Anti-Aliasing) effect used for post-processing rendering.
    /// </summary>
    public static FxaaEffect FxaaEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a grayscale filter.
    /// </summary>
    public static Effect GrayScaleFilterEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a bloom filter.
    /// </summary>
    public static BloomEffect BloomFilterEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a blur filter.
    /// </summary>
    public static BlurEffect BlurFilterEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a sobel filter.
    /// </summary>
    public static SobelEffect SobelFilterEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a predator filter.
    /// </summary>
    public static PredatorEffect PredatorFilterEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a posterization filter.
    /// </summary>
    public static PosterizationEffect PosterizationFilterEffect { get; private set; }
    
    /// <summary>
    /// The shader effect used to apply a pixelizer filter.
    /// </summary>
    public static PixelizerEffect PixelizerFilterEffect { get; private set; }
    
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
        PbrModelEffect = Effect.FromFiles(GraphicsDevice, "content", "content/sparkle/shaders/model/pbr_model.vert", "content/sparkle/shaders/model/pbr_model.frag", new CrossCompileOptions(), [
            new MacroDefinition("MATRIX_BUFFER_SET", "0"),
            new MacroDefinition("TRANSFORMER_BUFFER_SET", "1"),
            new MacroDefinition("MATERIAL_BUFFER_SET", "2"),
            new MacroDefinition("LIGHT_BUFFER_SET", "3"),
            new MacroDefinition("CLUSTER_DATA_BUFFER_SET", "4"),
            new MacroDefinition("CLUSTER_BUFFER_SET", "5"),
            new MacroDefinition("CLUSTER_LIGHT_INDEX_BUFFER_SET", "6"),
            new MacroDefinition("SHADOW_BUFFER_SET", "7"),
            new MacroDefinition("ENVIRONMENT_BUFFER_SET", "8"),
            new MacroDefinition("ALBEDO_TEXTURE_SET", "9"),
            new MacroDefinition("NORMAL_TEXTURE_SET", "10"),
            new MacroDefinition("METALLIC_TEXTURE_SET", "11"),
            new MacroDefinition("EMISSION_TEXTURE_SET", "12"),
            new MacroDefinition("SHADOW_MAP_TEXTURE_SET", "13"),
            new MacroDefinition("IRRADIANCE_MAP_TEXTURE_SET", "14"),
            new MacroDefinition("PREFILTER_MAP_TEXTURE_SET", "15"),
            new MacroDefinition("BRDF_LUT_TEXTURE_SET", "16")
        ]);
        PbrModelEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        PbrModelEffect.AddBufferLayout("TransformBuffer", 1, SimpleBufferType.Uniform, ShaderStages.Vertex);
        PbrModelEffect.AddBufferLayout("MaterialBuffer", 2, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PbrModelEffect.AddBufferLayout("LightBuffer", 3, SimpleBufferType.StructuredReadOnly, ShaderStages.Fragment);
        PbrModelEffect.AddBufferLayout("ClusterDataBuffer", 4, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PbrModelEffect.AddBufferLayout("ClusterBuffer", 5, SimpleBufferType.StructuredReadOnly, ShaderStages.Fragment);
        PbrModelEffect.AddBufferLayout("ClusterLightIndexBuffer", 6, SimpleBufferType.StructuredReadOnly, ShaderStages.Fragment);
        PbrModelEffect.AddBufferLayout("ShadowBuffer", 7, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PbrModelEffect.AddBufferLayout("EnvironmentBuffer", 8, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PbrModelEffect.AddTextureLayout(MaterialMapType.Albedo.GetName(), 9);
        PbrModelEffect.AddTextureLayout(MaterialMapType.Normal.GetName(), 10);
        PbrModelEffect.AddTextureLayout(MaterialMapType.Metallic.GetName(), 11);
        PbrModelEffect.AddTextureLayout(MaterialMapType.Emission.GetName(), 12);
        PbrModelEffect.AddTextureLayout("fShadowMap", 13);
        PbrModelEffect.AddTextureLayout("fIrradianceMap", 14);
        PbrModelEffect.AddTextureLayout("fPrefilterMap", 15);
        PbrModelEffect.AddTextureLayout("fBrdfLut", 16);
        
        // PBR skinned model effect.
        SkinnedPbrModelEffect = Effect.FromFiles(GraphicsDevice, "content", "content/sparkle/shaders/model/pbr_skinned_model.vert", "content/sparkle/shaders/model/pbr_model.frag", new CrossCompileOptions(), [
            new MacroDefinition("MATRIX_BUFFER_SET", "0"),
            new MacroDefinition("TRANSFORMER_BUFFER_SET", "1"),
            new MacroDefinition("BONE_BUFFER_SET", "2"),
            new MacroDefinition("MATERIAL_BUFFER_SET", "3"),
            new MacroDefinition("LIGHT_BUFFER_SET", "4"),
            new MacroDefinition("CLUSTER_DATA_BUFFER_SET", "5"),
            new MacroDefinition("CLUSTER_BUFFER_SET", "6"),
            new MacroDefinition("CLUSTER_LIGHT_INDEX_BUFFER_SET", "7"),
            new MacroDefinition("SHADOW_BUFFER_SET", "8"),
            new MacroDefinition("ENVIRONMENT_BUFFER_SET", "9"),
            new MacroDefinition("ALBEDO_TEXTURE_SET", "10"),
            new MacroDefinition("NORMAL_TEXTURE_SET", "11"),
            new MacroDefinition("METALLIC_TEXTURE_SET", "12"),
            new MacroDefinition("EMISSION_TEXTURE_SET", "13"),
            new MacroDefinition("SHADOW_MAP_TEXTURE_SET", "14"),
            new MacroDefinition("IRRADIANCE_MAP_TEXTURE_SET", "15"),
            new MacroDefinition("PREFILTER_MAP_TEXTURE_SET", "16"),
            new MacroDefinition("BRDF_LUT_TEXTURE_SET", "17")
        ]);
        SkinnedPbrModelEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkinnedPbrModelEffect.AddBufferLayout("TransformBuffer", 1, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkinnedPbrModelEffect.AddBufferLayout("BoneBuffer", 2, SimpleBufferType.Uniform, ShaderStages.Vertex);
        SkinnedPbrModelEffect.AddBufferLayout("MaterialBuffer", 3, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SkinnedPbrModelEffect.AddBufferLayout("LightBuffer", 4, SimpleBufferType.StructuredReadOnly, ShaderStages.Fragment);
        SkinnedPbrModelEffect.AddBufferLayout("ClusterDataBuffer", 5, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SkinnedPbrModelEffect.AddBufferLayout("ClusterBuffer", 6, SimpleBufferType.StructuredReadOnly, ShaderStages.Fragment);
        SkinnedPbrModelEffect.AddBufferLayout("ClusterLightIndexBuffer", 7, SimpleBufferType.StructuredReadOnly, ShaderStages.Fragment);
        SkinnedPbrModelEffect.AddBufferLayout("ShadowBuffer", 8, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SkinnedPbrModelEffect.AddBufferLayout("EnvironmentBuffer", 9, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SkinnedPbrModelEffect.AddTextureLayout(MaterialMapType.Albedo.GetName(), 10);
        SkinnedPbrModelEffect.AddTextureLayout(MaterialMapType.Normal.GetName(), 11);
        SkinnedPbrModelEffect.AddTextureLayout(MaterialMapType.Metallic.GetName(), 12);
        SkinnedPbrModelEffect.AddTextureLayout(MaterialMapType.Emission.GetName(), 13);
        SkinnedPbrModelEffect.AddTextureLayout("fShadowMap", 14);
        SkinnedPbrModelEffect.AddTextureLayout("fIrradianceMap", 15);
        SkinnedPbrModelEffect.AddTextureLayout("fPrefilterMap", 16);
        SkinnedPbrModelEffect.AddTextureLayout("fBrdfLut", 17);
        
        // Cluster lights effect.
        ClusterLightsEffect = new ComputeEffect(GraphicsDevice, ComputeEffect.LoadTextCodeFromFile("content/sparkle/shaders/compute/cluster_lights.comp"), new CrossCompileOptions(), [], 64, 1, 1);
        ClusterLightsEffect.AddBufferLayout("LightBuffer", 0, SimpleBufferType.StructuredReadOnly, ShaderStages.Compute);
        ClusterLightsEffect.AddBufferLayout("ClusterDataBuffer", 1, SimpleBufferType.Uniform, ShaderStages.Compute);
        ClusterLightsEffect.AddBufferLayout("ClusterBuffer", 2, SimpleBufferType.StructuredReadWrite, ShaderStages.Compute);
        ClusterLightsEffect.AddBufferLayout("ClusterLightIndexBuffer", 3, SimpleBufferType.StructuredReadWrite, ShaderStages.Compute);
        ClusterLightsEffect.BuildPipeline();
        
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
        
        // Exponential height fog effect.
        ExponentialHeightFogEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/fullscreen_triangle.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/post/exponential_height_fog.frag"), new CrossCompileOptions(), []);
        ExponentialHeightFogEffect.AddBufferLayout("FogBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        ExponentialHeightFogEffect.AddTextureLayout("fSceneColor", 1);
        ExponentialHeightFogEffect.AddTextureLayout("fDepth", 2);
        
        // Bloom filter effect.
        BloomEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/fullscreen_triangle.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/post/bloom.frag"), new CrossCompileOptions(), []);
        BloomEffect.AddBufferLayout("BloomBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        BloomEffect.AddTextureLayout("fBloomMask", 1);
        
        // Bloom composite effect.
        BloomCompositeEffect = new Effect(GraphicsDevice, Effect.LoadTextCodeFromFile("content/sparkle/shaders/fullscreen_triangle.vert"), Effect.LoadTextCodeFromFile("content/sparkle/shaders/post/bloom_composite.frag"), new CrossCompileOptions(), []);
        BloomCompositeEffect.AddTextureLayout("fSceneColor", 0);
        BloomCompositeEffect.AddTextureLayout("fBloomColor", 1);
        
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
        
        // Tile terrain effect.
        TileTerrainEffect = new TileTerrainEffect(GraphicsDevice, new CrossCompileOptions(), [
            new MacroDefinition("MATRIX_BUFFER_SET", "0"),
            new MacroDefinition("TRANSFORMER_BUFFER_SET", "1"),
            new MacroDefinition("MATERIAL_BUFFER_SET", "2"),
            new MacroDefinition("BRUSH_BUFFER_SET", "3"),
            new MacroDefinition("SOURCE_TEXTURE_SET", "4"),
            new MacroDefinition("TILES_TEXTURE_SET", "5")
        ]);
        TileTerrainEffect.AddBufferLayout("MatrixBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Vertex);
        TileTerrainEffect.AddBufferLayout("TransformBuffer", 1, SimpleBufferType.Uniform, ShaderStages.Vertex);
        TileTerrainEffect.AddBufferLayout("MaterialBuffer", 2, SimpleBufferType.Uniform, ShaderStages.Fragment);
        TileTerrainEffect.AddBufferLayout("BrushBuffer", 3, SimpleBufferType.Uniform, ShaderStages.Fragment);
        TileTerrainEffect.AddTextureLayout("fSources", 4);
        TileTerrainEffect.AddTextureLayout("fTiles", 5);
        
        // Pbr tile terrain effect.
        //TODO: DONE THIS!
        
        // FXAA post-processing effect.
        FxaaEffect = new FxaaEffect(GraphicsDevice, new CrossCompileOptions());
        FxaaEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        FxaaEffect.AddTextureLayout("fTexture", 1);
        
        // Gray scale filter effect.
        GrayScaleFilterEffect = new Effect(GraphicsDevice, "content/bliss/shaders/post/full_screen_render_pass.vert", "content/sparkle/shaders/post/filters/gray_scale.frag", new CrossCompileOptions());
        GrayScaleFilterEffect.AddTextureLayout("fTexture", 0);
        
        // Bloom filter effect.
        BloomFilterEffect = new BloomEffect(GraphicsDevice, new CrossCompileOptions());
        BloomFilterEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        BloomFilterEffect.AddTextureLayout("fTexture", 1);
        
        // Blur filter effect.
        BlurFilterEffect = new BlurEffect(GraphicsDevice, new CrossCompileOptions());
        BlurFilterEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        BlurFilterEffect.AddTextureLayout("fTexture", 1);
        
        // Sobel filter effect.
        SobelFilterEffect = new SobelEffect(GraphicsDevice, new CrossCompileOptions());
        SobelFilterEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        SobelFilterEffect.AddTextureLayout("fTexture", 1);
        
        // Predator filter effect.
        PredatorFilterEffect = new PredatorEffect(GraphicsDevice, new CrossCompileOptions());
        PredatorFilterEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PredatorFilterEffect.AddTextureLayout("fTexture", 1);
        
        // Posterization filter effect.
        PosterizationFilterEffect = new PosterizationEffect(GraphicsDevice, new CrossCompileOptions());
        PosterizationFilterEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PosterizationFilterEffect.AddTextureLayout("fTexture", 1);
        
        // Pixelizer filter effect.
        PixelizerFilterEffect = new PixelizerEffect(GraphicsDevice, new CrossCompileOptions());
        PixelizerFilterEffect.AddBufferLayout("ParameterBuffer", 0, SimpleBufferType.Uniform, ShaderStages.Fragment);
        PixelizerFilterEffect.AddTextureLayout("fTexture", 1);
    }
    
    /// <summary>
    /// Releases and disposes of all global graphics assets.
    /// </summary>
    internal static void Destroy() {
        PhysicsDebugEffect.Dispose();
        PbrModelEffect.Dispose();
        SkinnedPbrModelEffect.Dispose();
        ClusterLightsEffect.Dispose();
        ShadowEffect.Dispose();
        SkinnedShadowEffect.Dispose();
        PbrNormalPrepassEffect.Dispose();
        SkinnedPbrNormalPrepassEffect.Dispose();
        ExponentialHeightFogEffect.Dispose();
        BloomEffect.Dispose();
        BloomCompositeEffect.Dispose();
        SsrEffect.Dispose();
        SsrCompositeEffect.Dispose();
        IrradianceEffect.Dispose();
        PrefilterEffect.Dispose();
        BrdfLut.Dispose();
        TileTerrainEffect.Dispose();
        FxaaEffect.Dispose();
        GrayScaleFilterEffect.Dispose();
        BloomFilterEffect.Dispose();
        BlurFilterEffect.Dispose();
        SobelFilterEffect.Dispose();
        PredatorFilterEffect.Dispose();
        PosterizationFilterEffect.Dispose();
        PixelizerFilterEffect.Dispose();
    }
}