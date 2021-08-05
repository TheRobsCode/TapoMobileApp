using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace TapoMobileApp
{
    public interface ISettingsService
    {
        string UserName { get; }
        string Password { get; }
    }

    public class SettingsService : ISettingsService
    {
        public SettingsService()
        {
            // Get the assembly this code is executing in
            var assembly = Assembly.GetExecutingAssembly();

            // Look up the resource names and find the one that ends with settings.json
            // Your resource names will generally be prefixed with the assembly's default namespace
            // so you can short circuit this with the known full name if you wish
            var resName = assembly.GetManifestResourceNames()
                ?.FirstOrDefault(r => r.EndsWith("settings.json", StringComparison.OrdinalIgnoreCase));

            // Load the resource file
            using (var file = assembly.GetManifestResourceStream(resName))
            {
                using (var sr = new StreamReader(file))
                {
                    var json = sr.ReadToEnd();

                    var j = JObject.Parse(json);

                    UserName = j.Value<string>("userName");
                    Password = CreateMD5(j.Value<string>("password"));
                }
            }
        }

        public string UserName { get; }
        public string Password { get; }

        private string CreateMD5(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++) sb.Append(hashBytes[i].ToString("X2"));
                return sb.ToString().ToUpper();
            }
        }
    }
}