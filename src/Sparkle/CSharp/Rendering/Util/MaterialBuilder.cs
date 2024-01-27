using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Rendering.Util;

public class MaterialBuilder {

    private Model _model;
    private Material[] _materials;

    /// <summary>
    /// Represents a builder class for creating materials.
    /// </summary>
    public MaterialBuilder(Model model) {
        this._model = model;
        this._materials = new Material[this._model.MaterialCount];
        this.SetupMaterial();
    }

    /// <summary>
    /// Sets up the materials for the model with the specified shader.
    /// </summary>
    private unsafe void SetupMaterial() {
        for (int i = 0; i < this._model.MaterialCount; i++) {
            this._materials[i] = this._model.Materials[i];
        }
    }

    /// <summary>
    /// Adds a texture to all the materials in the MaterialBuilder.
    /// </summary>
    /// <param name="mapIndex">The index of the material map to update.</param>
    /// <param name="texture">The texture to add.</param>
    /// <returns>A reference to the MaterialBuilder instance for method chaining.</returns>
    public MaterialBuilder Add(MaterialMapIndex mapIndex, Texture2D texture) {
        for (int i = 0; i < this._materials.Length; i++) {
            MaterialHelper.SetTexture(ref this._materials[i], mapIndex, texture);
        }
        
        return this;
    }

    /// <summary>
    /// Adds a color to the specified map index for all materials in the material builder.
    /// </summary>
    /// <param name="mapIndex">The map index to add the color to.</param>
    /// <param name="color">The color to add.</param>
    /// <returns>The updated material builder.</returns>
    public MaterialBuilder Add(MaterialMapIndex mapIndex, Color color) {
        for (int i = 0; i < this._materials.Length; i++) {
            MaterialHelper.SetColor(ref this._materials[i], mapIndex, color);
        }
        
        return this;
    }

    /// <summary>
    /// Adds the specified value to the maps at the specified map index for all materials in the MaterialBuilder.
    /// </summary>
    /// <param name="mapIndex">The map index at which to add the value.</param>
    /// <param name="value">The value to add to the maps.</param>
    /// <returns>The updated MaterialBuilder object.</returns>
    public MaterialBuilder Add(MaterialMapIndex mapIndex, float value) {
        for (int i = 0; i < this._materials.Length; i++) {
            MaterialHelper.SetValue(ref this._materials[i], mapIndex, value);
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