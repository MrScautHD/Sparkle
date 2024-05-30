using Raylib_CSharp;
using Raylib_CSharp.Geometry;
using Raylib_CSharp.Materials;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class ModelProcessor : IContentProcessor {

    public unsafe object Load<T>(IContentType<T> type) {
        ModelContent contentType = (ModelContent) type;
        Model model = Model.Load(contentType.Path);
        
        for (int i = 0; i < model.MeshCount; i++) {
            if (model.Meshes[i].TangentsPtr == default) {
                model.Meshes[i].GenTangents();
            }
        }

        foreach (Material material in model.Materials) {
            if (material.Shader.Id != RlGl.GetShaderIdDefault()) {
                if (material.Shader.IsReady()) {
                    Game.Instance.Content.AddUnmanagedItem(material.Shader);
                }
            }

            for (int i = 0; i < 12; i++) {
                MaterialMap map = material.Maps[i];

                if (map.Texture.Id != RlGl.GetTextureIdDefault()) {
                    if (map.Texture.IsReady()) {
                        Game.Instance.Content.AddUnmanagedItem(map.Texture);
                    }
                }
            }
        }
        
        contentType.Manipulator?.Build(ref model);
        return model;
    }
    
    public void Unload(object item) {
        ((Model) item).Unload();
    }
}