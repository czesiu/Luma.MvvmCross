using Android.App;
using Android.OS;

namespace Xamarin.Android.DataForm
{
    [Activity(Label = "Xamarin.Android.DataForm", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
        }
    }
}

