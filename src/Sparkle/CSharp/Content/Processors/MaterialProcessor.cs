using Raylib_cs;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Content.Processors;

public class MaterialProcessor : IContentProcessor {
    
    public unsafe object Load<T>(IContentType<T> type) {
        using AnsiBuffer path = type.Path.ToAnsiBuffer();
        int materialCount;
        
        Material* material = MaterialHelper.LoadMaterials(path.AsPointer(), &materialCount);
        
        Material[] materials = new Material[materialCount];

        for (int i = 0; i < materialCount; i++) {
            materials[i] = material[i];
        }
        
        return materials;
    }

    public void Unload(object item) {
        Material[] materials = (Material[]) item;
        
        foreach (Material material in materials) {
            MaterialHelper.Unload(material);
        }
    }
}