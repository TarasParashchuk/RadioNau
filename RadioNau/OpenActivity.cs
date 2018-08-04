using Android.OS;
using Android.App;
using Android.Content.PM;
using Android.Content;
using RadioNau.Services;

namespace RadioNau
{
    [Activity(Theme = "@style/Theme.Splash",
        MainLauncher = true,
        ScreenOrientation = ScreenOrientation.Portrait,
        NoHistory = false)]
    public class OpenActivity : Activity
    {
        public static bool pause = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            StartService(new Intent(GetInfoRadio.ActionStartGetInfoRadio));
            StartActivity(new Intent(this, typeof(MainActivity)));
        }
    }
}