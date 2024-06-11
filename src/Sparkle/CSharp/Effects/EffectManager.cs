using Sparkle.CSharp.Logging;

namespace Sparkle.CSharp.Effects;

public static class EffectManager {
    
    internal static List<Effect> Effects = new();
    
    public static bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    internal static void Init() {
        foreach (Effect effect in Effects) {
            if (!effect.HasInitialized) {
                effect.Init();
            }
        }
        HasInitialized = true;
    }
    
    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    internal static void Update() {
        foreach (Effect effect in Effects) {
            if (effect.HasInitialized) {
                effect.Update();
            }
        }
    }
    
    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    internal static void AfterUpdate() {
        foreach (Effect effect in Effects) {
            if (effect.HasInitialized) {
                effect.AfterUpdate();
            }
        }
    }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    internal static void FixedUpdate() {
        foreach (Effect effect in Effects) {
            if (effect.HasInitialized) {
                effect.FixedUpdate();
            }
        }
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    internal static void Draw() {
        foreach (Effect effect in Effects) {
            if (effect.HasInitialized) {
                effect.Draw();
            }
        }
    }
    
    /// <summary>
    /// Adds an effect to the list of effects.
    /// </summary>
    /// <param name="effect">The effect to be added.</param>
    public static void Add(Effect effect) {
        if (Effects.Contains(effect)) {
            Logger.Warn($"The Effect with the shader ID [{effect.Shader.Id}] is already present in the EffectManager!");
            return;
        }
        
        if (HasInitialized) {
            if (!effect.HasInitialized) {
                effect.Init();
            }
        }
        
        Logger.Info($"Added Effect with shader ID [{effect.Shader.Id}] successfully.");
        Effects.Add(effect);
    }
    
    /// <summary>
    /// Performs cleanup operations.
    /// </summary>
    public static void Destroy() {
        foreach (Effect effect in Effects.ToList()) {
            effect.Dispose();
        }
    }
}