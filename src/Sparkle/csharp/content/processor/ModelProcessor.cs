using Raylib_cs;

namespace Sparkle.csharp.content.processor; 

public class ModelProcessor : IContentProcessor {

    public object Load(string path) {
        return Raylib.LoadModel(path);
    }
    
    public void Unload(object content) {
        Raylib.UnloadModel((Model) content);
    }
}