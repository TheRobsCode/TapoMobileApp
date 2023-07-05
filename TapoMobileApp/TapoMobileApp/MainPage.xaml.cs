using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Plugin.AppShortcuts;
using Plugin.AppShortcuts.Icons;
using Xamarin.Forms;
using Button = Xamarin.Forms.Button;

namespace TapoMobileApp
{
    public partial class MainPage : ContentPage
    {
        private const string PortsConfig = "Ports";
        private readonly IStoredProperties _storedProperties;
        private readonly ITapoService _tapoService;

        public MainPage()
        {
            InitializeComponent();
            var settingService = new SettingsService();
            _storedProperties = new StoredProperties();

            var httpClient = new TapoHttpClient(settingService, _storedProperties);
            httpClient.OnChanged += HttpClient_OnChanged;
            _tapoService = new TapoService(httpClient, _storedProperties);
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
        }

        private void DisplayMessage(List<TapoServiceEvent> messages)
        {
            foreach(var e in messages)
            {
                DisplayMessage(e.Port, e.Message);
            }
        }
        private void DisplayMessage(int port, string message)
        {
            var name = "lblPort" + port;
            if (!_portOutputDictionary.ContainsKey(name))
                return;
            var label = _portOutputDictionary[name];
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

            SetupOutputLabels();
            _tapoService.Initialize(GetPorts());

            Task.Run( () => 
            {
                 Device.BeginInvokeOnMainThread(async () =>
                {
                    await CheckState();
                });
            });
            Task.Run(async () => await AddShortcuts());

        }
        private readonly Dictionary<string,Label> _portOutputDictionary = new Dictionary<string, Label>();
        private void SetupOutputLabels()
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
                stack.Children.Add(new Label { Text= "Port " + port + ":", FontSize = 22 });
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

        private async Task AddShortcuts()
        {
            if (!CrossAppShortcuts.IsSupported) return;

            var shortCurts = await CrossAppShortcuts.Current.GetShortcuts();
            if (shortCurts.FirstOrDefault(prop => prop.Label == "Privacy On") == null)
            {
                var shortcut = new Shortcut
                {
                    Label = "Privacy On",
                    Description = "Turn Privacy On",
                    Icon = new PauseIcon(),
                    Uri = $"{Constants.AppShortcutUriBase}{Constants.ShortcutTurnPrivacyOn}"
                };
                await CrossAppShortcuts.Current.AddShortcut(shortcut);
            }

            if (shortCurts.FirstOrDefault(prop => prop.Label == "Privacy Off") == null)
            {
                var shortcut = new Shortcut
                {
                    Label = "Privacy Off",
                    Description = "Turn Privacy Off",
                    Icon = new PlayIcon(),
                    Uri = $"{Constants.AppShortcutUriBase}{Constants.ShortcutTurnPrivacyOff}"
                };
                await CrossAppShortcuts.Current.AddShortcut(shortcut);
            }
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
                Device.BeginInvokeOnMainThread(() => { SetButtonState(Scan, "Please Wait...", false); }));

            var ports = await _tapoService.Scan();
            _storedProperties.Set(PortsConfig, string.Join(",", ports));
            _ports.Text = _storedProperties.Get(PortsConfig);

            await Task.Run(() => Device.BeginInvokeOnMainThread(() => { SetButtonState(Scan, "Scan", true); }));

            var message = "No Tapo Devices Found";
            if (ports.Any())
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
        private async Task CheckState()
        {
            await _tapoService.CheckState(GetPorts());

            //DisplayMessage(results);
        }
        public async Task ChangeState(bool toggleOnOrOff)
        {
            await _tapoService.ChangeState(GetPorts(), toggleOnOrOff);
            await CheckState();
        }

        private async Task _ports_TextChanged(object sender, TextChangedEventArgs e)
        {
            _storedProperties.Set(PortsConfig, e.NewTextValue);
            CheckToEnableScan();
            SetupOutputLabels();
        }

        private int[] GetPorts()
        {
            var portsStr = _storedProperties.Get(PortsConfig);
            if (string.IsNullOrEmpty(portsStr))
                return new int[0];

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

            return result.ToArray();
        }
    }
}