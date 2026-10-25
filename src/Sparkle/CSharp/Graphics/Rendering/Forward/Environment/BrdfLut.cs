using Bliss.CSharp;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines;
using Bliss.CSharp.Graphics.Pipelines.Textures;
using Veldrith;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Environment;

public class BrdfLut : Disposable {
    
    /// <summary>
    /// The graphics device used to create the GPU resources.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// The width and height of the BRDF LUT in pixels.
    /// </summary>
    public int Resolution {
        get => this._resolution;
        set {
            if (this._resolution == value) {
                return;
            }
            
            this._resolution = value;
            this.ClearResources();
            this.CreateFramebuffer();
            this.Bake();
        }
    }
    
    /// <summary>
    /// The texture that stores the baked split-sum BRDF lookup values.
    /// </summary>
    public Texture Texture { get; private set; }
    
    /// <summary>
    /// The framebuffer used while baking the BRDF LUT.
    /// </summary>
    public Framebuffer Framebuffer { get; private set; }
    
    /// <summary>
    /// The effect used for rendering the GPU-baked BRDF LUT.
    /// </summary>
    private Effect _effect;
    
    /// <summary>
    /// The current resolution in pixels.
    /// </summary>
    private int _resolution;
    
    /// <summary>
    /// Description of the pipeline used for baking.
    /// </summary>
    private SimplePipelineDescription _pipelineDescription;
    
    /// <summary>
    /// The cached resource sets, keyed by sampler and layout.
    /// </summary>
    private Dictionary<(Sampler, SimpleTextureLayout), ResourceSet> _cachedResourceSets;
    
    /// <summary>
    /// Creates a new GPU-baked BRDF LUT.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create the GPU resources.</param>
    /// <param name="resolution">The width and height of the BRDF LUT in pixels.</param>
    public BrdfLut(GraphicsDevice graphicsDevice, int resolution = 256) {
        this.GraphicsDevice = graphicsDevice;
        this._effect = GlobalGraphicsAssets.BrdfLut;
        this._resolution = resolution;
        this._cachedResourceSets = new Dictionary<(Sampler, SimpleTextureLayout), ResourceSet>();
        
        // Create the pipeline description.
        this._pipelineDescription = new SimplePipelineDescription() {
            BlendState = BlendStateDescription.SINGLE_DISABLED,
            DepthStencilState = DepthStencilStateDescription.DISABLED,
            RasterizerState = RasterizerStateDescription.CULL_NONE,
            PrimitiveTopology = PrimitiveTopology.TriangleList
        };
        
        this.CreateFramebuffer();
        this.Bake();
    }
    
    /// <summary>
    /// Gets the cached resource set for the given sampler and layout, or creates it if it doesn't exist yet.
    /// </summary>
    /// <param name="sampler">The sampler used to sample the BRDF LUT.</param>
    /// <param name="layout">The texture layout of the resource set.</param>
    /// <returns>The resource set.</returns>
    public ResourceSet GetResourceSet(Sampler sampler, SimpleTextureLayout layout) {
        if (!this._cachedResourceSets.TryGetValue((sampler, layout), out ResourceSet? resourceSet)) {
            ResourceSet newResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout.Layout, this.Texture, sampler));
            
            this._cachedResourceSets.Add((sampler, layout), newResourceSet);
            return newResourceSet;
        }
        
        return resourceSet;
    }
    
    /// <summary>
    /// Executes the BRDF LUT baking process using the configured pipeline and resources.
    /// </summary>
    private void Bake() {
        using CommandList commandList = this.GraphicsDevice.ResourceFactory.CreateCommandList();
        
        commandList.Begin();
        
        // Set the pipeline parameters.
        this._pipelineDescription.Outputs = this.Framebuffer.OutputDescription;
        this._pipelineDescription.BufferLayouts = this._effect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = this._effect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = new ShaderSetDescription([], this._effect.Shaders);
        
        // Set framebuffer.
        commandList.SetFramebuffer(this.Framebuffer);
        commandList.ClearColorTarget(0, RgbaFloat.BLACK);
        
        // Set pipeline.
        commandList.SetPipeline(this._effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Apply effect.
        this._effect.Apply(commandList);
        
        // Draw.
        commandList.Draw(3);
        
        commandList.End();
        
        this.GraphicsDevice.SubmitCommands(commandList);
        this.GraphicsDevice.WaitForIdle();
    }
    
    /// <summary>
    /// Creates the texture and framebuffer using the current resolution.
    /// </summary>
    private void CreateFramebuffer() {
        this.Texture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint) this._resolution, (uint) this._resolution, 1, 1, PixelFormat.R16G16Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
        this.Framebuffer = this.GraphicsDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, this.Texture));
    }
    
    /// <summary>
    /// Releases the resources allocated.
    /// </summary>
    private void ClearResources() {
        foreach (ResourceSet resourceSet in this._cachedResourceSets.Values) {
            resourceSet.Dispose();
        }
        
        this._cachedResourceSets.Clear();
        
        this.Texture.Dispose();
        this.Framebuffer.Dispose();
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.ClearResources();
        }
    }
}