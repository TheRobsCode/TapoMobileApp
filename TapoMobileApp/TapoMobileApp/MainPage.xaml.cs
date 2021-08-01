﻿using Android.Widget;
using Plugin.AppShortcuts;
using Plugin.AppShortcuts.Icons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
namespace TapoMobileApp
{
    public partial class MainPage : ContentPage
    {
        private const string PortsConfig = "Ports";
        private readonly ITapoService _tapoService = new TapoService(new SettingsService());

        public MainPage()
        {
            InitializeComponent();
            ButtonOff.Clicked += async (sender, e) =>
            {
                await ButtonOff_Clicked(sender,e);
            };
            ButtonOn.Clicked += async (sender, e) =>
            {
                await ButtonOn_Clicked(sender, e);
            };
            ButtonCheck.Clicked += async (sender, e) =>
            {
                await ButtonCheck_Clicked(sender, e);
            };
            Scan.Clicked += async (sender, e) =>
            {
                await ScanButton_Clicked(sender, e);
            };
            if (Application.Current.Properties.ContainsKey(PortsConfig))
            {
                _ports.Text = Application.Current.Properties[PortsConfig].ToString();
            }

            Task.Run(async () => await AddShortcuts());

        }

        private async Task AddShortcuts()
        {
            if (!CrossAppShortcuts.IsSupported)
            {
                return;
            }

            var shortCurts = await CrossAppShortcuts.Current.GetShortcuts();
            if (shortCurts.FirstOrDefault(prop => prop.Label == "Privacy On") == null)
            {
                var shortcut = new Shortcut()
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
                var shortcut = new Shortcut()
                {
                    Label = "Privacy Off",
                    Description = "Turn Privacy Off",
                    Icon = new PlayIcon(),
                    Uri = $"{Constants.AppShortcutUriBase}{Constants.ShortcutTurnPrivacyOff}"
                };
                await CrossAppShortcuts.Current.AddShortcut(shortcut);
            }
        }

        private void SetButtonState(Xamarin.Forms.Button button, string text, bool enabled)
        {
            button.Text = text;
            button.IsEnabled = enabled;
        }
        private async Task ScanButton_Clicked(object sender, EventArgs e)
        {
            await Task.Run(() => Device.BeginInvokeOnMainThread(() => { SetButtonState(Scan, "Please Wait...", false); }));

            var ports = await _tapoService.Scan();
            Application.Current.Properties[PortsConfig] = string.Join(",", ports);
            _ports.Text = Application.Current.Properties[PortsConfig].ToString();

            await Task.Run(() => Device.BeginInvokeOnMainThread(() => { SetButtonState(Scan, "Scan", true); }));

            var message = "No Tapo Devices Found";
            if (ports.Any())
                message = "Found: " + string.Join(",", ports);
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }

        public async Task ButtonOn_Clicked(object sender, EventArgs e)
        {
            await ChangeState(true);
        }

        public async Task ButtonOff_Clicked(object sender, EventArgs e)
        {
            await ChangeState(false);
        }
        public async Task ButtonCheck_Clicked(object sender, EventArgs e)
        {
            var results = await _tapoService.CheckState(GetPorts());

            var message = string.Join("\r\n", results);
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }
        public async Task ChangeState(bool toggleOnOrOff)
        {
            var errors = await _tapoService.ChangeState(GetPorts(), toggleOnOrOff);
            var message = "Done";
            if (errors.Count > 0)
                message = "An Error Occured with ports:" + string.Join(",", errors);

            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }
        private void _ports_TextChanged(object sender, TextChangedEventArgs e)
        {
            Application.Current.Properties[PortsConfig] = e.NewTextValue;
        }

        private int[] GetPorts()
        {
            var ports = Application.Current.Properties[PortsConfig].ToString().Split(',');
            var result = new List<int>();
            foreach(var port in ports)
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
