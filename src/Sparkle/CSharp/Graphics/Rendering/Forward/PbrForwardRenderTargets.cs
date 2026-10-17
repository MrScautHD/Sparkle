using Bliss.CSharp;
using Bliss.CSharp.Graphics.Pipelines.Textures;
using Veldrith;

namespace Sparkle.CSharp.Graphics.Rendering.Forward;

public class PbrForwardRenderTargets : Disposable {
    
    /// <summary>
    /// The graphics device used to create and dispose the resources.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// The lit scene color at full resolution. It is sampled at the ray hit position to get the reflected color.
    /// </summary>
    public Texture ColorTexture { get; private set; }
    
    /// <summary>
    /// The world-space normals from the normal prepass, encoded into the [0, 1] range.
    /// </summary>
    public Texture NormalTexture { get; private set; }
    
    /// <summary>
    /// The scene depth, shared by the normal prepass and the main scene pass.
    /// </summary>
    public Texture DepthTexture { get; private set; }
    
    /// <summary>
    /// The per-pixel material reflectivity (red channel), written by the main scene pass.
    /// </summary>
    public Texture SsrMaskTexture { get; private set; }
    
    /// <summary>
    /// The traced reflections (rgb = reflected color, a = opacity), rendered at the scaled SSR resolution.
    /// </summary>
    public Texture SsrTexture { get; private set; }
    
    /// <summary>
    /// The fogged scene color at full resolution.
    /// </summary>
    public Texture FogTexture { get; private set; }
    
    /// <summary>
    /// The framebuffer of the main scene pass. It writes the scene color and the SSR mask and uses the shared depth texture.
    /// </summary>
    public Framebuffer ColorFramebuffer { get; private set; }
    
    /// <summary>
    /// The framebuffer of the normal prepass. It writes the normals and uses the shared depth texture.
    /// </summary>
    public Framebuffer NormalFramebuffer { get; private set; }
    
    /// <summary>
    /// The framebuffer of the SSR pass. It only writes the reflection result and has no depth attachment.
    /// </summary>
    public Framebuffer SsrFramebuffer { get; private set; }
    
    /// <summary>
    /// The framebuffer of the fog pass. It writes the fogged scene color and has no depth attachment.
    /// </summary>
    public Framebuffer FogFramebuffer { get; private set; }
    
    /// <summary>
    /// The width of the full-resolution targets in pixels.
    /// </summary>
    private uint _width;
    
    /// <summary>
    /// The height of the full-resolution targets in pixels.
    /// </summary>
    private uint _height;
    
    /// <summary>
    /// The resolution of the SSR result relative to the screen resolution.
    /// </summary>
    private float _ssrResolutionScale;
    
    /// <summary>
    /// The pixel format of the scene color and SSR result textures.
    /// </summary>
    private PixelFormat _colorFormat;
    
    /// <summary>
    /// The cached resource sets of the scene color texture, one per sampler and layout combination.
    /// </summary>
    private Dictionary<(Sampler, SimpleTextureLayout), ResourceSet> _cachedColorResourceSets;
    
    /// <summary>
    /// The cached resource sets of the normal texture, one per sampler and layout combination.
    /// </summary>
    private Dictionary<(Sampler, SimpleTextureLayout), ResourceSet> _cachedNormalResourceSets;
    
    /// <summary>
    /// The cached resource sets of the depth texture, one per sampler and layout combination.
    /// </summary>
    private Dictionary<(Sampler, SimpleTextureLayout), ResourceSet> _cachedDepthResourceSets;
    
    /// <summary>
    /// The cached resource sets of the SSR mask texture, one per sampler and layout combination.
    /// </summary>
    private Dictionary<(Sampler, SimpleTextureLayout), ResourceSet> _cachedSsrMaskResourceSets;
    
    /// <summary>
    /// The cached resource sets of the SSR result texture, one per sampler and layout combination.
    /// </summary>
    private Dictionary<(Sampler, SimpleTextureLayout), ResourceSet> _cachedSsrResourceSets;
    
    /// <summary>
    /// The cached resource sets of the fogged scene color texture, one per sampler and layout combination.
    /// </summary>
    private Dictionary<(Sampler, SimpleTextureLayout), ResourceSet> _cachedFogResourceSets;
    
