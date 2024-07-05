using Sparkle.CSharp.Logging;

namespace Sparkle.CSharp.Rendering.Gifs;

public static class GifManager {
    
    internal static List<Gif> Gifs = new();
    
    public static bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Initializes the current scene and sets the main camera if available.
    /// </summary>
    internal static void Init() {
        foreach (Gif gif in Gifs) {
            if (!gif.HasInitialized) {
                gif.Init();
            }
        }
        
        HasInitialized = true;
    }
    
    /// <summary>
    /// Adds an gif to the list of gifs.
    /// </summary>
    /// <param name="gif">The gif to be added.</param>
    public static void Add(Gif gif) {
        if (Gifs.Contains(gif)) {
            Logger.Warn($"The Gif with the ID [{gif.Id}] is already present in the GifManager!");
            return;
        }
        
        if (HasInitialized) {
            if (!gif.HasInitialized) {
                gif.Init();
            }
        }
        
        Logger.Info($"Added Gif with ID [{gif.Id}] successfully.");
        Gifs.Add(gif);
    }
    
    /// <summary>
    /// Performs cleanup operations.
    /// </summary>
    public static void Destroy() {
        foreach (Gif gif in Gifs.ToList()) {
            gif.Dispose();
        }
    }
}