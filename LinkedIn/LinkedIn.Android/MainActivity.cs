using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Widget;
using Android.OS;
using Plugin.LinkedInClient;

namespace LinkedIn.Droid
{
    [Activity(Label = "LinkedIn", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
		private LinkedInClientManager linkedInClientManager;
        protected override void OnCreate(Bundle bundle)
        {
            
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            LinkedInClientManager.Initialize(this);
			linkedInClientManager = (LinkedInClientManager)CrossLinkedInClient.Current;
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            linkedInClientManager.OnActivityResult(requestCode, resultCode, data);
        }
    }
}

