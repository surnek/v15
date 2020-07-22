﻿using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using DeepSound.Activities.Default;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace DeepSound.Activities.SettingsUser.Security
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class PasswordActivity : AppCompatActivity
    {
        #region Variables Basic

        private Toolbar Toolbar;
        private TextView SaveTextView, LinkTextView;
        private EditText CurrentPasswordEditText, NewPasswordEditText, RepeatPasswordEditText;

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
                SetContentView(Resource.Layout.PasswordLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                AdsGoogle.Ad_AdMobNative(this);
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
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
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
                SaveTextView = FindViewById<TextView>(Resource.Id.toolbar_title);
                LinkTextView = FindViewById<TextView>(Resource.Id.linkText);
                CurrentPasswordEditText = FindViewById<EditText>(Resource.Id.currentPasswordText);
                NewPasswordEditText = FindViewById<EditText>(Resource.Id.newPsswordText);
                RepeatPasswordEditText = FindViewById<EditText>(Resource.Id.repeatPasswordText);

                SaveTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetColorEditText(CurrentPasswordEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black); 
                Methods.SetColorEditText(NewPasswordEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(RepeatPasswordEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

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
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                {
                    Toolbar.Title = GetText(Resource.String.Lbl_Change_Password);
                    Toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(Toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    Toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ?  Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
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
                    SaveTextView.Click += SaveTextViewOnClick;
                    LinkTextView.Click += LinkTextViewOnClick;
                }
                else
                {
                    SaveTextView.Click -= SaveTextViewOnClick;
                    LinkTextView.Click -= LinkTextViewOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion

        #region Events

        private void LinkTextViewOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(ForgotPasswordActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        // Save New Password
        private async void SaveTextViewOnClick(object sender, EventArgs e)
        {
            try
            {
                if (CurrentPasswordEditText.Text == "" || NewPasswordEditText.Text == "" || RepeatPasswordEditText.Text == "")
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Long).Show();
                    return;
                }

                if (NewPasswordEditText.Text != RepeatPasswordEditText.Text || CurrentPasswordEditText.Text != UserDetails.Password)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Your_password_dont_match), ToastLength.Long).Show();
                }
                else
                {
                    if (Methods.CheckConnectivity())
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        //Send Api
                        (int apiStatus, var respond) = await RequestsAsync.User.ChangePasswordAsync(UserDetails.UserId.ToString(), CurrentPasswordEditText.Text, NewPasswordEditText.Text, RepeatPasswordEditText.Text);
                        if (apiStatus == 200)
                        {
                            UserDetails.Password = NewPasswordEditText.Text;
                            AndHUD.Shared.ShowSuccess(this, GetText(Resource.String.Lbl_Done), MaskType.Clear, TimeSpan.FromSeconds(2));
                            Finish();
                        }
                        else 
                        {
                            if (respond is ErrorObject error)
                            {
                                var errorText = error.Error.Replace("&#039;", "'");
                                AndHUD.Shared.ShowError(this, errorText, MaskType.Clear, TimeSpan.FromSeconds(2));
                            }
                            Methods.DisplayReportResult(this, respond);
                        }

                        AndHUD.Shared.Dismiss(this);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion

    }
}