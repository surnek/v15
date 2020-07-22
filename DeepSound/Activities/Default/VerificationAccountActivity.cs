using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient;
using DeepSoundClient.Classes.Auth;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace DeepSound.Activities.Default
{
    [Activity(Icon ="@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class VerificationAccountActivity : AppCompatActivity
    {
        #region Variables Basic

        private EditText TxtNumber1;
        private Button BtnVerify, BtnReSent;
        private string  /*Email, */Type;
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
                SetContentView(Resource.Layout.VerificationAccountLayout);

                Type = Intent.GetStringExtra("Type") ?? "";
                //Email = Intent.GetStringExtra("Email") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
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
                TxtNumber1 = FindViewById<EditText>(Resource.Id.TextNumber1);
                BtnVerify = FindViewById<Button>(Resource.Id.verifyButton);
                BtnReSent = FindViewById<Button>(Resource.Id.reSentButton);

                BtnReSent.Visibility = ViewStates.Gone;

                Methods.SetColorEditText(TxtNumber1, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = " ";
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
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
                    BtnVerify.Click += BtnVerifyOnClick;
                    //BtnReSent.Click += BtnReSentOnClick;
                }
                else
                {
                    BtnVerify.Click -= BtnVerifyOnClick;
                    //BtnReSent.Click -= BtnReSentOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private async void BtnVerifyOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(TxtNumber1.Text) && !string.IsNullOrWhiteSpace(TxtNumber1.Text))
                {
                    if (Type == "TwoFactor")
                    {
                        (int apiStatus, var respond) = await RequestsAsync.Auth.TwoFactorAsync(UserDetails.UserId.ToString(), TxtNumber1.Text);
                        if (apiStatus == 200)
                        {
                            if (respond is LoginObject auth)
                            {
                                AndHUD.Shared.Dismiss(this);

                                SetDataLogin(auth);
                                StartActivity(new Intent(this, typeof(BoardingActivity)));
                                Finish(); 
                            }
                        }
                        else
                        {
                            if (respond is ErrorObject errorMessage)
                            {
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorMessage.Error, GetText(Resource.String.Lbl_Ok));
                            }
                            Methods.DisplayReportResult(this, respond);
                        }
                    }
                    //else
                    //{
                    //    if (Methods.CheckConnectivity())
                    //    {
                    //        //Show a progress
                    //        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    //        (int apiStatus, var respond) = await RequestsAsync.Auth.ActivateAccountAsync(Email, TxtNumber1.Text, UserDetails.DeviceId);
                    //        if (apiStatus == 200)
                    //        {
                    //            if (respond is LoginObject auth)
                    //            {
                    //                SetDataLogin(auth);
                    //                AndHUD.Shared.Dismiss(this);

                    //                StartActivity(AppSettings.ShowWalkTroutPage ? new Intent(this, typeof(BoardingActivity)) : new Intent(this, typeof(HomeActivity)));
                    //                Finish();
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (respond is ErrorObject errorMessage)
                    //            {
                    //                var errorId = errorMessage.ErrorData.ErrorId;
                    //                if (errorId == "3")
                    //                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CodeNotCorrect), GetText(Resource.String.Lbl_Ok));
                    //            }
                    //            Methods.DisplayReportResult(this, respond);
                    //        }

                    //        AndHUD.Shared.Dismiss(this);
                    //    }
                    //    else
                    //    {
                    //        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    //    }
                    //} 
                }
                else
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        //private async void BtnReSentOnClick(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (Methods.CheckConnectivity())
        //        {
        //            //Show a progress
        //            AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

        //            (int apiStatus, var respond) = await RequestsAsync.Auth.ResendEmailAsync(Email);
        //            if (apiStatus != 200)
        //                Methods.DisplayReportResult(this, respond);

        //            AndHUD.Shared.Dismiss(this);
        //        }
        //        else
        //        {
        //            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.WriteLine(exception);
        //        AndHUD.Shared.Dismiss(this);
        //    }
        //}

        #endregion

        private void SetDataLogin(LoginObject auth)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.Data.Id;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId.ToString(),
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UserDetails.Username,
                    Password = UserDetails.Password,
                    Status = "Pending",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId,
                    Email = UserDetails.Email,
                };
                ListUtils.DataUserLoginList.Add(user);

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);

                if (auth.Data != null)
                {
                    dbDatabase.InsertOrUpdate_DataMyInfo(auth.Data);
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetInfoData(this, UserDetails.UserId.ToString()) });
                }

                dbDatabase.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}