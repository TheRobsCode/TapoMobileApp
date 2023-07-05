using System.Threading.Tasks;
using Newtonsoft.Json;
using TapoMobileApp;

namespace TestTapoMobileApp
{
    public class MockTapoHttpClient : TapoHttpClient
    {
        private string _command;

        public MockTapoHttpClient(ISettingsService settingsService, IStoredProperties storedProperties) : base(
            settingsService, storedProperties)
        {
        }

        public void SetFakeCommandReturns(string command)
        {
            _command = command;
        }

        protected override async Task<(bool success, TResult result)> DoTapoCommandImp<TResult, TCall>(string url, TCall callObj)
        {
            if (_command == "HappyPathCachedLogin")
            {
                var res = new TapoResult {error_code = 0, result = new Result {stok = "Stok"}};
                var json = JsonConvert.SerializeObject(res);
                return (true, await Task.FromResult(JsonConvert.DeserializeObject<TResult>(json)));
            }

            if (_command == "HappyPathNoCachedLogin")
            {
                var res = new TapoResult {error_code = 0, result = new Result {stok = "Stok"}};
                var json = JsonConvert.SerializeObject(res);
                return (true, await Task.FromResult(JsonConvert.DeserializeObject<TResult>(json)));
            }

            if (_command == "LoginFails")
            {
                return (false,default);
            }

            return (false, default);
        }
    }
}