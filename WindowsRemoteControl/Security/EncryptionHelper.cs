using System.Security.Cryptography;
using System.Text;

namespace WindowsRemoteControl.Security;

public static class EncryptionHelper
{
    private static byte[] GenerateKey(string password)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public static string Encrypt(string plainText, string password)
    {
        byte[] key = GenerateKey(password);
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        
        // Generate random IV
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        byte[] encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
        
        // Combine IV and encrypted data
        byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);
        
        return Convert.ToBase64String(result);
    }

    public static string Decrypt(string cipherText, string password)
    {
        byte[] key = GenerateKey(password);
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        
        // Extract IV from beginning of ciphertext
        byte[] iv = new byte[aes.IV.Length];
        byte[] encryptedData = new byte[cipherBytes.Length - aes.IV.Length];
        
        Buffer.BlockCopy(cipherBytes, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(cipherBytes, iv.Length, encryptedData, 0, encryptedData.Length);
        
        aes.IV = iv;
        
        using var decryptor = aes.CreateDecryptor();
        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