    /// <summary>
    /// Creates the render targets for the PBR forward renderer.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create the resources.</param>
    /// <param name="width">The width of the full-resolution targets in pixels.</param>
    /// <param name="height">The height of the full-resolution targets in pixels.</param>
    /// <param name="colorFormat">The pixel format of the scene color and SSR result textures.</param>
    /// <param name="ssrResolutionScale">The resolution of the SSR result relative to the screen resolution, clamped to [0.1, 1.0].</param>
    public PbrForwardRenderTargets(GraphicsDevice graphicsDevice, uint width, uint height, PixelFormat colorFormat, float ssrResolutionScale = 1.0F) {
        this.GraphicsDevice = graphicsDevice;
        this._width = width;
        this._height = height;
        this._colorFormat = colorFormat;
        this._ssrResolutionScale = Math.Clamp(ssrResolutionScale, 0.1F, 1.0F);
        this._cachedColorResourceSets = new Dictionary<(Sampler, SimpleTextureLayout), ResourceSet>();
        this._cachedNormalResourceSets = new Dictionary<(Sampler, SimpleTextureLayout), ResourceSet>();
        this._cachedDepthResourceSets = new Dictionary<(Sampler, SimpleTextureLayout), ResourceSet>();
        this._cachedSsrMaskResourceSets = new Dictionary<(Sampler, SimpleTextureLayout), ResourceSet>();
        this._cachedSsrResourceSets = new Dictionary<(Sampler, SimpleTextureLayout), ResourceSet>();
        this._cachedFogResourceSets = new Dictionary<(Sampler, SimpleTextureLayout), ResourceSet>();
        this.CreateFrameBuffers();
    }
    
    /// <summary>
    /// Gets the resource set of the scene color texture for the given sampler and layout.
    /// </summary>
    /// <param name="sampler">The sampler used to read the texture.</param>
    /// <param name="layout">The texture layout the resource set has to match.</param>
    /// <returns>The cached or newly created resource set.</returns>
    public ResourceSet GetColorResourceSet(Sampler sampler, SimpleTextureLayout layout) {
        if (!this._cachedColorResourceSets.TryGetValue((sampler, layout), out ResourceSet? resourceSet)) {
            ResourceSet newResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout.Layout, this.ColorTexture, sampler));
            
