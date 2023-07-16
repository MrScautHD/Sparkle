using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class MaterialHelper {
    
    public static Material LoadDefault() => Raylib.LoadMaterialDefault();
    public static unsafe Material* LoadMaterials(sbyte* fileName, int* materialCount) => Raylib.LoadMaterials(fileName, materialCount);
    public static bool IsReady() => Raylib.IsMaterialReady();
    public static void Unload(Material material) => Raylib.UnloadMaterial(material);

    public static void SetTexture(ref Material material, MaterialMapIndex mapType, Texture2D texture) => Raylib.SetMaterialTexture(ref material, mapType, texture);
}