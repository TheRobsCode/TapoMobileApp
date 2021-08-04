using Newtonsoft.Json;
using System.Threading.Tasks;
using TapoMobileApp;

namespace TestTapoMobileApp
{
    public class MockTapoHttpClient : TapoHttpClient
    {
        private string _command;
        public MockTapoHttpClient(ISettingsService settingsService, IStoredProperties storedProperties) : base(settingsService, storedProperties)
        {
        }

        public void SetFakeCommandReturns(string command)
        {
            _command = command;

        }

        protected override async Task<TResult> DoTapoCommand<TResult, TCall>(string url, TCall callObj)
        {

            if (_command == "HappyPathCachedLogin")
            {
                var res = new TapoResult { error_code = 0, result = new Result { stok = "Stok" } };
                var json = JsonConvert.SerializeObject(res);
                return await Task.FromResult(JsonConvert.DeserializeObject<TResult>(json));
            }
            else if (_command == "HappyPathNoCachedLogin")
            {
                var res = new TapoResult { error_code = 0, result = new Result { stok = "Stok" } };
                var json = JsonConvert.SerializeObject(res);
                return await Task.FromResult(JsonConvert.DeserializeObject<TResult>(json));
            }
            else if (_command == "LoginFails")
            {
                return default(TResult);
            }
            
            return default(TResult);
        }
    }
}