            this._cachedColorResourceSets.Add((sampler, layout), newResourceSet);
            return newResourceSet;
        }
        
        return resourceSet;
    }
    
    /// <summary>
    /// Gets the resource set of the normal texture for the given sampler and layout.
    /// </summary>
    /// <param name="sampler">The sampler used to read the texture.</param>
    /// <param name="layout">The texture layout the resource set has to match.</param>
    /// <returns>The cached or newly created resource set.</returns>
    public ResourceSet GetNormalResourceSet(Sampler sampler, SimpleTextureLayout layout) {
        if (!this._cachedNormalResourceSets.TryGetValue((sampler, layout), out ResourceSet? resourceSet)) {
            ResourceSet newResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout.Layout, this.NormalTexture, sampler));
            
            this._cachedNormalResourceSets.Add((sampler, layout), newResourceSet);
            return newResourceSet;
        }
        
        return resourceSet;
    }
    
    /// <summary>
    /// Gets the resource set of the depth texture for the given sampler and layout.
    /// </summary>
    /// <param name="sampler">The sampler used to read the texture.</param>
    /// <param name="layout">The texture layout the resource set has to match.</param>
    /// <returns>The cached or newly created resource set.</returns>
    public ResourceSet GetDepthResourceSet(Sampler sampler, SimpleTextureLayout layout) {
        if (!this._cachedDepthResourceSets.TryGetValue((sampler, layout), out ResourceSet? resourceSet)) {
            ResourceSet newResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout.Layout, this.DepthTexture, sampler));
            
            this._cachedDepthResourceSets.Add((sampler, layout), newResourceSet);
            return newResourceSet;
        }
        
        return resourceSet;
    }
    
    /// <summary>
    /// Gets the resource set of the SSR mask texture for the given sampler and layout.
    /// </summary>
    /// <param name="sampler">The sampler used to read the texture.</param>
    /// <param name="layout">The texture layout the resource set has to match.</param>
    /// <returns>The cached or newly created resource set.</returns>
    public ResourceSet GetSsrMaskResourceSet(Sampler sampler, SimpleTextureLayout layout) {
        if (!this._cachedSsrMaskResourceSets.TryGetValue((sampler, layout), out ResourceSet? resourceSet)) {
            ResourceSet newResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout.Layout, this.SsrMaskTexture, sampler));
            
            this._cachedSsrMaskResourceSets.Add((sampler, layout), newResourceSet);
            return newResourceSet;
        }
        
        return resourceSet;
    }
    
    /// <summary>
    /// Gets the resource set of the SSR result texture for the given sampler and layout.
    /// </summary>
    /// <param name="sampler">The sampler used to read the texture.</param>
    /// <param name="layout">The texture layout the resource set has to match.</param>
    /// <returns>The cached or newly created resource set.</returns>
    public ResourceSet GetSsrResourceSet(Sampler sampler, SimpleTextureLayout layout) {
        if (!this._cachedSsrResourceSets.TryGetValue((sampler, layout), out ResourceSet? resourceSet)) {
            ResourceSet newResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout.Layout, this.SsrTexture, sampler));
            
            this._cachedSsrResourceSets.Add((sampler, layout), newResourceSet);
            return newResourceSet;
        }
        
        return resourceSet;
    }
    
    /// <summary>
    /// Gets the resource set of the fogged scene color texture for the given sampler and layout.
    /// </summary>
    /// <param name="sampler">The sampler used to read the texture.</param>
    /// <param name="layout">The texture layout the resource set has to match.</param>
    /// <returns>The cached or newly created resource set.</returns>
    public ResourceSet GetFogResourceSet(Sampler sampler, SimpleTextureLayout layout) {
        if (!this._cachedFogResourceSets.TryGetValue((sampler, layout), out ResourceSet? resourceSet)) {
            ResourceSet newResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout.Layout, this.FogTexture, sampler));
            
            this._cachedFogResourceSets.Add((sampler, layout), newResourceSet);
            return newResourceSet;
        }
        
        return resourceSet;
    }
    
    /// <summary>
    /// Changes the size, color format and resolution scale in one step.
    /// </summary>
    /// <remarks>
    /// The resources are recreated only once, and only if at least one value actually changed.
    /// This is cheaper than setting the properties one by one, because each property setter recreates everything on its own.
    /// </remarks>
    /// <param name="width">The new width of the full-resolution targets in pixels.</param>
    /// <param name="height">The new height of the full-resolution targets in pixels.</param>
    /// <param name="colorFormat">The new pixel format of the scene color and SSR result textures.</param>
    /// <param name="ssrResolutionScale">The new resolution of the SSR result relative to the screen resolution, clamped to [0.1, 1.0].</param>
    public void Resize(uint width, uint height, PixelFormat colorFormat, float ssrResolutionScale) {
        float clampedScale = Math.Clamp(ssrResolutionScale, 0.1F, 1.0F);
        
        if (this._width == width && this._height == height && this._colorFormat == colorFormat && MathF.Abs(this._ssrResolutionScale - clampedScale) <= 0.0001F) {
            return;
        }
        
        this._width = width;
        this._height = height;
        this._colorFormat = colorFormat;
        this._ssrResolutionScale = clampedScale;
        
        this.ClearResources();
        this.CreateFrameBuffers();
    }
    
    /// <summary>
    /// Creates all textures and framebuffers for the current size, format, and scale.
    /// </summary>
    private void CreateFrameBuffers() {
        uint ssrWidth = Math.Max(1, (uint) MathF.Ceiling(this._width * this._ssrResolutionScale));
        uint ssrHeight = Math.Max(1, (uint) MathF.Ceiling(this._height * this._ssrResolutionScale));
        
        this.ColorTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(this._width, this._height, 1, 1, this._colorFormat, TextureUsage.RenderTarget | TextureUsage.Sampled));
        this.NormalTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(this._width, this._height, 1, 1, PixelFormat.R16G16B16A16Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
        this.SsrMaskTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(this._width, this._height, 1, 1, PixelFormat.R8UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
        this.SsrTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(ssrWidth, ssrHeight, 1, 1, this._colorFormat, TextureUsage.RenderTarget | TextureUsage.Sampled));
        this.FogTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(this._width, this._height, 1, 1, this._colorFormat, TextureUsage.RenderTarget | TextureUsage.Sampled));
        this.DepthTexture = this.GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(this._width, this._height, 1, 1, PixelFormat.R32Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
        
        this.ColorFramebuffer = this.GraphicsDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription(this.DepthTexture, this.ColorTexture, this.SsrMaskTexture));
        this.NormalFramebuffer = this.GraphicsDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription(this.DepthTexture, this.NormalTexture));
        this.FogFramebuffer = this.GraphicsDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, this.FogTexture));
        this.SsrFramebuffer = this.GraphicsDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, this.SsrTexture));
    }
    
    /// <summary>
    /// Releases the resources allocated.
    /// </summary>
    private void ClearResources() {
        foreach (ResourceSet resourceSet in this._cachedColorResourceSets.Values) {
            resourceSet.Dispose();
        }
        
        this._cachedColorResourceSets.Clear();
        
        foreach (ResourceSet resourceSet in this._cachedNormalResourceSets.Values) {
            resourceSet.Dispose();
        }
        
        this._cachedNormalResourceSets.Clear();
        
        foreach (ResourceSet resourceSet in this._cachedDepthResourceSets.Values) {
            resourceSet.Dispose();
        }
        
        this._cachedDepthResourceSets.Clear();
        
        foreach (ResourceSet resourceSet in this._cachedSsrMaskResourceSets.Values) {
            resourceSet.Dispose();
        }
        
        this._cachedSsrMaskResourceSets.Clear();
        
        foreach (ResourceSet resourceSet in this._cachedSsrResourceSets.Values) {
            resourceSet.Dispose();
        }
        
        this._cachedSsrResourceSets.Clear();
        
        foreach (ResourceSet resourceSet in this._cachedFogResourceSets.Values) {
            resourceSet.Dispose();
        }
        
        this._cachedFogResourceSets.Clear();
        
        this.ColorTexture.Dispose();
        this.NormalTexture.Dispose();
        this.DepthTexture.Dispose();
        this.SsrMaskTexture.Dispose();
        this.SsrTexture.Dispose();
        this.FogTexture.Dispose();
        
        this.ColorFramebuffer.Dispose();
        this.NormalFramebuffer.Dispose();
        this.SsrFramebuffer.Dispose();
        this.FogFramebuffer.Dispose();
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.ClearResources();
        }
    }
}