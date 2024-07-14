using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sparkle.CSharp.IO;
using Sparkle.CSharp.Logging;

namespace Sparkle.CSharp.Scenes.Saving;

public class StoreSettings : Disposable {

    public readonly string Directory;
    public readonly string Name;
    public readonly string Path;
    
    private Dictionary<string, object> _values;
    private string _encryptKey;

    public StoreSettings(string directory, string name, string encryptKey = "") {
        this.Directory = directory;
        this.Name = name;
        this.Path = FileAccessor.GetPath(this.Directory, $"{this.Name}.json");
        this._values = new Dictionary<string, object>();
        this._encryptKey = encryptKey;
    }
    
    private void Load() {
        FileAccessor.Create(this.Path, $"{this.Name}.json");

        if (this.IsValid()) {
            foreach (var jsonObject in this.GetAllValues()) {
                if (!this._values.TryAdd(jsonObject.Key, jsonObject.Value!)) {
                    Logger.Error($"Unable to add {jsonObject.Key}!");
                }
            }
        }
        else {
            FileAccessor.Clear(this.Path);
            FileAccessor.WriteLine(this.Path, CryptoProvider.Encrypt(JsonConvert.SerializeObject(this._values, Formatting.Indented), this._encryptKey));
            Logger.Warn($"Successfully rewrote '{this.Name}'.");
        }
    }

    public T GetData<T>(string key) {
        return (T) this._values[key];
    }

    public void SetData<T>(string key, T value) {
        this._values[key] = value!;
    }

    public void Save() {
        FileAccessor.Clear(this.Path);
        FileAccessor.WriteLine(this.Path, CryptoProvider.Encrypt(JsonConvert.SerializeObject(this._values, Formatting.Indented), this._encryptKey));
        Logger.Warn($"Successfully store '{this.Name}'.");
    }
    
    public bool IsValid() {
        try {
            this.GetAllValues();
            return true;
        }
        catch (Exception) {
            return false;
        }
    }
    
    public JObject GetAllValues() {
        return JObject.Parse(CryptoProvider.Decrypt(FileAccessor.ReadAll(this.Path), this._encryptKey));
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._values.Clear();
        }
    }
}