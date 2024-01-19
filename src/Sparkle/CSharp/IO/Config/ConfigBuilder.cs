namespace Sparkle.CSharp.IO.Config; 

public class ConfigBuilder {
    
    private Dictionary<string, object> _dictionary = new();

    private string _directory;
    private string _name;
    private string _encryptKey;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigBuilder"/>, specifying the directory, name of the config file, and an optional encryption key.
    /// </summary>
    /// <param name="directory">The directory where the config file will be located.</param>
    /// <param name="name">The name of the config file without extension.</param>
    /// <param name="encryptKey">Optional encryption key for securing the config file. Default is an empty string, meaning no encryption.</param>
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
        return new Config(this._directory, this._name, this._dictionary, this._encryptKey);
    }
}