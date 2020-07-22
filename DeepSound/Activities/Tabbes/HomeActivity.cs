//###############################################################
// Author >> Elin Doughouz
// Copyright (c) DeepSound 25/04/2019 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Clans.Fab; 
using Com.Sothree.Slidinguppanel;
using Com.Theartofdev.Edmodo.Cropper;
using DeepSound.Activities.Artists;
using DeepSound.Activities.Chat;
using DeepSound.Activities.Chat.Service;
using DeepSound.Activities.Library.Listeners;
using DeepSound.Activities.Tabbes.Adapters;
using DeepSound.Activities.Tabbes.Fragments;
using DeepSound.Activities.Upload;
using DeepSound.Activities.UserProfile;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.OneSignal;
using DeepSound.Payment;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Chat;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Tracks;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using Q.Rorbin.Badgeview;
using Xamarin.PayPal.Android;
using Console = System.Console;
using Exception = System.Exception;
using Extensions = Android.Runtime.Extensions;
using Math = System.Math;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace DeepSound.Activities.Tabbes
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
    public class HomeActivity : AppCompatActivity, SlidingPaneLayout.IPanelSlideListener, SlidingUpPanelLayout.IPanelSlideListener, AppBarLayout.IOnOffsetChangedListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, ServiceResultReceiver.IReceiver
    {
        #region Variables Basic

        private static HomeActivity Instance; 
        public SlidingUpPanelLayout SlidingUpPanel; 
        public MainFeedFragment MainFragment;
        public PlaylistFragment PlaylistFragment;
        public BrowseFragment BrowseFragment;
        public LibraryFragment LibraryFragment;
        public ProfileFragment ProfileFragment;
        private FrameLayout NavigationTabBar;
        public CustomNavigationController FragmentBottomNavigator;
        private PowerManager.WakeLock Wl;
        public SoundController SoundController;
        public Timer Timer;
        private string RunTimer = "Run", TypeImage = "";
        public LibrarySynchronizer LibrarySynchronizer;
        public FloatingActionMenu MoreMultiButtons;
        private readonly Handler ExitHandler = new Handler();
        private bool RecentlyBackPressed;
        private InitPayPalPayment InitPayPalPayment;
        private ServiceResultReceiver Receiver;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                AddFlagsWakeLock();

                // Create your application here
                SetContentView(Resource.Layout.TabbedMainLayout);

                Instance = this;
                Constant.Context = this;

                LibrarySynchronizer = new LibrarySynchronizer(this);
                InitPayPalPayment = new InitPayPalPayment(this);

                //Get Value 
                InitComponent();
                SetupFloatingActionMenus();
                SetupBottomNavigationView();
                
                SoundController = new SoundController(this);
                SoundController.InitializeUi();
                 
                new Handler(Looper.MainLooper).Post(new Runnable(GetGeneralAppData)); 
                if (UserDetails.IsLogin)
                {
                    // Run timer
                    Timer = new Timer
                    {
                        Interval = AppSettings.RefreshGetNotification
                    };
                    Timer.Elapsed += TimerOnElapsed;
                    Timer.Enabled = true;
                    Timer.Start();
                }
                
                GetOneSignalNotification();
                SetService();
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

                if (Timer != null)
                {
                    Timer.Enabled = true;
                    Timer.Start();
                }
                FragmentBottomNavigator?.RewardedVideoAd?.OnResume(this);
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

                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }
                FragmentBottomNavigator?.RewardedVideoAd?.OnPause(this);

                OffWakeLock();
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
                FragmentBottomNavigator?.RewardedVideoAd?.OnDestroy(this);

                InitPayPalPayment?.StopPayPalService();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        AppSettings.SetTabDarkTheme = false;
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        AppSettings.SetTabDarkTheme = true;
                        break;
                }

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                SlidingUpPanel = FindViewById<SlidingUpPanelLayout>(Resource.Id.sliding_layout);
                SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Hidden);
                SlidingUpPanel.AddPanelSlideListener(this); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetupFloatingActionMenus()
        {
            try
            {
                MoreMultiButtons = FindViewById<FloatingActionMenu>(Resource.Id.multistroybutton);
                MoreMultiButtons.GetChildAt(0).Click += BtnUploadSingleSongOnClick;
                MoreMultiButtons.GetChildAt(1).Click += BtnUploadAnAlbumOnClick;
                MoreMultiButtons.GetChildAt(2).Click += BtnImportSongOnClick;

                MoreMultiButtons.Visibility = ViewStates.Invisible;

                if (!AppSettings.ShowButtonUploadSingleSong)
                {
                    MoreMultiButtons.GetChildAt(0).Visibility = ViewStates.Gone;
                }

                if (!AppSettings.ShowButtonUploadAlbum)
                {
                    MoreMultiButtons.GetChildAt(1).Visibility = ViewStates.Gone;
                }

                if (!AppSettings.ShowButtonUploadSingleSong && !AppSettings.ShowButtonUploadAlbum)
                {
                    MoreMultiButtons.GetChildAt(0).Visibility = ViewStates.Gone;
                    MoreMultiButtons.GetChildAt(1).Visibility = ViewStates.Gone;
                    MoreMultiButtons.GetChildAt(2).Visibility = ViewStates.Gone;
                    MoreMultiButtons.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static HomeActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public void SetToolBar(Toolbar toolbar, string title, bool showIconBack = true , bool setBackground = true )
        {
            try
            {
                if (toolbar != null)
                {
                    if (!string.IsNullOrEmpty(title))
                        toolbar.Title = title;

                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(showIconBack);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    if (setBackground)
                        toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Functions ¯\_(ツ)_/¯

        private async void GetOneSignalNotification()
        {
            try
            {
                string type = Intent.GetStringExtra("TypeNotification") ?? "Don't have type";
                if (!string.IsNullOrEmpty(type) && type != "Don't have type")
                {
                    if (type == "User")
                    {
                        OpenProfile(OneSignalNotification.UserData.Id, OneSignalNotification.UserData);
                    }
                    else if (type == "Track")
                    {
                        (int apiStatus, var respond) = await RequestsAsync.Tracks.GetTrackInfoAsync(OneSignalNotification.TrackId);
                        if (apiStatus.Equals(200))
                        {
                            if (respond is GetTrackInfoObject result)
                            {
                                Constant.PlayPos = 0;
                                SoundController?.StartPlaySound(result.Data, new ObservableCollection<SoundDataObject>() { result.Data });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                RunApiTimer();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async void RunApiTimer()
        {
            try
            {
                if (RunTimer == "Run")
                {
                    RunTimer = "Off";

                   
                        var (countNotifications, countMessages) = await ApiRequest.GetCountNotifications().ConfigureAwait(false);
                        if (MainFragment?.NotificationIcon != null && countNotifications != 0 && countNotifications != UserDetails.CountNotificationsStatic)
                        {
                            UserDetails.CountNotificationsStatic = Convert.ToInt32(countNotifications);
                            ShowOrHideBadgeViewIcon(UserDetails.CountNotificationsStatic, true);
                        }
                        else
                        {
                            ShowOrHideBadgeViewIcon();
                        }

                        Console.WriteLine(countMessages);
                        //if (tabMessages != null && countMessages != 0 && countMessages != CountMessagesStatic)
                        //{
                        //    RunOnUiThread(() =>
                        //    {
                        //        try
                        //        {
                        //            CountMessagesStatic = countMessages;
                        //            tabMessages.BadgeTitle = countMessages.ToString();
                        //            tabMessages.UpdateBadgeTitle(countMessages.ToString());
                        //            tabMessages.ShowBadge();
                        //        }
                        //        catch (Exception e)
                        //        {
                        //            Console.WriteLine(e);
                        //        }
                        //    });
                        //} 
                    
                }
                RunTimer = "Run";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                RunTimer = "Run";
            }
        }

        public void ShowOrHideBadgeViewIcon(int countNotifications = 0, bool show = false)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        if (show)
                        {
                            if (MainFragment?.NotificationIcon != null)
                            {
                                int gravity = (int)(GravityFlags.End | GravityFlags.Bottom);
                                QBadgeView badge = new QBadgeView(this);
                                badge.BindTarget(MainFragment?.NotificationIcon);
                                badge.SetBadgeNumber(countNotifications);
                                badge.SetBadgeGravity(gravity);
                                badge.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                badge.SetGravityOffset(10, true);
                            }
                        }
                        else
                        {
                            if (MainFragment?.NotificationIcon != null && countNotifications != 0 && countNotifications != UserDetails.CountNotificationsStatic)
                            {
                                new QBadgeView(this).BindTarget(MainFragment?.NotificationIcon).Hide(true);
                                UserDetails.CountNotificationsStatic = 0;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Set Tab

        private void SetupBottomNavigationView()
        {
            try
            {
                NavigationTabBar = FindViewById<FrameLayout>(Resource.Id.buttomnavigationBar);
                FragmentBottomNavigator = new CustomNavigationController(this);
               
             

                MainFragment = new MainFeedFragment();
                PlaylistFragment = new PlaylistFragment();
                BrowseFragment = new BrowseFragment();

                FragmentBottomNavigator.FragmentListTab0.Add(MainFragment);
                FragmentBottomNavigator.FragmentListTab1.Add(PlaylistFragment);
                FragmentBottomNavigator.FragmentListTab2.Add(BrowseFragment);

                if (UserDetails.IsLogin)
                {
                    LibraryFragment = new LibraryFragment();
                    ProfileFragment = new ProfileFragment();
                    FragmentBottomNavigator.FragmentListTab3.Add(LibraryFragment);
                    FragmentBottomNavigator.FragmentListTab4.Add(ProfileFragment);
                }

                FragmentBottomNavigator.ShowFragment0();

                if (UserDetails.IsLogin && LibraryFragment != null && LibraryFragment?.MAdapter == null)
                    LibraryFragment.MAdapter = new LibraryAdapter(this);

                //if (UserDetails.IsLogin && PlaylistFragment?.PlaylistAdapter == null)
                //    PlaylistFragment.PlaylistAdapter = new HPlaylistAdapter(this, true) { PlaylistList = new ObservableCollection<PlaylistDataObject>() };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Event Back
         
        public override void OnBackPressed()
        {
            try
            {
                if (SlidingUpPanel != null && (SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded || SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Anchored))
                {
                    SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                }
                else if (FragmentBottomNavigator.GetCountFragment() > 0)
                {
                    FragmentNavigatorBack();
                }
                else
                {
                    if (RecentlyBackPressed)
                    {
                        ExitHandler.RemoveCallbacks(() => { RecentlyBackPressed = false; });
                        RecentlyBackPressed = false;
                        MoveTaskToBack(true);
                    }
                    else
                    {
                        RecentlyBackPressed = true;
                        Toast.MakeText(this, GetString(Resource.String.press_again_exit), ToastLength.Long).Show();
                        ExitHandler.PostDelayed(() => { RecentlyBackPressed = false; }, 2000L);
                    }
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void FragmentNavigatorBack()
        {
            try
            {
                FragmentBottomNavigator.OnBackStackClickFragment();
            }
            catch (Exception e)
            {
                Console.WriteLine(e); 
            } 
        }

        #endregion

        #region Events

        //Upload An Album
        private void BtnUploadAnAlbumOnClick(object sender, EventArgs e)
        {
            try
            { 
                StartActivity(new Intent(this, typeof(UploadAlbumActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Upload Single Song
        private void BtnUploadSingleSongOnClick(object sender, EventArgs e)
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int) Build.VERSION.SdkInt < 23)
                    new IntentController(this).OpenIntentAudio(); //505
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        new IntentController(this).OpenIntentAudio(); //505
                    else
                        new PermissionsController(this).RequestPermission(100);
                }   
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
      
        private void BtnImportSongOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(ImportSongActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                 
                if (requestCode == 108 || requestCode == CropImage.CropImageActivityRequestCode) //==> change Avatar Or Cover
                {
                    if (Methods.CheckConnectivity())
                    {
                        var result = CropImage.GetActivityResult(data);

                        if (result.IsSuccessful)
                        {
                            var resultPathImage = result.Uri.Path;

                            if (ProfileFragment?.ImageAvatar == null || ProfileFragment?.ImageCover == null)
                                return;

                            if (!string.IsNullOrEmpty(resultPathImage))
                            {
                                if (TypeImage == "Avatar")
                                {
                                    GlideImageLoader.LoadImage(this, resultPathImage, ProfileFragment?.ImageAvatar, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                                    await Task.Run(async () =>
                                    {
                                        (int apiStatus, var respond) = await RequestsAsync.User.UpdateAvatarAsync(resultPathImage).ConfigureAwait(false);
                                        if (apiStatus.Equals(200))
                                        {
                                            if (respond is UpdateImageUserObject image)
                                            {
                                                var dataUser = ListUtils.MyUserInfoList.FirstOrDefault();
                                                if (dataUser != null)
                                                {
                                                    dataUser.Avatar = image.Img;
                                                     
                                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                                    dbDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                                                    dbDatabase.Dispose();
                                                }
                                            }
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }); 
                                }
                                else if (TypeImage == "Cover")
                                {
                                    GlideImageLoader.LoadImage(this, resultPathImage, ProfileFragment?.ImageCover, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                                    await Task.Run(async () =>
                                    {
                                        (int apiStatus, var respond) = await RequestsAsync.User.UpdateCoverAsync(resultPathImage).ConfigureAwait(false);
                                        if (apiStatus.Equals(200))
                                        {
                                            if (respond is UpdateImageUserObject image)
                                            {
                                                var dataUser = ListUtils.MyUserInfoList.FirstOrDefault();
                                                if (dataUser != null)
                                                {
                                                    dataUser.Cover = image.Img;

                                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                                    dbDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                                                    dbDatabase.Dispose();
                                                }
                                            }
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    });
                                }
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                            } 
                        }
                    }
                }
                else if (requestCode == 505) //==> Audio
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Audio")
                        {
                            Intent intent = new Intent(this, typeof(UploadSongActivity));
                            intent.PutExtra("SongLocation", filepath);
                            StartActivity(intent);
                        } 
                    }
                }
                else if (requestCode == InitPayPalPayment?.PayPalDataRequestCode)
                {
                    switch (resultCode)
                    {
                        case Result.Ok:
                            var confirmObj = data.GetParcelableExtra(PaymentActivity.ExtraResultConfirmation);
                            PaymentConfirmation configuration = Extensions.JavaCast<PaymentConfirmation>(confirmObj);
                            if (configuration != null)
                            {
                                //string createTime = configuration.ProofOfPayment.CreateTime;
                                //string intent = configuration.ProofOfPayment.Intent;
                                //string paymentId = configuration.ProofOfPayment.PaymentId;
                                //string state = configuration.ProofOfPayment.State;
                                //string transactionId = configuration.ProofOfPayment.TransactionId;

                                if (PayType == "PurchaseSong")
                                {
                                    if (Methods.CheckConnectivity())
                                    { 
                                        (int apiStatus, var respond) = await RequestsAsync.User.PurchaseTrackAsync(PaymentSoundObject.Id.ToString() ,UserDetails.UserId.ToString() , "PayPal");
                                        if (apiStatus == 200)
                                        {
                                            if (respond is MessageObject result)
                                            {
                                                Console.WriteLine(result.Message);
                                                PaymentSoundObject.IsPurchased = true;
                                                Constant.PlayPos = Constant.ArrayListPlay.IndexOf(PaymentSoundObject);
                                                SoundController?.StartPlaySound(PaymentSoundObject, Constant.ArrayListPlay);

                                                Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                                StopService(new Intent(this, typeof(PayPalService)));
                                            }
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    else
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                }
                                else if (PayType == "PurchaseAlbum")
                                {
                                    if (Methods.CheckConnectivity())
                                    {
                                        (int apiStatus, var respond) = await RequestsAsync.User.PurchaseAlbumAsync(PaymentAlbumsObject.Id.ToString(), UserDetails.UserId.ToString() ,"PayPal");
                                        if (apiStatus == 200)
                                        {
                                            if (respond is MessageObject result)
                                            {
                                                Console.WriteLine(result.Message);
                                                PaymentAlbumsObject.IsPurchased = 1;
                                                 
                                                Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                                StopService(new Intent(this, typeof(PayPalService)));
                                            }
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    else
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                }
                            }
                            break;
                        case Result.Canceled:
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long).Show();
                            break;
                    }
                }
                else if (requestCode == PaymentActivity.ResultExtrasInvalid)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Invalid), ToastLength.Long).Show();
                }
            }    
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        OpenDialogGallery(TypeImage);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long).Show();
                    }
                } 
                else if (requestCode == 100)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        new IntentController(this).OpenIntentAudio(); //505
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
         
        #region WakeLock System

        private void AddFlagsWakeLock()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.WakeLock) == Permission.Granted)
                    {
                        Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                    }
                    else
                    {
                        //request Code 110
                        new PermissionsController(this).RequestPermission(110);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetWakeLock()
        {
            try
            {
                if (Wl == null)
                {
                    PowerManager pm = (PowerManager)GetSystemService(PowerService);
                    Wl = pm.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                    Wl.Acquire();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OffWakeLock()
        {
            try
            {
                // ..screen will stay on during this section..
                Wl?.Release();
                Wl = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Listener Panel Layout
         
        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            var percentage = (float)Math.Abs(verticalOffset) / appBarLayout.TotalScrollRange;
            Console.WriteLine(percentage);
        }

        public void OnPanelClosed(View panel)
        {

        }

        public void OnPanelOpened(View panel)
        {

        }

        public void OnPanelSlide(View panel, float slideOffset)
        {
            try
            {
                NavigationTabBar.Alpha = 1 - slideOffset;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public void OnPanelStateChanged(View p0, SlidingUpPanelLayout.PanelState p1, SlidingUpPanelLayout.PanelState p2)
        {
            try
            {
                if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (SoundController?.BackIcon.Tag.ToString() == "Close")
                    {
                        SoundController?.BackIcon.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                        SoundController.BackIcon.Tag = "Open";
                        SoundController?.SetUiSliding(false);
                       //NavigationTabBar.Hide();
                    }
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Hidden && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (SoundController?.BackIcon != null && SoundController?.BackIcon.Tag.ToString() == "Open")
                    {
                        SoundController?.BackIcon.SetImageResource(Resource.Drawable.ic_action_close);
                        SoundController.BackIcon.Tag = "Close";
                        SoundController?.SetUiSliding(true);
                        NavigationTabBar.Visibility = ViewStates.Gone;
                    }
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Anchored)
                {
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Expanded)
                {
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Hidden)
                {
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Dragging && p2 == SlidingUpPanelLayout.PanelState.Expanded)
                {
                    if (SoundController?.BackIcon != null && SoundController?.BackIcon.Tag.ToString() == "Close")
                    {
                        SoundController?.BackIcon.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                        SoundController.BackIcon.Tag = "Open";
                        SoundController?.SetUiSliding(false);
                        NavigationTabBar.Visibility = ViewStates.Gone;
                    }
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Dragging && p2 == SlidingUpPanelLayout.PanelState.Hidden)
                {
                    // Toast.MakeText(this, "p1 Anchored + Anchored ", ToastLength.Short).Show();
                }
                if (p1 == SlidingUpPanelLayout.PanelState.Collapsed && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (SoundController?.BackIcon != null && SoundController?.BackIcon.Tag.ToString() == "Open")
                    {
                        SoundController?.BackIcon.SetImageResource(Resource.Drawable.ic_action_close);
                        SoundController.BackIcon.Tag = "Close";
                        SoundController?.SetUiSliding(true);
                        NavigationTabBar.Visibility = ViewStates.Visible;
                    }
                }
                if (p1 == SlidingUpPanelLayout.PanelState.Dragging && p2 == SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    if (SoundController?.BackIcon != null && SoundController?.BackIcon.Tag.ToString() == "Open")
                    {
                        SoundController?.BackIcon.SetImageResource(Resource.Drawable.ic_action_close);
                        SoundController.BackIcon.Tag = "Close";
                        SoundController?.SetUiSliding(true);
                        NavigationTabBar.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        SoundController?.BtPlay.SetImageResource(Resource.Drawable.icon_player_pause);
                        SoundController?.BtnPlayImage.SetImageResource(Resource.Drawable.icon_player_pause);
                        NavigationTabBar.Visibility = ViewStates.Visible;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == GetString(Resource.String.Btn_Paypal))
                {
                    if (DialogType == "DialogPurchaseSound")
                    {
                        var price = ListUtils.PriceList.FirstOrDefault(a => a.Id == PaymentSoundObject.Price)?.Price;
                        PayType = "PurchaseSong";
                        InitPayPalPayment.BtnPaypalOnClick(price, "PurchaseSong");
                    }
                    else if(DialogType == "DialogPurchaseAlbum")
                    {
                        var price = ListUtils.PriceList.FirstOrDefault(a => a.Id == PaymentAlbumsObject.Price)?.Price;
                        PayType = "PurchaseAlbum";
                        InitPayPalPayment.BtnPaypalOnClick(price, "PurchaseAlbum"); 
                    }
                } 
                else if (text == GetString(Resource.String.Lbl_BankTransfer))
                {
                    OpenIntentBankTransfer();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                   
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();

                    //if (Constant.PlayPos < Constant.ArrayListPlay.Count - 1)
                    //    Constant.PlayPos += 1;
                    //else
                    //    Constant.PlayPos = 0;

                    //var nextSoundObject = Constant.ArrayListPlay[Constant.PlayPos];
                    //if (nextSoundObject != null)
                    //{
                    //    SoundController?.StartPlaySound(nextSoundObject, Constant.ArrayListPlay);
                    //} 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OpenIntentBankTransfer()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PaymentLocalActivity));

                if (DialogType == "DialogPurchaseSound")
                {
                    var price = ListUtils.PriceList.FirstOrDefault(a => a.Id == PaymentSoundObject.Price)?.Price;
                    PayType = "PurchaseSong";
                    intent.PutExtra("Id", PaymentSoundObject.AudioId);
                    intent.PutExtra("Price", price);
                    intent.PutExtra("payType", "track");
                }
                else if (DialogType == "DialogPurchaseAlbum")
                {
                    var price = ListUtils.PriceList.FirstOrDefault(a => a.Id == PaymentAlbumsObject.Price)?.Price;
                    PayType = "PurchaseAlbum";
                    intent.PutExtra("Id", PaymentAlbumsObject.AlbumId);
                    intent.PutExtra("Price", price);
                    intent.PutExtra("payType", "album");
                }

                StartActivity(intent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Purchase the song Or album

        private static SoundDataObject PaymentSoundObject;
        private static DataAlbumsObject PaymentAlbumsObject;
        private string PayType , DialogType = "";

        public void OpenDialogPurchaseSound(SoundDataObject soundObject)
        {
            try
            {
                PaymentSoundObject = soundObject;
                 
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                dialog.Title(Resource.String.Lbl_PurchaseRequired);
                dialog.Content(GetText(Resource.String.Lbl_PurchaseRequiredContent));
                dialog.PositiveText(GetText(Resource.String.Lbl_Purchase)).OnPositive((materialDialog, action) =>
                {
                    try
                    {
                        DialogType = "DialogPurchaseSound";
                        OpenDialogSelectPayment(); 
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public void OpenDialogPurchaseAlbum(DataAlbumsObject albumsObject)
        {
            try
            {
                PaymentAlbumsObject = albumsObject;

                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                dialog.Title(Resource.String.Lbl_PurchaseRequired);
                dialog.Content(GetText(Resource.String.Lbl_PurchaseRequiredContent));
                dialog.PositiveText(GetText(Resource.String.Lbl_Purchase)).OnPositive((materialDialog, action) =>
                {
                    try
                    {
                        DialogType = "DialogPurchaseAlbum";
                        OpenDialogSelectPayment(); 
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OpenDialogSelectPayment()
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                if (AppSettings.ShowPaypal)
                    arrayAdapter.Add(GetString(Resource.String.Btn_Paypal));

                if (AppSettings.ShowBankTransfer)
                    arrayAdapter.Add(GetString(Resource.String.Lbl_BankTransfer));

                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
        #region Service Chat

        public void SetService(bool run = true)
        {
            try
            {
                if (!UserDetails.IsLogin)
                    return;
                 
                if (run)
                {
                    try
                    {
                        Receiver = new ServiceResultReceiver(new Handler());
                        Receiver.SetReceiver(this);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    var intent = new Intent(this, typeof(ScheduledApiService));
                    intent.PutExtra("receiverTag", Receiver);
                    StartService(intent);
                }
                else
                {
                    var intentService = new Intent(this, typeof(ScheduledApiService));
                    StopService(intentService);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnReceiveResult(int resultCode, Bundle resultData)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<GetConversationListObject>(resultData.GetString("Json"));
                if (result != null)
                {
                    LastChatActivity.GetInstance()?.LoadDataJsonLastChat(result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
          
        public void OpenDialogGallery(string typeImage)
        {
            try
            {
                TypeImage = typeImage;
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDcimImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Builder()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Done))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDcimImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Builder()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Done))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OpenProfile(long userId, UserDataObject item)
        {
            try
            {
                if (userId != UserDetails.UserId)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("UserId", userId.ToString());
                    if (item.Artist == 0)
                    { 
                        UserProfileFragment userProfileFragment = new UserProfileFragment
                        {
                            Arguments = bundle
                        };
                        FragmentBottomNavigator.DisplayFragment(userProfileFragment);
                    }
                    else
                    {
                        //open profile Artist
                        ArtistsProfileFragment artistsProfileFragment = new ArtistsProfileFragment
                        {
                            Arguments = bundle
                        };
                        FragmentBottomNavigator.DisplayFragment(artistsProfileFragment);
                    }  
                }
                else
                { 
                    if (UserDetails.IsLogin)
                        FragmentBottomNavigator.ShowFragment4();
                  
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(this, null, "Login");
                        dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Login), GetText(Resource.String.Lbl_Message_Sorry_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    }
                }

                if (SlidingUpPanel != null && SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded)
                    SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        private void GetGeneralAppData()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();

                var data = ListUtils.DataUserLoginList.FirstOrDefault();
                if (data != null && data.Status != "Active" && UserDetails.IsLogin)
                {
                    data.Status = "Active";
                    UserDetails.Status = "Active";
                    sqlEntity.InsertOrUpdateLogin_Credentials(data);
                }

                sqlEntity.GetSettings();
                if (UserDetails.IsLogin)
                {
                    sqlEntity.GetDataMyInfo();
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this), ApiRequest.GetMyPlaylist_Api, () => ApiRequest.GetInfoData(this, UserDetails.UserId.ToString()), });
                }
                else
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });
                }

                ListUtils.GenresList = sqlEntity.Get_GenresList();
                ListUtils.PriceList = sqlEntity.Get_PriceList();
                 
                sqlEntity.Dispose();
                 
                if (ListUtils.GenresList?.Count == 0)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetGenres_Api });

                if (ListUtils.PriceList?.Count == 0 && AppSettings.ShowPrice)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetPrices_Api });  

                RunOnUiThread(() =>
                {
                    try
                    {
                        if (AppSettings.ShowGoPro && UserDetails.IsLogin)
                        {
                            var pro = ListUtils.MyUserInfoList.FirstOrDefault()?.IsPro;
                            MainFragment.ProIcon.Visibility = pro == 1 ? ViewStates.Gone : ViewStates.Visible;
                        }
                        else
                        {
                            MainFragment.ProIcon.Visibility = ViewStates.Gone;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e); 
                    } 
                }); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 
    }
} 