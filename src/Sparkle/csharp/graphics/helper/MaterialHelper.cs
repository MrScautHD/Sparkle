using Raylib_cs;

namespace Sparkle.csharp.graphics.helper; 

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
    
    /// <inheritdoc cref="Raylib.SetMaterialTexture(ref Material, MaterialMapIndex, Texture2D)"/>
    public static void SetTexture(ref Material material, MaterialMapIndex mapType, Texture2D texture) => Raylib.SetMaterialTexture(ref material, mapType, texture);
    
    /// <inheritdoc cref="Raylib.SetMaterialTexture(ref Model, int, MaterialMapIndex, ref Texture2D)"/>
    public static void SetTexture(ref Model model, int materialIndex, MaterialMapIndex mapIndex, ref Texture2D texture) => Raylib.SetMaterialTexture(ref model, materialIndex, mapIndex, ref texture);
    
    /// <inheritdoc cref="Raylib.SetMaterialShader"/>
    public static void SetShader(ref Model model, int materialIndex, ref Shader shader) => Raylib.SetMaterialShader(ref model, materialIndex, ref shader);
}