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
using Sparkle.CSharp.Graphics.Rendering.Forward.Environment;
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
    /// List of opaque renderables waiting to be drawn.
    /// </summary>
    private List<Renderable> _opaqueRenderables;
    
    /// <summary>
    /// List of translucent renderables waiting to be drawn.
    /// </summary>
    private List<Renderable> _translucentRenderables;
    
    /// <summary>
    /// List of lights waiting to be uploaded for the current frame.
    /// </summary>
    private List<Light> _lights;
    
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
    /// Screen-space reflection render target holder.
    /// </summary>
    private ScreenSpaceReflectionMap? _screenSpaceReflectionMap;
    
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
    /// CPU-side light data uploaded to the GPU.
    /// </summary>
    private PbrLightData _lightData;
    
    /// <summary>
    /// CPU-side shadow data uploaded to the GPU.
    /// </summary>
    private ShadowData _shadowData;
    
    /// <summary>
    /// CPU-side environment data uploaded to the GPU.
    /// </summary>
    private EnvironmentData _environmentData;
    
    /// <summary>
    /// CPU-side SSR parameter data.
    /// </summary>
    private SsrData _ssrData;
    
    /// <summary>
    /// GPU-side light buffer.
    /// </summary>
    private SimpleUniformBuffer<PbrLightData> _lightBuffer;
    
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
    /// GPU-side SSR parameter buffer.
    /// </summary>
    private SimpleUniformBuffer<SsrData> _ssrBuffer;
    
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
    /// <param name="shadowMapResolution">The resolution of the square shadow map texture. Higher values result in better shadow quality at the cost of increased memory usage.</param>
    /// <param name="shadowCameraDistance">The distance between the shadow camera and its focus point, measured along the light direction.</param>
    /// <param name="shadowCameraSize">The size of the orthographic projection used by the shadow camera.</param>
    /// <param name="shadowCameraNear">The near-plane distance for the shadow camera, defining the start of the camera's visible range.</param>
    /// <param name="shadowCameraFar">The far-plane distance for the shadow camera, defining the end of the camera's visible range.</param>
    /// <param name="shadowBias">A constant depth bias value applied during shadow sampling to reduce shadow acne artifacts.</param>
    /// <param name="shadowNormalBias">A bias applied in the direction of the surface normal to mitigate artifacts at grazing angles during shadow sampling.</param>
    /// <param name="shadowBiasClamp">The maximum allowable offset for shadow bias adjustments, helping to prevent excessive "peter-panning" effects.</param>
    /// <param name="enableSsr">Specifies whether screen space reflections (SSR) are enabled for the renderer.</param>
    /// <param name="ssrQuality">The quality setting for screen space reflections, influencing the detail and performance of SSR effects.</param>
    /// <param name="ssrMaxDistance">The maximum traceable distance for screen space reflections, determining their area of influence.</param>
    /// <param name="ssrMaxSteps">The maximum number of ray marching steps used in SSR tracing.</param>
    /// <param name="ssrStepSize">The size of each incremental step during SSR ray tracing.</param>
    /// <param name="ssrThickness">The thickness threshold used to resolve SSR artifacts caused by thin geometry.</param>
    /// <param name="ssrStrength">The intensity of screen space reflections, determining their visual prominence.</param>
    /// <param name="ssrEdgeFade">The rate at which SSR effects fade toward screen edges, improving visual smoothness and blending.</param>
    public PbrForwardRenderer(
        GraphicsDevice graphicsDevice,
        SkyBox? skyBox = null,
        Color? ambientColor = null,
        int shadowMapResolution = 2048,
        float shadowCameraDistance = 80.0F,
        float shadowCameraSize = 120.0F,
        float shadowCameraNear = 0.1F,
        float shadowCameraFar = 250.0F,
        float shadowBias = 0.0005F,
        float shadowNormalBias = 0.0025F,
        float shadowBiasClamp = 0.01F,
        bool enableSsr = true,
        ReflectionQuality ssrQuality = ReflectionQuality.Medium,
        float ssrMaxDistance = 40.0F,
        float ssrMaxSteps = 48.0F,
        float ssrStepSize = 0.35F,
        float ssrThickness = 0.35F,
        float ssrStrength = 0.45F,
        float ssrEdgeFade = 0.08F) {
        this.GraphicsDevice = graphicsDevice;
        this.SkyBox = skyBox;
        this.AmbientColor = ambientColor?.ToRgbaFloatVec4() ?? new Vector4(0.03F, 0.03F, 0.03F, 1.0F);
        this.ShadowCameraDistance = shadowCameraDistance;
        this.ShadowCameraSize = shadowCameraSize;
        this.ShadowCameraNear = shadowCameraNear;
        this.ShadowCameraFar = shadowCameraFar;
        this.ShadowBias = shadowBias;
        this.ShadowNormalBias = shadowNormalBias;
        this.ShadowBiasClamp = shadowBiasClamp;
        this.EnableSsr = enableSsr;
        this.SsrQuality = ssrQuality;
        this.SsrMaxDistance = ssrMaxDistance;
        this.SsrMaxSteps = ssrMaxSteps;
        this.SsrStepSize = ssrStepSize;
        this.SsrThickness = ssrThickness;
        this.SsrStrength = ssrStrength;
        this.SsrEdgeFade = ssrEdgeFade;
        
        // Create lists for renderables.
        this._opaqueRenderables = new List<Renderable>();
        this._translucentRenderables = new List<Renderable>();
        this._lights = new List<Light>();
        
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
        
        // Create light data.
        this._lightData = new PbrLightData() {
            NumOfLights = 0,
            AmbientColor = this.AmbientColor
        };
        
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
        
        // Create SSR data.
        this._ssrData = new SsrData();
        
        // Create light buffer.
        this._lightBuffer = new SimpleUniformBuffer<PbrLightData>(graphicsDevice, 1, ShaderStages.Fragment);
        this._lightBuffer.DeviceBuffer.Name = "LightBuffer";
        
        // Create shadow buffer.
        this._shadowBuffer = new SimpleUniformBuffer<ShadowData>(graphicsDevice, 1, ShaderStages.Fragment);
        this._shadowBuffer.DeviceBuffer.Name = "ShadowBuffer";
        
        // Create shadow matrix buffer.
        this._shadowMatrixBuffer = new SimpleUniformBuffer<Matrix4x4>(graphicsDevice, 2, ShaderStages.Vertex);
        this._shadowMatrixBuffer.DeviceBuffer.Name = "MatrixBuffer";
        
        // Create environment buffer.
        this._environmentBuffer = new SimpleUniformBuffer<EnvironmentData>(graphicsDevice, 1, ShaderStages.Fragment);
        this._environmentBuffer.DeviceBuffer.Name = "EnvironmentBuffer";
        
        // Create SSR buffer.
        this._ssrBuffer = new SimpleUniformBuffer<SsrData>(graphicsDevice, 1, ShaderStages.Fragment);
        this._ssrBuffer.DeviceBuffer.Name = "SsrBuffer";
        
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
        
        if (this._lights.Count >= PbrLightData.MaxLightCount) {
            return;
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
        
        // Update light buffer.
        this.UpdateLightBuffer(commandList);
        
        // Ensure PBR IBL maps are baked for the current skybox.
        this.EnsurePbrEnvironmentMap(commandList);
        
        // Draw shadow scene.
        this.DrawShadowScene(commandList, framebuffer, cam3D);
        
        // Draw scene.
        if (this.EnableSsr) {
            this.EnsureScreenSpaceReflectionMap(framebuffer);
            
            // Draw normal prepass.
            commandList.SetFramebuffer(this._screenSpaceReflectionMap!.NormalFramebuffer);
            commandList.ClearColorTarget(0, new RgbaFloat(0.5F, 0.5F, 1.0F, 1.0F));
            commandList.ClearDepthStencil(1.0F);
            
            this.DrawNormalPrepass(commandList, cam3D);
            
            // Draw main pass.
            commandList.SetFramebuffer(this._screenSpaceReflectionMap.ColorFramebuffer);
            commandList.ClearColorTarget(0, Color.Black.ToRgbaFloat());
            commandList.ClearColorTarget(1, new RgbaFloat(0.0F, 0.0F, 0.0F, 1.0F));
            
            // Draw Skybox.
            this.SkyBox?.Draw(commandList, this._screenSpaceReflectionMap.ColorFramebuffer);
            
            this.BeforeOpaquePass?.Invoke(commandList, this._screenSpaceReflectionMap.ColorFramebuffer);
            
            // Draw opaques renderables.
            this.DrawOpaqueScene(commandList, this._screenSpaceReflectionMap.ColorFramebuffer, cam3D);
            
            this.AfterOpaquePass?.Invoke(commandList, this._screenSpaceReflectionMap.ColorFramebuffer);
            
            // Draw translucent renderables.
            this.DrawTranslucentScene(commandList, this._screenSpaceReflectionMap.ColorFramebuffer, cam3D);
            
            this.AfterTranslucentPass?.Invoke(commandList, this._screenSpaceReflectionMap.ColorFramebuffer);
            
            // Draw SSR pass.
            this.DrawSsrPass(commandList, framebuffer, cam3D);
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
        Effect effect = renderable.Mesh.IsSkinned ? GlobalGraphicsAssets.SkinnedShadowEffect : GlobalGraphicsAssets.ShadowEffect;
        
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
        Effect effect = renderable.Mesh.IsSkinned ? GlobalGraphicsAssets.SkinnedPbrNormalPrepassEffect : GlobalGraphicsAssets.PbrNormalPrepassEffect;
        
        if (renderable.UseInstancing) {
            effect = effect.GetEffectVariant(["USE_INSTANCING"]).Effect;
        }
        
        // Set the pipeline parameters.
        this._pipelineDescription.Outputs = this._screenSpaceReflectionMap!.NormalFramebuffer.OutputDescription;
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
    /// Traces the screen-space reflections and composites them onto the final framebuffer.
    /// </summary>
    /// <param name="commandList">The command list used to record the rendering commands.</param>
    /// <param name="framebuffer">The framebuffer that receives the final composited image.</param>
    /// <param name="camera">The 3D camera providing the matrices used to move between screen and world space.</param>
    private void DrawSsrPass(CommandList commandList, Framebuffer framebuffer, Cam3D camera) {
        Effect effect = GlobalGraphicsAssets.SsrEffect;
        
        Matrix4x4 projection = camera.GetProjection();
        Matrix4x4 view = camera.GetView();
        Matrix4x4 viewProjection = view * projection;
        
        Matrix4x4.Invert(projection, out Matrix4x4 inverseProjection);
        Matrix4x4.Invert(view, out Matrix4x4 inverseView);
        Matrix4x4.Invert(viewProjection, out Matrix4x4 inverseViewProjection);
        
        // Fill the SSR buffer.
        this._ssrData.ScreenSize = new Vector2(this._screenSpaceReflectionMap!.ColorFramebuffer.Width, this._screenSpaceReflectionMap.ColorFramebuffer.Height);
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
        this._pipelineDescription.Outputs = this._screenSpaceReflectionMap.SsrFramebuffer.OutputDescription;
        this._pipelineDescription.BlendState = BlendStateDescription.SINGLE_DISABLED;
        this._pipelineDescription.RasterizerState = RasterizerStateDescription.CULL_NONE;
        this._pipelineDescription.DepthStencilState = DepthStencilStateDescription.DISABLED;
        this._pipelineDescription.BufferLayouts = effect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = effect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = new ShaderSetDescription([], effect.Shaders);
        this._pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        
        // Render into the SSR framebuffer. A fully transparent clear color means "no reflection" for every pixel the shader does not write.
        commandList.SetFramebuffer(this._screenSpaceReflectionMap.SsrFramebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(0.0F, 0.0F, 0.0F, 0.0F));
        
        // Set pipeline.
        commandList.SetPipeline(effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set the SSR buffer.
        commandList.SetGraphicsResourceSet(effect.GetBufferLayoutSlot("SsrBuffer"), this._ssrBuffer.GetResourceSet(effect.GetBufferLayout("SsrBuffer")));
        
        // Set the lit scene color, which is sampled at the hit position to get the reflected color.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSceneColor"), this._screenSpaceReflectionMap.GetColorResourceSet(this._ssrSampler, effect.GetTextureLayout("fSceneColor")));
        
        // Set the world-space normals from the normal prepass, used to calculate the reflection direction.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fNormal"), this._screenSpaceReflectionMap.GetNormalResourceSet(this._ssrSampler, effect.GetTextureLayout("fNormal")));
        
        // Set the scene depth, used to reconstruct world positions and to detect ray hits.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fDepth"), this._screenSpaceReflectionMap.GetDepthResourceSet(this._ssrSampler, effect.GetTextureLayout("fDepth")));
        
        // Set the per-pixel material reflectivity, which decides which pixels are traced at all.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSsrMask"), this._screenSpaceReflectionMap.GetSsrMaskResourceSet(this._ssrSampler, effect.GetTextureLayout("fSsrMask")));
        
        // Apply effect.
        effect.Apply(commandList);
        
        // Draw.
        commandList.Draw(3);
        
        // Blend the traced reflections onto the scene.
        this.DrawSsrCompositePass(commandList, framebuffer);
    }
    
    /// <summary>
    /// Composites the traced screen-space reflections onto the scene in the final framebuffer.
    /// </summary>
    /// <param name="commandList">The command list used to record the rendering commands.</param>
    /// <param name="framebuffer">The framebuffer that receives the final composited image.</param>
    private void DrawSsrCompositePass(CommandList commandList, Framebuffer framebuffer) {
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
        
        // Render into the final framebuffer.
        commandList.SetFramebuffer(framebuffer);
        
        // Set pipeline.
        commandList.SetPipeline(effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set the lit scene color, which is the base image the reflections are composited onto.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSceneColor"), this._screenSpaceReflectionMap!.GetColorResourceSet(this._ssrSampler, effect.GetTextureLayout("fSceneColor")));
        
        // Set the traced reflections (rgb = reflected color, a = opacity).
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSsrColor"), this._screenSpaceReflectionMap.GetSsrResourceSet(this._ssrSampler, effect.GetTextureLayout("fSsrColor")));
        
        // Set the per-pixel material reflectivity, which scales the reflection strength.
        commandList.SetGraphicsResourceSet(effect.GetTextureLayoutSlot("fSsrMask"), this._screenSpaceReflectionMap.GetSsrMaskResourceSet(this._ssrSampler, effect.GetTextureLayout("fSsrMask")));
        
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
    private void UpdateLightBuffer(CommandList commandList) { // TODO: CHECK IF IT NEEDS TO BE UPDATED EVERY FRAME!
        this._lightData.NumOfLights = Math.Min(this._lights.Count, PbrLightData.MaxLightCount);
        this._lightData.AmbientColor = this.AmbientColor;
        
        Span<PbrGpuLight> gpuLights = this._lightData.Lights.AsSpan();
        
        for (int i = 0; i < this._lightData.NumOfLights; i++) {
            Light light = this._lights[i];
            
            // Calculate direction for the light.
            Vector3 direction = Vector3.Transform(-Vector3.UnitZ, light.LerpedRotation);
            
            if (direction.LengthSquared() <= 0.0F) {
                direction = -Vector3.UnitY;
            }
            else {
                direction = Vector3.Normalize(direction);
            }
            
            // Fill data to the GPU light.
            gpuLights[i] = new PbrGpuLight(
                light.LightType,
                light.LerpedGlobalPosition,
                direction,
                light.Color.ToRgbaFloatVec4().AsVector3(),
                light.Intensity,
                light.Range,
                float.DegreesToRadians(light.InnerSpotAngle),
                float.DegreesToRadians(light.OuterSpotAngle)
            );
        }
        
        // Fill the rest with empty/default data.
        for (int i = this._lightData.NumOfLights; i < PbrLightData.MaxLightCount; i++) {
            gpuLights[i] = default;
        }
        
        // Upload buffer to GPU.
        this._lightBuffer.SetValue(0, this._lightData);
        this._lightBuffer.UpdateBufferDeferred(commandList);
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
    /// Ensures the SSR map matches the current framebuffer size, color format, and SSR quality.
    /// </summary>
    /// <param name="framebuffer">The final target framebuffer.</param>
    private void EnsureScreenSpaceReflectionMap(Framebuffer framebuffer) {
        PixelFormat colorFormat = framebuffer.OutputDescription.ColorAttachments[0].Format;
        float ssrResolutionScale = this.GetSsrResolutionScale();
        
        if (this._screenSpaceReflectionMap == null) {
            this._screenSpaceReflectionMap = new ScreenSpaceReflectionMap(this.GraphicsDevice, framebuffer.Width, framebuffer.Height, colorFormat, ssrResolutionScale);
            return;
        }
        
        this._screenSpaceReflectionMap.Resize(framebuffer.Width, framebuffer.Height, colorFormat, ssrResolutionScale);
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
            this._screenSpaceReflectionMap?.Dispose();
            this._shadowSampler.Dispose();
            this._irradianceSampler.Dispose();
            this._prefilterSampler.Dispose();
            this._brdfLutSampler.Dispose();
            this._ssrSampler.Dispose();
            this._lightBuffer.Dispose();
            this._shadowBuffer.Dispose();
            this._shadowMatrixBuffer.Dispose();
            this._ssrBuffer.Dispose();
        }
    }
}