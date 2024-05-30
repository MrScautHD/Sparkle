using Raylib_CSharp.Geometry;
using Raylib_CSharp.Unsafe.Spans.Data;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class ModelAnimationProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return new ReadOnlySpanData<ModelAnimation>(ModelAnimation.LoadAnimations(type.Path));
    }

    public void Unload(object item) {
        ModelAnimation.UnloadAnimations(((ReadOnlySpanData<ModelAnimation>) item).GetSpan());
    }
}