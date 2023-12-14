using Microsoft.Extensions.Logging;

namespace TapoMobileApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })

                .ConfigureEssentials(essentials =>
                {
                    essentials
                        .AddAppAction("privacy_on", "Privacy On", icon: "app_info_action_icon")
                        .AddAppAction("privacy_off", "Privacy Off")
                        .OnAppAction(App.HandleAppActions);
                }); 

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
