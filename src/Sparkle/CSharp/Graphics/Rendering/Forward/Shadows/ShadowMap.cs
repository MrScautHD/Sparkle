using Bliss.CSharp;
using Bliss.CSharp.Graphics.Pipelines.Textures;
using Veldrith;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Shadows;

public class ShadowMap : Disposable {
    
    /// <summary>
    /// Number of cascades stored in the shadow map texture array.
    /// </summary>
    public const int CascadeCount = 4;
    
    /// <summary>
    /// The graphics device used to create the GPU resources.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// The width and height of each cascade shadow map layer in pixels.
    /// Changing it recreates the framebuffer.
    /// </summary>
    public int Resolution {
        get => this._resolution;
        set {
            if (this._resolution == value) {
                return;
            }
            
            this._resolution = value;
            this.ClearResources();
            this.CreateFramebuffers();
        }
    }
    
    /// <summary>
    /// The texture array that stores the depth values written during the shadow pass.
    /// Each array layer is one cascade.
    /// </summary>
    public Texture DepthTexture { get; private set; }
    
    /// <summary>
    /// The depth-stencil texture array used for depth testing during the shadow pass.
    /// Each array layer matches one cascade.
    /// </summary>
    public Texture DepthTestTexture { get; private set; }
    
    /// <summary>
    /// The framebuffer for each cascade layer.
    /// </summary>
    public Framebuffer[] Framebuffers { get; private set; }
    
    /// <summary>
    /// The current resolution in pixels.
    /// </summary>
    private int _resolution;
    
    /// <summary>
    /// The cached resource sets, keyed by sampler and layout.
    /// </summary>
    private Dictionary<(Sampler, SimpleTextureLayout), ResourceSet> _cachedResourceSets;
    
    /// <summary>
    /// Creates a new cascaded shadow map.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create the GPU resources.</param>
    /// <param name="resolution">The width and height of each cascade layer in pixels.</param>
    public ShadowMap(GraphicsDevice graphicsDevice, int resolution = 2048) {
        this.GraphicsDevice = graphicsDevice;
        this._resolution = resolution;
        this.Framebuffers = new Framebuffer[CascadeCount];
        this._cachedResourceSets = new Dictionary<(Sampler, SimpleTextureLayout), ResourceSet>();
        this.CreateFramebuffers();
    }
    
    /// <summary>
    /// Gets the cached resource set for the given sampler and layout, or creates it if it doesn't exist yet.
    /// </summary>
    /// <param name="sampler">The sampler used to sample the shadow map.</param>
    /// <param name="layout">The texture layout of the resource set.</param>
    /// <returns>The resource set.</returns>
    public ResourceSet GetResourceSet(Sampler sampler, SimpleTextureLayout layout) {
        if (!this._cachedResourceSets.TryGetValue((sampler, layout), out ResourceSet? resourceSet)) {
            ResourceSet newResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout.Layout, this.DepthTexture, sampler));
            
            this._cachedResourceSets.Add((sampler, layout), newResourceSet);
            return newResourceSet;
        }
        
        return resourceSet;
    }
    
    /// <summary>
    /// Creates the texture arrays and one framebuffer per cascade layer.
    /// </summary>
    private void CreateFramebuffers() {
        this.DepthTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint) this._resolution, (uint) this._resolution, 1, CascadeCount, PixelFormat.R32Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
        this.DepthTestTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint) this._resolution, (uint) this._resolution, 1, CascadeCount, PixelFormat.D32FloatS8UInt, TextureUsage.DepthStencil));
        
        for (uint cascadeIndex = 0; cascadeIndex < CascadeCount; cascadeIndex++) {
            this.Framebuffers[cascadeIndex] = this.GraphicsDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription(new FramebufferAttachmentDescription(this.DepthTestTexture, cascadeIndex), [
                    new FramebufferAttachmentDescription(this.DepthTexture, cascadeIndex)
                ])
            );
        }
    }
    
    /// <summary>
    /// Releases the resources allocated.
    /// </summary>
    private void ClearResources() {
        foreach (ResourceSet resourceSet in this._cachedResourceSets.Values) {
            resourceSet.Dispose();
        }
        
        this._cachedResourceSets.Clear();
        
        foreach (Framebuffer framebuffer in this.Framebuffers) {
            framebuffer.Dispose();
        }
        
        this.DepthTexture.Dispose();
        this.DepthTestTexture.Dispose();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.ClearResources();
        }
    }
}