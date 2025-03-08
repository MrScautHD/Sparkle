using Bliss.CSharp.Textures;
using MiniAudioEx;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content.Processors;

public class AudioClipProcessor : IContentProcessor {
    
    public object Load<T>(GraphicsDevice graphicsDevice, IContentType<T> type) {
        AudioClipContent contentType = (AudioClipContent) type;
        return new AudioClip(contentType.Path, contentType.StreamFromDisk);
    }

    public void Unload(object item) {
        ((Texture2D) item).Dispose();
    }
}