using System.Security.Cryptography;
using System.Text;

namespace Sparkle.CSharp.IO;

public class CryptoProvider {
    
    /// <summary>
    /// Encrypts a string using AES encryption with the provided key.
    /// </summary>
    /// <param name="text">The text to be encrypted.</param>
    /// <param name="key">The encryption key.</param>
    /// <returns>The encrypted text as a Base64-encoded string.</returns>
    public static string Encrypt(string text, string key) {
        if (key == string.Empty) {
            return text;
        }
        
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = new byte[16];

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        
        using MemoryStream memoryStream = new MemoryStream();
        using CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        using StreamWriter streamWriter = new StreamWriter(cryptoStream);
        streamWriter.Write(text);
        
        return Convert.ToBase64String(memoryStream.ToArray(), Base64FormattingOptions.InsertLineBreaks);
    }
    
    /// <summary>
    /// Decrypts a Base64-encoded string using AES decryption with the provided key.
    /// </summary>
    /// <param name="text">The encrypted text to be decrypted.</param>
    /// <param name="key">The decryption key.</param>
    /// <returns>The decrypted text.</returns>
    public static string Decrypt(string text, string key) {
        if (key == string.Empty) {
            return text;
        }
        
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = new byte[16];
        
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(text));
        using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader streamReader = new StreamReader(cryptoStream);
        
        return streamReader.ReadToEnd();
    }
}