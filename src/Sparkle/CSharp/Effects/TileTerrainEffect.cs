using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics;
using Bliss.CSharp.Materials;
using Veldrith;
using Veldrith.SPIRV;

namespace Sparkle.CSharp.Effects;

public class TileTerrainEffect : Effect {
    
    /// <summary>
    /// Path to the terrain tile vertex shader.
    /// </summary>
    public static readonly string VertPath = "content/sparkle/shaders/terrain_tiles.vert";
    
    /// <summary>
    /// Path to the terrain tile fragment shader.
    /// </summary>
    public static readonly string FragPath = "content/sparkle/shaders/terrain_tiles.frag";
    
    /// <summary>
    /// Source texture arrays per material.
    /// </summary>
    private Dictionary<Material, Texture> _sourcesTextures;
    
    /// <summary>
    /// Tile index textures per material.
    /// </summary>
    private Dictionary<Material, Texture> _tilesTextures;
    
    /// <summary>
    /// Source texture samplers per material.
    /// </summary>
    private Dictionary<Material, Sampler> _sourcesSamplers;
    
    /// <summary>
    /// Tile index samplers per material.
    /// </summary>
    private Dictionary<Material, Sampler> _tilesSamplers;
    
    /// <summary>
    /// Source texture resource sets per material.
    /// </summary>
    private Dictionary<Material, ResourceSet> _sourcesResourceSets;
    
    /// <summary>
    /// Tile index resource sets per material.
    /// </summary>
    private Dictionary<Material, ResourceSet> _tilesResourceSets;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TileTerrainEffect"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="compileOptions">Optional cross-compilation options used when creating the shaders.</param>
    /// <param name="macros">Optional macro definitions injected during shader compilation.</param>
    public TileTerrainEffect(GraphicsDevice graphicsDevice, CrossCompileOptions compileOptions, MacroDefinition[]? macros = null) : base(graphicsDevice, LoadTextCodeFromFile(VertPath), LoadTextCodeFromFile(FragPath), compileOptions, macros ?? []) {
        this._sourcesTextures = new Dictionary<Material, Texture>();
        this._tilesTextures = new Dictionary<Material, Texture>();
        this._sourcesSamplers = new Dictionary<Material, Sampler>();
        this._tilesSamplers = new Dictionary<Material, Sampler>();
        this._sourcesResourceSets = new Dictionary<Material, ResourceSet>();
        this._tilesResourceSets = new Dictionary<Material, ResourceSet>();
    }
    
    /// <summary>
    /// Sets the source texture array used by the terrain shader for a specific material.
    /// </summary>
    /// <param name="material">The material that owns the texture binding.</param>
    /// <param name="texture">The source texture array.</param>
    /// <param name="sampler">Optional sampler.</param>
    public void SetSourcesTexture(Material material, Texture texture, Sampler? sampler = null) {
        this._sourcesTextures[material] = texture;
        this._sourcesSamplers[material] = sampler ?? GraphicsHelper.GetSampler(this.GraphicsDevice, SamplerType.PointWrap);
        
        if (this._sourcesResourceSets.Remove(material, out ResourceSet? resourceSet)) {
            resourceSet.Dispose();
        }
    }
    
    /// <summary>
    /// Sets the tile index texture used by the terrain shader for a specific material.
    /// </summary>
    /// <param name="material">The material that owns the texture binding.</param>
    /// <param name="texture">The tile index texture.</param>
    /// <param name="sampler">Optional sampler.</param>
    public void SetTilesTexture(Material material, Texture texture, Sampler? sampler = null) {
        this._tilesTextures[material] = texture;
        this._tilesSamplers[material] = sampler ?? GraphicsHelper.GetSampler(this.GraphicsDevice, SamplerType.PointWrap);
        
        if (this._tilesResourceSets.Remove(material, out ResourceSet? resourceSet)) {
            resourceSet.Dispose();
        }
    }
    
    /// <summary>
    /// Applies the terrain effect using the current texture values.
    /// </summary>
    /// <param name="commandList">The command list used to issue GPU commands.</param>
    /// <param name="material">An optional material to apply with the effect.</param>
    public override void Apply(CommandList commandList, Material? material = null) {
        base.Apply(commandList, material);
        
        // Bind the source texture array for this material.
        if (material != null && this._sourcesTextures.TryGetValue(material, out Texture? sourcesTexture)) {
            if (!this._sourcesResourceSets.TryGetValue(material, out ResourceSet? sourcesResourceSet)) {
                sourcesResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this.GetTextureLayout("fSources").Layout, sourcesTexture, this._sourcesSamplers[material]));
                this._sourcesResourceSets[material] = sourcesResourceSet;
            }
            
            commandList.SetGraphicsResourceSet(this.GetTextureLayoutSlot("fSources"), sourcesResourceSet);
        }
        
        // Bind the tile index texture for this material.
        if (material != null && this._tilesTextures.TryGetValue(material, out Texture? tilesTexture)) {
            if (!this._tilesResourceSets.TryGetValue(material, out ResourceSet? tilesResourceSet)) {
                tilesResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this.GetTextureLayout("fTiles").Layout, tilesTexture, this._tilesSamplers[material]));
                this._tilesResourceSets[material] = tilesResourceSet;
            }
            
            commandList.SetGraphicsResourceSet(this.GetTextureLayoutSlot("fTiles"), tilesResourceSet);
        }
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        
        if (disposing) {
            this._sourcesTextures.Clear();
            this._tilesTextures.Clear();
            this._sourcesSamplers.Clear();
            this._tilesSamplers.Clear();
            
            foreach (ResourceSet resourceSet in this._sourcesResourceSets.Values) {
                resourceSet.Dispose();
            }
            
            this._sourcesResourceSets.Clear();
            
            foreach (ResourceSet resourceSet in this._tilesResourceSets.Values) {
                resourceSet.Dispose();
            }
            
            this._tilesResourceSets.Clear();
        }
    }
}