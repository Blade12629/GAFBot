using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GAFStreamTool.Encryption
{
    public static class AESEncryption
    {
        #region encryption
        private static byte[] _encK;
        public static byte[] EncryptionKey
        {
            get
            {
                if (_encK == null)
                    LoadEncryptionKey();

                return _encK;
            }
        }

        private static void LoadEncryptionKey()
        {
            if (!File.Exists("enc.key"))
                GenerateEncryptionKey();

            using (FileStream fstream = File.OpenRead("enc.key"))
            {
                _encK = new byte[fstream.Length];
                fstream.Read(_encK, 0, _encK.Length);
            }
        }

        private static void GenerateEncryptionKey()
        {
            using (Aes aes = Aes.Create())
            {
                aes.IV = new byte[16];
                aes.Padding = PaddingMode.None;

                aes.GenerateKey();

                using (FileStream fstream = File.OpenWrite("enc.key"))
                {
                    fstream.Write(aes.Key, 0, aes.Key.Length);
                    fstream.Flush();
                }
            }
        }

        public static string EncryptString(string plainInput)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainInput);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static byte[] EncryptBytes(byte[] decrypted)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.Padding = PaddingMode.None;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (BinaryWriter writer = new BinaryWriter((Stream)cryptoStream))
                        {
                            writer.Write(decrypted);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return array;
        }

        public static byte[] DecryptBytes(byte[] encrypted)
        {
            byte[] iv = new byte[16];
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV = iv;
                aes.Padding = PaddingMode.None;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream(encrypted))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (BinaryReader reader = new BinaryReader((Stream)cryptoStream))
                        {
                            long length = reader.BaseStream.Length;
                            byte[] result = new byte[length];

                            reader.BaseStream.Read(result, 0, result.Length);

                            return result;
                        }
                    }
                }
            }
        }

        public static string DecryptString(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        #endregion
    }
}
