using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.NetworkInformation;

namespace TapoMobileApp
{
    public interface ITapoService
    {
        
        List<int> ChangeState(int[] ports, bool toggleOnOrOff);
        Task<int[]> Scan();
    }
    public class TapoService : ITapoService
    {
        private SettingsService _settings;
        public TapoService(SettingsService settings)
        {
            _settings = settings;
        }

        public List<int> ChangeState(int[] ports, bool toggleOnOrOff)
        {
           
             var errors = new List<int>();
             Parallel.ForEach(ports, async port =>
             {
                 await LoginAndChangePrivacy(port, toggleOnOrOff, errors);
             });
            return errors;
        }

        private async Task LoginAndChangePrivacy(int port, bool toggleOnOrOff, List<int> errors)
        {
            var url = GetIPAddress(port);
            try
            {
                var stok = await DoLogin(url);
                if (string.IsNullOrEmpty(stok))
                    errors.Add(port);
                var changePricacy = await ChangePrivacy(url, stok, toggleOnOrOff);
                if (!changePricacy)
                    errors.Add(port);
            }
            catch (Exception)
            {
                errors.Add(port);
            }
        }

        private async Task<bool> ChangePrivacy(string url, string loginStok, bool toggleOnOrOff)
        {
            var obj = new PrivacyCall { method = "set", lens_mask = new LensMask { lens_mask_info = new LensMaskInfo { enabled = "off" } } };
            if (toggleOnOrOff)
            {
                obj.lens_mask.lens_mask_info.enabled = "on";
            }
            var ret = await DoTapoCommand<PrivacyCall>(url + "/stok=" + loginStok + @"/ds", obj);
            if (ret == null)
                return false;
            return true;
        }

        public async Task<int[]> Scan()
        {
            var result = new List<int>();
            var tasks = new List<Task>();
            for (var port = 2; port < 254; port++)
            {
                 tasks.Add(ScanPort(result, port));
            }
            await Task.WhenAll(tasks);
            return await Task.FromResult(result.ToArray());
        }

        private bool Ping(string url)
        {
            Ping p = new Ping();
            PingReply r;
            r = p.Send(url,1000);

            return r.Status == IPStatus.Success;
        }
        private async Task<bool> ScanPort(List<int> result, int port)
        {
            try
            {
                var url = "192.168.1." + port;
                if (!Ping(url))
                    return false;

                var login = await DoLogin("https://" + url);
                if (!string.IsNullOrEmpty(login))
                {
                    result.Add(port);
                    return true;
                }
            }
            catch (Exception e)
            {
            }
            return false;
        }
        private async Task<TapoResult> DoTapoCommand<TCall>(string url, TCall callObj)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                using (var http = new HttpClient(httpClientHandler) { Timeout = TimeSpan.FromSeconds(15) })
                {
                    var json = JsonConvert.SerializeObject(callObj);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        var result = await http.PostAsync(url, content);
                        if (!result.IsSuccessStatusCode)
                            return null;
                        var cont = await result.Content.ReadAsStringAsync();
                        var loginResult = JsonConvert.DeserializeObject<TapoResult>(cont);
                        return loginResult;
                    }
                    catch(Exception e)
                    {
                        return null;
                    }
                }
            }
        }
        private async Task<string> DoLogin(string url)
        {
            var obj = new LoginCall { method = "login", @params = new Params { hashed = true, password = GetPassword(), username = _settings.UserName } };
            var result = await DoTapoCommand<LoginCall>(url, obj);
            if (result == null)
                return null;
            return result.result.stok;
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
                return sb.ToString();
            }
        }
        private string GetPassword()
        {
            return CreateMD5(_settings.Password).ToUpper();
        }
        private string GetIPAddress(int port)
        {
            return "https://192.168.1." + port;
        }

    }
}
