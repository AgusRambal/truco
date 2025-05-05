using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class AESHelper
{
    // Clave y vector fijos para encriptar/desencriptar (puedes cambiarlos)
    private static readonly string key = "B8zP1rM2vXf9QaW7tKjN6dE4UyHb0RZ3"; // Debe ser de 16, 24 o 32 caracteres
    private static readonly string iv = "L9mB2vC8xR5zT1qW"; // 16 caracteres sí o sí

    public static string Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                    swEncrypt.Flush(); // <<< FLUSH el stream
                    csEncrypt.FlushFinalBlock(); // <<< Cerrar bien el CryptoStream
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }


    public static string Decrypt(string cipherText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (var srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }
}
