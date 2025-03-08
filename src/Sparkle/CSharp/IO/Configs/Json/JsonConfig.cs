using Bliss.CSharp.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sparkle.CSharp.IO.Configs.Json;

public class JsonConfig {
    
    /// <summary>
    /// Gets the directory where the JSON configuration file is stored.
    /// </summary>
    public string Directory { get; private set; }
    
    /// <summary>
    /// Gets the name of the JSON configuration file, excluding the extension.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Gets the full file path of the JSON configuration file.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Stores default values for the JSON configuration file.
    /// These values are used to initialize the configuration or validate missing keys.
    /// </summary>
    private Dictionary<string, object> _defaultValues;

    /// <summary>
    /// Stores the encryption key used for encrypting and decrypting the JSON configuration file content.
    /// </summary>
    private string _encryptKey;

    /// <summary>
    /// Represents a JSON configuration file.
    /// </summary>
    public JsonConfig(string directory, string name, Dictionary<string, object> defaultValues, string encryptKey = "") {
        this.Name = name;
        this.Directory = directory;
        this.Path = FileAccessor.GetPath(this.Directory, $"{this.Name}.json");
        this._defaultValues = defaultValues;
        this._encryptKey = encryptKey;
        this.Setup();
    }

    /// <summary>
    /// Performs the initial setup of the JSON configuration file.
    /// </summary>
    private void Setup() {
        FileAccessor.Create(this.Directory, $"{this.Name}.json");

        if (this.IsValid()) {
            foreach (var jsonObject in this.GetAllValues()) {
                if (!this._defaultValues.ContainsKey(jsonObject.Key)) {
                    this.RemoveValue(jsonObject.Key);
                }
            }

            foreach (var defaultValue in this._defaultValues) {
                if (!this.GetAllValues().ContainsKey(defaultValue.Key)) {
                    this.AddValue(defaultValue.Key, defaultValue.Value);
                }
            }
        }
        else {
            FileAccessor.Clear(this.Path);
            FileAccessor.WriteLine(this.Path, CryptoProvider.Encrypt(JsonConvert.SerializeObject(this._defaultValues, Formatting.Indented), this._encryptKey));
            Logger.Warn($"Successfully rewrote '{this.Name}'.");
        }
    }
    
    /// <summary>
    /// Checks if the JSON configuration file is valid.
    /// </summary>
    /// <returns>True if the JSON configuration file is valid, otherwise false.</returns>
    public bool IsValid() {
        try {
            this.GetAllValues();
            return true;
        }
        catch (Exception) {
            return false;
        }
    }

    /// <summary>
    /// Sets the value of a key in the JSON configuration file.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key associated with the value.</param>
    /// <param name="value">The new value to set.</param>
    public void SetValue<T>(string key, T value) {
        JObject values = this.GetAllValues();
        
        if (values.ContainsKey(key)) {
            values[key] = JToken.FromObject(value!);
            
            FileAccessor.WriteAll(this.Path, CryptoProvider.Encrypt(values.ToString(), this._encryptKey));
            Logger.Info($"Successfully updated the value '{key}' in the file '{this.Name}'.");
        }
        else {
            Logger.Warn($"Unable to update the value '{key}' in the file '{this.Name}'.");
        }
    }

    /// <summary>
    /// Adds a value to the JSON configuration file.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key associated with the value.</param>
    /// <param name="value">The value to be added.</param>
    public void AddValue<T>(string key, T value) {
        JObject values = this.GetAllValues();

        if (!values.ContainsKey(key)) {
            values.Add(key, JToken.FromObject(value!));
        
            FileAccessor.WriteAll(this.Path, CryptoProvider.Encrypt(values.ToString(), this._encryptKey));
            Logger.Info($"Successfully add the value '{key}' to the file '{this.Name}'.");
        }
        else {
            Logger.Warn($"Unable to add the value '{key}' to the file '{this.Name}'.");
        }
    }
    
    /// <summary>
    /// Removes a value from the JSON configuration file.
    /// </summary>
    /// <param name="key">The key of the value to be removed.</param>
    public void RemoveValue(string key) {
        JObject values = this.GetAllValues();

        if (values.ContainsKey(key)) {
            values.Remove(key);

            FileAccessor.WriteAll(this.Path, CryptoProvider.Encrypt(values.ToString(), this._encryptKey));
            Logger.Info($"Successfully removed the value '{key}' from the file '{this.Name}'.");
        }
        else {
            Logger.Warn($"Unable to remove the value '{key}' from the file '{this.Name}'.");
        }
    }
    
    /// <summary>
    /// Retrieves the value associated with the specified key from the JSON configuration file.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the specified key, or default(T) if the key is not found.</returns>
    public T? GetValue<T>(string key) {
        if (!this.GetAllValues().TryGetValue(key, out JToken? token)) {
            Logger.Error($"Unable to locate value '{key}' in the file '{this.Name}'!");
            return default;
        }

        return token.Value<T>()!;
    }

    /// <summary>
    /// Retrieves all values from the JSON configuration file.
    /// </summary>
    /// <returns> A JObject containing all the values from the JSON configuration file.</returns>
    /// <exception cref="Exception">Thrown if unable to retrieve the values from the file.</exception>
    public JObject GetAllValues() {
        return JObject.Parse(CryptoProvider.Decrypt(FileAccessor.ReadAll(this.Path), this._encryptKey));
    }
}