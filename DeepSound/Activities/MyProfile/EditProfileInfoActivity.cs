using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using AndroidHUD;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Exception = System.Exception;

namespace DeepSound.Activities.MyProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class EditProfileInfoActivity : AppCompatActivity
    {
        #region Variables Basic

        private TextView BackIcon, NameIcon,  AboutIcon, FacebookIcon, WebsiteIcon;
        private EditText EdtFullName, EdtAbout, EdtFacebook, EdtWebsite;
        private Button BtnSave;
        private AdView MAdView;
        private AdsGoogle.AdMobRewardedVideo RewardedVideoAd;

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
                SetContentView(Resource.Layout.EditMyProfileLayout);

                //Get Value And Set Toolbar
                InitComponent();

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, null);

                GetMyInfoData();

                RewardedVideoAd = AdsGoogle.Ad_RewardedVideo(this);
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
                RewardedVideoAd?.OnResume(this);
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
                RewardedVideoAd?.OnPause(this);
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
                RewardedVideoAd?.OnDestroy(this);
                MAdView?.Destroy();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                BackIcon = FindViewById<TextView>(Resource.Id.IconBack);

                NameIcon = FindViewById<TextView>(Resource.Id.IconFullName);
                EdtFullName = FindViewById<EditText>(Resource.Id.FullNameEditText);

                AboutIcon = FindViewById<TextView>(Resource.Id.IconAbout);
                EdtAbout = FindViewById<EditText>(Resource.Id.AboutEditText);

                FacebookIcon = FindViewById<TextView>(Resource.Id.IconFacebook);
                EdtFacebook = FindViewById<EditText>(Resource.Id.FacebookEditText);

                WebsiteIcon = FindViewById<TextView>(Resource.Id.IconWebsite);
                EdtWebsite = FindViewById<EditText>(Resource.Id.WebsiteEditText);

                BtnSave = FindViewById<Button>(Resource.Id.ApplyButton);
                
                Methods.SetColorEditText(EdtFullName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtAbout, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtFacebook, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtWebsite, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, BackIcon, IonIconsFonts.ChevronLeft);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, NameIcon, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, AboutIcon, FontAwesomeIcon.InfoCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, FacebookIcon, IonIconsFonts.SocialFacebookOutline);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, WebsiteIcon, FontAwesomeIcon.GlobeAmericas);
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
                    BackIcon.Click += BackIconOnClick;
                    BtnSave.Click += BtnSaveOnClick;
                }
                else
                {
                    BackIcon.Click -= BackIconOnClick;
                    BtnSave.Click -= BtnSaveOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        #endregion

        #region Events
         
        //Click save data and sent api
        private async void BtnSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    

                    var dictionary = new Dictionary<string, string>
                    {
                        {"name", EdtFullName.Text},                      
                        {"about_me", EdtAbout.Text},
                        {"facebook", EdtFacebook.Text},
                        {"website", EdtWebsite.Text}, 
                    };

                    if (string.IsNullOrEmpty(dictionary["website"]))
                        dictionary["website"] = "https://www.example.com/";

                    (int apiStatus, var respond) = await RequestsAsync.User.UpdateProfileAsync(UserDetails.UserId.ToString(),dictionary);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            Console.WriteLine(result.Message);
                            var local = ListUtils.MyUserInfoList.FirstOrDefault();
                            if (local != null)
                            {
                                local.Name = EdtFullName.Text;
                                local.About = EdtAbout.Text;
                                local.Facebook = EdtFacebook.Text;
                                local.Website = EdtWebsite.Text;

                                //TextSanitizer aboutSanitizer = new TextSanitizer(HomeActivity.GetInstance()?.ProfileFragment.TxtAbout, this);
                                //aboutSanitizer.Load(Methods.FunString.DecodeString(EdtAbout.Text));
                                 
                                SqLiteDatabase database = new SqLiteDatabase();
                                database.InsertOrUpdate_DataMyInfo(local);
                                database.Dispose();
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Short).Show();
                            AndHUD.Shared.Dismiss(this);

                           
                            Intent returnIntent = new Intent();

                            returnIntent.PutExtra("name", dictionary["name"]);
                            returnIntent.PutExtra("about_me", dictionary["about_me"]);
                            returnIntent.PutExtra("facebook", dictionary["facebook"]);
                            returnIntent.PutExtra("website", dictionary["website"]);

                            SetResult(Result.Ok, returnIntent);

                            Finish();
                        }
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
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        //Back
        private void BackIconOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent resultIntent = new Intent();
                SetResult(Result.Canceled, resultIntent);
                Finish();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        private void GetMyInfoData()
        {
            try
            {
                if (ListUtils.MyUserInfoList.Count == 0)
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.GetDataMyInfo();
                    sqlEntity.Dispose();
                }

                var dataUser = ListUtils.MyUserInfoList.FirstOrDefault();
                if (dataUser != null)
                {
                    EdtFullName.Text = dataUser.Name;
                    EdtAbout.Text = Methods.FunString.DecodeString(dataUser.About);
                    EdtFacebook.Text = dataUser.Facebook; 
                    EdtWebsite.Text = dataUser.Website; 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}