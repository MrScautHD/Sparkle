using Raylib_CSharp.Geometry;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class ModelAnimationProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        ReadOnlySpan<ModelAnimation> span = ModelAnimation.Load(type.Path);
        ModelAnimation[] animations = span.ToArray();
        ModelAnimation.Unload(span);

        return animations;
    }

    public void Unload(object item) { }
}