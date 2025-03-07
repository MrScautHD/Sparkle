using Bliss.CSharp.Logging;
using Sparkle.CSharp.Content;

namespace Sparkle.CSharp.Registries;

public static class RegistryManager {
    
    internal static List<Registry> RegisterTypes = new();
    
    public static bool HasLoaded { get; private set; }
    public static bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Used for loading resources.
    /// </summary>
    internal static void Load(ContentManager content) {
        foreach (Registry registry in RegisterTypes) {
            registry.Load(content);
        }
        
        HasLoaded = true;
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    internal static void Init() {
        foreach (Registry registry in RegisterTypes) {
            registry.Init();
        }
        
        HasInitialized = true;
    }

    /// <summary>
    /// Adds a Registry type to the registry system for managing registered objects.
    /// </summary>
    /// <param name="type">The Registry object representing a type to be added to the system.</param>
    public static void Add(Registry type) {
        if (RegisterTypes.Any(registry => registry.GetType() == type.GetType())) {
            Logger.Warn($"The registry type [{type.GetType().Name}] is already present in the RegistryManager!");
            return;
        }

        if (HasLoaded || HasInitialized) {
            Logger.Warn($"Add the registry type [{type.GetType().Name}] before loading and initializing the game in the Game.OnRun() method!");
            return;
        }
        
        Logger.Info($"Added RegisterType: {type.GetType().Name}");
        RegisterTypes.Add(type);
    }

    /// <summary>
    /// Removes a Registry type from the registry system.
    /// </summary>
    /// <param name="type">The Registry object representing a type to be removed from the system.</param>
    public static void Remove(Registry type) {
        type.Dispose();
    }
    
    /// <summary>
    /// Performs cleanup operations.
    /// </summary>
    public static void Destroy() {
        foreach (Registry registry in RegisterTypes.ToList()) {
            registry.Dispose();
        }
    }
}