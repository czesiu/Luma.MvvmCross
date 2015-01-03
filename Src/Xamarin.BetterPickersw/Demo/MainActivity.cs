using Android.App;
using Android.Support.V4.App;
using Android.Widget;
using Android.OS;
using Xamarin.BetterPickers;

namespace Demo
{
    [Activity(Label = "Demo", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : FragmentActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += (sender, args) =>
            {
                NumberPickerBuilder npb = new NumberPickerBuilder()
                    .setFragmentManager(SupportFragmentManager)
                    .setStyleResId(Resource.Style.BetterPickersDialogFragment);
                npb.show();
            };
        }
    }
}

