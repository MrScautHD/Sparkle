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

namespace Sparkle.CSharp.Effects.Terrain;

public class PbrTileTerrainEffect : TerrainEffect {
    
    /// <summary>
    /// Path to the terrain tile vertex shader.
    /// </summary>
    public static readonly string VertPath = "content/sparkle/shaders/terrain/pbr_terrain_tiles.vert";
    
    /// <summary>
    /// Path to the terrain tile fragment shader.
    /// </summary>
    public static readonly string FragPath = "content/sparkle/shaders/terrain/pbr_terrain_tiles.frag";
    
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
    private Dictionary<Material, Texture> _sourceTextureArray;
    
    /// <summary>
    /// Tile index textures per material.
    /// </summary>
    private Dictionary<Material, Texture> _terrainLayerMap;
    
    /// <summary>
    /// Source texture samplers per material.
    /// </summary>
    private Dictionary<Material, Sampler> _sourceTextureArraySampler;
    
    /// <summary>
    /// Tile index samplers per material.
    /// </summary>
    private Dictionary<Material, Sampler> _terrainLayerMapSampler;
    
    /// <summary>
    /// Source texture resource sets per material.
    /// </summary>
    private Dictionary<Material, ResourceSet> _sourceTextureArrayResourceSets;
    
    /// <summary>
    /// Tile index resource sets per material.
    /// </summary>
    private Dictionary<Material, ResourceSet> _terrainLayerMapResourceSets;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PbrTileTerrainEffect"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create GPU resources.</param>
    /// <param name="vertText">The GLSL source code of the vertex shader, with all includes inserted.</param>
    /// <param name="fragText">The GLSL source code of the fragment shader, with all includes inserted.</param>
    /// <param name="compileOptions">The cross compile options.</param>
    /// <param name="macros">The shader macro definitions.</param>
    private PbrTileTerrainEffect(GraphicsDevice graphicsDevice, string vertText, string fragText, CrossCompileOptions compileOptions, MacroDefinition[] macros) : base(graphicsDevice, vertText, fragText, compileOptions, macros) {
        this._dirtyBrushMaterials = new HashSet<Material>();
        this._brushParameters = new Dictionary<Material, BrushParameters>();
        this._brushBuffers = new Dictionary<Material, SimpleUniformBuffer<BrushParameters>>();
        this._sourceTextureArray = new Dictionary<Material, Texture>();
        this._terrainLayerMap = new Dictionary<Material, Texture>();
        this._sourceTextureArraySampler = new Dictionary<Material, Sampler>();
        this._terrainLayerMapSampler = new Dictionary<Material, Sampler>();
        this._sourceTextureArrayResourceSets = new Dictionary<Material, ResourceSet>();
        this._terrainLayerMapResourceSets = new Dictionary<Material, ResourceSet>();
    }
    
    /// <summary>
    /// Creates a new <see cref="PbrTileTerrainEffect"/> by loading the terrain shaders from files.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create GPU resources.</param>
    /// <param name="compileOptions">The cross compile options.</param>
    /// <param name="macros">Optional macro definitions injected during shader compilation. If <see langword="null"/>, no macros are defined.</param>
    /// <returns>The compiled terrain effect.</returns>
    /// <exception cref="FileNotFoundException">Thrown if a shader file or one of its includes cannot be found.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the includes form a cycle.</exception>
    public static PbrTileTerrainEffect FromFiles(GraphicsDevice graphicsDevice, CrossCompileOptions compileOptions, MacroDefinition[]? macros = null) {
        ShaderIncludeProcessor processor = new ShaderIncludeProcessor("content");
        
        string vertText = processor.ProcessFile(VertPath);
        string fragText = processor.ProcessFile(FragPath);
        
        return new PbrTileTerrainEffect(graphicsDevice, vertText, fragText, compileOptions, macros ?? []);
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
    public override void SetBrushPreview(Material material, Vector3 center, float radius, TerrainBrushType brushType, Vector4 color, float strength = 1.0F, float wireThickness = 1.25F) {
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
    public override void DisableBrushPreview(Material material) {
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
    /// Sets the source texture array of a material, which holds one texture layer for each terrain material.
    /// </summary>
    /// <param name="material">The material that owns the texture array.</param>
    /// <param name="texture">The texture array with one layer per terrain material.</param>
    /// <param name="sampler">The sampler used to read the texture. If <see langword="null"/>, a point sampler with wrap addressing is used.</param>
    public override void SetSourceTextureArray(Material material, Texture texture, Sampler? sampler = null) {
        this._sourceTextureArray[material] = texture;
        this._sourceTextureArraySampler[material] = sampler ?? GraphicsHelper.GetSampler(this.GraphicsDevice, SamplerType.PointWrap);
        
        if (this._sourceTextureArrayResourceSets.Remove(material, out ResourceSet? resourceSet)) {
            resourceSet.Dispose();
        }
    }
    
    /// <summary>
    /// Sets the terrain layer map of a material, which stores the terrain material index of every tile.
    /// </summary>
    /// <param name="material">The material that owns the layer map.</param>
    /// <param name="texture">The tile index texture, with one texel per tile.</param>
    /// <param name="sampler">The sampler used to read the texture. If <see langword="null"/>, a point sampler with wrap addressing is used.</param>
    public override void SetTerrainLayerMap(Material material, Texture texture, Sampler? sampler = null) {
        this._terrainLayerMap[material] = texture;
        this._terrainLayerMapSampler[material] = sampler ?? GraphicsHelper.GetSampler(this.GraphicsDevice, SamplerType.PointWrap);
        
        if (this._terrainLayerMapResourceSets.Remove(material, out ResourceSet? resourceSet)) {
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
                brushBuffer.DeviceBuffer.Name = "BrushBuffer";
                
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
        if (material != null && this._sourceTextureArray.TryGetValue(material, out Texture? sourcesTexture)) {
            if (!this._sourceTextureArrayResourceSets.TryGetValue(material, out ResourceSet? sourcesResourceSet)) {
                sourcesResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this.GetTextureLayout("fSources").Layout, sourcesTexture, this._sourceTextureArraySampler[material]));
                this._sourceTextureArrayResourceSets[material] = sourcesResourceSet;
            }
            
            commandList.SetGraphicsResourceSet(this.GetTextureLayoutSlot("fSources"), sourcesResourceSet);
        }
        
        // Bind the tile index texture for this material.
        if (material != null && this._terrainLayerMap.TryGetValue(material, out Texture? tilesTexture)) {
            if (!this._terrainLayerMapResourceSets.TryGetValue(material, out ResourceSet? tilesResourceSet)) {
                tilesResourceSet = this.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this.GetTextureLayout("fTiles").Layout, tilesTexture, this._terrainLayerMapSampler[material]));
                this._terrainLayerMapResourceSets[material] = tilesResourceSet;
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
            this._sourceTextureArray.Clear();
            this._terrainLayerMap.Clear();
            this._sourceTextureArraySampler.Clear();
            this._terrainLayerMapSampler.Clear();
            
            foreach (ResourceSet resourceSet in this._sourceTextureArrayResourceSets.Values) {
                resourceSet.Dispose();
            }
            
            this._sourceTextureArrayResourceSets.Clear();
            
            foreach (ResourceSet resourceSet in this._terrainLayerMapResourceSets.Values) {
                resourceSet.Dispose();
            }
            
            this._terrainLayerMapResourceSets.Clear();
        }
    }
}