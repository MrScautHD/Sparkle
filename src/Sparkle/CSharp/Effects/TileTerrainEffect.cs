using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Materials;
using Sparkle.CSharp.Terrain;
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
    /// Tracks materials whose brush preview buffer needs to be updated.
    /// </summary>
    private HashSet<Material> _dirtyBrushMaterials;
    
    /// <summary>
    /// Stores brush preview parameters per material.
    /// </summary>
    private Dictionary<Material, BrushParameters> _brushParameters;
    
    /// <summary>
    /// Stores brush preview buffers per material.
    /// </summary>
    private Dictionary<Material, SimpleUniformBuffer<BrushParameters>> _brushBuffers;
    
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
        this._dirtyBrushMaterials = new HashSet<Material>();
        this._brushParameters = new Dictionary<Material, BrushParameters>();
        this._brushBuffers = new Dictionary<Material, SimpleUniformBuffer<BrushParameters>>();
        this._sourcesTextures = new Dictionary<Material, Texture>();
        this._tilesTextures = new Dictionary<Material, Texture>();
        this._sourcesSamplers = new Dictionary<Material, Sampler>();
        this._tilesSamplers = new Dictionary<Material, Sampler>();
        this._sourcesResourceSets = new Dictionary<Material, ResourceSet>();
        this._tilesResourceSets = new Dictionary<Material, ResourceSet>();
    }
    
    /// <summary>
    /// Enables or updates the terrain brush preview mask for a specific material.
    /// </summary>
    /// <param name="material">The material that owns this brush preview state.</param>
    /// <param name="center">The terrain-space brush center.</param>
    /// <param name="radius">The brush radius in terrain units.</param>
    /// <param name="brushType">The brush shape to preview.</param>
    /// <param name="color">The preview overlay color.</param>
    /// <param name="strength">The preview blend strength.</param>
    /// <param name="wireThickness">The screen-space wire thickness.</param>
    public void SetBrushPreview(Material material, Vector3 center, float radius, TerrainBrushType brushType, Vector4 color, float strength = 1.0F, float wireThickness = 1.25F) {
        this._brushParameters[material] = new BrushParameters() {
            BrushType = (int) brushType,
            BrushPreviewEnabled = 1,
            BrushRadius = radius,
            BrushPreviewStrength = strength,
            BrushPreviewColor = color,
            BrushCenter = center,
            BrushWireThickness = wireThickness
        };
        
        this._dirtyBrushMaterials.Add(material);
    }
    
    /// <summary>
    /// Disables the terrain brush preview mask for a specific material.
    /// </summary>
    /// <param name="material">The material whose brush preview should be disabled.</param>
    public void DisableBrushPreview(Material material) {
        if (!this._brushParameters.TryGetValue(material, out BrushParameters parameters)) {
            parameters = new BrushParameters() {
                BrushType = 0,
                BrushPreviewEnabled = 0,
                BrushRadius = 0.0F,
                BrushPreviewStrength = 0.0F,
                BrushPreviewColor = Color.White.ToRgbaFloatVec4(),
                BrushCenter = Vector3.Zero,
                BrushWireThickness = 0.5F
            };
            
            this._brushParameters[material] = parameters;
            this._dirtyBrushMaterials.Add(material);
            return;
        }
        
        if (parameters.BrushPreviewEnabled == 0) {
            return;
        }
        
        parameters.BrushPreviewEnabled = 0;
        this._brushParameters[material] = parameters;
        this._dirtyBrushMaterials.Add(material);
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
        
        // Bind/Update brush buffer.
        if (material != null) {
            if (!this._brushParameters.TryGetValue(material, out BrushParameters brushParameters)) {
                brushParameters = new BrushParameters() {
                    BrushType = 0,
                    BrushPreviewEnabled = 0,
                    BrushRadius = 0.0F,
                    BrushPreviewStrength = 0.0F,
                    BrushPreviewColor = Color.White.ToRgbaFloatVec4(),
                    BrushCenter = Vector3.Zero,
                    BrushWireThickness = 0.5F
                };
                
                this._brushParameters[material] = brushParameters;
                this._dirtyBrushMaterials.Add(material);
            }
            
            // Get brush buffer for the material.
            if (!this._brushBuffers.TryGetValue(material, out SimpleUniformBuffer<BrushParameters>? brushBuffer)) {
                brushBuffer = new SimpleUniformBuffer<BrushParameters>(this.GraphicsDevice, 1, ShaderStages.Fragment);
                this._brushBuffers[material] = brushBuffer;
                this._dirtyBrushMaterials.Add(material);
            }
            
            // Update dirty brush buffers.
            if (this._dirtyBrushMaterials.Remove(material)) {
                brushBuffer.SetValue(0, brushParameters);
                brushBuffer.UpdateBufferDeferred(commandList);
            }
            
            commandList.SetGraphicsResourceSet(this.GetBufferLayoutSlot("BrushBuffer"), brushBuffer.GetResourceSet(this.GetBufferLayout("BrushBuffer")));
        }
        
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
    
    /// <summary>
    /// Struct holding configurable brush parameters.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BrushParameters {
        public int BrushType;
        public int BrushPreviewEnabled;
        public float BrushRadius;
        public float BrushPreviewStrength;
        public Vector4 BrushPreviewColor;
        public Vector3 BrushCenter;
        public float BrushWireThickness;
        private float _padding;
        private float _padding1;
        private float _padding2;
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        
        if (disposing) {
            
            // Dispose buffers.
            this._dirtyBrushMaterials.Clear();
            this._brushParameters.Clear();
            
            foreach (SimpleUniformBuffer<BrushParameters> brushBuffer in this._brushBuffers.Values) {
                brushBuffer.Dispose();
            }
            
            this._brushBuffers.Clear();
            
            // Dispose texture ResourceSets.
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