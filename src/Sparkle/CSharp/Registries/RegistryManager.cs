using Bliss.CSharp.Logging;
using Sparkle.CSharp.Content;

namespace Sparkle.CSharp.Registries;

public static class RegistryManager {
    
    /// <summary>
    /// A list of all registered <see cref="Registry"/> instances.
    /// </summary>
    internal static Dictionary<Type, Registry> Registries;
    
    /// <summary>
    /// Initializes the registry manager.
    /// </summary>
    internal static void Init() {
        Registries = new Dictionary<Type, Registry>();
    }
    
    /// <summary>
    /// Loads content for all active registries using the given <see cref="ContentManager"/>.
    /// </summary>
    /// <param name="content">The content manager used to load assets.</param>
    internal static void OnLoad(ContentManager content) {
        foreach (Registry registry in Registries.Values) {
            if (!registry.HasDisposed) {
                registry.Load(content);
            }
        }
    }

    /// <summary>
    /// Initializes all active registries.
    /// </summary>
    internal static void OnInit() {
        foreach (Registry registry in Registries.Values) {
            if (!registry.HasDisposed) {
                registry.Init();
            }
        }
    }

    /// <summary>
    /// Retrieves all registered <see cref="Registry"/> instances as an array.
    /// </summary>
    /// <returns>An array containing all registered <see cref="Registry"/> instances.</returns>
    public static Registry[] GetRegistries() {
        return Registries.Values.ToArray();
    }

    /// <summary>
    /// Determines whether a registry of the specified type exists in the registry manager.
    /// </summary>
    /// <typeparam name="T">The type of the registry to check for.</typeparam>
    /// <returns>True if a registry of the specified type exists; otherwise, false.</returns>
    public static bool HasRegistry<T>() where T : Registry {
        return GetRegistry<T>() != null;
    }

    /// <summary>
    /// Retrieves a registry of the specified type from the registry manager.
    /// </summary>
    /// <typeparam name="T">The type of the registry to retrieve.</typeparam>
    /// <returns>Returns an instance of the registry if found; otherwise, null.</returns>
    public static T? GetRegistry<T>() where T : Registry {
        if (!TryGetRegistry(out T? result)) {
            return null;
        }
        
        return result;
    }

    /// <summary>
    /// Attempts to retrieve a registry of the specified type from the registry manager.
    /// </summary>
    /// <typeparam name="T">The type of the registry to retrieve.</typeparam>
    /// <param name="registry">The output parameter that will contain the registry instance if found; otherwise, null.</param>
    /// <returns>Returns true if the registry is successfully located; otherwise, false.</returns>
    public static bool TryGetRegistry<T>(out T? registry) where T : Registry {
        if (!Registries.TryGetValue(typeof(T), out Registry? result)) {
            Logger.Error($"Unable to locate Registry for type [{typeof(T)}]!");
            registry = null;
            return false;
        }

        registry = (T) result;
        return true;
    }

    /// <summary>
    /// Adds a registry to the registry manager.
    /// </summary>
    /// <param name="registry">The registry instance to add to the manager.</param>
    public static void AddRegistry(Registry registry) {
        TryAddRegistry(registry);
    }

    /// <summary>
    /// Attempts to add a new registry to the registry manager.
    /// </summary>
    /// <param name="registry">The registry instance to add.</param>
    /// <returns>Returns true if the registry was successfully added; false if the registry type is already present.</returns>
    public static bool TryAddRegistry(Registry registry) {
        if (Registries.ContainsKey(registry.GetType())) {
            Logger.Error($"The registry type [{registry.GetType().Name}] is already present in the RegistryManager!");
            return false;
        }
        
        Registries.Add(registry.GetType(), registry);
        return true;
    }

    /// <summary>
    /// Removes a specified registry from the list of managed registries.
    /// </summary>
    /// <param name="registry">The registry to be removed.</param>
    public static void RemoveRegistry(Registry registry) {
        TryRemoveRegistry(registry);
    }

    /// <summary>
    /// Attempts to remove a specified registry from the list of managed registries.
    /// </summary>
    /// <param name="registry">The registry to be removed.</param>
    /// <returns>True if the registry is successfully removed; otherwise, false if the registry does not exist in the list.</returns>
    public static bool TryRemoveRegistry(Registry registry) {
        if (!Registries.ContainsKey(registry.GetType())) {
            Logger.Error($"Failed to Remove/Dispose the registry type [{registry.GetType().Name}] from the RegistryManager!");
            return false;
        }
        
        registry.Dispose();
        
        // Ensure the registry is removed, even if `Dispose` was overridden incorrectly.
        if (Registries.ContainsKey(registry.GetType())) {
            Registries.Remove(registry.GetType());
        }
        
        return true;
    }

    /// <summary>
    /// Removes a registry of the specified type from the registry manager.
    /// </summary>
    /// <typeparam name="T">The type of the registry to remove.</typeparam>
    public static void RemoveRegistry<T>() where T : Registry {
        TryRemoveRegistry<T>();
    }

    /// <summary>
    /// Attempts to remove the specified registry type from the list of managed registries.
    /// </summary>
    /// <typeparam name="T">The type of the registry to remove.</typeparam>
    /// <returns>True if the registry was successfully removed; otherwise, false.</returns>
    public static bool TryRemoveRegistry<T>() where T : Registry {
        if (!Registries.TryGetValue(typeof(T), out Registry? registry)) {
            Logger.Error($"Failed to Remove/Dispose the registry type [{typeof(T).Name}] from the RegistryManager!");
            return false;
        }
    
        registry.Dispose();
        
        // Ensure the registry is removed, even if `Dispose` was overridden incorrectly.
        if (Registries.ContainsKey(registry.GetType())) {
            Registries.Remove(registry.GetType());
        }
        
        return true;
    }

    /// <summary>
    /// Clears all registries.
    /// </summary>
    internal static void Destroy() {
        var enumerator = Registries.GetEnumerator();

        while (enumerator.MoveNext()) {
            Registry registry = enumerator.Current.Value;
            registry.Dispose();
            
            // Ensure the registry is removed, even if `Dispose` was overridden incorrectly.
            if (Registries.ContainsKey(registry.GetType())) {
                Registries.Remove(registry.GetType());
            }
        }
    }
}