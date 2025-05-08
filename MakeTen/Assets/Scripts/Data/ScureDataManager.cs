using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;

public static class SecureDataManager
{
    private static readonly string key = "YOUR_32_CHAR_SECRET_KEY_HERE"; // 32 bytes
    private static readonly string iv = "YOUR_16_CHAR_IV_HERE";          // 16 bytes

    public static void SaveEncrypted<T>(string filename, T data)
    {
        string json = JsonUtility.ToJson(data);
        byte[] encrypted = EncryptStringToBytes(json, key, iv);
        File.WriteAllBytes(GetPath(filename), encrypted);
    }

    public static T LoadEncrypted<T>(string filename)
    {
        string path = GetPath(filename);
        if (!File.Exists(path))
            return default;

        byte[] encrypted = File.ReadAllBytes(path);
        string json = DecryptStringFromBytes(encrypted, key, iv);
        return JsonUtility.FromJson<T>(json);
    }

    private static string GetPath(string filename)
    {
        return Path.Combine(Application.persistentDataPath, filename);
    }

    private static byte[] EncryptStringToBytes(string plainText, string keyStr, string ivStr)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(keyStr);
            aes.IV = Encoding.UTF8.GetBytes(ivStr);
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                    sw.Write(plainText);
                return ms.ToArray();
            }
        }
    }

    private static string DecryptStringFromBytes(byte[] cipherText, string keyStr, string ivStr)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(keyStr);
            aes.IV = Encoding.UTF8.GetBytes(ivStr);
            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(cipherText))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
                return sr.ReadToEnd();
        }
    }
}
