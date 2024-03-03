namespace Sparkle.CSharp.IO.Configs.Json;

public class JsonConfigBuilder {
    
    private string _directory;
    private string _name;
    private string _encryptKey;
    
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
        return new JsonConfig(this._directory, this._name, this._values, this._encryptKey);
    }
}