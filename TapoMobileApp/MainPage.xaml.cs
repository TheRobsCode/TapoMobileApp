using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Diagnostics;
using System.Xml.Linq;

namespace TapoMobileApp
{
    public partial class MainPage : ContentPage
    {
        private const string PortsConfig = "Ports";
        private readonly StoredProperties _storedProperties;
        protected readonly ITapoService _tapoService;
        private readonly Dictionary<string, Label> _portOutputDictionary = [];
        private bool _running = false;

        public MainPage() : this(false) { }

        public MainPage(bool running)
        {
            _running = running;
            InitializeComponent();

            _storedProperties = new StoredProperties();
            var settingService = new SettingsService();
            var httpClient = new TapoSecureHttpClient(settingService, _storedProperties);

            httpClient.OnChanged += HttpClient_OnChanged;
            _tapoService = new TapoSecureService(httpClient, _storedProperties);

            WireUpEventHandlers();

            if (_storedProperties.ContainsKey(PortsConfig) && !string.IsNullOrEmpty(_storedProperties.Get(PortsConfig)))
                _ports.Text = _storedProperties.Get(PortsConfig);

            ShowLogs();
            SetupOutputLabels();
            _tapoService.Initialize(GetPorts());

            if (!_running)
            {
                Task.Run(() =>
                {
                    MainThread.BeginInvokeOnMainThread(async () => await CheckState());
                });
            }
        }

        private void WireUpEventHandlers()
        {
            ButtonOff.Clicked += async (s, e) => await ButtonOff_Clicked(s, e);
            ButtonOn.Clicked += async (s, e) => await ButtonOn_Clicked(s, e);
            ButtonCheck.Clicked += async (s, e) => await ButtonCheck_Clicked(s, e);
            ButtonClear.Clicked += async (s, e) => await ButtonClear_Clicked(s, e);
            Scan.Clicked += async (s, e) => await ScanButton_Clicked(s, e);
            _ports.TextChanged += async (s, e) => await Ports_TextChanged(s, e);
            LogMessage.TextChanged += async (s, e) => await LogMessage_TextChanged(s, e);
        }

        private void DisplayMessage(int port, string message)
        {
            var name = $"lblPort{port}";
            if (_portOutputDictionary.TryGetValue(name, out var label) && label != null)
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    label.Text = message;
                });
            
        }

        private void HttpClient_OnChanged(object sender, TapoServiceEvent e) =>
            DisplayMessage(e.Port, e.Message);

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected void SetupOutputLabels()
        {
            foreach (var port in GetPorts())
            {
                var name = $"lblPort{port}";
                if (_portOutputDictionary.ContainsKey(name))
                    continue;

                var stack = new StackLayout { Orientation = StackOrientation.Horizontal };
                var label = new Label { FontSize = 22 };
                stack.Children.Add(new Label { Text = $"Port {port}:", FontSize = 22 });
                stack.Children.Add(label);
                _portOutputDictionary.Add(name, label);
                CameraOutput.Children.Add(stack);
            }
        }

        private void CheckToEnableScan()
        {
            try
            {
                Scan.IsEnabled = string.IsNullOrEmpty(_ports.Text);
            }
            catch
            {
                Scan.IsEnabled = false;
            }
        }

        public void ShowLogs()
        {
            var log = _storedProperties.Get("log");
            if (string.IsNullOrEmpty(log))
                return;
            LogMessage.Text = log.Replace("\n", "");
            //var stack = new StackLayout { Orientation = StackOrientation.Horizontal };
            //var label = new Label { FontSize = 22 };
            //stack.Children.Add(new Label { Text = log, FontSize = 22 });
            //stack.Children.Add(label);
            //CameraOutput.Children.Add(stack);
        }

        private void SetButtonState(Button button, string text, bool enabled)
        {
            button.Text = text;
            button.IsEnabled = enabled;
        }

        private async Task ScanButton_Clicked(object sender, EventArgs e)
        {
            DisplayStatus("Working...");
            await SetScanButtonStateAsync("Please Wait...", false);

            var ports = await _tapoService.Scan();
            _storedProperties.Set(PortsConfig, string.Join(",", ports));
            _ports.Text = _storedProperties.Get(PortsConfig);

            await SetScanButtonStateAsync("Scan", true);

            DisplayStatus(ports.Length == 0 ? "No Tapo Devices Found" : $"Found: {string.Join(",", ports)}");
        }

        private Task SetScanButtonStateAsync(string text, bool enabled) =>
            Task.Run(() => MainThread.BeginInvokeOnMainThread(() => SetButtonState(Scan, text, enabled)));

        private void DisplayStatus(string message) => StatusMessage.Text = message;

        public async Task ButtonOn_Clicked(object sender, EventArgs e) => await ChangeState(true);

        public async Task ButtonOff_Clicked(object sender, EventArgs e) => await ChangeState(false);

        public async Task ButtonClear_Clicked(object sender, EventArgs e)
        {
            DisplayStatus("Working...");
            _storedProperties.Clear();
            DisplayStatus("Done");
        }

        public async Task ButtonCheck_Clicked(object sender, EventArgs e) => await CheckState();

        public async Task CheckState() => await _tapoService.CheckState(GetPorts());

        public async Task ChangeState(bool toggleOnOrOff)
        {
            try
            {
                await _tapoService.ChangeState(GetPorts(), toggleOnOrOff);
                await CheckState();
            }
            catch (Exception ex)
            {
                _storedProperties.StoreLog($"{ex.Message}{ex.InnerException}");
            }
        }
        private async Task LogMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            _storedProperties.StoreLog(e.NewTextValue);
        }
        private async Task Ports_TextChanged(object sender, TextChangedEventArgs e)
        {
            _storedProperties.Set(PortsConfig, e.NewTextValue);
            CheckToEnableScan();
            SetupOutputLabels();
        }

        protected int[] GetPorts()
        {
            var portsStr = _storedProperties.Get(PortsConfig);
            if (string.IsNullOrEmpty(portsStr))
                return [];

            var result = new List<int>();
            foreach (var port in portsStr.Split(','))
            {
                if (int.TryParse(port.Trim(), out var portNum) && portNum is >= 2 and <= 254)
                    result.Add(portNum);
            }
            return [.. result];
        }
    }
}
