using Raylib_cs;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Content.Processors; 

public class ModelAnimationProcessor : IContentProcessor {
    
    public unsafe object Load<T>(IContentType<T> type) {
        uint count = 0;
        ModelAnimation* animation = ModelHelper.LoadAnimations(type.Path, ref count);
        
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