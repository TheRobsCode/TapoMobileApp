
using Microsoft.Maui.ApplicationModel;

namespace TapoMobileApp
{

    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            return new Window(new MainPage());
        }

        public static void HandleAppActions(AppAction appAction)
        {
            _ = Current.Dispatcher.Dispatch(async () =>
            {
                if (App.Current.Windows.Count > 0 && App.Current.Windows[0].Page is MainPage mainPage)
                {
                    if (appAction.Id == "privacy_on")
                    {
                        await mainPage.ChangeState(true);
                    }
                    else
                    {
                        await mainPage.ChangeState(false);
                    }
                }
            });
        }
    }
}
