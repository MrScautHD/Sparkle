using Raylib_cs;

namespace Sparkle.CSharp.Rendering.Helpers; 

public static class MaterialHelper {
    
    /// <inheritdoc cref="Raylib.LoadMaterialDefault"/>
    public static Material LoadDefault() => Raylib.LoadMaterialDefault();
    
    /// <inheritdoc cref="Raylib.LoadMaterials"/>
    public static unsafe Material* LoadMaterials(sbyte* fileName, int* materialCount) => Raylib.LoadMaterials(fileName, materialCount);
    
    /// <inheritdoc cref="Raylib.IsMaterialReady"/>
    public static bool IsReady(Material material) => Raylib.IsMaterialReady(material);
    
    /// <inheritdoc cref="Raylib.UnloadMaterial"/>
    public static void Unload(Material material) => Raylib.UnloadMaterial(material);

    
    /// <inheritdoc cref="Raylib.GetMaterialTexture"/>
    public static Texture2D GetTexture(ref Model model, int materialIndex, MaterialMapIndex mapIndex) => Raylib.GetMaterialTexture(ref model, materialIndex, mapIndex);
    
    /// <inheritdoc cref="Raylib.SetMaterialShader"/>
    public static void SetShader(ref Model model, int materialIndex, ref Shader shader) => Raylib.SetMaterialShader(ref model, materialIndex, ref shader);
    
    /// <inheritdoc cref="Raylib.SetMaterialTexture(ref Material, MaterialMapIndex, Texture2D)"/>
    public static void SetTexture(ref Material material, MaterialMapIndex mapType, Texture2D texture) => Raylib.SetMaterialTexture(ref material, mapType, texture);
    
    /// <inheritdoc cref="Raylib.SetMaterialTexture(ref Model, int, MaterialMapIndex, ref Texture2D)"/>
    public static void SetTexture(ref Model model, int materialIndex, MaterialMapIndex mapIndex, ref Texture2D texture) => Raylib.SetMaterialTexture(ref model, materialIndex, mapIndex, ref texture);

    /// <summary>
    /// Sets the color of a specified material map.
    /// </summary>
    /// <param name="material">The reference to the material object.</param>
    /// <param name="mapIndex">The index of the material map to set the color for.</param>
    /// <param name="color">The color to set.</param>
    public static unsafe void SetColor(ref Material material, MaterialMapIndex mapIndex, Color color) {
        material.Maps[(int) mapIndex].Color = color;
    }

    /// <summary>
    /// Sets the color of a specified material map in a model.
    /// </summary>
    /// <param name="model">The model to modify.</param>
    /// <param name="materialIndex">The index of the material to modify.</param>
    /// <param name="mapIndex">The index of the material map to modify.</param>
    /// <param name="color">The new color to set.</param>
    public static unsafe void SetColor(ref Model model, int materialIndex, MaterialMapIndex mapIndex, Color color) {
        model.Materials[materialIndex].Maps[(int) mapIndex].Color = color;
    }

    /// <summary>
    /// Sets the value of a material map in a given material.
    /// </summary>
    /// <param name="material">The material whose map value needs to be set.</param>
    /// <param name="mapIndex">The index of the map in the material.</param>
    /// <param name="value">The new value to set for the map.</param>
    public static unsafe void SetValue(ref Material material, MaterialMapIndex mapIndex, float value) {
        material.Maps[(int) mapIndex].Value = value;
    }

    /// <summary>
    /// Sets the value of a material map in a model.
    /// </summary>
    /// <param name="model">A reference to the model.</param>
    /// <param name="materialIndex">The index of the material.</param>
    /// <param name="mapIndex">The index of the material map.</param>
    /// <param name="value">The value to set.</param>
    public static unsafe void SetValue(ref Model model, int materialIndex, MaterialMapIndex mapIndex, float value) {
        model.Materials[materialIndex].Maps[(int) mapIndex].Value = value;
    }
}