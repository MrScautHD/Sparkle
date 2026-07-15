using Newtonsoft.Json.Linq;

namespace Sparkle.CSharp.IO.Json;

public class JsonConfig : JsonStorage {
    
    /// <summary>
    /// The default key-value pairs that define the expected schema of the configuration file.
    /// </summary>
    private readonly Dictionary<string, object> _defaultValues;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConfig"/> class, loading the existing file (if any), reconciling it against the provided default values, and persisting any resulting changes.
    /// </summary>
    /// <param name="name">The name of the JSON config file without the file extension.</param>
    /// <param name="directory">The directory in which the configuration file is stored.</param>
    /// <param name="defaultValues">The default key-value pairs used to populate missing entries.</param>
    /// <param name="encryptKey">The key used to encrypt/decrypt the file. Empty disables encryption.</param>
    /// <param name="removeUnknownValues">Whether to strip keys that are not part of the default schema.</param>
    public JsonConfig(string name, string directory, Dictionary<string, object> defaultValues, string encryptKey = "", bool removeUnknownValues = true) : base(name, directory, encryptKey) {
        this._defaultValues = defaultValues;
        
        // Load the current JSON data from file.
        JObject data = this.GetData();
        bool changed = false;
        
        // Remove keys that are no longer part of the default config schema.
        if (removeUnknownValues) {
            foreach (JProperty property in data.Properties().ToList()) {
                if (!this._defaultValues.ContainsKey(property.Name)) {
                    property.Remove();
                    changed = true;
                }
            }
        }
        
        // Add missing default values to the config file.
        foreach (KeyValuePair<string, object> defaultValue in this._defaultValues) {
            if (!data.ContainsKey(defaultValue.Key)) {
                data.Add(defaultValue.Key, JToken.FromObject(defaultValue.Value));
                changed = true;
            }
        }
        
        // Save only if something was actually changed.
        if (changed) {
            this.WriteData(data);
        }
    }
    
    /// <summary>
    /// Gets the default values used to initialize and validate this configuration.
    /// </summary>
    /// <returns>A read-only dictionary of the default key-value pairs.</returns>
    public IReadOnlyDictionary<string, object> GetDefaultValues() {
        return this._defaultValues;
    }
    
    /// <summary>
    /// Resets the configuration file back to its default values.
    /// </summary>
    public void Reset() {
        this.WriteData(JObject.FromObject(this._defaultValues));
    }
}