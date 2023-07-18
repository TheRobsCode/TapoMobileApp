using System;
using Xamarin.Forms;

namespace TapoMobileApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            var option = uri.ToString().Replace(Constants.AppShortcutUriBase, "");
            if (!string.IsNullOrEmpty(option))
            {

                Device.BeginInvokeOnMainThread(async () => {
                    var mainPage = (MainPage)MainPage;
                    var turnOn = option == Constants.ShortcutTurnPrivacyOn;
                    await mainPage.ChangeState(turnOn);
                });
            
            }
            else
            {
                base.OnAppLinkRequestReceived(uri);
            }
        }
    }
}