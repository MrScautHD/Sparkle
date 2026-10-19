using System.Numerics;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Materials;
using Sparkle.CSharp.Terrain;
using Veldrith;
using Veldrith.SPIRV;

namespace Sparkle.CSharp.Effects.Terrain;

public abstract class TerrainEffect : Effect {
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainEffect"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create shaders and related GPU resources.</param>
    /// <param name="vertPath">The file path to the compiled vertex shader bytecode.</param>
    /// <param name="fragPath">The file path to the compiled fragment shader bytecode.</param>
    /// <param name="compileOptions">The cross-compilation options used when creating the shaders.</param>
    public TerrainEffect(GraphicsDevice graphicsDevice, string vertPath, string fragPath, CrossCompileOptions compileOptions) : base(graphicsDevice, vertPath, fragPath, compileOptions) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainEffect"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create shaders and related GPU resources.</param>
    /// <param name="vertBytes">The compiled vertex shader bytecode.</param>
    /// <param name="fragBytes">The compiled fragment shader bytecode.</param>
    /// <param name="compileOptions">The cross-compilation options used when creating the shaders.</param>
    public TerrainEffect(GraphicsDevice graphicsDevice, byte[] vertBytes, byte[] fragBytes, CrossCompileOptions compileOptions) : base(graphicsDevice, vertBytes, fragBytes, compileOptions) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainEffect"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create GPU resources.</param>
    /// <param name="vertText">The GLSL source code for the vertex shader.</param>
    /// <param name="fragText">The GLSL source code for the fragment shader.</param>
    /// <param name="compileOptions">The cross-compilation and specialization options.</param>
    /// <param name="macros">The macro definitions injected during shader compilation.</param>
    public TerrainEffect(GraphicsDevice graphicsDevice, string vertText, string fragText, CrossCompileOptions compileOptions, MacroDefinition[] macros) : base(graphicsDevice, vertText, fragText, compileOptions, macros) { }
    
    /// <summary>
    /// Enables or updates the terrain brush preview for a specific material.
    /// </summary>
    /// <param name="material">The material that owns this brush preview state.</param>
    /// <param name="center">The terrain-space brush center.</param>
    /// <param name="radius">The brush radius in terrain units.</param>
    /// <param name="brushType">The brush shape to preview.</param>
    /// <param name="color">The preview overlay color. W stores the opacity.</param>
    /// <param name="strength">The preview blend strength.</param>
    /// <param name="wireThickness">The screen-space wire thickness.</param>
    public abstract void SetBrushPreview(Material material, Vector3 center, float radius, TerrainBrushType brushType, Vector4 color, float strength = 1.0F, float wireThickness = 1.25F);
    
    /// <summary>
    /// Disables the terrain brush preview for a specific material.
    /// </summary>
    /// <param name="material">The material whose brush preview should be disabled.</param>
    public abstract void DisableBrushPreview(Material material);
    
    /// <summary>
    /// Sets the source texture array of a material, which holds one texture layer for each terrain material.
    /// </summary>
    /// <param name="material">The material that owns the texture array.</param>
    /// <param name="texture">The texture array with one layer per terrain material.</param>
    /// <param name="sampler">The sampler used to read the texture. If <see langword="null"/>, the implementation chooses a default sampler.</param>
    public abstract void SetSourceTextureArray(Material material, Texture texture, Sampler? sampler = null);
    
    /// <summary>
    /// Sets the terrain layer map of a material, which stores the terrain material index of every tile.
    /// </summary>
    /// <param name="material">The material that owns the layer map.</param>
    /// <param name="texture">The tile index texture, with one texel per tile.</param>
    /// <param name="sampler">The sampler used to read the texture. If <see langword="null"/>, the implementation chooses a default sampler.</param>
    public abstract void SetTerrainLayerMap(Material material, Texture texture, Sampler? sampler = null);
}