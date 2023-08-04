using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sparkle.csharp.file; 

public static class FileManager {
    
    public static void CreateFile(string directory, string name, bool overwrite = false) {
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        if (overwrite || !File.Exists(GetPath(directory, name))) {
            File.Create(GetPath(directory, name)).Close();
        }
    }
    
    public static void WriteLine(object? text, string path, string key = "") {
        using (StreamWriter writer = new StreamWriter(path, true)) {
            writer.WriteLine(EncryptString(text?.ToString()!, key));
        }
    }
    
    public static void WriteJson<T>(T obj, string path, string key = "") {
        WriteLine(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj)), path, key);
    }

    public static string[] ReadAllLines<T>(string path) {
        return File.ReadAllLines(path);
    }

    public static JObject ReadJsonAsObject(string path, string key = "") {
        return JObject.Parse(DecryptString(File.ReadAllText(path), key));
    }
    
    public static string GetPath(string directory, string name) {
        return Path.Combine(directory, name);
    }
    
    public static void ClearFile(string path) {
        File.WriteAllText(path, string.Empty);
    }
    
    public static bool IsJsonValid(string path, string key = "") {
        try {
            ReadJsonAsObject(path, key);
            return true;
        } catch (Exception) {
            return false;
        }
    }

    public static string EncryptString(string text, string key) {
        if (key == String.Empty) {
            return text;
        }
        
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = new byte[16];

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        using var streamWriter = new StreamWriter(cryptoStream);
        streamWriter.Write(text);
        
        return Convert.ToBase64String(memoryStream.ToArray(), Base64FormattingOptions.InsertLineBreaks);
    }
    
    public static string DecryptString(string text, string key) {
        if (key == String.Empty) {
            return text;
        }
        
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = new byte[16];
                
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream(Convert.FromBase64String(text));
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);
        
        return streamReader.ReadToEnd();
    }
}