using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TapoMobileApp
{
    public class SettingsService
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

                    UserName =j.Value<string>("userName");
                    Password = j.Value<string>("password");
                }

                   
            }
        }
    }
}
