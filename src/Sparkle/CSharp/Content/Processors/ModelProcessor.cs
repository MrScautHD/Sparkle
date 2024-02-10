using Raylib_cs;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Content.Processors; 

public class ModelProcessor : IContentProcessor {

    public unsafe object Load<T>(IContentType<T> type) {
        Model model = ModelHelper.Load(type.Path);

        for (int i = 0; i < model.MeshCount; i++) {
            if (model.Meshes[i].Tangents == default) {
                Raylib.GenMeshTangents(ref model.Meshes[i]);
            }
        }
        
        return model;
    }
    
    public void Unload(object item) { // TODO UNLOAD MATERIAL SHADER AND TEXTURES
        ModelHelper.Unload((Model) item);
    }
}