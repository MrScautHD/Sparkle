using Raylib_CSharp.Materials;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class MaterialProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        ReadOnlySpan<Material> span = Material.Load(type.Path);
        Material[] materials = span.ToArray();

        foreach (Material material in span) {
            Material.Unload(material);
        }

        return materials;
    }

    public void Unload(object item) { }
}