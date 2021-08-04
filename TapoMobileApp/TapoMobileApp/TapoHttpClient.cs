using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TapoMobileApp
{
    public interface ITapoHttpClient
    {
        Task<TResult> DoTapoCommand<TResult, TCall>(int port, TCall callObj);
        Task<string> DoLogin(int port, bool useCache);
    }
    public class TapoHttpClient : ITapoHttpClient
    {
        private ISettingsService _settings;
        private IStoredProperties _storedProperties;
        public TapoHttpClient(ISettingsService settingsService, IStoredProperties storedProperties)
        {
            _settings = settingsService;
            _storedProperties = storedProperties;
        }
        private string GetIPAddress(int port)
        {
            return "https://192.168.1." + port;
        }
        public async Task<string> DoLogin(int port, bool useCache)
        {
            var stok = GetStokFromCache(port, useCache);
            if (!string.IsNullOrEmpty(stok))
                return stok;

            var url = GetIPAddress(port);
            var obj = new LoginCall { method = "login", @params = new Params { hashed = true, password = _settings.Password, username = _settings.UserName } };
            var result = await DoTapoCommand<TapoResult, LoginCall>(url, obj);
            if (result == null)
                return null;
            StoreInCache(result.result.stok, port);
            return result.result.stok;
        }

        private void StoreInCache(string stok, int port)
        {
            var cacheProp = "CacheProp" + port;
            _storedProperties.Set(cacheProp, new LoginCache { Stok = stok, ExpiryDate = DateTime.Now.AddMinutes(60) });
        }

        private string GetStokFromCache(int port, bool useCache)
        {
            var cacheProp = "CacheProp" + port;
            if (!useCache)
                return null;
            if (!_storedProperties.ContainsKey(cacheProp))
                return null;
            if (_storedProperties.Get(cacheProp) == null)
                return null;
            var prop = (LoginCache)_storedProperties.Get(cacheProp);
            if (prop.ExpiryDate < DateTime.Now)
                return null;
            return prop.Stok;
        }
        protected virtual async Task<TResult> DoTapoCommand<TResult, TCall>(string url, TCall callObj)
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
                            return default(TResult);
                        var cont = await result.Content.ReadAsStringAsync();
                        var loginResult = JsonConvert.DeserializeObject<TResult>(cont);
                        return loginResult;
                    }
                    catch (Exception e)
                    {
                        return default(TResult);
                    }
                }
            }
        }
        public async Task<TResult> DoTapoCommand<TResult, TCall>(int port, TCall callObj)
        {
            TResult ret = default(TResult);
            foreach(var useCache in new[] { true,false})
            {
                var stok = await DoLogin(port, useCache);
                if (string.IsNullOrEmpty(stok))
                    continue;
                var url = GetIPAddress(port) + "/stok=" + stok + @"/ds"; //"/stok=" + loginStok + @"/ds"
                ret =  await DoTapoCommand<TResult, TCall>(url, callObj);
                if (ret != null)
                    return ret;
            }
            return ret;
        }

    }

    public class LoginCache
    {
        public string Stok { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
