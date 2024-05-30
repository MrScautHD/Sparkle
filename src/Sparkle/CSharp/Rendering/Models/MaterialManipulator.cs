using Raylib_CSharp.Colors;
using Raylib_CSharp.Geometry;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Textures;
using Sparkle.CSharp.Effects;

namespace Sparkle.CSharp.Rendering.Models;

public struct MaterialManipulator {

    private List<(int, Effect)> _effects;
    private List<(int, MaterialMapIndex, Texture2D)> _textures;
    private List<(int, MaterialMapIndex, Color)> _colors;
    private List<(int, MaterialMapIndex, float)> _values;
    private List<(int, int, float)> _params;
    
    /// <summary>
    /// Constructor for creating a MaterialManipulator object.
    /// </summary>
    public MaterialManipulator() {
        this._effects = new List<(int, Effect)>();
        this._textures = new List<(int, MaterialMapIndex, Texture2D)>();
        this._colors = new List<(int, MaterialMapIndex, Color)>();
        this._values = new List<(int, MaterialMapIndex, float)>();
        this._params = new List<(int, int, float)>();
    }
    
    /// <summary>
    /// Sets the shader effect for the specified material index in the model.
    /// </summary>
    /// <param name="materialIndex">The index of the material to set the shader for.</param>
    /// <param name="effect">The effect to apply to the material.</param>
    /// <returns>The current instance of MaterialManipulator for method chaining.</returns>
    public MaterialManipulator Set(int materialIndex, Effect effect) {
        this._effects.Add((materialIndex, effect));
        return this;
    }
    
    /// <summary>
    /// Sets the texture for the specified material and map index in the model.
    /// </summary>
    /// <param name="materialIndex">The index of the material to set the texture for.</param>
    /// <param name="mapIndex">The index of the material map to set the texture for.</param>
    /// <param name="texture">The texture to apply to the material.</param>
    /// <returns>The current instance of MaterialManipulator for method chaining.</returns>
    public MaterialManipulator Set(int materialIndex, MaterialMapIndex mapIndex, Texture2D texture) {
        this._textures.Add((materialIndex, mapIndex, texture));
        return this;
    }

    /// <summary>
    /// Sets the color for the specified material and map index in the model.
    /// </summary>
    /// <param name="materialIndex">The index of the material to set the color for.</param>
    /// <param name="mapIndex">The index of the material map to set the color for.</param>
    /// <param name="color">The color to apply to the material.</param>
    /// <returns>The current instance of MaterialManipulator for method chaining.</returns>
    public MaterialManipulator Set(int materialIndex, MaterialMapIndex mapIndex, Color color) {
        this._colors.Add((materialIndex, mapIndex, color));
        return this;
    }
    
    /// <summary>
    /// Sets the float value for the specified material and map index in the model.
    /// </summary>
    /// <param name="materialIndex">The index of the material to set the value for.</param>
    /// <param name="mapIndex">The index of the material map to set the value for.</param>
    /// <param name="value">The float value to apply to the material.</param>
    /// <returns>The current instance of MaterialManipulator for method chaining.</returns>
    public MaterialManipulator Set(int materialIndex, MaterialMapIndex mapIndex, float value) {
        this._values.Add((materialIndex, mapIndex, value));
        return this;
    }
    
    /// <summary>
    /// Sets the float value for the specified parameter index of a material in the model.
    /// </summary>
    /// <param name="materialIndex">The index of the material to set the value for.</param>
    /// <param name="paramIndex">The index of the parameter within the material to set the value for.</param>
    /// <param name="value">The float value to apply to the parameter.</param>
    /// <returns>The current instance of MaterialManipulator for method chaining.</returns>
    public MaterialManipulator Set(int materialIndex, int paramIndex, float value) {
        this._params.Add((materialIndex, paramIndex, value));
        return this;
    }

    /// <summary>
    /// Sets the material properties for a given model using the configured effects, textures, colors, values, and params.
    /// </summary>
    /// <param name="model">The model to apply the material properties to.</param>
    internal void Build(ref Model model) {
        foreach (var effect in this._effects) {
            model.SetMaterialShader(effect.Item1, effect.Item2.Shader);
        }
        
        foreach (var texture in this._textures) {
            model.SetMaterialTexture(texture.Item1, texture.Item2, texture.Item3);
        }
        
        foreach (var color in this._colors) {
            model.SetMaterialColor(color.Item1, color.Item2, color.Item3);
        }
        
        foreach (var value in this._values) {
            model.SetMaterialValue(value.Item1, value.Item2, value.Item3);
        }
        
        foreach (var param in this._params) {
            model.Materials[param.Item1].Param[param.Item2] = param.Item3;
        }
        
        this._effects.Clear();
        this._textures.Clear();
        this._colors.Clear();
        this._values.Clear();
        this._params.Clear();
    }
}