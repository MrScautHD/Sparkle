using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sparkle.CSharp.IO.Json;

public class JsonStorage {
    
    /// <summary>
    /// Gets the name of the JSON storage file without the file extension.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Gets the directory where the JSON storage file is located.
    /// </summary>
    public string Directory { get; private set; }
    
    /// <summary>
    /// Gets the full path to the JSON storage file.
    /// </summary>
    public string Path { get; private set; }
    
    /// <summary>
    /// The encryption key used to encrypt and decrypt the JSON file content.
    /// </summary>
    private readonly string _encryptKey;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonStorage"/> class.
    /// Creates the JSON file if it does not exist and initializes it with an empty JSON object if needed.
    /// </summary>
    /// <param name="name">The name of the JSON storage file without the file extension.</param>
    /// <param name="directory">The directory where the JSON storage file is located.</param>
    /// <param name="encryptKey">The optional encryption key used for securing the file content.</param>
    public JsonStorage(string name, string directory, string encryptKey = "") {
        this.Name = name;
        this.Directory = directory;
        this.Path = FileAccessor.GetPath(this.Directory, $"{this.Name}.json");
        this._encryptKey = encryptKey;
        
        // Create file if not already exist.
        FileAccessor.Create(this.Directory, $"{this.Name}.json");
        
        // Init JSON storage.
        if (string.IsNullOrWhiteSpace(FileAccessor.ReadAll(this.Path)) || !this.IsValid()) {
            this.WriteData(new JObject());
        }
    }
    
    /// <summary>
    /// Checks whether the JSON storage file contains valid JSON content.
    /// </summary>
    /// <returns>True if the JSON file is valid, otherwise false.</returns>
    public bool IsValid() {
        try {
            this.GetData();
            return true;
        }
        catch {
            return false;
        }
    }
    
    /// <summary>
    /// Retrieves the root JSON object from the storage file.
    /// </summary>
    /// <returns>The root JSON object.</returns>
    public JObject GetData() {
        return JObject.Parse(CryptoProvider.Decrypt(FileAccessor.ReadAll(this.Path), this._encryptKey));
    }
    
    /// <summary>
    /// Writes the root JSON object to the JSON storage file.
    /// </summary>
    /// <param name="data">The root JSON object to write.</param>
    public void WriteData(JObject data) {
        FileAccessor.WriteAll(this.Path, CryptoProvider.Encrypt(data.ToString(Formatting.Indented), this._encryptKey));
    }
    
    /// <summary>
    /// Checks whether the JSON storage file contains the specified key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key exists, otherwise false.</returns>
    public bool ContainsKey(string key) {
        return this.GetData().ContainsKey(key);
    }
    
    /// <summary>
    /// Gets the value associated with the specified key.
    /// Returns default(T) if the key does not exist or the value cannot be converted.
    /// </summary>
    /// <typeparam name="T">The expected value type.</typeparam>
    /// <param name="key">The key of the value.</param>
    /// <returns>The converted value, or default(T) if it could not be retrieved.</returns>
    public T? GetValue<T>(string key) {
        if (!this.GetData().TryGetValue(key, out JToken? token)) {
            return default;
        }
        
        try {
            return token.ToObject<T>();
        }
        catch {
            return default;
        }
    }
    
    /// <summary>
    /// Attempts to get the value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The expected value type.</typeparam>
    /// <param name="key">The key of the value.</param>
    /// <param name="value">The converted value if successful.</param>
    /// <returns>True if the value was found and converted successfully, otherwise false.</returns>
    public bool TryGetValue<T>(string key, [NotNullWhen(true)] out T? value) {
        if (!this.GetData().TryGetValue(key, out JToken? token)) {
            value = default;
            return false;
        }
        
        try {
            value = token.ToObject<T>();
            return value != null;
        }
        catch {
            value = default;
            return false;
        }
    }
    
    /// <summary>
    /// Adds a value to the JSON storage file.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key associated with the value.</param>
    /// <param name="value">The value to add.</param>
    /// <exception cref="ArgumentException">Thrown when the key already exists.</exception>
    public void AddValue<T>(string key, T value) {
        if (!this.TryAddValue(key, value)) {
            throw new ArgumentException($"The value '{key}' already exists in the file '{this.Name}'.", nameof(key));
        }
    }
    
    /// <summary>
    /// Attempts to add a value to the JSON storage file.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key associated with the value.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>True if the value was added, otherwise false.</returns>
    public bool TryAddValue<T>(string key, T value) {
        JObject data = this.GetData();
        
        if (data.ContainsKey(key)) {
            return false;
        }
        
        data.Add(key, JToken.FromObject(value!));
        this.WriteData(data);
        return true;
    }
    
    /// <summary>
    /// Removes a value from the JSON storage file.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
    public void RemoveValue(string key) {
        if (!this.TryRemoveValue(key)) {
            throw new KeyNotFoundException($"Unable to locate value '{key}' in the file '{this.Name}'.");
        }
    }
    
    /// <summary>
    /// Attempts to remove a value from the JSON storage file.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <returns>True if the value was removed, otherwise false.</returns>
    public bool TryRemoveValue(string key) {
        JObject data = this.GetData();
        
        if (!data.ContainsKey(key)) {
            return false;
        }
        
        data.Remove(key);
        this.WriteData(data);
        return true;
    }
    
    /// <summary>
    /// Sets or creates the value of a key in the JSON storage file.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key associated with the value.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue<T>(string key, T value) {
        JObject values = this.GetData();
        values[key] = JToken.FromObject(value!);
        
        this.WriteData(values);
    }
}