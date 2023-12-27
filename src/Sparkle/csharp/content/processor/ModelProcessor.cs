using Raylib_cs;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics.helper;

namespace Sparkle.csharp.content.processor; 

public class ModelProcessor : IContentProcessor {

    public object Load<T>(IContentType<T> type) {
        return ModelHelper.Load(type.Path);
    }
    
    public unsafe void Unload(object item) {
        Model model = (Model) item;
        
        for (int i = 0; i < model.MaterialCount; i++) {
            MaterialHelper.Unload(model.Materials[i]);
        }
        
        ModelHelper.Unload((Model) item);
    }
}