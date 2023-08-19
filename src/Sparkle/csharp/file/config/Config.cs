using Newtonsoft.Json.Linq;

namespace Sparkle.csharp.file.config; 

public struct Config {
    
    public string Directory { get; }
    public string Name { get; }
    public string Path { get; }
    
    private readonly Dictionary<string, object> _dictionary;
    private readonly string _encryptKey;
    
    public Config(string directory, string name, Dictionary<string, object> dictionary, string encryptKey = "") {
        this.Directory = directory;
        this.Name = name;
        this.Path = FileManager.GetPath(this.Directory, $"{this.Name}.json");
        this._dictionary = dictionary;
        this._encryptKey = encryptKey;
    }
    
    /// <summary>
    /// Creates or updates a configuration file with the values in the current dictionary.
    /// </summary>
    /// <returns>The Config instance representing the created or updated configuration.</returns>
    public Config Create() {
        FileManager.CreateFile(this.Directory, $"{this.Name}.json");
        
        if (FileManager.IsJsonValid(this.Path, this._encryptKey)) {
            foreach (var jsonObjectPair in FileManager.ReadJsonAsObject(this.Path, this._encryptKey)) {
                if (!this._dictionary.ContainsKey(jsonObjectPair.Key)) {
                    JObject jsonObject = FileManager.ReadJsonAsObject(this.Path, this._encryptKey);
                    jsonObject.Remove(jsonObjectPair.Key);
                    
                    File.WriteAllText(this.Path, FileManager.EncryptString(jsonObject.ToString(), this._encryptKey));
                    Logger.Warn($"Value {jsonObjectPair.Key} removed from file {this.Name}");
                }
            }
            
            foreach (var dictionary in this._dictionary) {
                if (!FileManager.ReadJsonAsObject(this.Path, this._encryptKey).ContainsKey(dictionary.Key)) {
                    JObject jsonObject = FileManager.ReadJsonAsObject(this.Path, this._encryptKey);

                    jsonObject[dictionary.Key] = JToken.FromObject(dictionary.Value);
                    File.WriteAllText(this.Path, FileManager.EncryptString(jsonObject.ToString(), this._encryptKey));
                    Logger.Info($"File {this.Name} updated: {dictionary.Key} = {dictionary.Value}");
                }
            }
        }
        else {
            FileManager.ClearFile(this.Path);
            FileManager.WriteJson(this._dictionary, this.Path, this._encryptKey);
            Logger.Warn($"Re/Wrote {this.Name}");
        }

        return this;
    }
    
    /// <summary>
    /// Retrieves a value of a specific type from a collection of values stored as JToken objects.
    /// </summary>
    /// <param name="name">The name of the value to retrieve.</param>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <returns>The retrieved value of type T if found; otherwise, the default value or null.</returns>
    public T GetValue<T>(string name) {
        if (!this.GetAllValues().TryGetValue(name, out JToken? jToken)) {
            Logger.Error($"Unable to locate value [{name}]!");
        }
        
        return jToken!.Value<T>()!;
    }

    /// <summary>
    /// Retrieves all values as a JObject from a JSON file using the specified path and encryption key.
    /// </summary>
    /// <returns>A JObject containing all values from the JSON file.</returns>
    public JObject GetAllValues() {
        return FileManager.ReadJsonAsObject(this.Path, this._encryptKey);
    }
}