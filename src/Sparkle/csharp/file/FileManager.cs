using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sparkle.csharp.file; 

public static class FileManager {
    
    /// <summary>
    /// Creates a file with the specified name in the given directory.
    /// </summary>
    /// <param name="directory">The directory where the file will be created.</param>
    /// <param name="name">The name of the file to be created.</param>
    /// <param name="overwrite">Set to true to overwrite the file if it already exists, otherwise false.</param>
    public static void CreateFile(string directory, string name, bool overwrite = false) {
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        if (overwrite || !File.Exists(GetPath(directory, name))) {
            File.Create(GetPath(directory, name)).Close();
        }
    }
    
    /// <summary>
    /// Writes a line of text to a file at the specified path, optionally encrypted with a key.
    /// </summary>
    /// <param name="text">The text to be written to the file.</param>
    /// <param name="path">The path of the file where the text will be written.</param>
    /// <param name="key">An optional key used to encrypt the text. Leave empty for no encryption.</param>
    public static void WriteLine(object? text, string path, string key = "") {
        using StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(EncryptString(text?.ToString()!, key));
    }
    
    /// <summary>
    /// Serializes an object to JSON format and writes it to a file, optionally encrypted with a key.
    /// </summary>
    /// <typeparam name="T">The type of the object to be serialized.</typeparam>
    /// <param name="obj">The object to be serialized and written to the file.</param>
    /// <param name="path">The path of the file where the JSON data will be written.</param>
    /// <param name="key">An optional key used to encrypt the JSON data. Leave empty for no encryption.</param>
    public static void WriteJson<T>(T obj, string path, string key = "") {
        WriteLine(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj)), path, key);
    }

    /// <summary>
    /// Reads all lines from a text file and returns them as an array of strings.
    /// </summary>
    /// <param name="path">The path of the text file to read.</param>
    /// <returns>An array containing all the lines from the text file.</returns>
    public static string[] ReadAllLines<T>(string path) {
        return File.ReadAllLines(path);
    }

    /// <summary>
    /// Reads a JSON file, decrypts its contents if a key is provided, and returns the content as a JObject.
    /// </summary>
    /// <param name="path">The path of the JSON file to read.</param>
    /// <param name="key">An optional key used to decrypt the JSON content. Leave empty for no decryption.</param>
    /// <returns>A JObject containing the parsed JSON content.</returns>
    public static JObject ReadJsonAsObject(string path, string key = "") {
        return JObject.Parse(DecryptString(File.ReadAllText(path), key));
    }
    
    /// <summary>
    /// Combines a directory path and a file name to create a full file path.
    /// </summary>
    /// <param name="directory">The directory path.</param>
    /// <param name="name">The name of the file.</param>
    /// <returns>The full path combining the directory and file name.</returns>
    public static string GetPath(string directory, string name) {
        return Path.Combine(directory, name);
    }
    
    /// <summary>
    /// Clears the contents of a file by overwriting it with an empty string.
    /// </summary>
    /// <param name="path">The path of the file to be cleared.</param>
    public static void ClearFile(string path) {
        File.WriteAllText(path, string.Empty);
    }
    
    /// <summary>
    /// Checks whether a JSON file is valid by attempting to parse it as a JObject.
    /// </summary>
    /// <param name="path">The path of the JSON file to check.</param>
    /// <param name="key">An optional key used to decrypt the JSON content. Leave empty for no decryption.</param>
    /// <returns>True if the JSON is valid, otherwise false.</returns>
    public static bool IsJsonValid(string path, string key = "") {
        try {
            ReadJsonAsObject(path, key);
            return true;
        } catch (Exception) {
            return false;
        }
    }

    /// <summary>
    /// Encrypts a string using AES encryption with the provided key.
    /// </summary>
    /// <param name="text">The text to be encrypted.</param>
    /// <param name="key">The encryption key.</param>
    /// <returns>The encrypted text as a Base64-encoded string.</returns>
    public static string EncryptString(string text, string key) {
        if (key == string.Empty) {
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
    
    /// <summary>
    /// Decrypts a Base64-encoded string using AES decryption with the provided key.
    /// </summary>
    /// <param name="text">The encrypted text to be decrypted.</param>
    /// <param name="key">The decryption key.</param>
    /// <returns>The decrypted text.</returns>
    public static string DecryptString(string text, string key) {
        if (key == string.Empty) {
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