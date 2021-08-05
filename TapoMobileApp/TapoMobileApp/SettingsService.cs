using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TapoMobileApp
{
    public interface ISettingsService
    {
        string UserName { get; }
        string Password { get; }

    }
    public class SettingsService : ISettingsService
    {
        public string UserName { get; private set; }
        public string Password { get; private set; }
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
        private string CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToUpper();
            }
        }
    }
}
