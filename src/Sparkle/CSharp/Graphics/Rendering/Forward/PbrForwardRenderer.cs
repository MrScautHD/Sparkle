using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics;
using Bliss.CSharp.Graphics.Pipelines;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Graphics.Pipelines.Textures;
using Bliss.CSharp.Graphics.Rendering;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Textures.Cubemaps;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics.Rendering.Forward.Bloom;
using Sparkle.CSharp.Graphics.Rendering.Forward.Environment;
using Sparkle.CSharp.Graphics.Rendering.Forward.Fog;
using Sparkle.CSharp.Graphics.Rendering.Forward.Lighting;
using Sparkle.CSharp.Graphics.Rendering.Forward.Lighting.Pbr;
using Sparkle.CSharp.Graphics.Rendering.Forward.Reflections;
using Sparkle.CSharp.Graphics.Rendering.Forward.Shadows;
using Veldrith;

namespace Sparkle.CSharp.Graphics.Rendering.Forward;

public class PbrForwardRenderer : Disposable, IRenderer, ILightReceiver {
    
    /// <summary>
    /// The graphics device used for rendering.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// The skybox used for rendering a three-dimensional background.
    /// </summary>
    public SkyBox? SkyBox { get; set; }
    
    /// <summary>
    /// Occurs after the skybox/background was rendered and before opaque geometry is rendered.
    /// </summary>
    public event Action<CommandList, Framebuffer>? BeforeOpaquePass;
    
    /// <summary>
    /// Occurs after opaque geometry was rendered and before translucent geometry is rendered.
    /// </summary>
    public event Action<CommandList, Framebuffer>? AfterOpaquePass;
    
    /// <summary>
    /// Occurs after translucent geometry was rendered and before renderer-internal fullscreen passes.
    /// </summary>
    public event Action<CommandList, Framebuffer>? AfterTranslucentPass;
    
    /// <summary>
    /// Global ambient scene light color. W stores intensity.
    /// </summary>
    public Vector4 AmbientColor;
    
    /// <summary>
    /// The number of lights the GPU light buffer can hold. It limits the directional and local lights that are uploaded per frame together.
    /// </summary>
    public int LightCapacity { get; private set; }
    
    /// <summary>
    /// The maximum number of directional lights that are uploaded per frame. Further directional lights are ignored.
    /// </summary>
    public int MaxDirectionalLights;
    
    /// <summary>
    /// The maximum number of local lights (point and spot) that are uploaded per frame.
    /// </summary>
    public int MaxVisibleLocalLights => this.LightCapacity - this.MaxDirectionalLights;
    
    /// <summary>
    /// The intensity at or below which a light is skipped, because it would be too weak to notice.
    /// </summary>
    public float MinLightIntensity;
    
    /// <summary>
    /// The distance to the camera, in world units, up to which local lights keep their full strength.
    /// </summary>
    public float LocalLightFadeStartDistance;
    
    /// <summary>
    /// The distance to the camera, in world units, at which local lights have faded out completely.
    /// </summary>
    public float LocalLightFadeEndDistance;
    
    /// <summary>
    /// The maximum number of local light indices one cluster can reference.
    /// </summary>
    public int MaxLightsPerCluster;
    
    /// <summary>
    /// Number of clusters along the screen X axis.
    /// </summary>
    public int ClusterCountX;
    
    /// <summary>
    /// Number of clusters along the screen Y axis.
    /// </summary>
    public int ClusterCountY;
    
    /// <summary>
    /// Number of depth slices used by clustered lighting.
    /// </summary>
    public int ClusterCountZ;
    
    /// <summary>
    /// Near plane used for cluster Z slicing.
    /// </summary>
    public float ClusterNearPlane;
    
    /// <summary>
    /// Far plane used for cluster Z slicing.
    /// </summary>
    public float ClusterFarPlane;
    
    /// <summary>
    /// Resolution of the shadow map.
    /// </summary>
    public int ShadowMapResolution {
        get => this._shadowMap.Resolution;
        set => this._shadowMap.Resolution = value;
    }
    
    /// <summary>
    /// Distance of the virtual shadow camera from the active camera.
    /// </summary>
    public float ShadowCameraDistance;
    
    /// <summary>
    /// Orthographic size of the shadow camera.
    /// </summary>
    public float ShadowCameraSize;
    
    /// <summary>
    /// Near plane of the shadow camera.
    /// </summary>
    public float ShadowCameraNear;
    
    /// <summary>
    /// Far plane of the shadow camera.
    /// </summary>
    public float ShadowCameraFar;
    
    /// <summary>
    /// Constant depth bias applied to shadow comparisons.
    /// </summary>
    public float ShadowBias;
    
    /// <summary>
    /// Normal-based depth bias applied to shadow comparisons.
    /// </summary>
    public float ShadowNormalBias;
    
    /// <summary>
    /// Maximum depth bias applied to shadow comparisons.
    /// </summary>
    public float ShadowBiasClamp;
    
    /// <summary>
    /// Enables bloom from material emission maps.
    /// </summary>
    public bool EnableBloom;
    
    /// <summary>
    /// Number of samples used by the bloom blur.
    /// </summary>
    public float BloomSamples;
    
    /// <summary>
    /// Bloom blur spread.
    /// </summary>
    public float BloomQuality;
    
    /// <summary>
    /// Final bloom strength.
    /// </summary>
    public float BloomStrength;
    
    /// <summary>
    /// Bloom brightness threshold.
    /// </summary>
    public float BloomThreshold;
    
    /// <summary>
    /// Enables the screen-space reflection pass.
    /// </summary>
    public bool EnableSsr;
    
    /// <summary>
    /// Controls the internal SSR raymarch resolution.
    /// </summary>
    public ReflectionQuality SsrQuality;
    
    /// <summary>
    /// Maximum SSR ray distance.
    /// </summary>
    public float SsrMaxDistance;
    
    /// <summary>
    /// Maximum SSR ray steps.
    /// </summary>
    public float SsrMaxSteps;
    
    /// <summary>
    /// SSR ray step size.
    /// </summary>
    public float SsrStepSize;
    
    /// <summary>
    /// SSR depth thickness tolerance.
    /// </summary>
    public float SsrThickness;
    
    /// <summary>
    /// SSR reflection strength.
    /// </summary>
    public float SsrStrength;
    
    /// <summary>
    /// SSR edge fade size.
    /// </summary>
    public float SsrEdgeFade;
    
    /// <summary>
    /// Enables exponential height fog in the PBR model shader.
    /// </summary>
    public bool EnableFog;
    
    /// <summary>
    /// Fog color. W stores fog intensity.
    /// </summary>
    public Vector4 FogColor;
    
    /// <summary>
    /// Overall fog density.
    /// </summary>
    public float FogDensity;
    
    /// <summary>
    /// Controls how quickly fog density fades with height.
    /// </summary>
    public float FogFalloff;
    
    /// <summary>
    /// World-space height where fog density is strongest.
    /// </summary>
    public float FogBaseHeight;
    
    /// <summary>
    /// Distance from the camera before fog starts appearing.
    /// </summary>
    public float FogStartDistance;
    
    /// <summary>
    /// Distance where fog reaches its configured strength.
    /// </summary>
    public float FogEndDistance;
    
    /// <summary>
    /// Enables screen-space god rays/light shafts in the fog pass.
    /// </summary>
    public bool EnableGodRays;
    
    /// <summary>
    /// Color of the god rays. W is unused.
    /// </summary>
    public Vector4 GodRayColor;
    
    /// <summary>
    /// Overall god ray intensity.
    /// </summary>
    public float GodRayIntensity;
    
    /// <summary>
    /// Maximum world-space ray length used by the screen-space shaft projection.
    /// </summary>
    public float GodRayMaxLength;
    
    /// <summary>
    /// Number of screen-space samples used by the god ray pass.
    /// </summary>
    public float GodRaySampleCount;
    
    /// <summary>
    /// Controls how far each sample moves along the ray in screen space.
    /// </summary>
    public float GodRayDensity;
    
    /// <summary>
    /// Per-sample contribution multiplier.
    /// </summary>
    public float GodRayWeight;
    
    /// <summary>
    /// Per-sample decay multiplier.
    /// </summary>
    public float GodRayDecay;
    
    /// <summary>
    /// Final god ray exposure multiplier.
    /// </summary>
    public float GodRayExposure;
    
    /// <summary>
    /// Distance over which geometry stops occluding the ray.
    /// </summary>
    public float GodRayDepthFadeDistance;
    
    /// <summary>
    /// How much god rays prefer the horizon. 0 disables horizon shaping, 1 fully applies it.
    /// </summary>
    public float GodRayHorizonFade;
    
    /// <summary>
    /// List of opaque renderables waiting to be drawn.
    /// </summary>
    private List<Renderable> _opaqueRenderables;
    
    /// <summary>
    /// List of translucent renderables waiting to be drawn.
    /// </summary>
    private List<Renderable> _translucentRenderables;
    
    /// <summary>
    /// All lights that were submitted for the current frame.
    /// </summary>
    private List<Light> _lights;
    
    /// <summary>
    /// The lights that passed culling and ranking and are uploaded to the GPU this frame.
    /// </summary>
    private List<Light> _culledLights;
    
    /// <summary>
    /// A reusable scratch list that holds the local lights which passed culling, before they are ranked and trimmed to the available slots.
    /// </summary>
    private List<Light> _localLightCandidates;
    
    /// <summary>
    /// The first enabled directional light with CastShadows enabled.
    /// </summary>
    private Light? _shadowLight;
    
    /// <summary>
    /// Shadow map resource holder.
    /// </summary>
    private ShadowMap _shadowMap;
    
    /// <summary>
    /// The environment map.
    /// </summary>
    private EnvironmentMap? _environmentMap;
    
    /// <summary>
    /// GPU-baked split-sum BRDF lookup texture used by image-based specular lighting.
    /// </summary>
    private BrdfLut _brdfLut;
    
    /// <summary>
    /// Skybox cubemap used for the currently baked PBR IBL maps.
    /// </summary>
    private Cubemap? _bakedSkyboxCubemap;
    
    /// <summary>
    /// Render targets used by the PBR forward scene, fog, and SSR passes.
    /// </summary>
    private PbrForwardRenderTargets? _renderTargets;
    
    /// <summary>
    /// The shadow map sampler.
    /// </summary>
    private Sampler _shadowSampler;
    
    /// <summary>
    /// Sampler used for diffuse irradiance environment lighting.
    /// </summary>
    private Sampler _irradianceSampler;
    
    /// <summary>
    /// Sampler used for roughness-prefiltered environment reflections.
    /// </summary>
    private Sampler _prefilterSampler;
    
    /// <summary>
    /// Sampler used for the BRDF integration lookup texture.
    /// </summary>
    private Sampler _brdfLutSampler;
    
    /// <summary>
    /// Sampler used to sample SSR textures.
    /// </summary>
    private Sampler _ssrSampler;
    
    /// <summary>
    /// Number of lights that were uploaded during the previous light-buffer update.
    /// </summary>
    private int _lastUploadedLightCount;
    
    /// <summary>
    /// Last uploaded light-buffer header.
    /// </summary>
    private PbrLightData _lastUploadedLightHeader;
    
    /// <summary>
    /// CPU-side cluster data uploaded to the GPU.
    /// </summary>
    private PbrClusterData _clusterData;
    
    /// <summary>
    /// CPU-side shadow data uploaded to the GPU.
    /// </summary>
    private ShadowData _shadowData;
    
    /// <summary>
    /// CPU-side environment data uploaded to the GPU.
    /// </summary>
    private EnvironmentData _environmentData;
    
    /// <summary>
    /// CPU-side bloom parameter data.
    /// </summary>
    private BloomData _bloomData;
    
    /// <summary>
    /// CPU-side SSR parameter data.
    /// </summary>
    private SsrData _ssrData;
    
