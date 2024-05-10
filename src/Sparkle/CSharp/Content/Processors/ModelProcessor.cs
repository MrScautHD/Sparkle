using Raylib_CSharp.Geometry;
using Raylib_CSharp.Materials;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class ModelProcessor : IContentProcessor {

    private Dictionary<Model, Material[]> _materials;

    public ModelProcessor() {
        this._materials = new Dictionary<Model, Material[]>();
    }
    
    public unsafe object Load<T>(IContentType<T> type) {
        Model model = Model.Load(type.Path);

        for (int i = 0; i < model.MeshCount; i++) {
            if (model.Meshes[i].Tangents == default) {
                Mesh.GenTangents(ref model.Meshes[i]);
            }
        }

        ReadOnlySpan<Material> span = new ReadOnlySpan<Material>(model.MaterialsPtr, model.MaterialCount);
        Material[] materials = span.ToArray();
        this._materials.Add(model, materials);

        foreach (Material material in materials) {
            //Material.Unload(material);
        }
        
        return model;
    }
    
    public void Unload(object item) {
        Model model = (Model) item;

        if (!this._materials.TryGetValue(model, out Material[]? materials)) {
            Logger.Warn($"Unable to locate materials for type: [{model}]!");
        }
        else {
            foreach (Material material in materials) {
                Material.Unload(material);
            }
            
            model.MaterialCount = 0;
        }

        Model.Unload(model);
    }
}