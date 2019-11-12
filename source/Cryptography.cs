using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace DomMan
{
    /// <summary>
    /// Provides methods to support file encryption and decryption functionality.
    /// </summary>
    public static class Cryptography
    {
        /// <summary>
        /// String of characters added to the end of encrypted files.
        /// </summary>
        private const string filePadding = "<![encrypted]/>";

        /// <summary>
        /// Size of the random salt.
        /// </summary>
        private const int saltSize = 8;

        /// <summary>
        /// RSACryptoServiceProvider variable.
        /// </summary>
        public static RSACryptoServiceProvider rsa;

        /// <summary>
        /// Instantiate the CspParameters class.
        /// </summary>
        public static CspParameters cspp = new CspParameters
        {
            KeyContainerName = "AppKey"
        };

        /// <summary>
        /// Instantiate the RijndaelManaged class.
        /// </summary>
        private static readonly RijndaelManaged rjndl = new RijndaelManaged
        {
            KeySize = 256,
            BlockSize = 256,
            Mode = CipherMode.CBC
        };

        /// <summary>
        /// Checks if the filePadding characters are the last characters within a file.
        /// </summary>
        /// <param name="filePath">Path of the file to check if encrypted.</param>
        /// <returns>true or false</returns>
        public static bool IsEncrypted(string filePath)
        {
            bool encrypted = false;
            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                sr.DiscardBufferedData();
                sr.BaseStream.Seek(-filePadding.Length, SeekOrigin.End);
                if (sr.ReadToEnd() == filePadding)
                {
                    encrypted = true;
                }
            }
            return encrypted;
        }

        /// <summary>
        /// Initializes an RSA object using the decrypted application key.
        /// </summary>
        /// <param name="filePath">Application key file path.</param>
        /// <param name="password">Password to decrypt the application key file.</param>
        /// <returns>The exception message or key_loaded if it was successful.</returns>
        public static string LoadKey(string filePath, string password)
        {
            if (File.Exists(filePath))
            {
                string xml = DecryptKey(filePath, password);
                XmlDocument xdoc = new XmlDocument();
                try
                {
                    xdoc.LoadXml(xml);
                }
                catch (XmlException x)
                {
                    return "Error loading application key file, the password may be incorrect.\n\n" + filePath + "\n\n" + x.Message;
                }

                if (xdoc.DocumentElement != null && xdoc.DocumentElement.Name == "RSAKeyValue")
                {
                    try
                    {
                        xdoc.LoadXml(xml);
                    }
                    catch (XmlException x)
                    {
                        return "Error loading application key file.\n\n" + filePath + "\n\n" + x.Message;
                    }

                    try
                    {
                        rsa.FromXmlString(xml);
                    }
                    catch (CryptographicException x)
                    {
                        return "RSA initialization error using the key file.\n\n" + filePath + "\n\n" + x.Message;
                    }
                }
            }
            return "key_loaded";
        }

        /// <summary>
        /// Uses the Rijndael class to decrypt the application key file.
        /// </summary>
        /// <param name="filePath">Application key file path.</param>
        /// <param name="password">Password to decrypt the application key file.</param>
        /// <returns>The decrypted string.</returns>
        public static string DecryptKey(string filePath, string password)
        {
            FileInfo fi = new FileInfo(filePath);

            bool padding = false;
            if (DomMan.Cryptography.IsEncrypted(filePath) == true)
            {
                padding = true;
                RemoveFilePadding(fi.FullName);
            }

            FileStream fs = fi.OpenRead();

            byte[] salt = new byte[saltSize];

            fs.Read(salt, 0, saltSize);

            // Initialize the algorithm with salt.
            Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(password, salt);
            Rijndael rijndael = Rijndael.Create();
            rijndael.IV = keyGenerator.GetBytes(rijndael.BlockSize / saltSize);
            rijndael.Key = keyGenerator.GetBytes(rijndael.KeySize / saltSize);

            CryptoStream cs = new CryptoStream(fs, rijndael.CreateDecryptor(), CryptoStreamMode.Read);

            keyGenerator.Dispose();
            rijndael.Dispose();

            string plaintext;
            try
            {
                using (StreamReader sr = new StreamReader(cs))
                {
                    plaintext = sr.ReadToEnd();
                }
            }
            catch (CryptographicException x)
            {
                fs.Close();
                if (padding == true)
                {
                    AddFilePadding(filePath);
                }
                return "CryptographicException\n\n" + x.Message;
            }

            if (padding == true)
            {
                AddFilePadding(fi.FullName);
            }

            return plaintext;
        }

        /// <summary>
        /// Uses the Rijndael class to encrypt the application key file.
        /// </summary>
        /// <param name="filePath">Application key file path.</param>
        /// <param name="password">Password for encrypting the application key file.</param>
        public static void EncryptKey(string filePath, string password)
        {
            FileInfo fi = new FileInfo(filePath);

            if (IsEncrypted(filePath) == true)
            {
                RemoveFilePadding(filePath);
            }

            var keyGenerator = new Rfc2898DeriveBytes(password, saltSize);
            var rijndael = Rijndael.Create();

            // BlockSize, KeySize in bit --> divide by saltSize.
            rijndael.IV = keyGenerator.GetBytes(rijndael.BlockSize / saltSize);
            rijndael.Key = keyGenerator.GetBytes(rijndael.KeySize / saltSize);

            string plaintext = File.ReadAllText(filePath);

            FileStream fs = fi.Create();

            // Write random salt.
            fs.Write(keyGenerator.Salt, 0, saltSize);

            CryptoStream cs = new CryptoStream(fs, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(Encoding.UTF8.GetBytes(plaintext), 0, plaintext.Length);
            cs.FlushFinalBlock();
            cs.Close();

            AddFilePadding(filePath);

            keyGenerator.Dispose();
            rijndael.Dispose();
        }

        /// <summary>
        /// Uses a CryptoStream object to read and decrypt the cipher text section of the FileStream encryption package, in blocks of bytes, into the FileStream object for the decrypted file.
        /// </summary>
        /// <param name="filePath">Path of the file to decrypt.</param>
        /// <returns>The decrypted text.</returns>
        public static string DecryptFile(string filePath)
        {
            // Create byte arrays to get the length of the encrypted key and IV.
            // These values were stored as 4 bytes each at the beginning of the encrypted package.
            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            bool padding = false;
            if (IsEncrypted(filePath) == true)
            {
                padding = true;

                RemoveFilePadding(filePath);
            }

            // Use FileStream objects to read the encrypted file (fileStream) and save the decrypted file (fs).
            string plaintext;
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(LenK, 0, 3);
                fs.Seek(4, SeekOrigin.Begin);
                fs.Read(LenIV, 0, 3);

                // Convert the lengths to integer values.
                int lenK = BitConverter.ToInt32(LenK, 0);
                int lenIV = BitConverter.ToInt32(LenIV, 0);

                // Determine the start position of the ciphter text (startC) and its length(lenC).
                int startC = lenK + lenIV + saltSize;
                int lenC = (int)fs.Length - startC;

                // Create the byte arrays for the encrypted Rijndael key, the IV, and the cipher text.
                byte[] keyEncrypted = new byte[lenK];
                byte[] IV = new byte[lenIV];

                // Extract the key and IV starting from index 8 after the length values.
                fs.Seek(saltSize, SeekOrigin.Begin);
                fs.Read(keyEncrypted, 0, lenK);
                fs.Seek(saltSize + lenK, SeekOrigin.Begin);
                fs.Read(IV, 0, lenIV);

                // Use RSACryptoServiceProvider to decrypt the Rijndael key.
                byte[] KeyDecrypted;

                if (rsa == null) { MessageBox.Show("null"); return "CryptographicException"; }

                try
                {
                    KeyDecrypted = rsa.Decrypt(keyEncrypted, false);
                }
                catch (CryptographicException x)
                {
                    fs.Close();
                    if (padding == true)
                    {
                        AddFilePadding(filePath);
                    }
                    return "CryptographicException\n\n" + x.Message;
                }

                // Decrypt the key.
                ICryptoTransform transform = rjndl.CreateDecryptor(KeyDecrypted, IV);

                int count = 0;
                int blockSizeBytes = rjndl.BlockSize / saltSize;
                byte[] data = new byte[blockSizeBytes];

                // Start at the beginning of the cipher text.
                fs.Seek(startC, SeekOrigin.Begin);

                MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                {
                    do
                    {
                        count = fs.Read(data, 0, blockSizeBytes);
                        cs.Write(data, 0, count);
                    }
                    while (count > 0);

                    cs.FlushFinalBlock();
                }

                // Convert the decrypted data from a MemoryStream to a byte array.
                byte[] plainBytes = ms.ToArray();

                // Convert the encrypted byte array to a base64 encoded string.
                plaintext = Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);
            }

            if (padding == true)
            {
                AddFilePadding(filePath);
            }

            return plaintext;
        }

        /// <summary>
        /// Uses a CryptoStream object to read and encrypt the FileStream of the source file, in blocks of bytes, into a destination FileStream object for the encrypted file.
        /// </summary>
        /// <param name="filePath">Path of the file to encrypt.</param>
        /// <param name="plaintext">The text to encrypt.</param>
        public static void EncryptFile(string filePath, string plaintext)
        {
            // Create instance of Rijndael for symetric encryption of the data.
            ICryptoTransform transform = rjndl.CreateEncryptor();

            if (plaintext == null)
            {
                plaintext = File.ReadAllText(filePath);
            }

            // Use RSACryptoServiceProvider to encrypt the Rijndael key.
            byte[] keyEncrypted = rsa.Encrypt(rjndl.Key, false);

            int lKey = keyEncrypted.Length;
            byte[] LenK = BitConverter.GetBytes(lKey);
            int lIV = rjndl.IV.Length;
            byte[] LenIV = BitConverter.GetBytes(lIV);

            FileStream fs = new FileStream(filePath, FileMode.Create);
            fs.Write(LenK, 0, 4); // length of the key
            fs.Write(LenIV, 0, 4); // length of the IV
            fs.Write(keyEncrypted, 0, lKey); // encrypted key
            fs.Write(rjndl.IV, 0, lIV); // the IV

            // Write the cipher text using a CryptoStream for encrypting.
            using (CryptoStream cs = new CryptoStream(fs, transform, CryptoStreamMode.Write))
            {
                cs.Write(Encoding.UTF8.GetBytes(plaintext), 0, plaintext.Length);
                cs.FlushFinalBlock();
            }

            AddFilePadding(filePath);
        }

        /// <summary>
        /// Writes the filePadding text to the end of an encrypted file.
        /// </summary>
        /// <param name="filePath">Path of the file to decrypt.</param>
        public static void AddFilePadding(string filePath)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.Write(filePadding);
            }
        }

        /// <summary>
        /// Removes the filePadding text from the end of an encrypted file.
        /// </summary>
        /// <param name="filePath">Path of the file to decrypt.</param>
        public static void RemoveFilePadding(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            fs.SetLength(fs.Length - filePadding.Length);
            fs.Close();
        }
    }
}