using Bliss.CSharp.Logging;
using Sparkle.CSharp.Content;

namespace Sparkle.CSharp.Registries;

public static class RegistryManager {
    
    /// <summary>
    /// A list of all registered <see cref="Registry"/> instances.
    /// </summary>
    internal static List<Registry> Registries;
    
    /// <summary>
    /// Initializes the registry manager.
    /// </summary>
    internal static void Init() {
        Registries = new List<Registry>();
    }
    
    /// <summary>
    /// Loads content for all active registries using the given <see cref="ContentManager"/>.
    /// </summary>
    /// <param name="content">The content manager used to load assets.</param>
    internal static void OnLoad(ContentManager content) {
        foreach (Registry registry in Registries) {
            if (!registry.HasDisposed) {
                registry.Load(content);
            }
        }
    }

    /// <summary>
    /// Initializes all active registries.
    /// </summary>
    internal static void OnInit() {
        foreach (Registry registry in Registries) {
            if (!registry.HasDisposed) {
                registry.Init();
            }
        }
    }

    /// <summary>
    /// Adds the specified registry to the registry manager.
    /// </summary>
    /// <param name="registry">The registry to add.</param>
    public static void Add(Registry registry) {
        if (Registries.Contains(registry)) {
            Logger.Warn($"The registry [{registry.GetType().Name}] is already present in the RegistryManager!");
        }
        else {
            Registries.Add(registry);
        }
    }

    /// <summary>
    /// Removes the specified registry from the registry manager.
    /// </summary>
    /// <param name="registry">The registry to remove.</param>
    public static void Remove(Registry registry) {
        if (Registries.Contains(registry)) {
            Registries.Remove(registry);
        }
        else {
            Logger.Warn($"Failed to remove the registry [{registry.GetType().Name}] from the RegistryManager!");
        }
    }

    /// <summary>
    /// Clears all registries.
    /// </summary>
    internal static void Destroy() {
        Registries.Clear();
    }
}