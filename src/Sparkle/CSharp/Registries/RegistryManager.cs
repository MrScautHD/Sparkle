using Bliss.CSharp.Logging;
using Sparkle.CSharp.Content;

namespace Sparkle.CSharp.Registries;

public static class RegistryManager {
    
    /// <summary>
    /// A list of all registered <see cref="Registry"/> instances.
    /// </summary>
    internal static List<Registry> RegistryTypes;
    
    /// <summary>
    /// Initializes the registry manager.
    /// </summary>
    internal static void Init() {
        RegistryTypes = new List<Registry>();
    }
    
    /// <summary>
    /// Loads content for all active registries using the given <see cref="ContentManager"/>.
    /// </summary>
    /// <param name="content">The content manager used to load assets.</param>
    internal static void OnLoad(ContentManager content) {
        foreach (Registry registry in RegistryTypes) {
            if (!registry.HasDisposed) {
                registry.Load(content);
            }
        }
    }

    /// <summary>
    /// Initializes all active registries.
    /// </summary>
    internal static void OnInit() {
        foreach (Registry registry in RegistryTypes) {
            if (!registry.HasDisposed) {
                registry.Init();
            }
        }
    }

    /// <summary>
    /// Attempts to add a new registry to the registry manager.
    /// </summary>
    /// <param name="type">The registry instance to be added.</param>
    /// <returns>True if the registry was successfully added; otherwise, false if a registry of the same type is already present.</returns>
    public static bool TryAdd(Registry type) {
        if (RegistryTypes.Any(registry => registry.GetType() == type.GetType())) {
            Logger.Warn($"The registry type [{type.GetType().Name}] is already present in the RegistryManager!");
            return false;
        }
        
        RegistryTypes.Add(type);
        return true;
    }

    /// <summary>
    /// Attempts to remove and dispose of the specified registry from the registry manager.
    /// </summary>
    /// <param name="type">The registry instance to be removed and disposed.</param>
    /// <returns>True if the registry was successfully removed and disposed; otherwise, false if the registry was not found in the manager.</returns>
    public static bool TryRemove(Registry type) {
        if (!RegistryTypes.Contains(type)) {
            Logger.Warn($"Failed to Remove/Dispose the registry type [{type.GetType().Name}] from the RegistryManager!");
            return false;
        }
        
        type.Dispose();
        return true;
    }

    /// <summary>
    /// Attempts to remove and dispose of the registry of the specified type from the registry manager.
    /// </summary>
    /// <param name="type">The type of the registry to be removed and disposed.</param>
    /// <returns>True if a registry of the specified type was successfully removed and disposed; otherwise, false.</returns>
    public static bool TryRemove(Type type) {
        Registry? registry = RegistryTypes.FirstOrDefault(registry => registry.GetType() == type);
    
        if (registry == null) {
            Logger.Warn($"Failed to Remove/Dispose the registry type [{type.Name}] from the RegistryManager!");
            return false;
        }
    
        registry.Dispose();
        return true;
    }

    /// <summary>
    /// Clears all registries.
    /// </summary>
    internal static void Destroy() {
        while (RegistryTypes.Count > 0) {
            Registry registry = RegistryTypes[0];
            registry.Dispose();
        }
    }
}