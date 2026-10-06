using Bliss.CSharp;
using Bliss.CSharp.Graphics.Pipelines.Textures;
using Veldrith;

namespace Sparkle.CSharp.Graphics.Rendering.Forward;

public class ShadowMap : Disposable {
    
    /// <summary>
    /// The graphics device used to create the GPU resources.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// The width and height of the shadow map in pixels. Changing it recreates the framebuffer.
    /// </summary>
    public int Resolution {
        get => this._resolution;
        set {
            if (this._resolution == value) {
                return;
            }
            
            this._resolution = value;
            this.ClearResources();
            this.CreateFrameBuffer();
        }
    }
    
    /// <summary>
    /// The texture that stores the depth values written during the shadow pass.
    /// </summary>
    public Texture DepthTexture { get; private set; }
    
    /// <summary>
    /// The view of <see cref="DepthTexture"/> used to sample it in shaders.
    /// </summary>
    public TextureView DepthTextureView { get; private set; }
    
    /// <summary>
    /// The depth-stencil texture used for depth testing during the shadow pass.
    /// </summary>
    public Texture DepthTestTexture { get; private set; }
    
    /// <summary>
    /// The framebuffer used to render the shadow pass.
    /// </summary>
    public Framebuffer Framebuffer { get; private set; }
    
    /// <summary>
    /// The current resolution in pixels.
    /// </summary>
    private int _resolution;
    
    /// <summary>
    /// The cached resource sets, keyed by sampler and layout.
    /// </summary>
    private Dictionary<(Sampler, SimpleTextureLayout), ResourceSet> _cachedResourceSets;
    
    /// <summary>
    /// Creates a new shadow map.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create the GPU resources.</param>
    /// <param name="resolution">The width and height of the shadow map in pixels.</param>
    public ShadowMap(GraphicsDevice graphicsDevice, int resolution = 2048) {
        this.GraphicsDevice = graphicsDevice;
        this._resolution = resolution;
        this._cachedResourceSets = new Dictionary<(Sampler, SimpleTextureLayout), ResourceSet>();
        this.CreateFrameBuffer();
    }
    
    /// <summary>
    /// Gets the cached resource set for the given sampler and layout, or creates it if it doesn't exist yet.
    /// </summary>
    /// <param name="sampler">The sampler used to sample the shadow map.</param>
    /// <param name="layout">The texture layout of the resource set.</param>
    /// <returns>The resource set.</returns>
    public ResourceSet GetResourceSet(Sampler sampler, SimpleTextureLayout layout) {
        if (!this._cachedResourceSets.TryGetValue((sampler, layout), out ResourceSet? resourceSet)) {
            ResourceSet newResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout.Layout, this.DepthTextureView, sampler));
            
            this._cachedResourceSets.Add((sampler, layout), newResourceSet);
            return newResourceSet;
        }
        
        return resourceSet;
    }
    
    /// <summary>
    /// Creates the textures and the framebuffer using the current resolution.
    /// </summary>
    private void CreateFrameBuffer() {
        this.DepthTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint) this._resolution, (uint) this._resolution, 1, 1, PixelFormat.R32Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
        this.DepthTextureView = this.GraphicsDevice.ResourceFactory.CreateTextureView(this.DepthTexture);
        this.DepthTestTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint) this._resolution, (uint) this._resolution, 1, 1, PixelFormat.D32FloatS8UInt, TextureUsage.DepthStencil));
        this.Framebuffer = this.GraphicsDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription(this.DepthTestTexture, this.DepthTexture));
    }
    
    /// <summary>
    /// Releases the resources allocated.
    /// </summary>
    private void ClearResources() {
        foreach (ResourceSet resourceSet in this._cachedResourceSets.Values) {
            resourceSet.Dispose();
        }
        
        this._cachedResourceSets.Clear();
        
        this.DepthTexture.Dispose();
        this.DepthTextureView.Dispose();
        this.DepthTestTexture.Dispose();
        this.Framebuffer.Dispose();
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.ClearResources();
        }
    }
}