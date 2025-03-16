using Bliss.CSharp.Textures;
using MiniAudioEx;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content.Processors;

public class AudioClipProcessor : IContentProcessor {
    
    /// <summary>
    /// Loads an audio clip from the specified path and returns the loaded audio clip object.
    /// </summary>
    /// <typeparam name="T">The type of content to load, specifically an audio clip.</typeparam>
    /// <param name="graphicsDevice">The graphics device used for managing content.</param>
    /// <param name="type">The content type descriptor that contains information about the audio clip, including its path and whether to stream it from disk.</param>
    /// <returns>An object representing the loaded audio clip.</returns>
    public object Load<T>(GraphicsDevice graphicsDevice, IContentType<T> type) {
        AudioClipContent contentType = (AudioClipContent) type;
        return new AudioClip(contentType.Path, contentType.StreamFromDisk);
    }

    /// <summary>
    /// Unloads and disposes the specified content item, freeing any resources it holds.
    /// </summary>
    /// <param name="item">The content item to be unloaded and disposed.</param>
    public void Unload(object item) {
        ((Texture2D) item).Dispose();
    }
}