    /// <summary>
    /// CPU-side exponential height fog data.
    /// </summary>
    private ExponentialHeightFogData _fogData;
    
    /// <summary>
    /// GPU-side light buffer.
    /// </summary>
    private SimpleStructuredBuffer<PbrLightData, PbrGpuLight> _lightBuffer;
    
    /// <summary>
    /// GPU-side cluster parameter buffer.
    /// </summary>
    private SimpleUniformBuffer<PbrClusterData> _clusterDataBuffer;
    
    /// <summary>
    /// GPU-side cluster buffer.
    /// </summary>
    private SimpleStructuredBuffer<int, PbrLightCluster> _clusterBuffer;
    
    /// <summary>
    /// GPU-side flattened cluster light-index buffer.
    /// </summary>
    private SimpleStructuredBuffer<int, uint> _clusterLightIndexBuffer;
    
    /// <summary>
    /// GPU-side shadow buffer.
    /// </summary>
    private SimpleUniformBuffer<ShadowData> _shadowBuffer;
    
    /// <summary>
    /// Matrix buffer used by the shadow-map pass.
    /// </summary>
    private SimpleUniformBuffer<Matrix4x4> _shadowMatrixBuffer;
    
    /// <summary>
    /// GPU-side environment buffer.
    /// </summary>
    private SimpleUniformBuffer<EnvironmentData> _environmentBuffer;
    
    /// <summary>
    /// GPU-side bloom parameter buffer.
    /// </summary>
    private SimpleUniformBuffer<BloomData> _bloomBuffer;
    
    /// <summary>
    /// GPU-side SSR parameter buffer.
    /// </summary>
    private SimpleUniformBuffer<SsrData> _ssrBuffer;
    
    /// <summary>
    /// GPU-side exponential height fog buffer.
    /// </summary>
    private SimpleUniformBuffer<ExponentialHeightFogData> _fogBuffer;
    
    /// <summary>
    /// Description of the pipeline used for rendering.
    /// </summary>
    private SimplePipelineDescription _pipelineDescription;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PbrForwardRenderer"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for managing GPU resources and rendering operations.</param>
    /// <param name="skyBox">An optional <see cref="SkyBox"/> instance to use as the background of the rendered scene. If <see langword="null"/>, no skybox will be rendered.</param>
    /// <param name="ambientColor">The ambient light color applied to the scene. If <see langword="null"/>, a default dark gray color (0.03, 0.03, 0.03) is applied.</param>
    /// <param name="lightCapacity">The number of lights the GPU light buffer can hold. It limits the directional and local lights that are uploaded per frame together.</param>
    /// <param name="maxDirectionalLights">The maximum number of directional lights that are uploaded per frame. The remaining slots of the light buffer are available to local lights.</param>
    /// <param name="minLightIntensity">The intensity at or below which a light is skipped, because it would be too weak to notice.</param>
    /// <param name="localLightFadeStartDistance">The distance to the camera, in world units, up to which local lights (point and spot) keep their full strength.</param>
    /// <param name="localLightFadeEndDistance">The distance to the camera, in world units, at which local lights have faded out completely. Lights are also culled based on this distance plus their own range.</param>
    /// <param name="clusterCountX">The number of clusters along the screen X axis. Together with the other two counts, it splits the view frustum into a 3D grid for clustered lighting.</param>
    /// <param name="clusterCountY">The number of clusters along the screen Y axis.</param>
    /// <param name="clusterCountZ">The number of clusters along the depth axis (depth slices) between the cluster near and far plane.</param>
    /// <param name="maxLightsPerCluster">The maximum number of lights that can be stored for a single cluster. Further lights that affect the cluster are dropped. Together with the cluster counts, it defines the size of the cluster light index buffer.</param>
    /// <param name="clusterNearPlane">The distance to the camera, in world units, where the first depth slice of the cluster grid starts.</param>
    /// <param name="clusterFarPlane">The distance to the camera, in world units, where the last depth slice of the cluster grid ends.</param>
    /// <param name="shadowMapResolution">The resolution of the square shadow map texture. Higher values result in better shadow quality at the cost of increased memory usage.</param>
    /// <param name="shadowCameraDistance">The distance between the shadow camera and its focus point, measured along the light direction.</param>
    /// <param name="shadowCameraSize">The size of the orthographic projection used by the shadow camera.</param>
    /// <param name="shadowCameraNear">The near-plane distance for the shadow camera, defining the start of the camera's visible range.</param>
    /// <param name="shadowCameraFar">The far-plane distance for the shadow camera, defining the end of the camera's visible range.</param>
    /// <param name="shadowBias">A constant depth bias value applied during shadow sampling to reduce shadow acne artifacts.</param>
    /// <param name="shadowNormalBias">A bias applied in the direction of the surface normal to mitigate artifacts at grazing angles during shadow sampling.</param>
    /// <param name="shadowBiasClamp">The maximum allowable offset for shadow bias adjustments, helping to prevent excessive "peter-panning" effects.</param>
    /// <param name="enableBloom">Specifies whether bloom is applied, so bright and emissive surfaces glow into their surroundings.</param>
    /// <param name="bloomSamples">The number of samples taken per pixel when the bloom is blurred. Higher values give a smoother glow at a higher GPU cost.</param>
    /// <param name="bloomQuality">The quality of the bloom blur. Higher values give a wider and softer glow at a higher GPU cost.</param>
    /// <param name="bloomStrength">The strength of the bloom, which is a multiplier for the glow that is added to the scene. 0.0 gives no glow, and values above 1.0 exaggerate it.</param>
    /// <param name="bloomThreshold">The brightness above which a pixel contributes to the bloom. Lower values let more of the scene glow, and higher values restrict the glow to very bright surfaces.</param>
    /// <param name="enableSsr">Specifies whether screen space reflections (SSR) are enabled for the renderer.</param>
    /// <param name="ssrQuality">The quality setting for screen space reflections, which defines the resolution of the SSR result relative to the screen resolution.</param>
    /// <param name="ssrMaxDistance">The maximum traceable distance for screen space reflections, determining their area of influence.</param>
    /// <param name="ssrMaxSteps">The maximum number of ray marching steps used in SSR tracing.</param>
    /// <param name="ssrStepSize">The size of each incremental step during SSR ray tracing.</param>
    /// <param name="ssrThickness">The thickness threshold used to resolve SSR artifacts caused by thin geometry.</param>
    /// <param name="ssrStrength">The intensity of screen space reflections, determining their visual prominence.</param>
    /// <param name="ssrEdgeFade">The rate at which SSR effects fade toward screen edges, improving visual smoothness and blending.</param>
    /// <param name="enableFog">Specifies whether distance and height fog is applied to the scene.</param>
    /// <param name="fogColor">The color of the fog. If <see langword="null"/>, a default color is used.</param>
    /// <param name="fogDensity">The thickness of the fog. Higher values make the fog denser, so objects fade into it sooner.</param>
    /// <param name="fogFalloff">How quickly the fog thins out with increasing height above <paramref name="fogBaseHeight"/>. Higher values confine the fog to a thinner layer near the ground.</param>
    /// <param name="fogBaseHeight">The world-space height, in world units, up to which the fog has its full density.</param>
    /// <param name="fogStartDistance">The distance to the camera, in world units, where the fog starts to appear. Everything closer stays fog-free.</param>
    /// <param name="fogEndDistance">The distance to the camera, in world units, from which the fog is fully applied.</param>
    /// <param name="enableGodRays">Specifies whether god rays (light shafts) are rendered for the shadow light.</param>
    /// <param name="godRayColor">The color of the light shafts. If <see langword="null"/>, a default color is used.</param>
    /// <param name="godRayIntensity">The overall strength of the light shafts. The effect is added on top of the scene, so small values are usually enough.</param>
    /// <param name="godRayMaxLength">The maximum length, in world units, over which light shafts are traced from the camera.</param>
    /// <param name="godRaySampleCount">The number of samples taken per pixel along the shaft. Higher values give smoother shafts at a higher GPU cost.</param>
    /// <param name="godRayDensity">The distance covered by the samples, relative to the distance between the pixel and the light. Lower values give shorter, more compact shafts.</param>
    /// <param name="godRayWeight">The contribution of each individual sample to the final brightness.</param>
    /// <param name="godRayDecay">The factor that weakens each following sample along the shaft. Values close to 1.0 make long shafts, and lower values make them fade out sooner.</param>
    /// <param name="godRayExposure">A final multiplier applied to the accumulated light shafts.</param>
    /// <param name="godRayDepthFadeDistance">The distance, in world units, over which the shafts fade out near geometry. This avoids hard bright edges where objects meet the shafts.</param>
    /// <param name="godRayHorizonFade">The amount by which the shafts are weakened near the horizon, so they do not glow along the whole skyline.</param>
    public PbrForwardRenderer(
        GraphicsDevice graphicsDevice,
        SkyBox? skyBox = null,
        Color? ambientColor = null,
        int lightCapacity = 512,
        int maxDirectionalLights = 4,
        float minLightIntensity = 0.001F,
        float localLightFadeStartDistance = 180.0F,
        float localLightFadeEndDistance = 250.0F,
        int clusterCountX = 32,
        int clusterCountY = 18,
        int clusterCountZ = 40,
        int maxLightsPerCluster = 128,
        float clusterNearPlane = 1.0F,
        float clusterFarPlane = 250.0F,
        int shadowMapResolution = 2048,
        float shadowCameraDistance = 80.0F,
        float shadowCameraSize = 120.0F,
        float shadowCameraNear = 0.1F,
        float shadowCameraFar = 250.0F,
        float shadowBias = 0.0005F,
        float shadowNormalBias = 0.0025F,
        float shadowBiasClamp = 0.01F,
        bool enableBloom = true,
        float bloomSamples = 5.0F,
        float bloomQuality = 1.25F,
        float bloomStrength = 0.75F,
        float bloomThreshold = 0.05F,
        bool enableSsr = true,
        ReflectionQuality ssrQuality = ReflectionQuality.Medium,
        float ssrMaxDistance = 40.0F,
        float ssrMaxSteps = 32.0F,
        float ssrStepSize = 0.35F,
        float ssrThickness = 0.35F,
        float ssrStrength = 0.45F,
        float ssrEdgeFade = 0.08F,
        bool enableFog = true,
        Color? fogColor = null,
        float fogDensity = 0.015F,
        float fogFalloff = 0.04F,
        float fogBaseHeight = 0.0F,
        float fogStartDistance = 10.0F,
        float fogEndDistance = 250.0F,
        bool enableGodRays = true,
        Color? godRayColor = null,
        float godRayIntensity = 0.01F,
        float godRayMaxLength = 180.0F,
        float godRaySampleCount = 24.0F,
        float godRayDensity = 0.85F,
        float godRayWeight = 0.045F,
        float godRayDecay = 0.93F,
        float godRayExposure = 1.1F,
        float godRayDepthFadeDistance = 35.0F,
        float godRayHorizonFade = 0.65F) {
        this.GraphicsDevice = graphicsDevice;
        this.SkyBox = skyBox;
        this.AmbientColor = ambientColor?.ToRgbaFloatVec4() ?? new Vector4(0.03F, 0.03F, 0.03F, 1.0F);
        this.LightCapacity = lightCapacity;
        this.MaxDirectionalLights = maxDirectionalLights;
        this.MinLightIntensity = minLightIntensity;
        this.LocalLightFadeStartDistance = localLightFadeStartDistance;
        this.LocalLightFadeEndDistance = localLightFadeEndDistance;
        this.MaxLightsPerCluster = maxLightsPerCluster;
        this.ClusterCountX = clusterCountX;
        this.ClusterCountY = clusterCountY;
        this.ClusterCountZ = clusterCountZ;
        this.ClusterNearPlane = clusterNearPlane;
        this.ClusterFarPlane = clusterFarPlane;
        this.ShadowCameraDistance = shadowCameraDistance;
        this.ShadowCameraSize = shadowCameraSize;
        this.ShadowCameraNear = shadowCameraNear;
        this.ShadowCameraFar = shadowCameraFar;
        this.ShadowBias = shadowBias;
        this.ShadowNormalBias = shadowNormalBias;
        this.ShadowBiasClamp = shadowBiasClamp;
        this.EnableBloom = enableBloom;
        this.BloomSamples = bloomSamples;
        this.BloomQuality = bloomQuality;
        this.BloomStrength = bloomStrength;
        this.BloomThreshold = bloomThreshold;
        this.EnableSsr = enableSsr;
        this.SsrQuality = ssrQuality;
        this.SsrMaxDistance = ssrMaxDistance;
        this.SsrMaxSteps = ssrMaxSteps;
        this.SsrStepSize = ssrStepSize;
        this.SsrThickness = ssrThickness;
        this.SsrStrength = ssrStrength;
        this.SsrEdgeFade = ssrEdgeFade;
        this.EnableFog = enableFog;
        this.FogColor = fogColor?.ToRgbaFloatVec4() ?? new Vector4(0.55F, 0.62F, 0.70F, 0.35F);
        this.FogDensity = fogDensity;
        this.FogFalloff = fogFalloff;
        this.FogBaseHeight = fogBaseHeight;
        this.FogStartDistance = fogStartDistance;
        this.FogEndDistance = fogEndDistance;
        this.EnableGodRays = enableGodRays;
        this.GodRayColor = godRayColor?.ToRgbaFloatVec4() ?? new Vector4(1.0F, 0.78F, 0.45F, 1.0F);
        this.GodRayIntensity = godRayIntensity;
        this.GodRayMaxLength = godRayMaxLength;
        this.GodRaySampleCount = godRaySampleCount;
        this.GodRayDensity = godRayDensity;
        this.GodRayWeight = godRayWeight;
        this.GodRayDecay = godRayDecay;
        this.GodRayExposure = godRayExposure;
        this.GodRayDepthFadeDistance = godRayDepthFadeDistance;
        this.GodRayHorizonFade = godRayHorizonFade;
        
        // Create lists for renderables.
        this._opaqueRenderables = new List<Renderable>();
        this._translucentRenderables = new List<Renderable>();
        
        // Create light lists.
        this._lights = new List<Light>();
        this._culledLights = new List<Light>();
        this._localLightCandidates = new List<Light>();
        
        // Create shadow map.
        this._shadowMap = new ShadowMap(graphicsDevice, shadowMapResolution);
        
        // Create the BRDF LUT.
        this._brdfLut = new BrdfLut(graphicsDevice, 256);
        
        // Create shadow sampler.
        this._shadowSampler = graphicsDevice.ResourceFactory.CreateSampler(new SamplerDescription() {
            AddressModeU = SamplerAddressMode.Border,
            AddressModeV = SamplerAddressMode.Border,
            AddressModeW = SamplerAddressMode.Border,
            Filter = SamplerFilter.MinLinearMagLinearMipLinear,
            ComparisonKind = ComparisonKind.LessEqual,
            MaximumAnisotropy = 0,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            LodBias = 0,
            BorderColor = SamplerBorderColor.OpaqueWhite
        });
        
        // Create irradiance sampler.
        this._irradianceSampler = graphicsDevice.ResourceFactory.CreateSampler(new SamplerDescription() {
            AddressModeU = SamplerAddressMode.Wrap,
            AddressModeV = SamplerAddressMode.Wrap,
            AddressModeW = SamplerAddressMode.Wrap,
            Filter = SamplerFilter.MinLinearMagLinearMipLinear,
            ComparisonKind = null,
            MaximumAnisotropy = 4,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            LodBias = 0,
            BorderColor = SamplerBorderColor.OpaqueBlack
        });
        
        // Create prefilter sampler.
        this._prefilterSampler = graphicsDevice.ResourceFactory.CreateSampler(new SamplerDescription() {
            AddressModeU = SamplerAddressMode.Wrap,
            AddressModeV = SamplerAddressMode.Wrap,
            AddressModeW = SamplerAddressMode.Wrap,
            Filter = SamplerFilter.MinLinearMagLinearMipLinear,
            ComparisonKind = null,
            MaximumAnisotropy = 4,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            LodBias = 0,
            BorderColor = SamplerBorderColor.OpaqueBlack
        });
        
        // Create SSR sampler.
        this._ssrSampler = graphicsDevice.ResourceFactory.CreateSampler(new SamplerDescription() {
            AddressModeU = SamplerAddressMode.Clamp,
            AddressModeV = SamplerAddressMode.Clamp,
            AddressModeW = SamplerAddressMode.Clamp,
            Filter = SamplerFilter.MinLinearMagLinearMipLinear,
            ComparisonKind = null,
            MaximumAnisotropy = 0,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            LodBias = 0,
            BorderColor = SamplerBorderColor.OpaqueBlack
        });
        
        // Create brdfLut shader.
        this._brdfLutSampler = graphicsDevice.ResourceFactory.CreateSampler(new SamplerDescription() {
            AddressModeU = SamplerAddressMode.Clamp,
            AddressModeV = SamplerAddressMode.Clamp,
            AddressModeW = SamplerAddressMode.Clamp,
            Filter = SamplerFilter.MinLinearMagLinearMipLinear,
            ComparisonKind = null,
            MaximumAnisotropy = 0,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            LodBias = 0,
            BorderColor = SamplerBorderColor.OpaqueBlack
        });
        
        // Create shadow data.
        this._shadowData = new ShadowData() {
            ShadowMapResolution = shadowMapResolution,
            ShadowBias = shadowBias,
            ShadowNormalBias = shadowNormalBias,
            ShadowBiasClamp = shadowBiasClamp,
            AmbientColor = this.AmbientColor,
            LightVP = Matrix4x4.Identity,
            LightDirection = -Vector4.UnitY,
            LightColor = Vector4.Zero
        };
        
        // Create environment data.
        this._environmentData = new EnvironmentData() {
            MaxReflectionLod = 4.0F,
            EnvironmentColor = Vector4.One
        };
        
        // Create bloom data.
        this._bloomData = new BloomData();
        
        // Create SSR data.
        this._ssrData = new SsrData();
        
        // Create exponential height fog data.
        this._fogData = new ExponentialHeightFogData();
        
        // Create light buffer.
        this._lightBuffer = new SimpleStructuredBuffer<PbrLightData, PbrGpuLight>(graphicsDevice, 1, (uint) this.LightCapacity, ShaderStages.Compute | ShaderStages.Fragment);
        this._lightBuffer.DeviceBuffer.Name = "LightBuffer";
        
        // Create cluster data buffer.
        this._clusterDataBuffer = new SimpleUniformBuffer<PbrClusterData>(graphicsDevice, 1, ShaderStages.Compute | ShaderStages.Fragment);
        this._clusterDataBuffer.DeviceBuffer.Name = "ClusterDataBuffer";
        
        // Create cluster buffer
        int clusterCount = this.ClusterCountX * this.ClusterCountY * this.ClusterCountZ;
        this._clusterBuffer = new SimpleStructuredBuffer<int, PbrLightCluster>(graphicsDevice, 0, (uint) clusterCount, ShaderStages.Compute | ShaderStages.Fragment, true, false);
        this._clusterBuffer.DeviceBuffer.Name = "ClusterBuffer";
        
        // Create cluster light-index buffer.
        int clusterLightIndexCapacity = clusterCount * this.MaxLightsPerCluster;
        this._clusterLightIndexBuffer = new SimpleStructuredBuffer<int, uint>(graphicsDevice, 0, (uint) clusterLightIndexCapacity, ShaderStages.Compute | ShaderStages.Fragment, true, false);
        this._clusterLightIndexBuffer.DeviceBuffer.Name = "ClusterLightIndexBuffer";
        
        // Create shadow buffer.
        this._shadowBuffer = new SimpleUniformBuffer<ShadowData>(graphicsDevice, 1, ShaderStages.Fragment);
        this._shadowBuffer.DeviceBuffer.Name = "ShadowBuffer";
        
        // Create shadow matrix buffer.
        this._shadowMatrixBuffer = new SimpleUniformBuffer<Matrix4x4>(graphicsDevice, 2, ShaderStages.Vertex);
        this._shadowMatrixBuffer.DeviceBuffer.Name = "MatrixBuffer";
        
        // Create environment buffer.
        this._environmentBuffer = new SimpleUniformBuffer<EnvironmentData>(graphicsDevice, 1, ShaderStages.Fragment);
        this._environmentBuffer.DeviceBuffer.Name = "EnvironmentBuffer";
        
        // Create bloom buffer.
        this._bloomBuffer = new SimpleUniformBuffer<BloomData>(graphicsDevice, 1, ShaderStages.Fragment);
        this._bloomBuffer.DeviceBuffer.Name = "BloomBuffer";
        
        // Create SSR buffer.
        this._ssrBuffer = new SimpleUniformBuffer<SsrData>(graphicsDevice, 1, ShaderStages.Fragment);
        this._ssrBuffer.DeviceBuffer.Name = "SsrBuffer";
        
        // Create fog buffer.
        this._fogBuffer = new SimpleUniformBuffer<ExponentialHeightFogData>(graphicsDevice, 1, ShaderStages.Fragment);
        this._fogBuffer.DeviceBuffer.Name = "FogBuffer";
        
        // Create the pipeline description.
        this._pipelineDescription = new SimplePipelineDescription();
    }
    
