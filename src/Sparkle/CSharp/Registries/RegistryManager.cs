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
    /// Attempts to add a new registry to the list of managed registries.
    /// </summary>
    /// <param name="registry">The registry to be added.</param>
    /// <returns>True if the registry is successfully added; otherwise, false if a registry of the same type already exists.</returns>
    public static bool TryAdd(Registry registry) {
        if (RegistryTypes.Any(reg => reg.GetType() == registry.GetType())) {
            Logger.Warn($"The registry type [{registry.GetType().Name}] is already present in the RegistryManager!");
            return false;
        }
        
        RegistryTypes.Add(registry);
        return true;
    }

    /// <summary>
    /// Attempts to remove a specified registry from the list of managed registries.
    /// </summary>
    /// <param name="registry">The registry to be removed.</param>
    /// <returns>True if the registry is successfully removed; otherwise, false if the registry does not exist in the list.</returns>
    public static bool TryRemove(Registry registry) {
        if (!RegistryTypes.Contains(registry)) {
            Logger.Warn($"Failed to Remove/Dispose the registry type [{registry.GetType().Name}] from the RegistryManager!");
            return false;
        }
        
        registry.Dispose();
        
        // Ensure the registry is removed, even if `Dispose` was overridden incorrectly.
        if (RegistryTypes.Contains(registry)) {
            RegistryTypes.Remove(registry);
        }
        
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
        
        // Ensure the registry is removed, even if `Dispose` was overridden incorrectly.
        if (RegistryTypes.Contains(registry)) {
            RegistryTypes.Remove(registry);
        }
        
        return true;
    }

    /// <summary>
    /// Clears all registries.
    /// </summary>
    internal static void Destroy() {
        for (int i = RegistryTypes.Count - 1; i >= 0; i--) {
            Registry registry = RegistryTypes[i];
            registry.Dispose();

            // Ensure the registry is removed, even if `Dispose` was overridden incorrectly.
            if (RegistryTypes.Contains(registry)) {
                RegistryTypes.RemoveAt(i);
            }
        }
    }
}