
using Microsoft.Maui.ApplicationModel;

namespace TapoMobileApp
{

    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        public static void HandleAppActions(AppAction appAction)
        {
            App.Current.Dispatcher.Dispatch(async () =>
            {
                var mainPage = (MainPage)Application.Current.MainPage;
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
