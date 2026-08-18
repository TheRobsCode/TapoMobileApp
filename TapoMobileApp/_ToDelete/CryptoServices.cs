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
            return BitConverter.ToString(nonce).Replace("-", "").ToUpperInvariant();
        }

        public static string GetPassword(string password, string nonce, string cnonce)
        {
            var upperPassword = password.ToUpperInvariant();
            return GetHashSha256($"{upperPassword}{cnonce}{nonce}").ToUpperInvariant();
        }

        public static string GetHashSha256(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }

        private static byte[] GenerateEncryptionToken(string password, string tokenType, string cnonce, string nonce)
        {
            var upperPassword = password.ToUpperInvariant();
            var hashedKey = GetHashSha256($"{cnonce}{upperPassword}{nonce}").ToUpperInvariant();
            var tokenBytes = Encoding.UTF8.GetBytes($"{tokenType}{cnonce}{nonce}{hashedKey}");
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

        private static byte[] Encrypt(string plainText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            using var encryptor = aes.CreateEncryptor(key, iv);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
                sw.Write(plainText);
            return ms.ToArray();
        }

        public static string GetTag(string password, LoginCache cache, object request)
        {
            var passHash = GetHashSha256(password.ToUpperInvariant() + cache.CNonce).ToUpperInvariant();
            var tag = GetHashSha256(passHash + Json.Serialize(request) + cache.Seq.ToString()).ToUpperInvariant();
            return tag;
        }

        public static string Decrypt(byte[] cipherText, byte[] lsk, byte[] ivb)
        {
            using var aes = Aes.Create();
            using var decryptor = aes.CreateDecryptor(lsk, ivb);
            using var ms = new MemoryStream(cipherText);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            return reader.ReadToEnd();
        }
    }

    public static class Json
    {
        public static string Serialize<T>(T obj) =>
            JsonConvert.SerializeObject(obj);

        public static T Deserialize<T>(string s) =>
            JsonConvert.DeserializeObject<T>(s);
    }
}