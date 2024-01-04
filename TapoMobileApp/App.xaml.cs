
using Microsoft.Maui.ApplicationModel;

namespace TapoMobileApp
{

    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();

            //MainPage = new AppShell();
            MainPage = new MainPage();
        }
        
        public static void HandleAppActions(AppAction appAction)
        {
            _ = Current.Dispatcher.Dispatch(async () =>
            {
                var mainPage = (MainPage)App.Current.MainPage;
                if (appAction.Id == "privacy_on")
                {
                    await mainPage.ChangeState(true);

                }
                else
                    await mainPage.ChangeState(false);

            });
        }
    }
}
