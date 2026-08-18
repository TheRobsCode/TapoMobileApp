using System.Text;

namespace TapoMobileApp
{
    public class TapoSecureHttpClient : TapoHttpClient
    {
        public TapoSecureHttpClient(ISettingsService settingsService, IStoredProperties storedProperties)
            : base(settingsService, storedProperties)
        {
            _cacheExpiry = TimeSpan.FromMinutes(30);
        }

        public override async Task<LoginCache> DoLogin(int port, bool useCache)
        {
            var cache = GetStokFromCache(port, useCache);
            if (cache != null)
                return cache;

            var url = GetIPAddress(port);
            cache = new LoginCache { CNonce = CryptoServices.GenerateNonce() };
            var loginRequest = new SecureLoginCall
            {
                @params = new SecureLoginParams
                {
                    cnonce = cache.CNonce,
                    encrypt_type = 3,
                    username = _settings.UserName
                }
            };

            RaiseOnChangeEvent(port, $"Starting {loginRequest.Call()}");
            var loginResponse = await DoTapoCommandImp<SecureLogin, SecureLoginCall>(url, loginRequest).ConfigureAwait(false);
            if (loginResponse == null)
                return null;

            cache.Nonce = loginResponse.result.data.nonce;

            if (!loginResponse.IsSuccess())
                return null;

            RaiseOnChangeEvent(port, "Starting Digest");
            var digestLogin = await DoDigestLogin(url, cache).ConfigureAwait(false);
            if (digestLogin?.result == null)
                return null;

            cache.Stok = digestLogin.result.stok;
            cache.Seq = digestLogin.result.start_seq;
            StoreInCache(cache, port);
            return cache;
        }

        private async Task<DigestLogin> DoDigestLogin(string url, LoginCache loginData)
        {
            var password = (CryptoServices.GetPassword(_settings.Password, loginData.Nonce, loginData.CNonce)
                            + loginData.CNonce + loginData.Nonce).ToUpper();

            var digestLoginRequest = new DigestLoginRequest
            {
                @params = new DigestLoginParams
                {
                    cnonce = loginData.CNonce,
                    digest_passwd = password
                }
            };

            return await DoTapoCommandImp<DigestLogin, DigestLoginRequest>(url, digestLoginRequest).ConfigureAwait(false);
        }

        public override async Task<TResult> DoTapoCommand<TResult, TCall>(int port, TCall callObj)
        {
            await CheckOnWifi(port).ConfigureAwait(false);

            TResult result = default;
            for (var attempt = 1; attempt < 10; attempt++)
            {
                try
                {
                    var cache = await DoLogin(port, attempt == 1).ConfigureAwait(false);
                    if (cache == null || string.IsNullOrEmpty(cache.Stok))
                    {
                        await Delay().ConfigureAwait(false);
                        continue;
                    }

                    var url = $"{GetIPAddress(port)}/stok={cache.Stok}/ds";
                    CryptoServices.GenerateEncryptionTokens(_settings.Password, cache, out var lsk, out var ivb);

                    RaiseOnChangeEvent(port, $"Starting {callObj.Call()}({attempt})");

                    var multiRequest = CreateMultipleRequest(callObj);
                    var requestEnc = CryptoServices.Encrypt(multiRequest, lsk, ivb);
                    var secureRequest = new SecurePassthrough
                    {
                        @params = new SecureParams { request = requestEnc }
                    };

                    var headers = GetHeaders(secureRequest, cache);
                    var httpResult = await DoTapoCommandImp<SecureResult<TResult>, SecurePassthrough>(
                        url, secureRequest, headers).ConfigureAwait(false);

                    if (httpResult != null && httpResult.TryGetResult(lsk, ivb, out var response) && response.IsSuccess())
                    {
                        RaiseOnChangeEvent(port, $"{callObj.Call()} {response.Result()}");
                        cache.Seq++;
                        _storedProperties.Set(port, cache);
                        return response;
                    }

                    RaiseOnChangeEvent(port, $"Error {callObj.Call()}");
                }
                catch (Exception ex)
                {
                    RaiseOnChangeEvent(port, $"Error {callObj.Call()}: {ex.Message}");
                }
                await Delay().ConfigureAwait(false);
            }
            return result;
        }

        private static MultipleRequest<TCall> CreateMultipleRequest<TCall>(TCall callObj)
        {
            return new MultipleRequest<TCall>
            {
                @params = new MultipleRequestParams<TCall>
                {
                    requests = new List<TCall> { callObj }
                }
            };
        }

        private Dictionary<string, string> GetHeaders(SecurePassthrough secureRequest, LoginCache cache)
        {
            return new Dictionary<string, string>
            {
                { "Tapo_tag", CryptoServices.GetTag(_settings.Password, cache, secureRequest) },
                { "Seq", cache.Seq.ToString() }
            };
        }

        private void StoreInCache(LoginCache cache, int port)
        {
            cache.ExpiryDate = DateTime.Now.Add(_cacheExpiry);
            _storedProperties.Set(port, cache);
        }

        protected async Task<TResult> DoTapoCommandImp<TResult, TCall>(
            string url, TCall callObj, Dictionary<string, string> headers = null)
            where TCall : ICall
            where TResult : IResult
        {
            using var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            using var http = new HttpClient(httpClientHandler) { Timeout = TimeSpan.FromSeconds(15) };
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
                var response = await http.SendAsync(req).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Json.Deserialize<TResult>(content);
            }
            catch (Exception ex)
            {
                //RaiseOnChangeEvent(-1, $"HTTP error: {ex.Message}");
                return default;
            }
        }
    }
}