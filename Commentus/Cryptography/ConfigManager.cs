using Commentus.MVVM.ViewModels;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Commentus.Cryptography
{
    public static class ConfigManager
    {
        private static string path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName;
        private static string fileName = Path.Combine(path, "dbconnstring.bin");
        public static byte[] EncryptConfig(string config)
        {
            byte[] encryptedConfig;
            using (Aes aesAlg = Aes.Create())
            {
                byte[] key = Encoding.UTF8.GetBytes("Enter your key here");
                Array.Resize(ref key, 16);
                byte[] iv = Encoding.UTF8.GetBytes("Enter your IV here");
                Array.Resize(ref iv, 16);
                aesAlg.Key = key;
                aesAlg.IV = iv;

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(config);
                        }
                        encryptedConfig = msEncrypt.ToArray();
                    }
                }
            }

            return encryptedConfig;
        }

        public static string DecryptConfig(byte[] config)
        {
            string decryptedConfig = null;

            using (Aes aesAlg = Aes.Create())
            {
                byte[] key = Encoding.UTF8.GetBytes("Enter your key here");
                Array.Resize(ref key, 16);
                byte[] iv = Encoding.UTF8.GetBytes("Enter your IV here");
                Array.Resize(ref iv, 16);
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(config))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            decryptedConfig = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return decryptedConfig;
        }

        public static void SaveConfig(string config)
        {
            if (!File.Exists(fileName))
            {
                byte[] encrypted = ConfigManager.EncryptConfig(config);
                File.WriteAllBytes(fileName, encrypted);
            }
        }

        public static void LoadConfig()
        {
            if (File.Exists(fileName))
            {
                byte[] connstring = File.ReadAllBytes(fileName);
                string decrypted = ConfigManager.DecryptConfig(connstring);

                try
                {
                    MainViewModel.Instance.DbConnection = new MySqlConnection(decrypted);
                    MainViewModel.Instance.DbConnection.Open();
                }
                catch (Exception)
                { return; }
            }
        }
    }
}
