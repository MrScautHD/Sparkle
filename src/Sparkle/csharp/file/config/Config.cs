using Newtonsoft.Json.Linq;

namespace Sparkle.csharp.file.config; 

public struct Config {
    
    private Dictionary<string, object> _dictionary;
    
    public string Directory { get; }
    public string Name { get; }
    
    private string _encryptKey;
    
    public string Path => FileManager.GetPath(this.Directory, $"{this.Name}.json");

    public Config(string directory, string name, Dictionary<string, object> dictionary, string encryptKey = "") {
        this.Directory = directory;
        this.Name = name;
        this._dictionary = dictionary;
        this._encryptKey = encryptKey;
    }
    
    public Config Create() {
        FileManager.CreateFile(this.Directory, $"{this.Name}.json");
        
        if (FileManager.IsJsonValid(this.Path, this._encryptKey)) {
            foreach (var jsonObjectPair in FileManager.ReadJsonAsObject(this.Path, this._encryptKey)) {
                if (!this._dictionary.ContainsKey(jsonObjectPair.Key)) {
                    JObject jsonObject = FileManager.ReadJsonAsObject(this.Path, this._encryptKey);
                    jsonObject.Remove(jsonObjectPair.Key);
                    
                    File.WriteAllText(this.Path, FileManager.EncryptString(jsonObject.ToString(), this._encryptKey));
                    Logger.Warn($"Value {jsonObjectPair.Key} + get removed! In file {this.Name}");
                }
            }
            
            foreach (var dictionary in this._dictionary) {
                if (!FileManager.ReadJsonAsObject(this.Path, this._encryptKey).ContainsKey(dictionary.Key)) {
                    JObject jsonObject = FileManager.ReadJsonAsObject(this.Path, this._encryptKey);

                    jsonObject[dictionary.Key] = JToken.FromObject(dictionary.Value);
                    File.WriteAllText(this.Path, FileManager.EncryptString(jsonObject.ToString(), this._encryptKey));
                    Logger.Info($"File {this.Name} + added: {dictionary.Key} = {dictionary.Value}");
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
    
    public T GetValue<T>(string name) {
        return this.GetAllValues().Value<T>(name)!;
    }

    public JObject GetAllValues() {
        return FileManager.ReadJsonAsObject(this.Path, this._encryptKey);
    }
}