using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.OneSignal;
using DeepSound.SQLite;

namespace DeepSound.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/DarkHeader", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class FirstActivity : AppCompatActivity
    {
        #region Variables Basic

        private ImageView ImageBackgroundGradation;
        private Button BtnLogin, BtnRegister, BtnSkip;
        private TextView TxtFirstTitle;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                View mContentView = Window.DecorView;
                var uiOptions = (int)mContentView.SystemUiVisibility;
                var newUiOptions = uiOptions;

                newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.HideNavigation;
                mContentView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;

                Window.AddFlags(WindowManagerFlags.Fullscreen);
                 
                // Create your application here
                SetContentView(Resource.Layout.FirstLayout);

                //Get Value And Set Toolbar
                InitComponent();

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => ApiRequest.GetSettings_Api(this)}); 
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

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    // Check Created My Folder Or Not 
                    Methods.Path.Chack_MyFolder();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder(); 
                    }
                    else
                    { 
                        new PermissionsController(this).RequestPermission(100);
                    }
                }
                AddOrRemoveEvent(true);
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
                ImageBackgroundGradation = FindViewById<ImageView>(Resource.Id.backgroundGradation);
                BtnLogin = FindViewById<Button>(Resource.Id.LoginButton);
                BtnRegister = FindViewById<Button>(Resource.Id.RegisterButton);
                TxtFirstTitle = FindViewById<TextView>(Resource.Id.firstTitle);
                BtnSkip = FindViewById<Button>(Resource.Id.SkipButton);

                var display = WindowManager.DefaultDisplay;
                var size = new Point();
                display.GetSize(size);
                int width = size.X;
                int height = size.Y;
                 int[] color = { Color.ParseColor(AppSettings.BackgroundGradationColor1), Color.ParseColor(AppSettings.BackgroundGradationColor2) };
                var (gradient, bitmap) = ColorUtils.GetGradientDrawable(this, color, width, height);
                if (bitmap != null)
                {
                    ImageBackgroundGradation.SetImageBitmap(bitmap);
                }

                Console.WriteLine(gradient);
                TxtFirstTitle.Text = GetText(Resource.String.Lbl_FirstSubTitle);
                 
                if (!AppSettings.ShowSkipButton)
                    BtnSkip.Visibility = ViewStates.Gone;

                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.RegisterNotificationDevice();
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
                    BtnLogin.Click += BtnLoginOnClick;
                    BtnRegister.Click += BtnRegisterOnClick;
                    BtnSkip.Click += SkipButtonOnClick;
                }
                else
                {
                    BtnLogin.Click -= BtnLoginOnClick;
                    BtnRegister.Click -= BtnRegisterOnClick;
                    BtnSkip.Click -= SkipButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void SkipButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                UserDetails.Username = "";
                UserDetails.FullName = "";
                UserDetails.Password = "";
                UserDetails.AccessToken = "";
                UserDetails.UserId = 0;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = "";
                UserDetails.Email = "";
                  
                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId.ToString(),
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = "",
                    Password = "",
                    Status = "Pending",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId
                };
                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                UserDetails.IsLogin = false;

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);
                 
                dbDatabase.Dispose();

                StartActivity(new Intent(this, typeof(BoardingActivity)));
                Finish();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void BtnRegisterOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(RegisterActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void BtnLoginOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(LoginActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        #endregion

        #region Permissions  

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 100)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long).Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

    }
}