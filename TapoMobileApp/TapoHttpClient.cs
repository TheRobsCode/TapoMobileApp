
using System.Text;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Net.Http;

namespace TapoMobileApp
{
    public interface ITapoHttpClient
    {
        Task<TResult> DoTapoCommand<TResult, TCall>(int port, TCall callObj) where TCall : ICall
            where TResult : IResult;
        Task<LoginCache> DoLogin(int port, bool useCache);
        Task<LoginCache> DoLogin(int port);
        event EventHandler<TapoServiceEvent> OnChanged;
        Task<bool> Ping(int port);
    }

    public class TapoHttpClient : ITapoHttpClient
    {
        public event EventHandler<TapoServiceEvent> OnChanged;

        protected readonly ISettingsService _settings;
        protected readonly IStoredProperties _storedProperties;
        protected TimeSpan _cacheExpiry;

        public TapoHttpClient(ISettingsService settingsService, IStoredProperties storedProperties)
        {
            _settings = settingsService;
            _storedProperties = storedProperties;
            _cacheExpiry = TimeSpan.FromMinutes(30);
        }


        public async Task<LoginCache> DoLogin(int port)
        {
            LoginCache stok = null;
            var useCache = true;

            for (var retry =0;retry < 10; retry++)
            { 
                stok = await DoLogin(port, useCache);
                useCache = false;
                if (stok == null || string.IsNullOrEmpty(stok.Stok))
                    continue;
                return stok;
            }
            return stok;
        }

        public virtual async Task<LoginCache> DoLogin(int port, bool useCache)
        {
            var stok = GetStokFromCache(port, useCache);
            if (stok != null)
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
            if (tapoComand == null || !tapoComand.IsSuccess())
                return null;
            StoreInCache(tapoComand.result.stok, port);
            return new LoginCache { Stok = tapoComand.result.stok };
        }

        public virtual async Task<TResult> DoTapoCommand<TResult, TCall>(int port, TCall callObj) where TCall : ICall
            where TResult : IResult
        {
            await CheckOnWifi(port);

            TResult ret =default;
            for (var retryNum = 1; retryNum < 10; retryNum++)
            {
                try
                {
                    var loginData = await DoLogin(port, retryNum == 1);
                    if (loginData == null || string.IsNullOrEmpty(loginData.Stok))
                    {
                        await Delay();
                        continue;
                    }
                    var url = GetIPAddress(port) + "/stok=" + loginData.Stok + @"/ds"; //"/stok=" + loginStok + @"/ds"
                    RaiseOnChangeEvent(port, "Starting " + callObj.Call() + $"({retryNum})");

                    ret = await DoTapoCommandImp<TResult, TCall>(url, callObj);

                    if (ret != null && ret.IsSuccess())
                    {
                        RaiseOnChangeEvent(port, callObj.Call() + " " + ret.Result());
                        return ret;
                    }
                    RaiseOnChangeEvent(port, "Error " + callObj.Call());
                }
                catch (Exception ex)
                {
                    RaiseOnChangeEvent(port, "Error " + callObj.Call());
                }
                await Delay();
            }
            return ret;
        }

        protected void RaiseOnChangeEvent(int port, string message)
        {
            if (OnChanged == null)
                return;
            OnChanged.Invoke(this, new TapoServiceEvent { Port = port, Message = message });
        }

        protected string GetIPAddress(int port)
        {
            return "https://192.168.1." + port;
        }

        private void StoreInCache(string stok, int port)
        {
            var cacheProp = "CacheProp" + port;
            _storedProperties.Set(cacheProp, new LoginCache {Stok = stok, ExpiryDate = DateTime.Now.Add(_cacheExpiry)});
        }

        protected LoginCache GetStokFromCache(int port, bool useCache)
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
            return prop;
        }

        protected virtual async Task<TResult> DoTapoCommandImp<TResult, TCall>(string url, TCall callObj) where TCall : ICall
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
                    var json = Json.Serialize(callObj);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        var result = await http.PostAsync(url, content);
                        if (!result.IsSuccessStatusCode)
                            return default;
                        var cont = await result.Content.ReadAsStringAsync();
                        var loginResult = Json.Deserialize<TResult>(cont);
                        return loginResult;
                    }
                    catch (Exception e)
                    {
                        return default;
                    }
                }
            }
        }

        protected virtual async Task CheckOnWifi(int port)
        {
            var waitingChars = new[] { '/', '-', '\\', '-' };
            int i=0;
            while (!await IsConnectedToPort(port))
            {
                RaiseOnChangeEvent(port, waitingChars[i] + " Waiting For Wifi " + waitingChars[i]);
                i++;
                if (i >= waitingChars.Length)
                    i = 0;
                await Delay();
            }
        }

        private async Task<bool> IsConnectedToPort(int port)
        {
            var profiles = Connectivity.ConnectionProfiles;
            if (!profiles.Contains(ConnectionProfile.WiFi))
            {
                return false;
            }
            return await Ping(port);
        }
        public async Task<bool> Ping(int port)
        {
            return true;
            var url = "http://192.168.1." + port;

            using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) })
            {
                HttpRequestMessage request = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Head
                };
                var result = await httpClient.SendAsync(request);
                return result.IsSuccessStatusCode;
            }
                //var httpClient = new HttpClient();

        }
        /*public async Task<bool> Ping(int port)
        {
            try
            {
                var url = "192.168.1." + port;
                var p = new Ping();
                PingReply r;
                r = await p.SendPingAsync(url, 1000);

                return r.Status == IPStatus.Success || r.Status == IPStatus.TtlExpired;
            }
            catch(Exception)
            {
                return false;
            }
        }*/
        protected async Task Delay()
        {
            await Task.Delay(1000);
        }
    }

    public class LoginCache
    {
        public string Stok { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CNonce { get; set; }
        public string Nonce { get; set; }
        public int Seq { get; set; }
    }
}