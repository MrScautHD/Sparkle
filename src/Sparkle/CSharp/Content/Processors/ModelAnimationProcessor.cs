using Raylib_CSharp.Geometry;
using Raylib_CSharp.Unsafe.Spans.Data;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class ModelAnimationProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return new ReadOnlySpanData<ModelAnimation>(ModelAnimation.Load(type.Path));
    }

    public void Unload(object item) {
        ModelAnimation.Unload(((ReadOnlySpanData<ModelAnimation>) item).GetSpan());
    }
}