    /// <summary>
    /// Queues a renderable for drawing.
    /// </summary>
    /// <param name="renderable">The renderable to draw.</param>
    public void DrawRenderable(Renderable renderable) {
        if (renderable.Material.RenderMode == RenderMode.Translucent) {
            this._translucentRenderables.Add(renderable);
        }
        else {
            this._opaqueRenderables.Add(renderable);
        }
    }
    
    /// <summary>
    /// Queues a light for drawing.
    /// </summary>
    /// <param name="light">The light to queue.</param>
    public void SubmitLight(Light light) {
        if (!light.Enabled) {
            return;
        }
        
        if (light.CastShadows && light.LightType == LightType.Directional && this._shadowLight == null) {
            this._shadowLight = light;
        }
        
        this._lights.Add(light);
    }
    
    /// <summary>
    /// Executes the rendering process for drawing opaque and translucent renderable objects.
    /// </summary>
    /// <param name="commandList">The command list used to execute rendering commands.</param>
    /// <param name="framebuffer">The framebuffer that serves as the target for the rendered output.</param>
    public void Draw(CommandList commandList, Framebuffer framebuffer) {
        Cam3D? cam3D = Cam3D.ActiveCamera;
        
        if (cam3D == null) {
            return;
        }
        
        // Order renderables.
        this._opaqueRenderables.Sort((a, b) => Vector3.DistanceSquared(a.GetTransforms()[0].Translation, cam3D.Position).CompareTo(Vector3.DistanceSquared(b.GetTransforms()[0].Translation, cam3D.Position)));
        this._translucentRenderables.Sort((a, b) => Vector3.DistanceSquared(b.GetTransforms()[0].Translation, cam3D.Position).CompareTo(Vector3.DistanceSquared(a.GetTransforms()[0].Translation, cam3D.Position)));
        
        // Update opaques renderable buffers.
        foreach (Renderable renderable in this._opaqueRenderables) {
            this.UpdateRenderableBuffer(commandList, renderable);
        }
        
        // Update translucent renderable buffers.
        foreach (Renderable renderable in this._translucentRenderables) {
            this.UpdateRenderableBuffer(commandList, renderable);
        }
        
        // Cull and sort lights before uploading them to the GPU.
        this.UpdateCulledLights(cam3D);
        
        // Update light buffer.
        this.UpdateLightBuffer(commandList, cam3D);
        
        // Update cluster buffers.
        this.UpdateClusterBuffers(commandList, cam3D, framebuffer);
        
        // Ensure PBR IBL maps are baked for the current skybox.
        this.EnsurePbrEnvironmentMap(commandList);
        
        // Update fog buffer.
        this.UpdateFogBuffer(commandList, cam3D);
        
        // Draw shadow scene.
        this.DrawShadowScene(commandList, framebuffer, cam3D);
        
        // Draw scene.
        if (this.EnableSsr || this.EnableFog || this.EnableBloom) {
            this.EnsureRenderTargets(framebuffer);
            
            if (this.EnableSsr) {
                
                // Draw normal prepass.
                commandList.SetFramebuffer(this._renderTargets!.NormalFramebuffer);
                commandList.ClearColorTarget(0, new RgbaFloat(0.5F, 0.5F, 1.0F, 1.0F));
                commandList.ClearDepthStencil(1.0F);
                
                this.DrawNormalPrepass(commandList, cam3D);
            }
            
            // Draw main pass into the scene texture.
            commandList.SetFramebuffer(this._renderTargets!.ColorFramebuffer);
            commandList.ClearColorTarget(0, Color.Black.ToRgbaFloat());
            commandList.ClearColorTarget(1, new RgbaFloat(0.0F, 0.0F, 0.0F, 1.0F));
            commandList.ClearColorTarget(2, new RgbaFloat(0.0F, 0.0F, 0.0F, 1.0F));
            commandList.ClearDepthStencil(1.0F);
            
            // Draw Skybox.
            this.SkyBox?.Draw(commandList, this._renderTargets.ColorFramebuffer);
            
            this.BeforeOpaquePass?.Invoke(commandList, this._renderTargets.ColorFramebuffer);
            
            // Draw opaques renderables.
            this.DrawOpaqueScene(commandList, this._renderTargets.ColorFramebuffer, cam3D);
            
            this.AfterOpaquePass?.Invoke(commandList, this._renderTargets.ColorFramebuffer);
            
            // Draw translucent renderables.
            this.DrawTranslucentScene(commandList, this._renderTargets.ColorFramebuffer, cam3D);
            
            this.AfterTranslucentPass?.Invoke(commandList, this._renderTargets.ColorFramebuffer);
            
            // Draw Bloom/SSR/Fog.
            if (this.EnableBloom) {
                this.DrawBloomPass(commandList);
            }
            
            if (this.EnableFog && this.EnableSsr) {
                this.DrawFogPass(commandList, this._renderTargets.FogFramebuffer);
                this.DrawSsrPass(commandList, this.EnableBloom ? this._renderTargets.ColorFramebuffer : framebuffer, cam3D, true);
                
                if (this.EnableBloom) {
                    this.DrawBloomCompositePass(commandList, framebuffer, false);
                }
            }
            else if (this.EnableFog) {
                this.DrawFogPass(commandList, this.EnableBloom ? this._renderTargets.FogFramebuffer : framebuffer);
                
                if (this.EnableBloom) {
                    this.DrawBloomCompositePass(commandList, framebuffer, true);
                }
            }
            else if (this.EnableSsr) {
                this.DrawSsrPass(commandList, this.EnableBloom ? this._renderTargets.ColorFramebuffer : framebuffer, cam3D, false);
                
                if (this.EnableBloom) {
                    this.DrawBloomCompositePass(commandList, framebuffer, false);
                }
            }
            else if (this.EnableBloom) {
                this.DrawBloomCompositePass(commandList, framebuffer, false);
            }
        }
        else {
            commandList.SetFramebuffer(framebuffer);
            
            // Draw Skybox.
            this.SkyBox?.Draw(commandList, framebuffer);
            
            this.BeforeOpaquePass?.Invoke(commandList, framebuffer);
            
            // Draw opaques renderables.
            this.DrawOpaqueScene(commandList, framebuffer, cam3D);
            
            this.AfterOpaquePass?.Invoke(commandList, framebuffer);
            
            // Draw translucent renderables.
            this.DrawTranslucentScene(commandList, framebuffer, cam3D);
            
            this.AfterTranslucentPass?.Invoke(commandList, framebuffer);
        }
        
        // Clean up.
        this._opaqueRenderables.Clear();
        this._translucentRenderables.Clear();
        this._lights.Clear();
        this._culledLights.Clear();
        this._localLightCandidates.Clear();
        this._shadowLight = null;
    }
    
