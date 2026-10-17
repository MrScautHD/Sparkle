using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Graphics.Pipelines.Textures;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Textures.Cubemaps;
using Veldrith;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Environment;

public class EnvironmentMap : Disposable {
    
    /// <summary>
    /// The graphics device used to create and dispose the resources.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// The environment cubemap the maps are baked from.
    /// </summary>
    public Cubemap SourceEnvironmentMap { get; private set; }
    
    /// <summary>
    /// The number of mip levels of the prefilter map. Mip level i stores the roughness i / (mipLevels - 1).
    /// </summary>
    public uint PrefilterMipLevels { get; private set; }
    
    /// <summary>
    /// The diffuse irradiance cubemap, sampled with the surface normal to get the ambient diffuse light.
    /// </summary>
    public Texture IrradianceTexture { get; private set; }
    
    /// <summary>
    /// The prefiltered specular cubemap, sampled with the reflection vector. Rougher surfaces read from higher mip levels.
    /// </summary>
    public Texture PrefilterTexture { get; private set; }
    
    /// <summary>
    /// The framebuffers of the six irradiance cubemap faces.
    /// </summary>
    private Framebuffer[] _irradianceFramebuffers;
    
    /// <summary>
    /// The framebuffers of the prefilter cubemap, indexed by mip level and then by face.
    /// </summary>
    private Framebuffer[][] _prefilterFramebuffers;
    
    /// <summary>
    /// Temporary color textures used while baking the prefilter cubemap. Each one is copied into the real cubemap face/mip after rendering.
    /// </summary>
    private Texture[][] _prefilterColorTextures;
    
    /// <summary>
    /// The depth textures used by the irradiance framebuffers, one per face.
    /// </summary>
    private Texture[] _irradianceDepthTextures;
    
    /// <summary>
    /// The depth textures used by the prefilter framebuffers, indexed by mip level and then by face.
    /// </summary>
    private Texture[][] _prefilterDepthTextures;
    
    /// <summary>
    /// The effect that bakes the irradiance map.
    /// </summary>
    private Effect _irradianceEffect;
    
    /// <summary>
    /// The effect that bakes the prefilter map.
    /// </summary>
    private Effect _prefilterEffect;
    
    /// <summary>
    /// The sampler used to read the source environment cubemap while baking.
    /// </summary>
    private Sampler _environmentSampler;
    
    /// <summary>
    /// The buffer holding the projection and view matrix of the face that is currently rendered.
    /// </summary>
    private SimpleUniformBuffer<Matrix4x4> _matrixBuffer;
    
    /// <summary>
    /// The buffer holding the roughness (x) and the sample count (y) of the prefilter pass.
    /// </summary>
    private SimpleUniformBuffer<Vector4> _prefilterBuffer;
    
    /// <summary>
    /// The vertex buffer of the unit cube that is rendered once per cubemap face.
    /// </summary>
    private DeviceBuffer _vertexBuffer;
    
    /// <summary>
    /// The pipeline description that is reused for all baking passes.
    /// </summary>
    private SimplePipelineDescription _pipelineDescription;
    
    /// <summary>
    /// The cached resource sets of the irradiance map, one per sampler and layout combination.
    /// </summary>
    private Dictionary<(Sampler, SimpleTextureLayout), ResourceSet> _cachedIrradianceResourceSets;
    
    /// <summary>
    /// The cached resource sets of the prefilter map, one per sampler and layout combination.
    /// </summary>
    private Dictionary<(Sampler, SimpleTextureLayout), ResourceSet> _cachedPrefilterResourceSets;
    
