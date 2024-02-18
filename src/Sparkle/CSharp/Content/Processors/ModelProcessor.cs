using Raylib_cs;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Content.Processors; 

public class ModelProcessor : IContentProcessor {
    
    public unsafe object Load<T>(IContentType<T> type) {
        Model model = ModelHelper.Load(type.Path);

        for (int i = 0; i < model.MeshCount; i++) {
            if (model.Meshes[i].Tangents == default) {
                MeshHelper.GenTangents(ref model.Meshes[i]);
            }
        }
        
        Material[] materials = new Material[model.MaterialCount];

        for (int i = 0; i < model.MaterialCount; i++) {
            materials[i] = model.Materials[i];
        }
        
        Game.Instance.Content.AddUnmanagedContent(materials);
        
        return model;
    }
    
    public void Unload(object item) {
        ModelHelper.Unload((Model) item);
    }
}