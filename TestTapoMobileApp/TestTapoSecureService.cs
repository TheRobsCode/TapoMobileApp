using TapoMobileApp;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Text.Json;

namespace TestTapoMobileApp
{
    public class TestTapoSecureService
    {
        private readonly IStoredProperties _storedProperties;
        private readonly ITapoHttpClient _httpClient;
        private readonly ITapoService _tapoService;
        private readonly int[] _ports = new[] {14,105 };
        private string _message;
        public TestTapoSecureService()
        {
            var settings = new MockSettingsService() { Password = "2A3DE6A40DAB636ACBB19304B2D598CC2A4EA3ADDBCD06257E0EF95C24AE4835", UserName = "admin" };

            _storedProperties = new MockStoredProperties();
            _httpClient = new TapoSecureHttpClient(settings, _storedProperties);
            _tapoService = new TapoSecureService(_httpClient, _storedProperties);
        }
        [Fact]
        public async Task TestChangeState()
        {
            await _tapoService.CheckState(_ports);
        }
        [Fact]
        public async Task TestCameraOn()
        {
            _tapoService.OnChanged += _tapoService_OnChanged;
            _httpClient.OnChanged += _tapoService_OnChanged;
            await _tapoService.ChangeState(_ports, true);
            await _tapoService.CheckState(_ports);
        }


        [Fact]
        public async Task TestCameraOff()
        {
            _tapoService.OnChanged += _tapoService_OnChanged;
            _httpClient.OnChanged += _tapoService_OnChanged;
            await _tapoService.ChangeState(_ports, false);
            await _tapoService.CheckState(_ports);
        }

        private void _tapoService_OnChanged(object? sender, TapoServiceEvent e)
        {
            _message = e.Message;
        }
    }

    public class MockSettingsService : ISettingsService
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }

    public class MockStoredProperties : IStoredProperties
    {
        private Dictionary<string, string> _cache = new Dictionary<string, string>();
        public void Clear()
        {
            _cache.Clear();
        }

        public void Clear(int port)
        {
            _cache.Clear();
        }

        public bool ContainsKey(string key)
        {
            return _cache.ContainsKey(key);
        }

        public string Get(string key)
        {
            return _cache[key];
        }

        public T Get<T>(string key)
        {
            return JsonSerializer.Deserialize<T>(_cache[key]);
        }

        public void Set(string key, string obj)
        {
            _cache[key] = obj;
        }

        public void Set<T>(string key, T obj)
        {
            if (_cache.ContainsKey(key))
                _cache[key] = JsonSerializer.Serialize(obj);
            else
                _cache.Add(key, JsonSerializer.Serialize(obj));
        }
    }
}