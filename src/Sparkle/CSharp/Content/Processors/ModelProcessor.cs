using Raylib_CSharp.Geometry;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Unsafe.Spans.Data;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class ModelProcessor : IContentProcessor {

    private Dictionary<Model, ReadOnlySpanData<Material>> _materials;
    
    public ModelProcessor() {
        this._materials = new Dictionary<Model, ReadOnlySpanData<Material>>();
    }
    
    public unsafe object Load<T>(IContentType<T> type) {
        Model model = Model.Load(type.Path);

        for (int i = 0; i < model.MeshCount; i++) {
            if (model.Meshes[i].Tangents == default) {
                Mesh.GenTangents(ref model.Meshes[i]);
            }
        }
        
        this._materials.Add(model, new ReadOnlySpanData<Material>(model.MaterialsPtr, model.MaterialCount));
        return model;
    }
    
    public void Unload(object item) {
        Model model = (Model) item;

        if (!this._materials.TryGetValue(model, out ReadOnlySpanData<Material>? materials)) {
            Logger.Warn($"Unable to locate materials for type: [{model}]!");
        }
        else {
            foreach (Material material in materials.GetSpan()) {
                Material.Unload(material);
            }
            
            model.MaterialCount = 0;
        }

        Model.Unload(model);
    }
}