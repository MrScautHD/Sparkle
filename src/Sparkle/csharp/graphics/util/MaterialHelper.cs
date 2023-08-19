using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class MaterialHelper {
    
    /// <summary> See <see cref="Raylib.LoadMaterialDefault"/> </summary>
    public static Material LoadDefault() => Raylib.LoadMaterialDefault();
    
    /// <summary> See <see cref="Raylib.LoadMaterials"/> </summary>
    public static unsafe Material* LoadMaterials(sbyte* fileName, int* materialCount) => Raylib.LoadMaterials(fileName, materialCount);
    
    /// <summary> See <see cref="Raylib.IsMaterialReady"/> </summary>
    public static bool IsReady() => Raylib.IsMaterialReady();
    
    /// <summary> See <see cref="Raylib.UnloadMaterial"/> </summary>
    public static void Unload(Material material) => Raylib.UnloadMaterial(material);

    
    /// <summary> See <see cref="Raylib.SetMaterialTexture(ref Material, MaterialMapIndex, Texture2D)"/> </summary>
    public static void SetTexture(ref Material material, MaterialMapIndex mapType, Texture2D texture) => Raylib.SetMaterialTexture(ref material, mapType, texture);
}