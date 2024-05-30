using Raylib_CSharp.Materials;
using Raylib_CSharp.Unsafe.Spans.Data;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class MaterialProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return new ReadOnlySpanData<Material>(Material.LoadMaterials(type.Path));
    }

    public void Unload(object item) {
        foreach (Material material in ((ReadOnlySpanData<Material>) item).GetSpan()) {
            material.Unload();
        }
    }
}