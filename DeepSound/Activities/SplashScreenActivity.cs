using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using DeepSound.Activities.Default;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.SQLite;
using DeepSoundClient;
using Java.Lang;
using Exception = System.Exception;

namespace DeepSound.Activities
{
    [Activity(MainLauncher = true, Icon = "@mipmap/icon", Theme = "@style/SplashScreenTheme", NoHistory = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SplashScreenActivity : AppCompatActivity
    {
        #region Variables Basic

        private SqLiteDatabase DbDatabase;

        #endregion

        protected override void OnResume()
        {
            try
            {
                base.OnResume();

                DbDatabase = new SqLiteDatabase();
                DbDatabase.CheckTablesStatus();

                new Handler(Looper.MainLooper).Post(new Runnable(FirstRunExcite));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void FirstRunExcite()
        {
            try
            {
                DbDatabase = new SqLiteDatabase();
                DbDatabase.CheckTablesStatus();

                if (!string.IsNullOrEmpty(AppSettings.Lang))
                {
                    LangController.SetApplicationLang(this, AppSettings.Lang);
                }
                else
                {
                    UserDetails.LangName = Resources.Configuration.Locale.Language.ToLower();
                    LangController.SetApplicationLang(this, UserDetails.LangName);
                }

                var result = DbDatabase.Get_data_Login_Credentials();
                if (result != null)
                {
                    Current.AccessToken = result.AccessToken;

                    switch (result.Status)
                    {
                        case "Active":
                            UserDetails.IsLogin = true;
                            StartActivity(new Intent(this, typeof(HomeActivity)));
                            break;
                        case "Pending":
                            UserDetails.IsLogin = false;
                            StartActivity(new Intent(this, typeof(HomeActivity)));
                            break;
                        default:
                            StartActivity(new Intent(this, typeof(FirstActivity)));
                            break;
                    }
                }
                else
                {
                    StartActivity(new Intent(this, typeof(FirstActivity)));
                }
                DbDatabase.Dispose();
                 
                if (AppSettings.ShowAdMobBanner || AppSettings.ShowAdMobInterstitial || AppSettings.ShowAdMobRewardVideo)
                    MobileAds.Initialize(this, GetString(Resource.String.admob_app_id));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Toast.MakeText(this, e.Message, ToastLength.Short).Show();
            }
        }
    }
}