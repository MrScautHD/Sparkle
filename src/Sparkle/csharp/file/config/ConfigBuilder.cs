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
    
    public ConfigBuilder Add(string key, object obj) {
        this._dictionary.Add(key, obj);
        return this;
    }

    public Config Build() {
        return new Config(this._directory, this._name, this._dictionary, this._encryptKey).Create();
    }
}