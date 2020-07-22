using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace DeepSound.Activities.SettingsUser.Support
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AboutAppActivity : AppCompatActivity
    {
        #region Variables Basic

        private TextView TxtAppName, TxtPackageName, TxtCountVersion;
        private TextView IconVersion, IconChangelog, IconRateApp, IconTerms, IconPrivacy, IconAbout;
        private LinearLayout LayoutChangelog, LayoutRate, LayoutTerms, LayoutPrivacy, LayoutAbout;
        private AdView MAdView;
        private CardView CardView1, CardView2;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                // Create your application here
                SetContentView(Resource.Layout.AboutAppLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                
                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
                MAdView?.Resume();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                MAdView?.Pause();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                MAdView?.Destroy();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                TxtAppName = FindViewById<TextView>(Resource.Id.appName);
                TxtPackageName = FindViewById<TextView>(Resource.Id.appUsername);
                IconVersion = FindViewById<TextView>(Resource.Id.iconVersion);
                TxtCountVersion = FindViewById<TextView>(Resource.Id.countVersion);
                IconChangelog = FindViewById<TextView>(Resource.Id.iconChangelog);
                IconRateApp = FindViewById<TextView>(Resource.Id.iconRateApp);
                IconTerms = FindViewById<TextView>(Resource.Id.iconTerms);
                IconPrivacy = FindViewById<TextView>(Resource.Id.iconPrivacy);
                IconAbout = FindViewById<TextView>(Resource.Id.iconAbout);

                CardView1 = FindViewById<CardView>(Resource.Id.cardView1);
                CardView2 = FindViewById<CardView>(Resource.Id.cardView2);

                CardView1.SetBackgroundColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#efefef"));
                CardView2.SetBackgroundColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#efefef"));


                LayoutChangelog = FindViewById<LinearLayout>(Resource.Id.layoutChangelog);
                LayoutRate = FindViewById<LinearLayout>(Resource.Id.layoutRate);
                LayoutTerms = FindViewById<LinearLayout>(Resource.Id.layoutTerms);
                LayoutPrivacy = FindViewById<LinearLayout>(Resource.Id.layoutPrivacy);
                LayoutAbout = FindViewById<LinearLayout>(Resource.Id.layoutAbout);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconVersion, FontAwesomeIcon.InfoCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconChangelog, FontAwesomeIcon.SyncAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconRateApp, IonIconsFonts.AndroidStarHalf);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconTerms, FontAwesomeIcon.FileContract);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconPrivacy, FontAwesomeIcon.UserSecret);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeBrands, IconAbout, FontAwesomeIcon.Medapps);

                TxtAppName.Text = AppSettings.ApplicationName;

                PackageInfo info = PackageManager.GetPackageInfo(PackageName, 0);
                //int versionNumber = info.VersionCode;
                string versionName = info.VersionName;

                TxtPackageName.Text = PackageName;
                TxtCountVersion.Text = versionName;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_About);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ?  Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    LayoutChangelog.Click += LayoutChangelogOnClick;
                    LayoutRate.Click += LayoutRateOnClick;
                    LayoutTerms.Click += LayoutTermsOnClick;
                    LayoutPrivacy.Click += LayoutPrivacyOnClick;
                    LayoutAbout.Click += LayoutAboutOnClick;
                }
                else
                {
                    LayoutChangelog.Click -= LayoutChangelogOnClick;
                    LayoutRate.Click -= LayoutRateOnClick;
                    LayoutTerms.Click -= LayoutTermsOnClick;
                    LayoutPrivacy.Click -= LayoutPrivacyOnClick;
                    LayoutAbout.Click -= LayoutAboutOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        //About
        private void LayoutAboutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", Client.WebsiteUrl + "/terms/about");
                intent.PutExtra("Type", GetString(Resource.String.Lbl_About));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Privacy
        private void LayoutPrivacyOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", Client.WebsiteUrl + "/terms/privacy");
                intent.PutExtra("Type", GetText(Resource.String.Lbl_PrivacyPolicy));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void LayoutTermsOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", Client.WebsiteUrl + "/terms/terms");
                intent.PutExtra("Type", GetText(Resource.String.Lbl_TermsOfUse));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open PackageName In Google play
        private void LayoutRateOnClick(object sender, EventArgs e)
        {
            try
            {
                new IntentController(this).OpenAppOnGooglePlay(PackageName);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open PackageName In Google play
        private void LayoutChangelogOnClick(object sender, EventArgs e)
        {
            try
            {
                new IntentController(this).OpenAppOnGooglePlay(PackageName);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion
    }
}