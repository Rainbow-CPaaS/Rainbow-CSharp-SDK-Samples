using Android.App;
using Android.Content.PM;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
[IntentFilter(new[] { Android.Content.Intent.ActionView },
    Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
    DataScheme = "rainbow")]
public class WebAuthenticationCallbackActivity : Xamarin.Essentials.WebAuthenticatorCallbackActivity
{
}