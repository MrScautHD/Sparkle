namespace Sparkle.csharp.file.config; 

public class ConfigBuilder {
    
    private Dictionary<string, object> _dictionary = new();

    private string _directory;
    private string _name;
    private string _encryptKey;
    
    public ConfigBuilder(string directory, string name, string encryptKey = "") {
        this._directory = directory;
        this._name = name;
        this._encryptKey = encryptKey;
    }
    
    /// <summary>
    /// Adds a key-value pair to the configuration dictionary being built.
    /// </summary>
    /// <param name="key">The key to add to the dictionary.</param>
    /// <param name="obj">The value to associate with the key.</param>
    /// <returns>The ConfigBuilder instance with the added key-value pair.</returns>
    public ConfigBuilder Add(string key, object obj) {
        this._dictionary.Add(key, obj);
        return this;
    }

    /// <summary>
    /// Builds and creates a configuration using the specified directory, name, dictionary, and encryption key.
    /// </summary>
    /// <returns>The created Config instance representing the built configuration.</returns>
    public Config Build() {
        return new Config(this._directory, this._name, this._dictionary, this._encryptKey).Create();
    }
}