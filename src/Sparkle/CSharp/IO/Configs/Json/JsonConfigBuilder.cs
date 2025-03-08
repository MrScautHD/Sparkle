namespace Sparkle.CSharp.IO.Configs.Json;

public class JsonConfigBuilder {
    
    /// <summary>
    /// Represents the directory where the JSON configuration files are stored.
    /// </summary>
    private string _directory;

    /// <summary>
    /// Represents the name of the JSON configuration file.
    /// </summary>
    private string _name;

    /// <summary>
    /// Represents the encryption key used for securing the JSON configuration file.
    /// </summary>
    private string _encryptKey;

    /// <summary>
    /// Represents the key-value pairs used as configuration data in the JSON configuration builder.
    /// </summary>
    private Dictionary<string, object> _values;

    /// <summary>
    /// Represents a builder for creating JSON configurations.
    /// </summary>
    public JsonConfigBuilder(string directory, string name, string encryptKey = "") {
        this._directory = directory;
        this._name = name;
        this._encryptKey = encryptKey;
        this._values = new Dictionary<string, object>();
    }

    /// <summary>
    /// Adds a key-value pair to the JSON configuration builder.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value.</param>
    /// <param name="value">The value to be added.</param>
    /// <returns>The updated JSON configuration builder.</returns>
    public JsonConfigBuilder Add<T>(string key, T value) {
        this._values.Add(key, value!);
        return this;
    }

    /// <summary>
    /// Builds a JSON configuration file using the values added to the builder.
    /// </summary>
    /// <returns>The created JSON configuration file.</returns>
    public JsonConfig Build() {
        JsonConfig config = new JsonConfig(this._directory, this._name, this._values, this._encryptKey);
        this._values.Clear();
        
        return config;
    }
}