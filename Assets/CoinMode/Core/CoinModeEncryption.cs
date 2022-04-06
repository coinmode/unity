using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CoinMode
{
    public static class CoinModeEncryption
    {
        public enum KeySize
        {
            _128 = 0,
            _192 = 1,
            _256 = 2,
            _Any = 3
        }

        private static readonly int[] keyLengths = { 16, 24, 32 };

        private const string default256Key = "7CB568B474F8C276D7F3DF3A32CFD5BC";

        // const keyString = crypto.createHash("sha256").update(String(keyStringIn)).digest("base64").substr(0, 32);

        public static bool TryEncryptString(string text, string key, out string encryptedString)
        {            
            try
            {
                encryptedString = EncryptString(text, key);
                return true;
            }
            catch(CryptographicException ce)
            {
                CoinModeLogging.LogWarning("CoinModeEncryption", "TryEncryptString", ce.Message);
                encryptedString = null;
                return false;
            }
        }

        public static string EncryptString(string text, string key = null, KeySize enforcedKeySize = KeySize._Any)
        {
            if (key == null) key = default256Key;
            byte[] cipherData;
            Aes aes = Aes.Create();

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            if(enforcedKeySize != KeySize._Any && keyBytes.Length != keyLengths[(int)enforcedKeySize])
            {
                byte[] fixedKey;
                FixKey(keyBytes, out fixedKey, enforcedKeySize);
                aes.Key = fixedKey;
            }
            else
            {
                aes.Key = keyBytes;
            }
            
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            ICryptoTransform cipher = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, cipher, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(text);
                    }
                }

                cipherData = ms.ToArray();
            }

            byte[] combinedData = new byte[aes.IV.Length + cipherData.Length];
            Array.Copy(aes.IV, 0, combinedData, 0, aes.IV.Length);
            Array.Copy(cipherData, 0, combinedData, aes.IV.Length, cipherData.Length);
            return Convert.ToBase64String(combinedData);
        }

        public static bool TryDecryptString(string encryptedText, string key, out string decryptedString)
        {
            try
            {
                decryptedString = DecryptString(encryptedText, key);
                return true;
            }
            catch (CryptographicException ce)
            {
                CoinModeLogging.LogWarning("CoinModeEncryption", "TryDecryptString", ce.Message);
                decryptedString = null;
                return false;
            }
        }

        public static string DecryptString(string encryptedText, string key = null, KeySize enforcedKeySize = KeySize._Any)
        {
            if (key == null) key = default256Key;

            string plainText;
            byte[] combinedData = Convert.FromBase64String(encryptedText);
            Aes aes = Aes.Create();

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            if (enforcedKeySize != KeySize._Any && keyBytes.Length != keyLengths[(int)enforcedKeySize])
            {
                byte[] fixedKey;
                FixKey(keyBytes, out fixedKey, enforcedKeySize);
                aes.Key = fixedKey;
            }
            else
            {
                aes.Key = keyBytes;
            }

            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] cipherText = new byte[combinedData.Length - iv.Length];
            Array.Copy(combinedData, iv, iv.Length);
            Array.Copy(combinedData, iv.Length, cipherText, 0, cipherText.Length);
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            ICryptoTransform decipher = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, decipher, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        plainText = sr.ReadToEnd();
                    }
                }

                return plainText;
            }
        }

        public static string CreateHashedKey(string key, KeySize enforcedKeySize = KeySize._256)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                string s = Convert.ToBase64String(bytes);
                int length = enforcedKeySize != KeySize._Any ? keyLengths[(int)enforcedKeySize] : s.Length;
                return s.Substring(0, length);
            }
        }

        private static void FixKey(byte[] inKey, out byte[] outKey, KeySize enforcedKeySize)
        {
            int keyLength = keyLengths[(int)enforcedKeySize];
            outKey = new byte[keyLength];
            Array.Copy(inKey, outKey, outKey.Length > inKey.Length ? inKey.Length : outKey.Length);
        }
    }

}
