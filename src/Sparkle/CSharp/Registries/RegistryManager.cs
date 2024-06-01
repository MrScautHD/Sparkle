using Sparkle.CSharp.Content;
using Sparkle.CSharp.Logging;

namespace Sparkle.CSharp.Registries;

public static class RegistryManager {
    
    internal static List<Registry> RegisterTypes = new();

    /// <summary>
    /// Adds a Registry type to the registry system for managing registered objects.
    /// </summary>
    /// <param name="type">The Registry object representing a type to be added to the system.</param>
    public static void AddType(Registry type) {
        if (RegisterTypes.All(registry => registry.GetType() != type.GetType())) {
            Logger.Info($"Added RegisterType: {type.GetType().Name}");
            RegisterTypes.Add(type);
        }
        else {
            Logger.Error($"Unable to add RegisterType: {type.GetType().Name}");
        }
    }
    
    /// <summary>
    /// Used for loading resources.
    /// </summary>
    internal static void Load(ContentManager content) {
        foreach (Registry registry in RegisterTypes) {
            registry.Load(content);
        }
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    internal static void Init() {
        foreach (Registry registry in RegisterTypes) {
            registry.Init();
        }
    }
}