    /// <summary>
    /// Renders all opaque objects in the scene.
    /// </summary>
    /// <param name="commandList">The command list used to issue draw commands to the GPU.</param>
    /// <param name="framebuffer">The target framebuffer where the opaque scene is rendered.</param>
    /// <param name="cam">The 3D camera providing the view and projection matrices for rendering.</param>
    private void DrawOpaqueScene(CommandList commandList, Framebuffer framebuffer, Cam3D cam) {
        foreach (Renderable renderable in this._opaqueRenderables) {
            this.DrawPreparedRenderable(commandList, framebuffer, cam, renderable);
        }
    }
    
    /// <summary>
    /// Renders all translucent objects in the scene.
    /// </summary>
    /// <param name="commandList">The command list used to issue rendering commands to the GPU.</param>
    /// <param name="framebuffer">The framebuffer where the translucent objects will be rendered.</param>
    /// <param name="cam">The camera providing the view and projection matrices for rendering.</param>
    private void DrawTranslucentScene(CommandList commandList, Framebuffer framebuffer, Cam3D cam) {
        foreach (Renderable renderable in this._translucentRenderables) {
            this.DrawPreparedRenderable(commandList, framebuffer, cam, renderable);
        }
    }
    
    /// <summary>
    /// Renders the shadow map from the shadow light's point of view and updates the shadow buffers.
    /// </summary>
    /// <param name="commandList">The command list used to record the GPU commands.</param>
    /// <param name="framebuffer">The framebuffer that is restored after the shadow pass.</param>
    /// <param name="cam">The camera the shadow volume is centered on.</param>
    private void DrawShadowScene(CommandList commandList, Framebuffer framebuffer, Cam3D cam) {
        if (this._shadowLight == null || !this._shadowLight.Enabled || !this._shadowLight.CastShadows) {
            this._shadowData.ShadowMapResolution = this.ShadowMapResolution;
            this._shadowData.ShadowBias = this.ShadowBias;
            this._shadowData.ShadowNormalBias = this.ShadowNormalBias;
            this._shadowData.ShadowBiasClamp = this.ShadowBiasClamp;
            this._shadowData.AmbientColor = this.AmbientColor;
            this._shadowData.LightVP = Matrix4x4.Identity;
            this._shadowData.LightDirection = -Vector4.UnitY;
            this._shadowData.LightColor = Vector4.Zero;
            
            this._shadowBuffer.SetValue(0, this._shadowData);
            this._shadowBuffer.UpdateBufferDeferred(commandList);
            return;
        }
        
        // Calculate direction for the light.
        Vector3 lightDirection = Vector3.Transform(-Vector3.UnitZ, this._shadowLight.LerpedRotation);
        
        if (lightDirection.LengthSquared() <= 0.0F) {
            lightDirection = -Vector3.UnitY;
        }
        else {
            lightDirection = Vector3.Normalize(lightDirection);
        }
        
        // Use a different up vector when the light points almost straight up or down, to avoid a degenerate view matrix.
        Vector3 up = MathF.Abs(Vector3.Dot(lightDirection, Vector3.UnitY)) > 0.95F ? Vector3.UnitZ : Vector3.UnitY;
        
        // Place the light camera behind the main camera, looking along the light direction.
        Vector3 lightPosition = cam.Position - lightDirection * this.ShadowCameraDistance;
        
        // Build the light view, the orthographic projection and the combined matrix.
        Matrix4x4 lightView = Matrix4x4.CreateLookAt(lightPosition, cam.Position, up);
        Matrix4x4 lightProjection = Matrix4x4.CreateOrthographic(this.ShadowCameraSize, this.ShadowCameraSize, this.ShadowCameraNear, this.ShadowCameraFar);
        Matrix4x4 lightVp = lightView * lightProjection;
        
        // Fill the shadow data.
        this._shadowData.ShadowMapResolution = this.ShadowMapResolution;
        this._shadowData.ShadowBias = this.ShadowBias;
        this._shadowData.ShadowNormalBias = this.ShadowNormalBias;
        this._shadowData.ShadowBiasClamp = this.ShadowBiasClamp;
        this._shadowData.AmbientColor = Color.Black.ToRgbaFloatVec4();
        this._shadowData.LightVP = lightVp;
        this._shadowData.LightDirection = lightDirection.AsVector4();
        this._shadowData.LightColor = new Vector4(this._shadowLight.Color.ToRgbaFloatVec4().AsVector3(), this._shadowLight.Intensity);
        
        // Upload the shadow data to the GPU.
        this._shadowBuffer.SetValue(0, this._shadowData);
        this._shadowBuffer.UpdateBufferDeferred(commandList);
        
        // Upload the light matrices.
        this._shadowMatrixBuffer.SetValue(0, lightProjection);
        this._shadowMatrixBuffer.SetValue(1, lightView);
        this._shadowMatrixBuffer.UpdateBufferDeferred(commandList);
        
        // Switch to the shadow map.
        commandList.SetFramebuffer(this._shadowMap.Framebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(1.0F, 1.0F, 1.0F, 1.0F));
        commandList.ClearDepthStencil(1.0F);
        
        // Render all opaque renderables into the shadow map.
        foreach (Renderable renderable in this._opaqueRenderables) {
            this.DrawPreparedShadowRenderable(commandList, renderable);
        }
        
        // Set to default framebuffer back.
        commandList.SetFramebuffer(framebuffer);
    }
    
    /// <summary>
    /// Draws world-space normal and depth data for SSR.
    /// </summary>
    /// <param name="commandList">The command list used for rendering.</param>
    /// <param name="camera">The active camera.</param>
    private void DrawNormalPrepass(CommandList commandList, Cam3D camera) {
        foreach (Renderable renderable in this._opaqueRenderables) {
            this.DrawPreparedNormalRenderable(commandList, camera, renderable);
        }
    }
    
    /// <summary>
    /// Draws a prepared renderable object by configuring necessary buffers, setting up pipeline parameters, and executing the rendering process.
    /// </summary>
    /// <param name="commandList">The command list used for issuing rendering commands.</param>
    /// <param name="framebuffer"></param>
    /// <param name="camera">The 3D camera providing view and projection matrices for rendering.</param>
    /// <param name="renderable">The renderable object containing mesh, material, transforms, and other rendering data to be processed.</param>
    private void DrawPreparedRenderable(CommandList commandList, Framebuffer framebuffer, Cam3D camera, Renderable renderable) {
        
        // Set the pipeline parameters.
        this._pipelineDescription.Outputs = framebuffer.OutputDescription;
        this._pipelineDescription.BlendState = renderable.Material.BlendState;
        this._pipelineDescription.RasterizerState = renderable.Material.RasterizerState;
        this._pipelineDescription.DepthStencilState = DepthStencilStateDescription.DEPTH_ONLY_LESS_EQUAL;
        this._pipelineDescription.BufferLayouts = renderable.Material.Effect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = renderable.Material.Effect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = new ShaderSetDescription(renderable.Mesh.VertexFormat.Layouts, renderable.Mesh.Material.Effect.Shaders);
        this._pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        
        // Set pipeline.
        commandList.SetPipeline(renderable.Material.Effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set matrix buffer.
        commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetBufferLayoutSlot("MatrixBuffer"), camera.GetMatrixBuffer().GetResourceSet(renderable.Material.Effect.GetBufferLayout("MatrixBuffer")));
        
        // Set transform buffer.
        commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetBufferLayoutSlot("TransformBuffer"), renderable.GetTransformBuffer().GetResourceSet(renderable.Material.Effect.GetBufferLayout("TransformBuffer")));
        
