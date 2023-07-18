using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace TapoMobileApp
{
    public interface ITapoHttpClient
    {
        Task<(bool success,TResult tapoResult)> DoTapoCommand<TResult, TCall>(int port, TCall callObj) where TCall : ICall
            where TResult : IResult;
        Task<string> DoLogin(int port, bool useCache);
        Task<string> DoLogin(int port);
        event EventHandler<TapoServiceEvent> OnChanged;

    }

    public class TapoHttpClient : ITapoHttpClient
    {
        public event EventHandler<TapoServiceEvent> OnChanged;

        private readonly ISettingsService _settings;
        private readonly IStoredProperties _storedProperties;

        public TapoHttpClient(ISettingsService settingsService, IStoredProperties storedProperties)
        {
            _settings = settingsService;
            _storedProperties = storedProperties;
        }


        public async Task<string> DoLogin(int port)
        {
            string stok = null;
            var useCache = true;

            for (var retry =0;retry < 10; retry++)
            { 
                stok = await DoLogin(port, useCache);
                useCache = false;
                if (string.IsNullOrEmpty(stok))
                    continue;
                return stok;
            }
            return stok;
        }

        public async Task<string> DoLogin(int port, bool useCache)
        {
            var stok = GetStokFromCache(port, useCache);
            if (!string.IsNullOrEmpty(stok))
                return stok;

            var url = GetIPAddress(port);
            var obj = new LoginCall
            {
                method = "login",
                @params = new Params { hashed = true, password = _settings.Password, username = _settings.UserName }
            };
            //OnChanged.Invoke(this, new TapoServiceEvent { Port = port, Message = "Starting " + obj.Call() });
            RaiseOnChangeEvent(port, "Starting " + obj.Call());
            var tapoComand = await DoTapoCommandImp<TapoResult, LoginCall>(url, obj);
            if (!tapoComand.success)
                return null;
            StoreInCache(tapoComand.result.result.stok, port);
            return tapoComand.result.result.stok;
        }

        public async Task<(bool success,TResult tapoResult)> DoTapoCommand<TResult, TCall>(int port, TCall callObj) where TCall : ICall
            where TResult : IResult
        {
            await CheckOnWifi(port);

            var useCache = true;
            (bool success, TResult result) ret = (success: false, result: default);
            for (var retryNum = 1; retryNum < 10; retryNum++)
            {
                try
                {
                    var stok = await DoLogin(port, useCache);
                    useCache = false;
                    var url = GetIPAddress(port) + "/stok=" + stok + @"/ds"; //"/stok=" + loginStok + @"/ds"
                    RaiseOnChangeEvent(port, "Starting " + callObj.Call() + $"({retryNum})");

                    ret = await DoTapoCommandImp<TResult, TCall>(url, callObj);

                    if (ret.success)
                    {
                        RaiseOnChangeEvent(port, callObj.Call() + " " + ret.result.Result());
                        return ret;
                    }
                    RaiseOnChangeEvent(port, "Error " + callObj.Call());
                }
                catch (Exception ex)
                {
                    RaiseOnChangeEvent(port, "Error " + callObj.Call());
                }

            }
            return ret;
        }

        private void RaiseOnChangeEvent(int port, string message)
        {
            if (OnChanged == null)
                return;
            OnChanged.Invoke(this, new TapoServiceEvent { Port = port, Message = message });
        }

        private string GetIPAddress(int port)
        {
            return "https://192.168.1." + port;
        }

        private void StoreInCache(string stok, int port)
        {
            var cacheProp = "CacheProp" + port;
            _storedProperties.Set(cacheProp, new LoginCache {Stok = stok, ExpiryDate = DateTime.Now.AddMinutes(60)});
        }

        private string GetStokFromCache(int port, bool useCache)
        {
            var cacheProp = "CacheProp" + port;
            if (!useCache)
                return null;
            if (!_storedProperties.ContainsKey(cacheProp))
                return null;
            var prop = _storedProperties.Get<LoginCache>(cacheProp);
            if (prop == null)
                return null;
            if (prop.ExpiryDate < DateTime.Now)
                return null;
            return prop.Stok;
        }

        protected virtual async Task<(bool success,TResult result)> DoTapoCommandImp<TResult, TCall>(string url, TCall callObj) where TCall : ICall
            where TResult : IResult
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    return true;
                };

                using (var http = new HttpClient(httpClientHandler) {Timeout = TimeSpan.FromSeconds(15)})
                {
                    var json = JsonConvert.SerializeObject(callObj);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        var result = await http.PostAsync(url, content);
                        if (!result.IsSuccessStatusCode)
                            return default;
                        var cont = await result.Content.ReadAsStringAsync();
                        var loginResult = JsonConvert.DeserializeObject<TResult>(cont);
                        return (loginResult.IsSuccess() ,loginResult);
                    }
                    catch (Exception e)
                    {
                        return (false, default);
                    }
                }
            }
        }

        protected virtual async Task CheckOnWifi(int port)
        {
            var waitingChars = new[] { '/', '-', '\\', '-' };
            int i=0;
            while (true)
            {
                var profiles = Connectivity.ConnectionProfiles;
                if (profiles.Contains(ConnectionProfile.WiFi))
                {
                    return;
                }
                RaiseOnChangeEvent(port, waitingChars[i] + " Waiting For Wifi " + waitingChars[i]);
                i++;
                if (i >= waitingChars.Length)
                    i = 0;
                await Task.Delay(1000);
            }
        }
    }

    public class LoginCache
    {
        public string Stok { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}