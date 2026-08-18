using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TapoMobileApp.Services.Configuration
{
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

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var j = JsonSerializer.Deserialize<Settings>(json, options);

                    UserName = j.UserName;
                    Password = j.Password; //CreateMD5(j.Password); - Store the password in settings.json already md5'ed
                    IpPrefix = j.IpPrefix ?? "192.168.1";
                }
            }
        }

        public string UserName { get; }
        public string Password { get; }
        public string IpPrefix { get; }

        private class Settings
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public string IpPrefix { get; set; }
        }

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
