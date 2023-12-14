using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace TapoMobileApp
{
    // [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [Activity(Label = "Robs Cameras", Theme = "@style/Maui.SplashTheme", MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]

    [IntentFilter(new[] { Platform.Intent.ActionAppAction },
   Categories = new[] { global::Android.Content.Intent.CategoryDefault },
   DataScheme = "robstapo",
   DataHost = "robstapochangeprivacy",
   AutoVerify = true)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnResume()
        {
            base.OnResume();

            Platform.OnResume(this);
        }
        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            Platform.OnNewIntent(intent);
        }
    }
}
