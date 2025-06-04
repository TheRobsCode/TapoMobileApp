//using Android.App;
//using Android.Content.PM;
//using static Android.Service.Notification.NotificationListenerService;

using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Xml.Linq;

namespace TapoMobileApp
{

    public partial class MainPage : ContentPage
    {
        private const string PortsConfig = "Ports";
        private readonly StoredProperties _storedProperties;
        protected readonly ITapoService _tapoService;
        public MainPage() : this(false)
        {
        }
        public MainPage(bool running) 
        {
            _running = running;
            InitializeComponent();
            var settingService = new SettingsService();
            _storedProperties = new StoredProperties();

             var httpClient = new TapoSecureHttpClient(settingService, _storedProperties);

            httpClient.OnChanged += HttpClient_OnChanged;
            _tapoService = new TapoSecureService(httpClient, _storedProperties);
            ButtonOff.Clicked += async (sender, e) => { await ButtonOff_Clicked(sender, e); };
            ButtonOn.Clicked += async (sender, e) => { await ButtonOn_Clicked(sender, e); };
            ButtonCheck.Clicked += async (sender, e) => { await ButtonCheck_Clicked(sender, e); };
            ButtonClear.Clicked += async (sender, e) => { await ButtonClear_Clicked(sender, e); };
            Scan.Clicked += async (sender, e) => { await ScanButton_Clicked(sender, e); };
            _ports.TextChanged += async (sender, e) => { await _ports_TextChanged(sender, e); };
            if (_storedProperties.ContainsKey(PortsConfig) && !string.IsNullOrEmpty(_storedProperties.Get(PortsConfig)))
            {
                _ports.Text = _storedProperties.Get(PortsConfig);
            }
            ShowLogs();
            //new
            SetupOutputLabels();
            _tapoService.Initialize(GetPorts());

            if (running)
                return;

            Task.Run(() =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await CheckState();
                });
            });
        }

        private void DisplayMessage(int port, string message)
        {
            var name = "lblPort" + port;
            if (!_portOutputDictionary.TryGetValue(name, out Label? value))
                return;
            var label = value;
            if (label == null)
                return;
            label.Text = message;
        }
        private void HttpClient_OnChanged(object sender, TapoServiceEvent e)
        {
            DisplayMessage(e.Port, e.Message);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
        private readonly Dictionary<string, Label> _portOutputDictionary = [];
        protected void SetupOutputLabels()
        {
            //CameraOutput.Children.Clear();
            foreach (var port in GetPorts())
            {
                var name = "lblPort" + port;

                if (_portOutputDictionary.ContainsKey(name))
                    continue;
                var stack = new StackLayout();
                var label = new Label() { FontSize = 22 };
                stack.Orientation = StackOrientation.Horizontal;
                stack.Children.Add(new Label { Text = "Port " + port + ":", FontSize = 22 });
                stack.Children.Add(label);
                _portOutputDictionary.Add(name, label);
                CameraOutput.Children.Add(stack);
            }
        }
        
        private void CheckToEnableScan()
        {
            try
            {
                Scan.IsEnabled = _ports.Text.Length == 0;
            }
            catch (Exception ex)
            {
                Scan.IsEnabled = false;
            }
        }
        public void ShowLogs()
        {
            var stack = new StackLayout();
            var label = new Label() { FontSize = 22 };
            stack.Orientation = StackOrientation.Horizontal;
            var log = _storedProperties.Get("log");
            if (string.IsNullOrEmpty(log))
                return;

            stack.Children.Add(new Label { Text = log, FontSize = 22 });
            stack.Children.Add(label);
            CameraOutput.Children.Add(stack);
        }

        private void SetButtonState(Button button, string text, bool enabled)
        {
            button.Text = text;
            button.IsEnabled = enabled;
        }

        private async Task ScanButton_Clicked(object sender, EventArgs e)
        {
            DisplayMessage("Working...");
            await Task.Run(() =>
                MainThread.BeginInvokeOnMainThread(() => { SetButtonState(Scan, "Please Wait...", false); }));

            var ports = await _tapoService.Scan();
            _storedProperties.Set(PortsConfig, string.Join(",", ports));
            _ports.Text = _storedProperties.Get(PortsConfig);

            await Task.Run(() => MainThread.BeginInvokeOnMainThread(() => { SetButtonState(Scan, "Scan", true); }));

            var message = "No Tapo Devices Found";
            if (ports.Length != 0)
                message = "Found: " + string.Join(",", ports);
            DisplayMessage(message);
        }

        private void DisplayMessage(string message)
        {
            StatusMessage.Text = message;
        }

        public async Task ButtonOn_Clicked(object sender, EventArgs e)
        {
            await ChangeState(true);
        }

        public async Task ButtonOff_Clicked(object sender, EventArgs e)
        {
            await ChangeState(false);
        }
        public async Task ButtonClear_Clicked(object sender, EventArgs e)
        {
            DisplayMessage("Working...");
            _storedProperties.Clear();
            DisplayMessage("Done");
        }

        public async Task ButtonCheck_Clicked(object sender, EventArgs e)
        {
            await CheckState();
        }
        public async Task CheckState()
        {
            await _tapoService.CheckState(GetPorts());

            //DisplayMessage(results);
        }
        private bool _running = false;
        public async Task ChangeState(bool toggleOnOrOff)
        {
            try
            {
                await _tapoService.ChangeState(GetPorts(), toggleOnOrOff);
                await CheckState();
            }
            catch (Exception ex)
            {
                _storedProperties.StoreLog(ex.Message + ex.InnerException);
            }
        }

        private async Task _ports_TextChanged(object sender, TextChangedEventArgs e)
        {
            _storedProperties.Set(PortsConfig, e.NewTextValue);
            CheckToEnableScan();
            SetupOutputLabels();
        }

        protected int[] GetPorts()
        {
            string portsStr = _storedProperties.Get(PortsConfig);
            if (string.IsNullOrEmpty(portsStr))
                return [];

            var ports = portsStr.Split(',');
            var result = new List<int>();
            foreach (var port in ports)
            {
                if (!int.TryParse(port.Trim(), out var portNum))
                    continue;
                if (portNum > 254 || portNum < 2)
                    continue;
                result.Add(portNum);
            }

            return [.. result];
        }
    }

}
