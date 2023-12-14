using System.Text;

namespace TapoMobileApp
{
    
    public class TapoSecureHttpClient : TapoHttpClient
    {
        public TapoSecureHttpClient(ISettingsService settingsService, IStoredProperties storedProperties) : base(settingsService, storedProperties)
        {
            _cacheExpiry = TimeSpan.FromSeconds(30);
        }

        public override async Task<LoginCache> DoLogin(int port, bool useCache)
        {
            var cache = GetStokFromCache(port, useCache);
            if (cache != null)
                return cache;

            var url = GetIPAddress(port);
            cache = new LoginCache { CNonce = CryptoServices.GenerateNonce() };//*CryptoServices.GenerateNonce()*/ 
            var obj = new SecureLoginCall
            {
                @params = new SecureLoginParams {cnonce = cache.CNonce, encrypt_type=3, username = _settings.UserName }
            };
            RaiseOnChangeEvent(port, "Starting " + obj.Call());
            var tapoComand = await DoTapoCommandImp<SecureLogin, SecureLoginCall>(url, obj);
            cache.Nonce = tapoComand.result.data.nonce;

            if (!tapoComand.IsSuccess())
                return null;

            RaiseOnChangeEvent(port, "Starting Digest");
            var digestLogin = await DoDigestLogin(url, cache);
            cache.Stok = digestLogin.result.stok;
            cache.Seq = digestLogin.result.start_seq;
            StoreInCache(cache, port);
            return cache;
        }
        private async Task<DigestLogin> DoDigestLogin(string url, LoginCache loginData)
        {
            var password = (CryptoServices.GetPassword(_settings.Password, loginData.Nonce, loginData.CNonce) + loginData.CNonce + loginData.Nonce).ToUpper();
            var digestLoginRequest = new DigestLoginRequest
            {
                @params = new DigestLoginParams
                {
                    cnonce = loginData.CNonce,
                    digest_passwd = password
                }
            };
            
            var result = await DoTapoCommandImp<DigestLogin, DigestLoginRequest>(url, digestLoginRequest);
            return result;
        }
        public override async Task<TResult> DoTapoCommand<TResult, TCall>(int port, TCall callObj) //where TCall : ICall
            //where TResult : IResult
        {
            await CheckOnWifi(port);

            TResult ret = default;
            for (var retryNum = 1; retryNum < 10; retryNum++)
            {
                try
                { 
                    var cache = await DoLogin(port, retryNum == 1); //
                    if (string.IsNullOrEmpty(cache.Stok))
                    {
                        await Delay();
                        continue;
                    }
                    var url = GetIPAddress(port) + "/stok=" + cache.Stok + @"/ds"; //"/stok=" + loginStok + @"/ds"
                    CryptoServices.GenerateEncryptionTokens(_settings.Password, cache, out var lsk, out var ivb);

                    RaiseOnChangeEvent(port, "Starting " + callObj.Call() + $"({retryNum})");

                    var multiRequest = new MultipleRequest<TCall>()
                    {
                        @params = new MultipleRequestParams<TCall>
                        {
                            requests = new List<TCall>()
                            {
                                     callObj 
                            }
                        }
                    };

                    var requestEnc = CryptoServices.Encrypt(multiRequest, lsk, ivb);
                    var secureRequest = new SecurePassthrough() { @params = new SecureParams() { request = requestEnc } };

                    var headers = GetHeaders(secureRequest, cache);
                    var httpResult = await DoTapoCommandImp<SecureResult<TResult>, SecurePassthrough>(url, secureRequest, headers);

                    if (httpResult.TryGetResult(lsk, ivb, out var response) && response.IsSuccess())
                    {
                        RaiseOnChangeEvent(port, callObj.Call() + " " + response.Result());
                        return response;
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

        //private void WriteOut(string str, string json)
        //{
        //    System.IO.File.AppendAllText("output.txt", str + ":\r\n");
        //    System.IO.File.AppendAllText("output.txt", json + "\r\n");
        //}
        //private void WriteOut(string str, object obj) 
        //{
        //    var output = Json.Serialize(obj);
        //    System.IO.File.AppendAllText("output.txt", str + ":\r\n");
        //    System.IO.File.AppendAllText("output.txt", output + "\r\n");
        //}

        private Dictionary<string, string> GetHeaders(SecurePassthrough secureRequest, LoginCache cache)
        {
            var result = new Dictionary<string, string>();
            var tag = CryptoServices.GetTag(_settings.Password, cache, secureRequest);
            result.Add("Tapo_tag", tag);
            result.Add("Seq", cache.Seq.ToString());
            return result;
        }

        private void StoreInCache(LoginCache obj, int port)
        {
            var cacheProp = "CacheProp" + port;
            obj.ExpiryDate = DateTime.Now.Add(_cacheExpiry);
            _storedProperties.Set(cacheProp, obj);
        }

        private LoginCache GetStokFromCache(int port, bool useCache)
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

        protected virtual async Task<TResult> DoTapoCommandImp<TResult, TCall>(string url, TCall callObj, Dictionary<string,string> headers= null) where TCall : ICall
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
                    var req = new HttpRequestMessage(HttpMethod.Post, url);
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            req.Headers.Add(header.Key, header.Value);
                        }
                    }
                    req.Content = new StringContent(Json.Serialize(callObj), Encoding.UTF8, "application/json");
                    try
                    {
                        var result = await http.SendAsync(req);
                        var cont = await result.Content.ReadAsStringAsync();
                        var loginResult = Json.Deserialize<TResult>(cont);
                        var isSuccess = loginResult.IsSuccess();
                        return loginResult;
                    }
                    catch (Exception e)
                    {
                        var message = e.Message;
                        return default;
                    }

                }
            }
        }

        
    }
}