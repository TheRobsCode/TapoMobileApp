using System;
using System.Collections.Generic;
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
            //var loginProvider = new LoginProvider(httpClient, settingService);
            _tapoService = new TapoService(httpClient);
            ButtonOff.Clicked += async (sender, e) => { await ButtonOff_Clicked(sender, e); };
            ButtonOn.Clicked += async (sender, e) => { await ButtonOn_Clicked(sender, e); };
            ButtonCheck.Clicked += async (sender, e) => { await ButtonCheck_Clicked(sender, e); };
            ButtonClear.Clicked += async (sender, e) => { await ButtonClear_Clicked(sender, e); };
            Scan.Clicked += async (sender, e) => { await ScanButton_Clicked(sender, e); };
            _ports.TextChanged += async (sender, e) => { await _ports_TextChanged(sender, e); };
            if (_storedProperties.ContainsKey(PortsConfig) && !string.IsNullOrEmpty(_storedProperties.Get(PortsConfig)))
            {
                _ports.Text = _storedProperties.Get(PortsConfig);
                _tapoService.Initialize(GetPorts());
            }
            //foreach (var item in Xamarin.Forms.Application.Current.Properties)
            //{
            //    LogMessage.Text += item.Key + "=" + item.Value;
            //}
            
            CheckToEnableScan();
            Task.Run(async () => await AddShortcuts());
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
            //Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
            DisplayMessage(message);
        }

        private void DisplayMessage(string message)
        {
            //Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
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
            DisplayMessage("Working...");
            await CheckState();
        }
        private async Task CheckState()
        {
            var results = await _tapoService.CheckState(GetPorts());

            var message = string.Join("\r\n", results);
            DisplayMessage(message);
        }
        public async Task ChangeState(bool toggleOnOrOff)
        {
            DisplayMessage("Working...");
            var errors = await _tapoService.ChangeState(GetPorts(), toggleOnOrOff);
            var message = "Done";
            if (errors.Count > 0)
                message = "An Error Occured with ports:" + string.Join(",", errors);

            //Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
            DisplayMessage(message);
            await CheckState();
        }

        private async Task _ports_TextChanged(object sender, TextChangedEventArgs e)
        {
            _storedProperties.Set(PortsConfig, e.NewTextValue);
            CheckToEnableScan();
        }

        private int[] GetPorts()
        {
            var ports = _storedProperties.Get(PortsConfig).Split(',');
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