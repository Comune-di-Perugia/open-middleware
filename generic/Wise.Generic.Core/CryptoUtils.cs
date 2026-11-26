using System.IO;
using System.Security.Cryptography;

namespace Wise.Generic.Core;

public static class CryptoUtils
{
  static byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
  static byte[] iv = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
  
  public static byte[] EncryptData(string jsondata, byte[] k=null, byte[] v=null)
  {
    byte[] encrypted;
    
    byte[] usedKey = k != null ? k : key;
    byte[] usedIV = v != null ? v : iv;
    
    using (Aes aesAlg = Aes.Create())
    {
      aesAlg.Key = usedKey;
      aesAlg.IV = usedIV;

      ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

      using (MemoryStream msEncrypt = new MemoryStream())
      using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
      {
        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
          swEncrypt.Write(jsondata);

        encrypted = msEncrypt.ToArray();
      }
    }
    
    // Return the encrypted bytes from the memory stream.
    return encrypted;
  }
  
  public static string DecryptData(byte[] encrypted, byte[] k=null, byte[] v=null)
  {
    string plaintext = null;
    
    byte[] usedKey = k != null ? k : key;
    byte[] usedIV = v != null ? v : iv;
    
    using (Aes aesAlg = Aes.Create())
    {
      aesAlg.Key = key;
      aesAlg.IV = iv;
      ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

      using (MemoryStream msDecrypt = new MemoryStream(encrypted))
      using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
      using (StreamReader srDecrypt = new StreamReader(csDecrypt))
        plaintext = srDecrypt.ReadToEnd();
    }

    return plaintext;
  }
}