    /// <summary>
    /// Creates the environment maps and bakes them from the given cubemap.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create the resources.</param>
    /// <param name="sourceEnvironmentMap">The environment cubemap to bake from.</param>
    /// <param name="irradianceSize">The size of one irradiance cubemap face in pixels. The map is very smooth, so a small size is enough.</param>
    /// <param name="prefilterSize">The size of one face of the first (sharpest) prefilter mip level in pixels.</param>
    /// <param name="prefilterMipLevels">The number of prefilter mip levels, which also defines how many roughness steps exist.</param>
    /// <param name="prefilterSampleCount">The number of importance samples per texel in the prefilter pass. Higher values reduce noise.</param>
    public EnvironmentMap(GraphicsDevice graphicsDevice, Cubemap sourceEnvironmentMap, uint irradianceSize = 32, uint prefilterSize = 128, uint prefilterMipLevels = 5, float prefilterSampleCount = 1024.0F) {
        this.GraphicsDevice = graphicsDevice;
        this.SourceEnvironmentMap = sourceEnvironmentMap;
        this.PrefilterMipLevels = prefilterMipLevels;
        this._irradianceEffect = GlobalGraphicsAssets.IrradianceEffect;
        this._prefilterEffect = GlobalGraphicsAssets.PrefilterEffect;
        this._cachedIrradianceResourceSets = new Dictionary<(Sampler, SimpleTextureLayout), ResourceSet>();
        this._cachedPrefilterResourceSets = new Dictionary<(Sampler, SimpleTextureLayout), ResourceSet>();
        
        // Create cubemap textures.
        this.IrradianceTexture = graphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(irradianceSize, irradianceSize, 1, 6, PixelFormat.R16G16B16A16Float, TextureUsage.RenderTarget | TextureUsage.Sampled | TextureUsage.Cubemap));
        this.PrefilterTexture = graphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(prefilterSize, prefilterSize, prefilterMipLevels, 6, PixelFormat.R16G16B16A16Float, TextureUsage.Sampled | TextureUsage.Cubemap));
        
        // Create framebuffers.
        this._irradianceFramebuffers = this.CreateCubemapFaceFramebuffers(this.IrradianceTexture, irradianceSize, irradianceSize, 0, out this._irradianceDepthTextures);
        this._prefilterFramebuffers = this.CreatePrefilterFramebuffers(prefilterSize, prefilterMipLevels, out this._prefilterColorTextures, out this._prefilterDepthTextures);
        
        // Create environment sampler.
        this._environmentSampler = graphicsDevice.ResourceFactory.CreateSampler(new SamplerDescription() {
            AddressModeU = SamplerAddressMode.Clamp,
            AddressModeV = SamplerAddressMode.Clamp,
            AddressModeW = SamplerAddressMode.Clamp,
            Filter = SamplerFilter.MinLinearMagLinearMipLinear,
            ComparisonKind = null,
            MaximumAnisotropy = 4,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            LodBias = 0,
            BorderColor = SamplerBorderColor.OpaqueBlack
        });
        
        // Create matrix buffer.
        this._matrixBuffer = new SimpleUniformBuffer<Matrix4x4>(graphicsDevice, 2, ShaderStages.Vertex | ShaderStages.Fragment);
        this._matrixBuffer.DeviceBuffer.Name = "MatrixBuffer";
        
        // Create prefilter buffer.
        this._prefilterBuffer = new SimpleUniformBuffer<Vector4>(graphicsDevice, 1, ShaderStages.Fragment);
        this._prefilterBuffer.DeviceBuffer.Name = "PrefilterBuffer";
        
        // Create vertex buffer.
        uint vertexBufferSize = (uint) Marshal.SizeOf<CubemapVertex3D>() * 36;
        this._vertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        graphicsDevice.UpdateBuffer(this._vertexBuffer, 0, this.GenVertices());
        
        // Create the pipeline description.
        this._pipelineDescription = new SimplePipelineDescription() {
            BlendState = BlendStateDescription.SINGLE_DISABLED,
            DepthStencilState = DepthStencilStateDescription.DEPTH_ONLY_LESS_EQUAL,
            RasterizerState = RasterizerStateDescription.CULL_NONE,
            PrimitiveTopology = PrimitiveTopology.TriangleList
        };
        
        this.Bake(prefilterSampleCount);
    }
    
    /// <summary>
    /// Gets the resource set of the irradiance map for the given sampler and layout.
    /// </summary>
    /// <param name="sampler">The sampler used to read the texture.</param>
    /// <param name="layout">The texture layout the resource set has to match.</param>
    /// <returns>The cached or newly created resource set.</returns>
    public ResourceSet GetIrradianceResourceSet(Sampler sampler, SimpleTextureLayout layout) {
        if (!this._cachedIrradianceResourceSets.TryGetValue((sampler, layout), out ResourceSet? resourceSet)) {
            ResourceSet newResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout.Layout, this.IrradianceTexture, sampler));
            
            this._cachedIrradianceResourceSets.Add((sampler, layout), newResourceSet);
            return newResourceSet;
        }
        
        return resourceSet;
    }
    
    /// <summary>
    /// Gets the resource set of the prefilter map for the given sampler and layout.
    /// </summary>
    /// <param name="sampler">The sampler used to read the texture.</param>
    /// <param name="layout">The texture layout the resource set has to match.</param>
    /// <returns>The cached or newly created resource set.</returns>
    public ResourceSet GetPrefilterResourceSet(Sampler sampler, SimpleTextureLayout layout) {
        if (!this._cachedPrefilterResourceSets.TryGetValue((sampler, layout), out ResourceSet? resourceSet)) {
            ResourceSet newResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout.Layout, this.PrefilterTexture, sampler));
            
            this._cachedPrefilterResourceSets.Add((sampler, layout), newResourceSet);
            return newResourceSet;
        }
        
        return resourceSet;
    }
    
    /// <summary>
    /// Bakes the irradiance and prefilter maps and waits until the GPU has finished.
    /// </summary>
    /// <param name="prefilterSampleCount">The number of importance samples per texel in the prefilter pass.</param>
    private void Bake(float prefilterSampleCount) {
        using CommandList commandList = this.GraphicsDevice.ResourceFactory.CreateCommandList();
        
        commandList.Begin();
        this.BakeIrradiance(commandList);
        this.BakePrefilter(commandList, prefilterSampleCount);
        commandList.End();
        
        this.GraphicsDevice.SubmitCommands(commandList);
        this.GraphicsDevice.WaitForIdle();
    }
    
    /// <summary>
    /// Renders the six faces of the irradiance cubemap.
    /// </summary>
    /// <param name="commandList">The command list used to record the rendering commands.</param>
    private void BakeIrradiance(CommandList commandList) {
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2.0F, 1.0F, 0.1F, 10.0F);
        Matrix4x4[] views = this.GetCubemapViews();
        
        // Set the pipeline parameters.
        this._pipelineDescription.Outputs = this._irradianceFramebuffers[0].OutputDescription;
        this._pipelineDescription.BufferLayouts = this._irradianceEffect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = this._irradianceEffect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = new ShaderSetDescription(CubemapVertex3D.VertexLayout.Layouts, this._irradianceEffect.Shaders);
        
        for (int face = 0; face < 6; face++) {
            
            // Upload the capture matrices.
            this._matrixBuffer.SetValue(0, projection);
            this._matrixBuffer.SetValue(1, views[face]);
            this._matrixBuffer.UpdateBufferDeferred(commandList);
            
            // Set framebuffer.
            commandList.SetFramebuffer(this._irradianceFramebuffers[face]);
            commandList.ClearColorTarget(0, RgbaFloat.BLACK);
            commandList.ClearDepthStencil(1.0F);
            
            // Set pipeline.
            commandList.SetPipeline(this._irradianceEffect.GetPipeline(this._pipelineDescription).Pipeline);
            
            // Set matrix buffer.
            commandList.SetGraphicsResourceSet(this._irradianceEffect.GetBufferLayoutSlot("MatrixBuffer"), this._matrixBuffer.GetResourceSet(this._irradianceEffect.GetBufferLayout("MatrixBuffer")));
            
            // Set environment map.
            commandList.SetGraphicsResourceSet(this._irradianceEffect.GetTextureLayoutSlot("fEnvironmentMap"), this.SourceEnvironmentMap.GetResourceSet(this._environmentSampler, this._irradianceEffect.GetTextureLayout("fEnvironmentMap")));
            
            // Apply effect.
            this._irradianceEffect.Apply(commandList);
            
            // Set vertex buffer.
            commandList.SetVertexBuffer(0, this._vertexBuffer);
            
            // Draw.
            commandList.Draw(36);
        }
    }
    
    /// <summary>
    /// Renders the six faces of every mip level of the prefilter cubemap.
    /// </summary>
    /// <param name="commandList">The command list used to record the rendering commands.</param>
    /// <param name="sampleCount">The number of importance samples per texel.</param>
    private void BakePrefilter(CommandList commandList, float sampleCount) {
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2.0F, 1.0F, 0.1F, 10.0F);
        Matrix4x4[] views = this.GetCubemapViews();
        
        for (uint mip = 0; mip < this.PrefilterMipLevels; mip++) {
            float roughness = this.PrefilterMipLevels <= 1 ? 0.0F : (float) mip / (float) (this.PrefilterMipLevels - 1);
            
            // Upload prefilter data.
            this._prefilterBuffer.SetValue(0, new Vector4(roughness, sampleCount, 0.0F, 0.0F));
            this._prefilterBuffer.UpdateBufferDeferred(commandList);
            
            // Set the pipeline parameters.
            this._pipelineDescription.Outputs = this._prefilterFramebuffers[mip][0].OutputDescription;
            this._pipelineDescription.BufferLayouts = this._prefilterEffect.GetBufferLayouts();
            this._pipelineDescription.TextureLayouts = this._prefilterEffect.GetTextureLayouts();
            this._pipelineDescription.ShaderSet = new ShaderSetDescription(CubemapVertex3D.VertexLayout.Layouts, this._prefilterEffect.Shaders);
            
            for (int face = 0; face < 6; face++) {
                
                // Upload the capture matrices.
                this._matrixBuffer.SetValue(0, projection);
                this._matrixBuffer.SetValue(1, views[face]);
                this._matrixBuffer.UpdateBufferDeferred(commandList);
                
                // Set framebuffer.
                commandList.SetFramebuffer(this._prefilterFramebuffers[mip][face]);
                commandList.ClearColorTarget(0, RgbaFloat.BLACK);
                commandList.ClearDepthStencil(1.0F);
                
                // Set pipeline.
                commandList.SetPipeline(this._prefilterEffect.GetPipeline(this._pipelineDescription).Pipeline);
                
                // Set matrix buffer.
                commandList.SetGraphicsResourceSet(this._prefilterEffect.GetBufferLayoutSlot("MatrixBuffer"), this._matrixBuffer.GetResourceSet(this._prefilterEffect.GetBufferLayout("MatrixBuffer")));
                
                // Set prefilter buffer.
                commandList.SetGraphicsResourceSet(this._prefilterEffect.GetBufferLayoutSlot("PrefilterBuffer"), this._prefilterBuffer.GetResourceSet(this._prefilterEffect.GetBufferLayout("PrefilterBuffer")));
                
                // Set environment map.
                commandList.SetGraphicsResourceSet(this._prefilterEffect.GetTextureLayoutSlot("fEnvironmentMap"), this.SourceEnvironmentMap.GetResourceSet(this._environmentSampler, this._prefilterEffect.GetTextureLayout("fEnvironmentMap")));
                
                // Apply effect.
                this._prefilterEffect.Apply(commandList);
                
                // Set vertex buffer.
                commandList.SetVertexBuffer(0, this._vertexBuffer);
                
                // Draw.
                commandList.Draw(36);
                
                // Copy the rendered temporary face into the real prefilter cubemap mip/face.
                uint mipSize = Math.Max(1, this.PrefilterTexture.Width >> (int) mip);
                commandList.CopyTexture(this._prefilterColorTextures[mip][face], 0, 0, 0, 0, 0, this.PrefilterTexture, 0, 0, 0, mip, (uint) face, mipSize, mipSize, 1, 1);
            }
        }
    }
    
    /// <summary>
    /// Creates one framebuffer per face of a cubemap for a single mip level.
    /// </summary>
    /// <param name="texture">The cubemap texture to render into.</param>
    /// <param name="width">The width of the target mip level in pixels.</param>
    /// <param name="height">The height of the target mip level in pixels.</param>
    /// <param name="mipLevel">The mip level of the cubemap that the framebuffers render into.</param>
    /// <param name="depthTextures">Returns the depth textures created for the framebuffers, so they can be disposed later.</param>
    /// <returns>The six framebuffers, in cubemap face order.</returns>
    private Framebuffer[] CreateCubemapFaceFramebuffers(Texture texture, uint width, uint height, uint mipLevel, out Texture[] depthTextures) {
        Framebuffer[] framebuffers = new Framebuffer[6];
        Texture[] textures = new Texture[6];
        
        for (uint face = 0; face < 6; face++) {
            Texture depthTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.D32FloatS8UInt, TextureUsage.DepthStencil));
            
            textures[face] = depthTexture;
            framebuffers[face] = this.GraphicsDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription(new FramebufferAttachmentDescription(depthTexture, 0), [
                    new FramebufferAttachmentDescription(texture, face, mipLevel)
                ])
            );
        }
        
        depthTextures = textures;
        return framebuffers;
    }
    
    /// <summary>
    /// Creates temporary 2D framebuffers for every face of every prefilter mip level.
    /// The rendered temporary color texture is copied into the real cubemap face/mip after each draw.
    /// </summary>
    /// <param name="width">The width of the first mip level in pixels.</param>
    /// <param name="mipLevels">The number of mip levels.</param>
    /// <param name="colorTextures">Returns the temporary color textures, indexed by mip level and then by face.</param>
    /// <param name="depthTextures">Returns the temporary depth textures, indexed by mip level and then by face.</param>
    /// <returns>The temporary framebuffers, indexed by mip level and then by face.</returns>
    private Framebuffer[][] CreatePrefilterFramebuffers(uint width, uint mipLevels, out Texture[][] colorTextures, out Texture[][] depthTextures) {
        Framebuffer[][] framebuffers = new Framebuffer[mipLevels][];
        Texture[][] colors = new Texture[mipLevels][];
        Texture[][] depths = new Texture[mipLevels][];
        
        for (uint mip = 0; mip < mipLevels; mip++) {
            uint mipSize = Math.Max(1, width >> (int) mip);
            
            framebuffers[mip] = new Framebuffer[6];
            colors[mip] = new Texture[6];
            depths[mip] = new Texture[6];
            
            for (uint face = 0; face < 6; face++) {
                Texture colorTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(mipSize, mipSize, 1, 1, PixelFormat.R16G16B16A16Float, TextureUsage.RenderTarget));
                Texture depthTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(mipSize, mipSize, 1, 1, PixelFormat.D32FloatS8UInt, TextureUsage.DepthStencil));
                
                colors[mip][face] = colorTexture;
                depths[mip][face] = depthTexture;
                
                framebuffers[mip][face] = this.GraphicsDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription(new FramebufferAttachmentDescription(depthTexture, 0), [
                        new FramebufferAttachmentDescription(colorTexture, 0)
                    ]
                ));
            }
        }
        
        colorTextures = colors;
        depthTextures = depths;
        return framebuffers;
    }
    
    /// <summary>
    /// Creates the cube vertex buffer used for cubemap capture rendering.
    /// </summary>
    /// <returns>The created vertex buffer.</returns>
    private CubemapVertex3D[] GenVertices() {
        Vector4 color = Color.White.ToRgbaFloatVec4();
        
        return [
            new CubemapVertex3D(new Vector3(-1, -1, -1), color),
            new CubemapVertex3D(new Vector3(-1, 1, -1), color),
            new CubemapVertex3D(new Vector3(1, 1, -1), color),
            new CubemapVertex3D(new Vector3(1, 1, -1), color),
            new CubemapVertex3D(new Vector3(1, -1, -1), color),
            new CubemapVertex3D(new Vector3(-1, -1, -1), color),
            
            new CubemapVertex3D(new Vector3(-1, -1, 1), color),
            new CubemapVertex3D(new Vector3(1, -1, 1), color),
            new CubemapVertex3D(new Vector3(1, 1, 1), color),
            new CubemapVertex3D(new Vector3(1, 1, 1), color),
            new CubemapVertex3D(new Vector3(-1, 1, 1), color),
            new CubemapVertex3D(new Vector3(-1, -1, 1), color),
            
            new CubemapVertex3D(new Vector3(-1, 1, 1), color),
            new CubemapVertex3D(new Vector3(-1, 1, -1), color),
            new CubemapVertex3D(new Vector3(-1, -1, -1), color),
            new CubemapVertex3D(new Vector3(-1, -1, -1), color),
            new CubemapVertex3D(new Vector3(-1, -1, 1), color),
            new CubemapVertex3D(new Vector3(-1, 1, 1), color),
            
            new CubemapVertex3D(new Vector3(1, 1, 1), color),
            new CubemapVertex3D(new Vector3(1, -1, 1), color),
            new CubemapVertex3D(new Vector3(1, -1, -1), color),
            new CubemapVertex3D(new Vector3(1, -1, -1), color),
            new CubemapVertex3D(new Vector3(1, 1, -1), color),
            new CubemapVertex3D(new Vector3(1, 1, 1), color),
            
            new CubemapVertex3D(new Vector3(-1, -1, -1), color),
            new CubemapVertex3D(new Vector3(1, -1, -1), color),
            new CubemapVertex3D(new Vector3(1, -1, 1), color),
            new CubemapVertex3D(new Vector3(1, -1, 1), color),
            new CubemapVertex3D(new Vector3(-1, -1, 1), color),
            new CubemapVertex3D(new Vector3(-1, -1, -1), color),
            
            new CubemapVertex3D(new Vector3(-1, 1, -1), color),
            new CubemapVertex3D(new Vector3(-1, 1, 1), color),
            new CubemapVertex3D(new Vector3(1, 1, 1), color),
            new CubemapVertex3D(new Vector3(1, 1, 1), color),
            new CubemapVertex3D(new Vector3(1, 1, -1), color),
            new CubemapVertex3D(new Vector3(-1, 1, -1), color)
        ];
    }
    
    /// <summary>
    /// Creates the six view matrices that look from the origin towards each cubemap face.
    /// </summary>
    /// <returns>The view matrices in cubemap face order (+X, -X, +Y, -Y, +Z, -Z), with up vectors chosen to match the face orientation.</returns>
    private Matrix4x4[] GetCubemapViews() {
        return [
            Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitX, -Vector3.UnitY),
            Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitX, -Vector3.UnitY),
            Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ),
            Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitY, -Vector3.UnitZ),
            Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitZ, -Vector3.UnitY),
            Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ, -Vector3.UnitY)
        ];
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            foreach (ResourceSet resourceSet in this._cachedIrradianceResourceSets.Values) {
                resourceSet.Dispose();
            }
            
            this._cachedIrradianceResourceSets.Clear();
            
            foreach (ResourceSet resourceSet in this._cachedPrefilterResourceSets.Values) {
                resourceSet.Dispose();
            }
            
            this._cachedPrefilterResourceSets.Clear();
            
            foreach (Framebuffer framebuffer in this._irradianceFramebuffers) {
                framebuffer.Dispose();
            }
            
            foreach (Framebuffer[] mipFramebuffers in this._prefilterFramebuffers) {
                foreach (Framebuffer framebuffer in mipFramebuffers) {
                    framebuffer.Dispose();
                }
            }
            
            foreach (Texture[] mipColorTextures in this._prefilterColorTextures) {
                foreach (Texture colorTexture in mipColorTextures) {
                    colorTexture.Dispose();
                }
            }
            
            foreach (Texture depthTexture in this._irradianceDepthTextures) {
                depthTexture.Dispose();
            }
            
            foreach (Texture[] mipDepthTextures in this._prefilterDepthTextures) {
                foreach (Texture depthTexture in mipDepthTextures) {
                    depthTexture.Dispose();
                }
            }
            
            this.IrradianceTexture.Dispose();
            this.PrefilterTexture.Dispose();
            this._environmentSampler.Dispose();
            this._matrixBuffer.Dispose();
            this._prefilterBuffer.Dispose();
            this._vertexBuffer.Dispose();
        }
    }
}