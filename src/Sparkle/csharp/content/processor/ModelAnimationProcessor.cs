using Raylib_cs;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics.helper;

namespace Sparkle.csharp.content.processor; 

public class ModelAnimationProcessor : IContentProcessor {
    
    public unsafe object Load(IContentType type, string directory) {
        uint count = 0;
        ModelAnimation* animation = ModelHelper.LoadAnimations(directory + type.Path, ref count);
        
        ModelAnimation[] animations = new ModelAnimation[count];
        
        for (int i = 0; i < count; i++) {
            animations[i] = animation[i];
        }

        return animations;
    }

    public unsafe void Unload(object item) {
        ModelAnimation[] animations = (ModelAnimation[]) item;

        fixed (ModelAnimation* animation = animations) {
            ModelHelper.UnloadAnimations(animation, (uint) animations.Length);
        }
    }
}