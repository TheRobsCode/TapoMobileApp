using System.Text;
using System.Net.NetworkInformation;
using System.Net.Http;
using TapoMobileApp.Events;
using TapoMobileApp.Models.Requests.Base;
using TapoMobileApp.Models.Requests.Authentication;
using TapoMobileApp.Models.Responses.Base;
using TapoMobileApp.Services.Configuration;
using TapoMobileApp.Services.Storage;
using TapoMobileApp.Utilities.Serialization;

namespace TapoMobileApp.Services.Http
{
    public class TapoHttpClient : ITapoHttpClient
    {
        private const int MaxRetries = 10;

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

            for (var retry =0;retry < MaxRetries; retry++)
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
                @params = new LoginParams { hashed = true, password = _settings.Password, username = _settings.UserName }
            };
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
            for (var retryNum = 1; retryNum < MaxRetries; retryNum++)
            {
                try
                {
                    var loginData = await DoLogin(port, retryNum == 1);
                    if (loginData == null || string.IsNullOrEmpty(loginData.Stok))
                    {
                        await Delay();
                        continue;
                    }
                    var url = GetIPAddress(port) + "/stok=" + loginData.Stok + @"/ds";
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
            return $"https://{_settings.IpPrefix}.{port}";
        }

        private void StoreInCache(string stok, int port)
        {
            _storedProperties.Set(port, new LoginCache {Stok = stok, ExpiryDate = DateTime.Now.Add(_cacheExpiry)});
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
                    catch (Exception ex)
                    {
                        _storedProperties.StoreLog($"Error in {nameof(DoTapoCommandImp)} to {url}: {ex.Message}");
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
            return true;
        }

        public async Task<bool> Ping(int port)
        {
            try
            {
                var url = $"{_settings.IpPrefix}.{port}";
                var p = new Ping();
                PingReply r;
                r = await p.SendPingAsync(url, 1000);

                return r.Status == IPStatus.Success || r.Status == IPStatus.TtlExpired;
            }
            catch(Exception)
            {
                return false;
            }
        }
        protected async Task Delay()
        {
            await Task.Delay(1000);
        }
    }
}