        // Set bone buffer.
        if (renderable.HasBones) {
            SimpleUniformBuffer<Matrix4x4>? boneBuffer = renderable.GetBoneBuffer();
            
            if (boneBuffer != null) {
                commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetBufferLayoutSlot("BoneBuffer"), boneBuffer.GetResourceSet(renderable.Material.Effect.GetBufferLayout("BoneBuffer")));
            }
        }
        
        // Set material map buffer.
        commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetBufferLayoutSlot("MaterialBuffer"), renderable.GetMaterialBuffer().GetResourceSet(renderable.Material.Effect.GetBufferLayout("MaterialBuffer")));
        
        // Set light buffer.
        commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetBufferLayoutSlot("LightBuffer"), this._lightBuffer.GetResourceSet(renderable.Material.Effect.GetBufferLayout("LightBuffer")));
        
        // Set cluster data buffer.
        commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetBufferLayoutSlot("ClusterDataBuffer"), this._clusterDataBuffer.GetResourceSet(renderable.Material.Effect.GetBufferLayout("ClusterDataBuffer")));
        
        // Set cluster buffer.
        commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetBufferLayoutSlot("ClusterBuffer"), this._clusterBuffer.GetResourceSet(renderable.Material.Effect.GetBufferLayout("ClusterBuffer")));
        
        // Set cluster light-index buffer.
        commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetBufferLayoutSlot("ClusterLightIndexBuffer"), this._clusterLightIndexBuffer.GetResourceSet(renderable.Material.Effect.GetBufferLayout("ClusterLightIndexBuffer")));
        
        // Set shadow buffer.
        commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetBufferLayoutSlot("ShadowBuffer"), this._shadowBuffer.GetResourceSet(renderable.Material.Effect.GetBufferLayout("ShadowBuffer")));
        
        // Set environment buffer.
        commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetBufferLayoutSlot("EnvironmentBuffer"), this._environmentBuffer.GetResourceSet(renderable.Material.Effect.GetBufferLayout("EnvironmentBuffer")));
        
        // Set material texture.
        foreach (SimpleTextureLayout textureLayout in renderable.Material.Effect.GetTextureLayouts()) {
            foreach (MaterialMapKey mapKey in renderable.Material.GetMaterialMapKeys()) {
                if (textureLayout.Name == mapKey.Name) {
                    string mapName = textureLayout.Name;
                    MaterialMap map = renderable.Material.GetMaterialMap(mapKey)!;
                    ResourceSet? resourceSet = map.GetTextureResourceSet(map.Sampler ?? GraphicsHelper.GetSampler(this.GraphicsDevice, SamplerType.PointWrap), renderable.Material.Effect.GetTextureLayout(mapName));
                    
                    if (resourceSet != null) {
                        commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetTextureLayoutSlot(mapName), resourceSet);
                    }
                }
            }
        }
        
        // Set shadow map.
        commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetTextureLayoutSlot("fShadowMap"), this._shadowMap.GetResourceSet(this._shadowSampler, renderable.Material.Effect.GetTextureLayout("fShadowMap")));
        
        // Set PBR environment reflection maps.
        if (this._environmentMap != null) {
            
            // Set irradiance map.
            commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetTextureLayoutSlot("fIrradianceMap"), this._environmentMap.GetIrradianceResourceSet(this._irradianceSampler, renderable.Material.Effect.GetTextureLayout("fIrradianceMap")));
            
            // Set prefilter map.
            commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetTextureLayoutSlot("fPrefilterMap"), this._environmentMap.GetPrefilterResourceSet(this._prefilterSampler, renderable.Material.Effect.GetTextureLayout("fPrefilterMap")));
        }
        
        // Set Brdf lut.
        commandList.SetGraphicsResourceSet(renderable.Material.Effect.GetTextureLayoutSlot("fBrdfLut"), this._brdfLut.GetResourceSet(this._brdfLutSampler, renderable.Material.Effect.GetTextureLayout("fBrdfLut")));
        
        // Apply effect.
        renderable.Material.Effect.Apply(commandList, renderable.Material);
        
        // Draw renderable and set vertex/index buffers.
        if (renderable.Mesh.IndexCount > 0) {
            
            // Set vertex and index buffer.
            commandList.SetVertexBuffer(0, renderable.Mesh.VertexBuffer);
            commandList.SetIndexBuffer(renderable.Mesh.IndexBuffer, IndexFormat.UInt32);
            
            if (renderable.UseInstancing) {
                
                // Set the instance buffer.
                commandList.SetVertexBuffer(1, renderable.GetInstanceVertexBuffer());
                
                // Draw.
                commandList.DrawIndexed(renderable.Mesh.IndexCount, renderable.InstanceCount, 0, 0, 0);
            }
            else {
                
                // Draw.
                commandList.DrawIndexed(renderable.Mesh.IndexCount);
            }
        }
        else {
            
            // Set vertex buffer.
            commandList.SetVertexBuffer(0, renderable.Mesh.VertexBuffer);
            
            if (renderable.UseInstancing) {
                
                // Set the instance buffer.
                commandList.SetVertexBuffer(1, renderable.GetInstanceVertexBuffer());
                
                // Draw.
                commandList.Draw(renderable.Mesh.VertexCount, renderable.InstanceCount, 0, 0);
            }
            else {
                
                // Draw.
                commandList.Draw(renderable.Mesh.VertexCount);
            }
        }
    }
    
    /// <summary>
    /// Draws a shadow renderable that has been prepared for rendering, using the appropriate effect and pipeline configuration.
    /// </summary>
    /// <param name="commandList">The command list used for issuing rendering commands to the GPU.</param>
    /// <param name="renderable">The renderable object representing the mesh and associated data needed for rendering.</param>
    private void DrawPreparedShadowRenderable(CommandList commandList, Renderable renderable) {
        Effect effect = renderable.HasBones ? GlobalGraphicsAssets.SkinnedShadowEffect : GlobalGraphicsAssets.ShadowEffect;
        
        // Set the pipeline parameters.
        this._pipelineDescription.Outputs = this._shadowMap.Framebuffer.OutputDescription;
        this._pipelineDescription.BlendState = BlendStateDescription.SINGLE_DISABLED;
        this._pipelineDescription.RasterizerState = RasterizerStateDescription.DEFAULT with { CullMode = FaceCullMode.Front };
        this._pipelineDescription.DepthStencilState = DepthStencilStateDescription.DEPTH_ONLY_LESS_EQUAL;
        this._pipelineDescription.BufferLayouts = effect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = effect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = new ShaderSetDescription(renderable.Mesh.VertexFormat.Layouts, effect.Shaders);
        
        // Set pipeline.
        commandList.SetPipeline(effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set matrix buffer.
        commandList.SetGraphicsResourceSet(effect.GetBufferLayoutSlot("MatrixBuffer"), this._shadowMatrixBuffer.GetResourceSet(effect.GetBufferLayout("MatrixBuffer")));
        
        // Set transform buffer.
        commandList.SetGraphicsResourceSet(effect.GetBufferLayoutSlot("TransformBuffer"), renderable.GetTransformBuffer().GetResourceSet(effect.GetBufferLayout("TransformBuffer")));
        
        // Set bone buffer.
        if (renderable.HasBones) {
            SimpleUniformBuffer<Matrix4x4>? boneBuffer = renderable.GetBoneBuffer();
            
            if (boneBuffer != null) {
                commandList.SetGraphicsResourceSet(effect.GetBufferLayoutSlot("BoneBuffer"), boneBuffer.GetResourceSet(effect.GetBufferLayout("BoneBuffer")));
            }
        }
        
        // Set material map buffer.
        commandList.SetGraphicsResourceSet(effect.GetBufferLayoutSlot("MaterialBuffer"), renderable.GetMaterialBuffer().GetResourceSet(effect.GetBufferLayout("MaterialBuffer")));
        
        // Set material texture.
        foreach (SimpleTextureLayout textureLayout in effect.GetTextureLayouts()) {
            foreach (MaterialMapKey mapKey in renderable.Material.GetMaterialMapKeys()) {
                if (textureLayout.Name == mapKey.Name) {
                    string mapName = textureLayout.Name;
                    MaterialMap map = renderable.Material.GetMaterialMap(mapKey)!;
                    ResourceSet? resourceSet = map.GetTextureResourceSet(map.Sampler ?? GraphicsHelper.GetSampler(this.GraphicsDevice, SamplerType.PointWrap), effect.GetTextureLayout(mapName));
                    
                    if (resourceSet != null) {
                        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot(mapName), resourceSet);
                    }
                }
            }
        }
        
        // Apply effect.
        effect.Apply(commandList, renderable.Material);
        
        // Draw renderable and set vertex/index buffers.
        if (renderable.Mesh.IndexCount > 0) {
            
            // Set vertex and index buffer.
            commandList.SetVertexBuffer(0, renderable.Mesh.VertexBuffer);
            commandList.SetIndexBuffer(renderable.Mesh.IndexBuffer, IndexFormat.UInt32);
            
            if (renderable.UseInstancing) {
                
                // Set the instance buffer.
                commandList.SetVertexBuffer(1, renderable.GetInstanceVertexBuffer());
                
                // Draw.
                commandList.DrawIndexed(renderable.Mesh.IndexCount, renderable.InstanceCount, 0, 0, 0);
            }
            else {
                
                // Draw.
                commandList.DrawIndexed(renderable.Mesh.IndexCount);
            }
        }
        else {
            
            // Set vertex buffer.
            commandList.SetVertexBuffer(0, renderable.Mesh.VertexBuffer);
            
            if (renderable.UseInstancing) {
                
                // Set the instance buffer.
                commandList.SetVertexBuffer(1, renderable.GetInstanceVertexBuffer());
                
                // Draw.
                commandList.Draw(renderable.Mesh.VertexCount, renderable.InstanceCount, 0, 0);
            }
            else {
                
                // Draw.
                commandList.Draw(renderable.Mesh.VertexCount);
            }
        }
    }

    /// <summary>
    /// Prepares and draws the normal renderable using the specified command list, camera, and renderable parameters.
    /// </summary>
    /// <param name="commandList">The command list used for issuing rendering commands.</param>
    /// <param name="camera">The 3D camera providing the view and projection matrices.</param>
    /// <param name="renderable">The renderable object containing mesh, material, and transformation data to be drawn.</param>
    private void DrawPreparedNormalRenderable(CommandList commandList, Cam3D camera, Renderable renderable) {
        Effect effect = renderable.HasBones ? GlobalGraphicsAssets.SkinnedPbrNormalPrepassEffect : GlobalGraphicsAssets.PbrNormalPrepassEffect;
        
        if (renderable.UseInstancing) {
            effect = effect.GetEffectVariant([("USE_INSTANCING", "1")]).Effect;
        }
        
        // Set the pipeline parameters.
        this._pipelineDescription.Outputs = this._renderTargets!.NormalFramebuffer.OutputDescription;
        this._pipelineDescription.BlendState = BlendStateDescription.SINGLE_DISABLED;
        this._pipelineDescription.RasterizerState = renderable.Material.RasterizerState;
        this._pipelineDescription.DepthStencilState = DepthStencilStateDescription.DEPTH_ONLY_LESS_EQUAL;
        this._pipelineDescription.BufferLayouts = effect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = effect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = new ShaderSetDescription(renderable.Mesh.VertexFormat.Layouts, effect.Shaders);
        this._pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        
        // Set pipeline.
        commandList.SetPipeline(effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set matrix buffer.
        commandList.SetGraphicsResourceSet(effect.GetBufferLayoutSlot("MatrixBuffer"), camera.GetMatrixBuffer().GetResourceSet(effect.GetBufferLayout("MatrixBuffer")));
        
        // Set transform buffer.
        commandList.SetGraphicsResourceSet(effect.GetBufferLayoutSlot("TransformBuffer"), renderable.GetTransformBuffer().GetResourceSet(effect.GetBufferLayout("TransformBuffer")));
        
        // Set bone buffer.
        if (renderable.HasBones) {
            SimpleUniformBuffer<Matrix4x4>? boneBuffer = renderable.GetBoneBuffer();
            
            if (boneBuffer != null) {
                commandList.SetGraphicsResourceSet(effect.GetBufferLayoutSlot("BoneBuffer"), boneBuffer.GetResourceSet(effect.GetBufferLayout("BoneBuffer")));
            }
        }
        
        // Apply effect.
        effect.Apply(commandList, renderable.Material);
        
        // Draw renderable and set vertex/index buffers.
        if (renderable.Mesh.IndexCount > 0) {
            
            // Set vertex and index buffer.
            commandList.SetVertexBuffer(0, renderable.Mesh.VertexBuffer);
            commandList.SetIndexBuffer(renderable.Mesh.IndexBuffer, IndexFormat.UInt32);
            
            if (renderable.UseInstancing) {
                
                // Set the instance buffer.
                commandList.SetVertexBuffer(1, renderable.GetInstanceVertexBuffer());
                
                // Draw.
                commandList.DrawIndexed(renderable.Mesh.IndexCount, renderable.InstanceCount, 0, 0, 0);
            }
            else {
                
                // Draw.
                commandList.DrawIndexed(renderable.Mesh.IndexCount);
            }
        }
        else {
            
            // Set vertex buffer.
            commandList.SetVertexBuffer(0, renderable.Mesh.VertexBuffer);
            
            if (renderable.UseInstancing) {
                
                // Set the instance buffer.
                commandList.SetVertexBuffer(1, renderable.GetInstanceVertexBuffer());
                
                // Draw.
                commandList.Draw(renderable.Mesh.VertexCount, renderable.InstanceCount, 0, 0);
            }
            else {
                
                // Draw.
                commandList.Draw(renderable.Mesh.VertexCount);
            }
        }
    }
    
    /// <summary>
    /// Blurs the emission bloom mask into the bloom texture using two cheap separable passes.
    /// </summary>
    /// <param name="commandList">The command list used to record rendering commands.</param>
    private void DrawBloomPass(CommandList commandList) {
        Effect effect = GlobalGraphicsAssets.BloomEffect;
        
        // Horizontal pass: bloom mask temp bloom texture.
        this.DrawBloomBlurPass(
            commandList,
            effect,
            this._renderTargets!.BloomTempFramebuffer,
            this._renderTargets.GetBloomMaskResourceSet(this._ssrSampler, effect.GetTextureLayout("fBloomMask")),
            new Vector2(this._renderTargets.BloomMaskTexture.Width, this._renderTargets.BloomMaskTexture.Height),
            new Vector2(1.0F, 0.0F),
            1.0F
        );
        
        // Vertical pass: temp bloom texture final bloom texture.
        this.DrawBloomBlurPass(
            commandList,
            effect,
            this._renderTargets.BloomFramebuffer,
            this._renderTargets.GetBloomTempResourceSet(this._ssrSampler, effect.GetTextureLayout("fBloomMask")),
            new Vector2(this._renderTargets.BloomTempTexture.Width, this._renderTargets.BloomTempTexture.Height),
            new Vector2(0.0F, 1.0F),
            this.BloomStrength
        );
    }
    
    /// <summary>
    /// Draws one separable bloom blur pass.
    /// </summary>
    /// <param name="commandList">The command list used to record rendering commands.</param>
    /// <param name="effect">The bloom blur effect.</param>
    /// <param name="targetFramebuffer">The framebuffer to render into.</param>
    /// <param name="sourceResourceSet">The source texture resource set.</param>
    /// <param name="sourceSize">The source texture size in pixels.</param>
    /// <param name="direction">The blur direction.</param>
    /// <param name="strength">The strength applied in this pass.</param>
    private void DrawBloomBlurPass(CommandList commandList, Effect effect, Framebuffer targetFramebuffer, ResourceSet sourceResourceSet, Vector2 sourceSize, Vector2 direction, float strength) {
        
        // Fill the bloom buffer.
        this._bloomData.ScreenSize = sourceSize;
        this._bloomData.Samples = this.BloomSamples;
        this._bloomData.Quality = this.BloomQuality;
        this._bloomData.Strength = strength;
        this._bloomData.Threshold = this.BloomThreshold;
        this._bloomData.Direction = direction;
        
        // Upload buffer to GPU.
        this._bloomBuffer.SetValue(0, this._bloomData);
        this._bloomBuffer.UpdateBufferDeferred(commandList);
        
        // Set the pipeline parameters.
        this._pipelineDescription.Outputs = targetFramebuffer.OutputDescription;
        this._pipelineDescription.BlendState = BlendStateDescription.SINGLE_DISABLED;
        this._pipelineDescription.RasterizerState = RasterizerStateDescription.CULL_NONE;
        this._pipelineDescription.DepthStencilState = DepthStencilStateDescription.DISABLED;
        this._pipelineDescription.BufferLayouts = effect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = effect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = new ShaderSetDescription([], effect.Shaders);
        this._pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        
        // Render into the target framebuffer.
        commandList.SetFramebuffer(targetFramebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(0.0F, 0.0F, 0.0F, 1.0F));
        
        // Set pipeline.
        commandList.SetPipeline(effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set the Bloom buffer.
        commandList.SetGraphicsResourceSet(effect.GetBufferLayoutSlot("BloomBuffer"), this._bloomBuffer.GetResourceSet(effect.GetBufferLayout("BloomBuffer")));
        
        // Set bloom mask.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fBloomMask"), sourceResourceSet);
        
        // Apply effect.
        effect.Apply(commandList);
        
        // Draw.
        commandList.Draw(3);
    }
    
    /// <summary>
    /// Composites blurred bloom over a scene color texture.
    /// </summary>
    /// <param name="commandList">The command list used to record rendering commands.</param>
    /// <param name="framebuffer">The framebuffer that receives the composited image.</param>
    /// <param name="useFoggedSceneColor">Whether bloom should sample the fogged scene color instead of the raw scene color.</param>
    private void DrawBloomCompositePass(CommandList commandList, Framebuffer framebuffer, bool useFoggedSceneColor) {
        Effect effect = GlobalGraphicsAssets.BloomCompositeEffect;
        
        // Set the pipeline parameters.
        this._pipelineDescription.Outputs = framebuffer.OutputDescription;
        this._pipelineDescription.BlendState = BlendStateDescription.SINGLE_DISABLED;
        this._pipelineDescription.RasterizerState = RasterizerStateDescription.CULL_NONE;
        this._pipelineDescription.DepthStencilState = DepthStencilStateDescription.DISABLED;
        this._pipelineDescription.BufferLayouts = effect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = effect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = new ShaderSetDescription([], effect.Shaders);
        this._pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        
        // Render into the target framebuffer.
        commandList.SetFramebuffer(framebuffer);
        
        // Set pipeline.
        commandList.SetPipeline(effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set scene color.
        if (useFoggedSceneColor) {
            commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSceneColor"), this._renderTargets!.GetFogResourceSet(this._ssrSampler, effect.GetTextureLayout("fSceneColor")));
        }
        else {
            commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSceneColor"), this._renderTargets!.GetColorResourceSet(this._ssrSampler, effect.GetTextureLayout("fSceneColor")));
        }
        
        // Set blurred bloom.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fBloomColor"), this._renderTargets.GetBloomResourceSet(this._ssrSampler, effect.GetTextureLayout("fBloomColor")));
        
        // Apply effect.
        effect.Apply(commandList);
        
        // Draw.
        commandList.Draw(3);
    }
    
    /// <summary>
    /// Traces the screen-space reflections and composites them onto the final framebuffer.
    /// </summary>
    /// <param name="commandList">The command list used to record the rendering commands.</param>
    /// <param name="framebuffer">The framebuffer that receives the final composited image.</param>
    /// <param name="camera">The 3D camera providing the matrices used to move between screen and world space.</param>
    /// <param name="useFoggedSceneColor">Whether SSR should sample the fogged scene color instead of the raw scene color.</param>
    private void DrawSsrPass(CommandList commandList, Framebuffer framebuffer, Cam3D camera, bool useFoggedSceneColor) {
        Effect effect = GlobalGraphicsAssets.SsrEffect;
        
        Matrix4x4 projection = camera.GetProjection();
        Matrix4x4 view = camera.GetView();
        Matrix4x4 viewProjection = view * projection;
        
        Matrix4x4.Invert(projection, out Matrix4x4 inverseProjection);
        Matrix4x4.Invert(view, out Matrix4x4 inverseView);
        Matrix4x4.Invert(viewProjection, out Matrix4x4 inverseViewProjection);
        
        // Fill the SSR buffer.
        this._ssrData.ScreenSize = new Vector2(this._renderTargets!.ColorFramebuffer.Width, this._renderTargets.ColorFramebuffer.Height);
        this._ssrData.MaxDistance = this.SsrMaxDistance;
        this._ssrData.MaxSteps = this.SsrMaxSteps;
        this._ssrData.StepSize = this.SsrStepSize;
        this._ssrData.Thickness = this.SsrThickness;
        this._ssrData.Strength = this.SsrStrength;
        this._ssrData.EdgeFade = this.SsrEdgeFade;
        this._ssrData.Projection = projection;
        this._ssrData.View = view;
        this._ssrData.ViewProjection = viewProjection;
        this._ssrData.InverseProjection = inverseProjection;
        this._ssrData.InverseView = inverseView;
        this._ssrData.InverseViewProjection = inverseViewProjection;
        
        // Upload buffer to GPU.
        this._ssrBuffer.SetValue(0, this._ssrData);
        this._ssrBuffer.UpdateBufferDeferred(commandList);
        
        // Set the pipeline parameters.
        this._pipelineDescription.Outputs = this._renderTargets.SsrFramebuffer.OutputDescription;
        this._pipelineDescription.BlendState = BlendStateDescription.SINGLE_DISABLED;
        this._pipelineDescription.RasterizerState = RasterizerStateDescription.CULL_NONE;
        this._pipelineDescription.DepthStencilState = DepthStencilStateDescription.DISABLED;
        this._pipelineDescription.BufferLayouts = effect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = effect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = new ShaderSetDescription([], effect.Shaders);
        this._pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        
        // Render into the SSR framebuffer. A fully transparent clear color means "no reflection" for every pixel the shader does not write.
        commandList.SetFramebuffer(this._renderTargets.SsrFramebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(0.0F, 0.0F, 0.0F, 0.0F));
        
        // Set pipeline.
        commandList.SetPipeline(effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set the SSR buffer.
        commandList.SetGraphicsResourceSet(effect.GetBufferLayoutSlot("SsrBuffer"), this._ssrBuffer.GetResourceSet(effect.GetBufferLayout("SsrBuffer")));
        
        // Set the lit scene color.
        if (useFoggedSceneColor) {
            commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSceneColor"), this._renderTargets.GetFogResourceSet(this._ssrSampler, effect.GetTextureLayout("fSceneColor")));
        }
        else {
            commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSceneColor"), this._renderTargets.GetColorResourceSet(this._ssrSampler, effect.GetTextureLayout("fSceneColor")));
        }
        
        // Set the world-space normals from the normal prepass, used to calculate the reflection direction.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fNormal"), this._renderTargets.GetNormalResourceSet(this._ssrSampler, effect.GetTextureLayout("fNormal")));
        
        // Set the scene depth, used to reconstruct world positions and to detect ray hits.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fDepth"), this._renderTargets.GetDepthResourceSet(this._ssrSampler, effect.GetTextureLayout("fDepth")));
        
        // Set the per-pixel material reflectivity, which decides which pixels are traced at all.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSsrMask"), this._renderTargets.GetSsrMaskResourceSet(this._ssrSampler, effect.GetTextureLayout("fSsrMask")));
        
        // Apply effect.
        effect.Apply(commandList);
        
        // Draw.
        commandList.Draw(3);
        
        // Blend the traced reflections onto the scene.
        this.DrawSsrCompositePass(commandList, framebuffer, useFoggedSceneColor);
    }
    
    /// <summary>
    /// Composites the traced screen-space reflections onto the scene in the final framebuffer.
    /// </summary>
    /// <param name="commandList">The command list used to record the rendering commands.</param>
    /// <param name="framebuffer">The framebuffer that receives the final composited image.</param>
    /// <param name="useFoggedSceneColor">Whether SSR should sample the fogged scene color instead of the raw scene color.</param>
    private void DrawSsrCompositePass(CommandList commandList, Framebuffer framebuffer, bool useFoggedSceneColor) {
        Effect effect = GlobalGraphicsAssets.SsrCompositeEffect;
        
        // Set the pipeline parameters.
        this._pipelineDescription.Outputs = framebuffer.OutputDescription;
        this._pipelineDescription.BlendState = BlendStateDescription.SINGLE_DISABLED;
        this._pipelineDescription.RasterizerState = RasterizerStateDescription.CULL_NONE;
        this._pipelineDescription.DepthStencilState = DepthStencilStateDescription.DISABLED;
        this._pipelineDescription.BufferLayouts = effect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = effect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = new ShaderSetDescription([], effect.Shaders);
        this._pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        
        // Render into the target framebuffer.
        commandList.SetFramebuffer(framebuffer);
        
        // Set pipeline.
        commandList.SetPipeline(effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set the lit scene color.
        if (useFoggedSceneColor) {
            commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSceneColor"), this._renderTargets!.GetFogResourceSet(this._ssrSampler, effect.GetTextureLayout("fSceneColor")));
        }
        else {
            commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSceneColor"), this._renderTargets!.GetColorResourceSet(this._ssrSampler, effect.GetTextureLayout("fSceneColor")));
        }
        
        // Set the traced reflections (rgb = reflected color, a = opacity).
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSsrColor"), this._renderTargets.GetSsrResourceSet(this._ssrSampler, effect.GetTextureLayout("fSsrColor")));
        
        // Set the per-pixel material reflectivity, which scales the reflection strength.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSsrMask"), this._renderTargets.GetSsrMaskResourceSet(this._ssrSampler, effect.GetTextureLayout("fSsrMask")));
        
        // Apply effect.
        effect.Apply(commandList);
        
        // Draw.
        commandList.Draw(3);
    }
    
    /// <summary>
    /// Applies fullscreen exponential height fog to the rendered scene color.
    /// </summary>
    /// <param name="commandList">The command list used to record rendering commands.</param>
    /// <param name="framebuffer">The framebuffer that receives the fogged image.</param>
    private void DrawFogPass(CommandList commandList, Framebuffer framebuffer) {
        Effect effect = GlobalGraphicsAssets.ExponentialHeightFogEffect;
        
        // Set the pipeline parameters.
        this._pipelineDescription.Outputs = framebuffer.OutputDescription;
        this._pipelineDescription.BlendState = BlendStateDescription.SINGLE_DISABLED;
        this._pipelineDescription.RasterizerState = RasterizerStateDescription.CULL_NONE;
        this._pipelineDescription.DepthStencilState = DepthStencilStateDescription.DISABLED;
        this._pipelineDescription.BufferLayouts = effect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = effect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = new ShaderSetDescription([], effect.Shaders);
        this._pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        
        // Render into the target framebuffer.
        commandList.SetFramebuffer(framebuffer);
        
        // Set pipeline.
        commandList.SetPipeline(effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set fog buffer.
        commandList.SetGraphicsResourceSet(effect.GetBufferLayoutSlot("FogBuffer"), this._fogBuffer.GetResourceSet(effect.GetBufferLayout("FogBuffer")));
        
        // Set the lit scene color.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSceneColor"), this._renderTargets!.GetColorResourceSet(this._ssrSampler, effect.GetTextureLayout("fSceneColor")));
        
        // Set the scene depth, used to reconstruct world positions and to detect ray hits.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fDepth"), this._renderTargets.GetDepthResourceSet(this._ssrSampler, effect.GetTextureLayout("fDepth")));
        
        // Apply effect.
        effect.Apply(commandList);
        
        // Draw.
        commandList.Draw(3);
    }
    
    /// <summary>
    /// Updates the necessary GPU buffers for the specified renderable based on its dirty state.
    /// </summary>
    /// <param name="commandList">The command list used to issue GPU commands.</param>
    /// <param name="renderable">The renderable object whose buffers need to be updated.</param>
    private void UpdateRenderableBuffer(CommandList commandList, Renderable renderable) {
        
        // Update transform buffer.
        if (renderable.IsTransformBufferDirty) {
            renderable.UpdateTransformBuffer(commandList);
        }
        
        // Update instance vertex buffer.
        if (renderable.IsInstanceVertexBufferDirty) {
            renderable.UpdateInstanceVertexBuffer(commandList);
        }
        
        // Update bone buffer.
        if (renderable.IsBoneBufferDirty) {
            renderable.UpdateBoneBuffer(commandList);
        }
        
        // Update material buffer.
        if (renderable.IsMaterialBufferDirty) {
            renderable.UpdateMaterialBuffer(commandList);
        }
    }
    
    /// <summary>
    /// Updates the light buffer from queued lights.
    /// </summary>
    /// <param name="commandList">The command list used to upload the buffer.</param>
    /// <param name="camera">The active camera used for distance fade.</param>
    private void UpdateLightBuffer(CommandList commandList, Cam3D camera) {
        int numOfLights = Math.Min(this._culledLights.Count, this.LightCapacity);
        int numOfDirectionalLights = 0;
        
        for (int i = 0; i < numOfLights; i++) {
            if (this._culledLights[i].LightType != LightType.Directional) {
                break;
            }
            
            numOfDirectionalLights++;
        }
        
        PbrLightData lightHeader = new PbrLightData() {
            NumOfLights = numOfLights,
            NumOfDirectionalLights = numOfDirectionalLights,
            AmbientColor = this.AmbientColor
        };
        
        bool headerChanged = lightHeader != this._lastUploadedLightHeader;
        
        if (headerChanged) {
            this._lightBuffer.SetHeaderValue(0, lightHeader);
            this._lastUploadedLightHeader = lightHeader;
        }
        
        Span<PbrGpuLight> gpuLights = this._lightBuffer.ElementData.AsSpan();
        
        int dirtyStart = this.LightCapacity;
        int dirtyEnd = 0;
        
        // Fill/update active lights only when the generated GPU data changed.
        for (int i = 0; i < numOfLights; i++) {
            Light light = this._culledLights[i];
            
            Vector3 direction = Vector3.Transform(-Vector3.UnitZ, light.LerpedRotation);
            
            if (direction.LengthSquared() <= 0.0F) {
                direction = -Vector3.UnitY;
            }
            else {
                direction = Vector3.Normalize(direction);
            }
            
            PbrGpuLight gpuLight = new PbrGpuLight(
                light.LightType,
                light.LerpedGlobalPosition,
                direction,
                light.Color.ToRgbaFloatVec4().AsVector3(),
                light.Intensity * this.GetLightDistanceFade(light, camera),
                light.Range,
                float.DegreesToRadians(light.InnerSpotAngle),
                float.DegreesToRadians(light.OuterSpotAngle)
            );
            
            if (gpuLights[i] == gpuLight) {
                continue;
            }
            
            gpuLights[i] = gpuLight;
            
            dirtyStart = Math.Min(dirtyStart, i);
            dirtyEnd = Math.Max(dirtyEnd, i + 1);
        }
        
        // Clear only slots that were used last frame but are unused now.
        if (this._lastUploadedLightCount > numOfLights) {
            int clearStart = numOfLights;
            int clearEnd = this._lastUploadedLightCount;
            
            gpuLights.Slice(clearStart, clearEnd - clearStart).Clear();
            
            dirtyStart = Math.Min(dirtyStart, clearStart);
            dirtyEnd = Math.Max(dirtyEnd, clearEnd);
        }
        
        this._lastUploadedLightCount = numOfLights;
        
        bool lightElementsChanged = dirtyStart < dirtyEnd;
        
        if (!headerChanged && !lightElementsChanged) {
            return;
        }
        
        if (headerChanged) {
            this._lightBuffer.UpdateHeaderDeferred(commandList);
        }
        
        if (lightElementsChanged) {
            this._lightBuffer.UpdateElementRangeDeferred(commandList, dirtyStart, dirtyEnd - dirtyStart);
        }
    }
    
    /// <summary>
    /// Decides whether a light should be considered for the GPU light buffer this frame.
    /// </summary>
    /// <param name="light">The light to test.</param>
    /// <param name="camera">The active camera, used for the distance test.</param>
    /// <returns><see langword="true"/> if the light should be uploaded; otherwise, <see langword="false"/>.</returns>
    private bool ShouldUploadLight(Light light, Cam3D camera) {
        if (!light.Enabled || light.Intensity <= this.MinLightIntensity) {
            return false;
        }
        
        if (light.LightType == LightType.Directional) {
            return true;
        }
        
        if (light.Range <= 0.0F) {
            return false;
        }
        
        Vector3 lightPosition = light.LerpedGlobalPosition;
        float cameraDistanceSquared = Vector3.DistanceSquared(lightPosition, camera.Position);
        float maxDistance = this.LocalLightFadeEndDistance + light.Range;
        
        return cameraDistanceSquared <= maxDistance * maxDistance;
    }
    
    /// <summary>
    /// Calculates a score that describes how much a light contributes to the image. It is used to rank lights when there are more than the buffer can hold.
    /// </summary>
    /// <param name="light">The light to rate.</param>
    /// <param name="camera">The active camera, used for the distance.</param>
    /// <returns>The importance of the light. Higher values mean a more important light.</returns>
    private static float GetLightImportance(Light light, Cam3D camera) {
        if (light.LightType == LightType.Directional) {
            return float.PositiveInfinity;
        }
        
        Vector3 lightPosition = light.LerpedGlobalPosition;
        float distanceSquared = MathF.Max(Vector3.DistanceSquared(lightPosition, camera.Position), 1.0F);
        float rangeSquared = light.Range * light.Range;
        
        return light.Intensity * rangeSquared / distanceSquared;
    }
    
    /// <summary>
    /// Calculates how strongly a light is faded out by its distance to the camera.
    /// </summary>
    /// <param name="light">The light to fade.</param>
    /// <param name="camera">The active camera, used for the distance.</param>
    /// <returns>A multiplier in the range [0, 1] that is applied to the light.</returns>
    private float GetLightDistanceFade(Light light, Cam3D camera) {
        if (light.LightType == LightType.Directional) {
            return 1.0F;
        }
        
        // The end distance is kept slightly above the start distance, so the division below can never hit zero.
        float fadeStart = MathF.Max(0.0F, this.LocalLightFadeStartDistance);
        float fadeEnd = MathF.Max(fadeStart + 0.001F, this.LocalLightFadeEndDistance);
        
        // Convert the distance into a 0 to 1 progress value between the fade start and end.
        float distance = Vector3.Distance(light.LerpedGlobalPosition, camera.Position);
        float fadeT = Math.Clamp((distance - fadeStart) / (fadeEnd - fadeStart), 0.0F, 1.0F);
        
        // Smoothstep makes the fade start and end gently instead of linearly.
        fadeT = fadeT * fadeT * (3.0F - 2.0F * fadeT);
        
        return 1.0F - fadeT;
    }
    
    /// <summary>
    /// Culls and ranks the scene lights and stores the ones that are uploaded to the GPU this frame in the culled light list.
    /// </summary>
    /// <param name="camera">The active camera, used for culling and ranking the lights.</param>
    private void UpdateCulledLights(Cam3D camera) {
        this._culledLights.Clear();
        
        int directionalCount = 0;
        
        // Collect the directional lights.
        foreach (Light light in this._lights) {
            if (!this.ShouldUploadLight(light, camera)) {
                continue;
            }
            
            if (light.LightType != LightType.Directional) {
                continue;
            }
            
            if (directionalCount >= this.MaxDirectionalLights) {
                continue;
            }
            
            this._culledLights.Add(light);
            directionalCount++;
        }
        
        this._localLightCandidates.Clear();
        
        // Collect all local lights that pass the culling test.
        foreach (Light light in this._lights) {
            if (light.LightType == LightType.Directional || !this.ShouldUploadLight(light, camera)) {
                continue;
            }
            
            this._localLightCandidates.Add(light);
        }
        
        // Sort the candidates from most to least important, so the best lights are kept when there are too many.
        this._localLightCandidates.Sort((a, b) => GetLightImportance(b, camera).CompareTo(GetLightImportance(a, camera)));
        
        // The local lights may only use the slots that the directional lights left free, and never more than the configured limit.
        int remainingSlots = Math.Min(this.MaxVisibleLocalLights, this.LightCapacity - this._culledLights.Count);
        
        // Take the best candidates until the slots are full.
        for (int i = 0; i < this._localLightCandidates.Count && i < remainingSlots; i++) {
            this._culledLights.Add(this._localLightCandidates[i]);
        }
    }
    
    /// <summary>
    /// Updates clustered-lighting buffers from the currently culled lights.
    /// </summary>
    /// <param name="commandList">The command list used to upload the buffers.</param>
    /// <param name="camera">The active camera.</param>
    /// <param name="framebuffer">The target framebuffer.</param>
    private void UpdateClusterBuffers(CommandList commandList, Cam3D camera, Framebuffer framebuffer) {
        Matrix4x4 projection = camera.GetProjection();
        Matrix4x4 view = camera.GetView();
        
        Matrix4x4.Invert(projection, out Matrix4x4 inverseProjection);
        Matrix4x4.Invert(view, out Matrix4x4 inverseView);
        
        // Fill the cluster data buffer.
        this._clusterData.ScreenSize = new Vector2(framebuffer.Width, framebuffer.Height);
        this._clusterData.MaxLightsPerCluster = (uint) this.MaxLightsPerCluster;
        this._clusterData.ClusterCountX = (uint) this.ClusterCountX;
        this._clusterData.ClusterCountY = (uint) this.ClusterCountY;
        this._clusterData.ClusterCountZ = (uint) this.ClusterCountZ;
        this._clusterData.NearPlane = this.ClusterNearPlane;
        this._clusterData.FarPlane = this.ClusterFarPlane;
        this._clusterData.Projection = projection;
        this._clusterData.View = view;
        this._clusterData.InverseProjection = inverseProjection;
        this._clusterData.InverseView = inverseView;
        
        // Upload buffer to GPU.
        this._clusterDataBuffer.SetValue(0, this._clusterData);
        this._clusterDataBuffer.UpdateBufferDeferred(commandList);
        
        uint clusterCount = (uint) (this.ClusterCountX * this.ClusterCountY * this.ClusterCountZ);
        
        ComputeEffect clusterLightsEffect = GlobalGraphicsAssets.ClusterLightsEffect;
        
        // Dispatch.
        clusterLightsEffect.DispatchInvocations(
            commandList,
            [
                (clusterLightsEffect.GetBufferLayoutSlot("LightBuffer"), this._lightBuffer.GetResourceSet(clusterLightsEffect.GetBufferLayout("LightBuffer"))),
                (clusterLightsEffect.GetBufferLayoutSlot("ClusterDataBuffer"), this._clusterDataBuffer.GetResourceSet(clusterLightsEffect.GetBufferLayout("ClusterDataBuffer"))),
                (clusterLightsEffect.GetBufferLayoutSlot("ClusterBuffer"), this._clusterBuffer.GetResourceSet(clusterLightsEffect.GetBufferLayout("ClusterBuffer"))),
                (clusterLightsEffect.GetBufferLayoutSlot("ClusterLightIndexBuffer"), this._clusterLightIndexBuffer.GetResourceSet(clusterLightsEffect.GetBufferLayout("ClusterLightIndexBuffer")))
            ],
            clusterCount,
            1,
            1
        );
    }
    
    /// <summary>
    /// Updates the exponential height fog buffer.
    /// </summary>
    /// <param name="commandList">The command list used to upload the fog buffer.</param>
    /// <param name="camera">The active camera.</param>
    private void UpdateFogBuffer(CommandList commandList, Cam3D camera) {
        Matrix4x4 projection = camera.GetProjection();
        Matrix4x4 view = camera.GetView();
        Matrix4x4 viewProjection = view * projection;
        
        Matrix4x4.Invert(projection, out Matrix4x4 inverseProjection);
        Matrix4x4.Invert(view, out Matrix4x4 inverseView);
        Matrix4x4.Invert(viewProjection, out Matrix4x4 inverseViewProjection);
        
        Vector3 godRayDirection = -Vector3.UnitY;
        float godRayIntensity = 0.0F;
        
        if (this.EnableGodRays && this._shadowLight != null && this._shadowLight.Enabled && this._shadowLight.LightType == LightType.Directional) {
            godRayDirection = Vector3.Transform(-Vector3.UnitZ, this._shadowLight.LerpedRotation);
            
            if (godRayDirection.LengthSquared() <= 0.0F) {
                godRayDirection = -Vector3.UnitY;
            }
            else {
                godRayDirection = Vector3.Normalize(godRayDirection);
            }
            
            godRayIntensity = MathF.Max(0.0F, this.GodRayIntensity) * MathF.Max(0.0F, this._shadowLight.Intensity);
        }
        
        // Fill the fog data buffer.
        this._fogData = new ExponentialHeightFogData() {
            Color = this.EnableFog ? this.FogColor : Vector4.Zero,
            Parameters = new Vector4(
                MathF.Max(0.0F, this.FogDensity),
                MathF.Max(0.0F, this.FogFalloff),
                this.FogBaseHeight,
                MathF.Max(0.0F, this.FogStartDistance)
            ),
            DistanceParameters = new Vector4(
                MathF.Max(this.FogStartDistance + 0.001F, this.FogEndDistance),
                0.0F,
                0.0F,
                0.0F
            ),
            CameraPosition = new Vector4(camera.Position, MathF.Max(this.FogEndDistance, this.ClusterFarPlane)),
            GodRayDirectionIntensity = new Vector4(godRayDirection, godRayIntensity),
            GodRayColorLength = new Vector4(this.GodRayColor.AsVector3(), MathF.Max(1.0F, this.GodRayMaxLength)),
            GodRayParameters = new Vector4(
                Math.Clamp(this.GodRaySampleCount, 1.0F, 96.0F),
                MathF.Max(0.0F, this.GodRayDensity),
                MathF.Max(0.0F, this.GodRayWeight),
                Math.Clamp(this.GodRayDecay, 0.0F, 1.0F)
            ),
            GodRayPostParameters = new Vector4(
                MathF.Max(0.0F, this.GodRayExposure),
                MathF.Max(0.001F, this.GodRayDepthFadeDistance),
                Math.Clamp(this.GodRayHorizonFade, 0.0F, 1.0F),
                0.0F
            ),
            InverseProjection = inverseProjection,
            InverseView = inverseView,
            InverseViewProjection = inverseViewProjection
        };
        
        // Upload buffer to GPU.
        this._fogBuffer.SetValue(0, this._fogData);
        this._fogBuffer.UpdateBufferDeferred(commandList);
    }
    
    /// <summary>
    /// Ensures the current skybox has baked PBR IBL maps.
    /// </summary>
    /// <param name="commandList">The command list used to upload the environment buffer.</param>
    private void EnsurePbrEnvironmentMap(CommandList commandList) {
        if (this.SkyBox == null) {
            this._environmentMap?.Dispose();
            this._environmentMap = null;
            this._bakedSkyboxCubemap = null;
            this._environmentData.MaxReflectionLod = 0.0F;
            this._environmentData.EnvironmentColor = Vector4.Zero;
            this._environmentBuffer.SetValue(0, this._environmentData);
            this._environmentBuffer.UpdateBufferDeferred(commandList);
            return;
        }
        
        // The skybox color tints and scales the environment light, so it is refreshed every frame.
        this._environmentData.EnvironmentColor = this.SkyBox.Color.ToRgbaFloatVec4();
        
        // The maps were already baked for this cubemap, so only the buffer needs to be uploaded.
        if (this._environmentMap != null && this._bakedSkyboxCubemap == this.SkyBox.Cubemap) {
            this._environmentBuffer.SetValue(0, this._environmentData);
            this._environmentBuffer.UpdateBufferDeferred(commandList);
            return;
        }
        
        // Recreate environment map.
        this._environmentMap?.Dispose();
        this._environmentMap = new EnvironmentMap(this.GraphicsDevice, this.SkyBox.Cubemap, irradianceSize: 32, prefilterSize: 128, prefilterMipLevels: 5, prefilterSampleCount: 1024.0F);
        this._environmentData.MaxReflectionLod = this._environmentMap.PrefilterMipLevels - 1.0F;
        this._bakedSkyboxCubemap = this.SkyBox.Cubemap;
        
        // Upload buffer to GPU.
        this._environmentBuffer.SetValue(0, this._environmentData);
        this._environmentBuffer.UpdateBufferDeferred(commandList);
    }
    
    /// <summary>
    /// Ensures the renderer targets match the current framebuffer size, color format, and SSR quality.
    /// </summary>
    /// <param name="framebuffer">The final target framebuffer.</param>
    private void EnsureRenderTargets(Framebuffer framebuffer) {
        PixelFormat colorFormat = framebuffer.OutputDescription.ColorAttachments[0].Format;
        float ssrResolutionScale = this.GetSsrResolutionScale();
        
        if (this._renderTargets == null) {
            this._renderTargets = new PbrForwardRenderTargets(this.GraphicsDevice, framebuffer.Width, framebuffer.Height, colorFormat, ssrResolutionScale);
            return;
        }
        
        this._renderTargets.Resize(framebuffer.Width, framebuffer.Height, colorFormat, ssrResolutionScale);
    }
    
    /// <summary>
    /// Gets the internal SSR raymarch resolution scale for the selected quality.
    /// </summary>
    private float GetSsrResolutionScale() {
        return this.SsrQuality switch {
            ReflectionQuality.Low => 0.25F,
            ReflectionQuality.Medium => 0.5F,
            ReflectionQuality.High => 0.75F,
            ReflectionQuality.Ultra => 1.0F,
            _ => 0.5F
        };
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._shadowMap.Dispose();
            this._environmentMap?.Dispose();
            this._brdfLut.Dispose();
            this._renderTargets?.Dispose();
            this._shadowSampler.Dispose();
            this._irradianceSampler.Dispose();
            this._prefilterSampler.Dispose();
            this._brdfLutSampler.Dispose();
            this._ssrSampler.Dispose();
            this._lightBuffer.Dispose();
            this._clusterDataBuffer.Dispose();
            this._clusterBuffer.Dispose();
            this._clusterLightIndexBuffer.Dispose();
            this._shadowBuffer.Dispose();
            this._shadowMatrixBuffer.Dispose();
            this._environmentBuffer.Dispose();
            this._bloomBuffer.Dispose();
            this._ssrBuffer.Dispose();
            this._fogBuffer.Dispose();
        }
    }
}