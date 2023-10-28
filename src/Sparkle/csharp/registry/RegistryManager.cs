namespace Sparkle.csharp.registry; 

public class RegistryManager {
    
    private static List<Registry> _registerTypes = new();
    
    /// <summary>
    /// Retrieves a list of registered types in the registry system.
    /// </summary>
    /// <returns>A list of registered types represented by Registry objects.</returns>
    public static List<Registry> GetTypes() {
        return _registerTypes;
    }

    /// <summary>
    /// Adds a Registry type to the registry system for managing registered objects.
    /// </summary>
    /// <param name="type">The Registry object representing a type to be added to the system.</param>
    public static void AddType(Registry type) {
        Logger.Debug($"Added RegisterType: {type.GetType().Name}");
        _registerTypes.Add(type);
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    internal static void Init() {
        foreach (Registry registry in GetTypes()) {
            registry.Init();
        }
    }
    
    /// <summary>
    /// Used for loading resources.
    /// </summary>
    internal static void Load() {
        foreach (Registry registry in GetTypes()) {
            registry.Load();
        }
    }
}