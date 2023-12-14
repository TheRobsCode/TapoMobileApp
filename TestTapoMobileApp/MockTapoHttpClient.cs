using System.Text.Json;
using System.Threading.Tasks;
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

        protected override async Task<TResult> DoTapoCommandImp<TResult, TCall>(string url, TCall callObj)
        {
            if (_command == "HappyPathCachedLogin")
            {
                var res = new TapoResult {error_code = 0, result = new Result {stok = "Stok"}};
                var json = JsonSerializer.Serialize(res);
                return await Task.FromResult(JsonSerializer.Deserialize<TResult>(json));
            }

            if (_command == "HappyPathNoCachedLogin")
            {
                var res = new TapoResult {error_code = 0, result = new Result {stok = "Stok"}};
                var json = JsonSerializer.Serialize(res);
                return await Task.FromResult(JsonSerializer.Deserialize<TResult>(json));
            }

            if (_command == "LoginFails")
            {
                return default;
            }

            return default;
        }

        protected override async Task CheckOnWifi(int port)
        {
            return;
        }
    }
}