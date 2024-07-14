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
    /// Removes the specified effect from the list of effects and disposes it.
    /// </summary>
    /// <param name="effect">The effect to be removed.</param>
    public static void Remove(Effect effect) {
        effect.Dispose();
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