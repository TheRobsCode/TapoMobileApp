using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace TapoMobileApp
{

    public class CryptoServices 
    {
        public static string GenerateNonce()
        {
            var nonce = RandomNumberGenerator.GetBytes(8);
            //using (var rng = new RNGCryptoServiceProvider())

            return BitConverter.ToString(nonce).Replace("-", "").ToUpper();
        }
        public static string GetPassword(string password, string nonce, string cnonce)
        {
            var pass = password.ToUpper();
            return getHashSha256(pass + cnonce + nonce).ToUpper();
        }
        public static string getHashSha256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            var hashstring = SHA256.Create(); //new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        private static byte[] GenerateEncryptionToken(string password, string tokenType, string cnonce, string nonce)
        {
            var pass = password.ToUpper();
            var hashedKey = getHashSha256(cnonce + pass + nonce).ToUpper();
            byte[] tokenBytes = Encoding.UTF8.GetBytes(tokenType + cnonce + nonce + hashedKey);
            return SHA256.HashData(tokenBytes).Take(16).ToArray();
        }

        public static void GenerateEncryptionTokens(string password, LoginCache data, out byte[] lsk, out byte[] ivb)
        {
            lsk = GenerateEncryptionToken(password, "lsk", data.CNonce, data.Nonce);
            ivb = GenerateEncryptionToken(password, "ivb", data.CNonce, data.Nonce);
        }

        public static string Encrypt(object obj, byte[] key, byte[] iv)
        {
            var json = Json.Serialize(obj);
            return Convert.ToBase64String(Encrypt(json, key, iv));
        }
        private static byte[] Encrypt(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            // Create a new AesManaged.
            using (var aes = Aes.Create())
            {
                // Create encryptor
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                // Create MemoryStream
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream
                    // to encrypt
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            // Return encrypted data
            return encrypted;
        }

        public static string GetTag(string password, LoginCache cache, object request)
        {
            var pass = getHashSha256(password.ToUpper() + cache.CNonce).ToUpper();
            var tag = getHashSha256(pass + Json.Serialize(request) + cache.Seq.ToString()).ToUpper();
            return tag;
        }

        public static string Decrypt(byte[] cipherText, byte[] lsk, byte[] ivb)
        {
            string plaintext = null;
            // Create AesManaged
            using (var aes = Aes.Create())
            {
                // Create a decryptor
                ICryptoTransform decryptor = aes.CreateDecryptor(lsk, ivb);
                // Create the streams used for decryption.
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    // Create crypto stream
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        // Read crypto stream
                        using (StreamReader reader = new StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }
    }
    /*
    public static class ByteExtensions
    {
        public static byte[] ToByteArray(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        public static string ToStringFromByteArray(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string FromBase64(this string str)
        {
            byte[] data = Convert.FromBase64String(str);
            string decodedString = System.Text.Encoding.UTF8.GetString(data);
            return decodedString;
        }
    }
    */
    public static class Json
    {
        public static string Serialize<T>(T obj)
        {
            //JsonSerializerOptions jso = new JsonSerializerOptions();
            //jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            //var ms = System.Text.Json.JsonSerializer.Serialize(obj, jso);
            //var net = JsonConvert.SerializeObject(obj);
            //if (ms != net)
            //    return net;

            return JsonConvert.SerializeObject(obj);
            //return JsonSerializer.Serialize(obj);
        }
        public static T Deserialize<T>(string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
            //return JsonSerializer.Deserialize<T>(s);
        }
    }
}