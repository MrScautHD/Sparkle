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
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
using Bliss.CSharp.Materials;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics.Rendering.Forward.Pbr.Data;
using Veldrith;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Pbr;

public class PbrForwardRenderer : Disposable, IRenderer, ILightReceiver {
    
    /// <summary>
    /// The graphics device used for rendering.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
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
    /// The shadow map sampler.
    /// </summary>
    private Sampler _shadowSampler;
    
    /// <summary>
    /// CPU-side light data uploaded to the GPU.
    /// </summary>
    private PbrLightData _lightData;
    
    /// <summary>
    /// CPU-side shadow data uploaded to the GPU.
    /// </summary>
    private ShadowData _shadowData;
    
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
    /// Description of the pipeline used for rendering.
    /// </summary>
    private SimplePipelineDescription _pipelineDescription;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PbrForwardRenderer"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create all GPU resources.</param>
    /// <param name="ambientColor">The ambient light color applied to the scene. If <see langword="null"/>, a dark gray (0.03, 0.03, 0.03) is used.</param>
    /// <param name="shadowMapResolution">The width and height of the square shadow map texture in pixels.</param>
    /// <param name="shadowCameraDistance">The distance of the shadow light camera from its focus point, measured along the inverse light direction.</param>
    /// <param name="shadowCameraSize">The size of the orthographic view volume of the shadow light camera.</param>
    /// <param name="shadowCameraNear">The near clipping plane distance of the shadow light camera.</param>
    /// <param name="shadowCameraFar">The far clipping plane distance of the shadow light camera.</param>
    /// <param name="shadowBias">The constant depth bias applied when comparing fragment depth against the shadow map, used to reduce shadow acne.</param>
    /// <param name="shadowNormalBias">The bias applied along the surface normal when sampling the shadow map, used to reduce shadow acne on surfaces at grazing angles.</param>
    /// <param name="shadowBiasClamp">The maximum allowed value for the total depth bias, preventing excessive offsets that would cause peter-panning.</param>
    public PbrForwardRenderer(
        GraphicsDevice graphicsDevice,
        Color? ambientColor = null,
        int shadowMapResolution = 2048,
        float shadowCameraDistance = 80.0F,
        float shadowCameraSize = 120.0F,
        float shadowCameraNear = 0.1F,
        float shadowCameraFar = 250.0F,
        float shadowBias = 0.0005F,
        float shadowNormalBias = 0.0025F,
        float shadowBiasClamp = 0.01F) {
        this.GraphicsDevice = graphicsDevice;
        this.AmbientColor = ambientColor?.ToRgbaFloatVec4() ?? new Vector4(0.03F, 0.03F, 0.03F, 1.0F);
        this.ShadowCameraDistance = shadowCameraDistance;
        this.ShadowCameraSize = shadowCameraSize;
        this.ShadowCameraNear = shadowCameraNear;
        this.ShadowCameraFar = shadowCameraFar;
        this.ShadowBias = shadowBias;
        this.ShadowNormalBias = shadowNormalBias;
        this.ShadowBiasClamp = shadowBiasClamp;
        
        // Create lists for renderables.
        this._opaqueRenderables = new List<Renderable>();
        this._translucentRenderables = new List<Renderable>();
        this._lights = new List<Light>();
        
        // Create shadow map.
        this._shadowMap = new ShadowMap(graphicsDevice, shadowMapResolution);
        
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
        
        // Create light buffer.
        this._lightBuffer = new SimpleUniformBuffer<PbrLightData>(graphicsDevice, 1, ShaderStages.Fragment);
        this._lightBuffer.DeviceBuffer.Name = "LightBuffer";
        
        // Create shadow buffer.
        this._shadowBuffer = new SimpleUniformBuffer<ShadowData>(graphicsDevice, 1, ShaderStages.Fragment);
        this._shadowBuffer.DeviceBuffer.Name = "ShadowBuffer";
        
        // Create shadow matrix buffer.
        this._shadowMatrixBuffer = new SimpleUniformBuffer<Matrix4x4>(graphicsDevice, 2, ShaderStages.Vertex);
        this._shadowMatrixBuffer.DeviceBuffer.Name = "MatrixBuffer";
        
        // Create the main pipeline description.
        this._pipelineDescription = new SimplePipelineDescription() {
            DepthStencilState = DepthStencilStateDescription.DEPTH_ONLY_LESS_EQUAL,
            PrimitiveTopology = PrimitiveTopology.TriangleList
        };
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
        
        // Draw shadow scene.
        this.DrawShadowScene(commandList, framebuffer, cam3D);
        
        // Draw scene.
        this.DrawScene(commandList, framebuffer, cam3D);
        
        // Clean up.
        this._opaqueRenderables.Clear();
        this._translucentRenderables.Clear();
        this._lights.Clear();
        this._shadowLight = null;
    }
    
    private void DrawScene(CommandList commandList, Framebuffer framebuffer, Cam3D cam) {
        
        // Draw opaques renderables.
        foreach (Renderable renderable in this._opaqueRenderables) {
            this.DrawPreparedRenderable(commandList, framebuffer, cam, renderable);
        }
        
        // Draw translucent renderables.
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
    /// Draws a prepared renderable object by configuring necessary buffers, setting up pipeline parameters, and executing the rendering process.
    /// </summary>
    /// <param name="commandList">The command list used for issuing rendering commands.</param>
    /// <param name="framebuffer"></param>
    /// <param name="camera">The 3D camera providing view and projection matrices for rendering.</param>
    /// <param name="renderable">The renderable object containing mesh, material, transforms, and other rendering data to be processed.</param>
    private void DrawPreparedRenderable(CommandList commandList, Framebuffer framebuffer, Cam3D camera, Renderable renderable) {
        
        // Set the main pipeline parameters.
        this._pipelineDescription.Outputs = framebuffer.OutputDescription;
        this._pipelineDescription.BlendState = renderable.Material.BlendState;
        this._pipelineDescription.RasterizerState = renderable.Material.RasterizerState;
        this._pipelineDescription.BufferLayouts = renderable.Material.Effect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = renderable.Material.Effect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = new ShaderSetDescription(renderable.Mesh.VertexFormat.Layouts, renderable.Mesh.Material.Effect.Shaders);
        
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
    
    private void DrawPreparedShadowRenderable(CommandList commandList, Renderable renderable) {
        Effect effect = renderable.Mesh.IsSkinned ? GlobalGraphicsAssets.SkinnedShadowEffect : GlobalGraphicsAssets.ShadowEffect;
        
        // Set the main pipeline parameters.
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
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._shadowMap.Dispose();
            this._shadowSampler.Dispose();
            this._lightBuffer.Dispose();
            this._shadowBuffer.Dispose();
            this._shadowMatrixBuffer.Dispose();
        }
    }
}