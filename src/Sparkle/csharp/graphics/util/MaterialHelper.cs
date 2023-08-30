using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

#if !HEADLESS
public static class MaterialHelper {
    
    /// <inheritdoc cref="Raylib.LoadMaterialDefault"/>
    public static Material LoadDefault() => Raylib.LoadMaterialDefault();
    
    /// <inheritdoc cref="Raylib.LoadMaterials"/>
    public static unsafe Material* LoadMaterials(sbyte* fileName, int* materialCount) => Raylib.LoadMaterials(fileName, materialCount);
    
    /// <inheritdoc cref="Raylib.IsMaterialReady"/>
    public static bool IsReady() => Raylib.IsMaterialReady();
    
    /// <inheritdoc cref="Raylib.UnloadMaterial"/>
    public static void Unload(Material material) => Raylib.UnloadMaterial(material);

    
    /// <inheritdoc cref="Raylib.SetMaterialTexture(ref Material, MaterialMapIndex, Texture2D)"/>
    public static void SetTexture(ref Material material, MaterialMapIndex mapType, Texture2D texture) => Raylib.SetMaterialTexture(ref material, mapType, texture);
}
#endif