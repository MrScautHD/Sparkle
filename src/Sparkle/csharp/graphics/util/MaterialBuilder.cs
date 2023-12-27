using Raylib_cs;
using Sparkle.csharp.registry.types;

namespace Sparkle.csharp.graphics.util;

public class MaterialBuilder {

    private Material[] _materials;
    private Shader _shader;

    /// <summary>
    /// Represents a builder class for creating materials.
    /// </summary>
    public unsafe MaterialBuilder(Model model, Shader? shader = default) {
        this._materials = new Material[model.MaterialCount];
        this._shader = shader ?? ShaderRegistry.DiscardAlpha;
        
        for (int i = 0; i < model.MaterialCount; i++) {
            this._materials[i] = model.Materials[i];
            this._materials[i].Shader = this._shader;
        }
    }

    /// <summary>
    /// Adds a texture to all the materials in the MaterialBuilder.
    /// </summary>
    /// <param name="mapIndex">The index of the material map to update.</param>
    /// <param name="texture">The texture to add.</param>
    /// <returns>A reference to the MaterialBuilder instance for method chaining.</returns>
    public unsafe MaterialBuilder Add(MaterialMapIndex mapIndex, Texture2D texture) {
        foreach (Material material in this._materials) {
            material.Maps[(int) mapIndex].Texture = texture;
        }
        
        return this;
    }

    /// <summary>
    /// Adds a color to the specified map index for all materials in the material builder.
    /// </summary>
    /// <param name="mapIndex">The map index to add the color to.</param>
    /// <param name="color">The color to add.</param>
    /// <returns>The updated material builder.</returns>
    public unsafe MaterialBuilder Add(MaterialMapIndex mapIndex, Color color) {
        foreach (Material material in this._materials) {
            material.Maps[(int) mapIndex].Color = color;
        }
        
        return this;
    }

    /// <summary>
    /// Adds the specified value to the maps at the specified map index for all materials in the MaterialBuilder.
    /// </summary>
    /// <param name="mapIndex">The map index at which to add the value.</param>
    /// <param name="value">The value to add to the maps.</param>
    /// <returns>The updated MaterialBuilder object.</returns>
    public unsafe MaterialBuilder Add(MaterialMapIndex mapIndex, float value) {
        foreach (Material material in this._materials) {
            material.Maps[(int) mapIndex].Value = value;
        }
        
        return this;
    }

    /// <summary>
    /// Builds and returns an array of materials.
    /// </summary>
    /// <returns>An array of materials.</returns>
    public Material[] Build() {
        return this._materials;
    }
}