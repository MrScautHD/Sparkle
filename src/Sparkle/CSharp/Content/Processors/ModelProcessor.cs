using Raylib_CSharp;
using Raylib_CSharp.Geometry;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Shaders;
using Raylib_CSharp.Textures;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class ModelProcessor : IContentProcessor {

    public unsafe object Load<T>(IContentType<T> type) {
        Model model = Model.Load(type.Path);
        
        for (int i = 0; i < model.MeshCount; i++) {
            if (model.Meshes[i].TangentsPtr == default) {
                Mesh.GenTangents(ref model.Meshes[i]);
            }
        }

        foreach (Material material in model.Materials) {
            if (material.Shader.Id != RlGl.GetShaderIdDefault()) {
                if (Shader.IsReady(material.Shader)) {
                    Game.Instance.Content.AddUnmanagedItem(material.Shader);
                }
            }

            for (int i = 0; i < 12; i++) {
                MaterialMap map = material.Maps[i];

                if (map.Texture.Id != RlGl.GetTextureIdDefault()) {
                    if (Texture2D.IsReady(map.Texture)) {
                        Game.Instance.Content.AddUnmanagedItem(map.Texture);
                    }
                }
            }
        }
        
        return model;
    }
    
    public void Unload(object item) {
        Model.Unload((Model) item);
    }
}