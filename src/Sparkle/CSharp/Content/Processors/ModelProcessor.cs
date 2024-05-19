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
            if (model.Meshes[i].TangentsPtr == default) {
                Mesh.GenTangents(ref model.Meshes[i]);
            }
        }
        
        this._materials.Add(model, new ReadOnlySpanData<Material>(model.MaterialsPtr, model.MaterialCount));
        return model;
    }
    
    // TODO Store TEXTURES, SHADERS, MAPS, PARMS in a own list and unload them instead the material because the material lists bound by a pointer and by repleacing the items they get not unloaded!
    
    public void Unload(object item) {
        Model model = (Model) item;

        if (!this._materials.TryGetValue(model, out ReadOnlySpanData<Material>? materials)) {
            Logger.Warn($"Unable to locate materials for type: [{model}]!");
        }
        else {
            foreach (Material material in materials.GetSpan()) {
                Logger.Error(material.Shader.Id + "");
                
                // TODO By stoping the App the game crash because this method but no error code is to see, so you dont unload the materials anymore just the resources that get loaded by loading the model.
                //Material.Unload(material);
            }
            
            model.MaterialCount = 0;
        }
        
        Logger.Error("AFTER");


        Model.Unload(model);
    }
}