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
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    internal static void Update() {
        foreach (Gif gif in Gifs) {
            if (gif.HasInitialized) {
                gif.Update();
            }
        }
    }
    
    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    internal static void AfterUpdate() {
        foreach (Gif gif in Gifs) {
            if (gif.HasInitialized) {
                gif.AfterUpdate();
            }
        }
    }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    internal static void FixedUpdate() {
        foreach (Gif gif in Gifs) {
            if (gif.HasInitialized) {
                gif.FixedUpdate();
            }
        }
    }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    internal static void Draw() {
        foreach (Gif gif in Gifs) {
            if (gif.HasInitialized) {
                gif.Draw();
            }
        }
    }
    
    /// <summary>
    /// Adds an gif to the list of gifs.
    /// </summary>
    /// <param name="gif">The gif to be added.</param>
    public static void Add(Gif gif) {
        if (Gifs.Contains(gif)) {
            Logger.Warn($"The Gif with the texture ID [{gif.Texture.Id}] is already present in the GifManager!");
            return;
        }
        
        if (HasInitialized) {
            gif.Init();
        }
        
        Logger.Info($"Added Gif with texture ID [{gif.Texture.Id}] successfully.");
        Gifs.Add(gif);
    }
}