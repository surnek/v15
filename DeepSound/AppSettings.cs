namespace DeepSound
{
    //For the accuracy of the icon and logo, please use this website " http://nsimage.brosteins.com " and add images according to size in folders " mipmap " 

    public static class AppSettings
    {
        //Main Settings >>>>>
        public static string Cert = "QFzEsgASlG//7xkYzNFHkNdGah9t1yBL8Pmra4MczHO0gRHDXHkLOUO3i5JWaFEizrvZcl01kvNHzWbXglDFFsI9Cx4WH9dUbqAO6EZ2Pal1vInKmdPb6Sh24+qU/Nlh5TDfpwjp6/C5PMgtdoruGe2z16EwO2TAPvefo1rRy9JeG+Sv6wVT7KFBpo2DYzW2T/qi1+QmPUO6hqM5vepNLgTwQpUY1GBAkRaAvkgRdPczxcnjcz312+SWJRAwFkbezuRtb9zQDYui03RW8Il10Q==";
        //*********************************************************

        public static string ApplicationName = "DeepSound";
        public static string Version = "1.5";

        //Main Colors >>
        //*********************************************************
        public static string MainColor = "#ffa142";

        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string Lang = ""; //Default language ar_AE

        //Error Report Mode
        //*********************************************************
        public static bool SetApisReportMode = false; 

        //Notification Settings >>
        //*********************************************************
        public static string OneSignalAppId = "bfc2e195-6c3c-4123-97ce-1e122f5809a9";

        public static bool ShowNotification = true;
        public static int RefreshGetNotification = 60000; // 1 minute

        //AdMob >> Please add the code ads in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowAdMobBanner = true;
        public static bool ShowAdMobInterstitial = true;
        public static bool ShowAdMobRewardVideo = true;
        public static bool ShowAdMobNative = true; 

        public static string AdInterstitialKey = "ca-app-pub-5135691635931982/5807300922";
        public static string AdRewardVideoKey = "ca-app-pub-5135691635931982/9993834236";
        public static string AdAdMobNativeKey = "ca-app-pub-5135691635931982/1307424613"; 

        //Three times after entering the ad is displayed
        public static int ShowAdMobInterstitialCount = 3;
        public static int ShowAdMobRewardedVideoCount = 3;
        //*********************************************************

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file  
        //Facebook >> ../values/analytic.xml .. line 10 - 11
        //Google >> ../values/analytic.xml .. line 15
        //*********************************************************
        public static bool ShowFacebookLogin = true;
        public static bool ShowGoogleLogin = true;

        public static bool ShowWoWonderLogin = true; //#New

        public static string ClientId = "104516058316-9vjdctmsk63o35nbpp872is04qqa84vc.apps.googleusercontent.com";

        public static string AppNameWoWonder = "WoWonder";//#New
         
        //*********************************************************
        public static bool ShowPrice = true;
        public static bool ShowSkipButton = true; 

        //Set Theme Full Screen App
        //*********************************************************
        public static bool EnableFullScreenApp = false;

        //Set Blur Screen Comment
        //*********************************************************
        public static bool EnableBlurBackgroundComment = true;

        //Set the radius of the Blur. Supported range 0 < radius <= 25
        public static readonly float BlurRadiusComment = 25f;

        //Import && Upload Videos >>  
        //*********************************************************
        public static bool ShowButtonUploadSingleSong { get; set; } = true;
        public static bool ShowButtonUploadAlbum { get; set; } = true;  //#New 

        //Offline Sound >>  
        //*********************************************************
        public static bool AllowOfflineDownload = true;
         
        //Profile >>  
        //*********************************************************
        public static bool ShowEmail = true; //#New

        public static bool ShowForwardTrack = true; 
        public static bool ShowBackwardTrack = true; 

        //Settings Page >>  
        //*********************************************************
        public static bool ShowEditPassword = true; 
        public static bool ShowWithdrawals = true; 
        public static bool ShowGoPro = true; 
        public static bool ShowBlockedUsers = true; 
        public static bool ShowBlog = true; //#New 
        public static bool ShowSettingsTwoFactor = true; //#New 
        public static bool ShowSettingsManageSessions = true; //#New 

        //Last_Messages Page >>
        ///********************************************************* 
        public static bool RunSoundControl = true; 
        public static int RefreshChatActivitiesSeconds = 6000; // 6 Seconds
        public static int MessageRequestSpeed = 3000; // 3 Seconds

        //Set Theme App >> Color - Tab
        //*********************************************************
        public static bool SetTabLightTheme = true;
        public static bool SetTabColoredTheme = false;
        public static bool SetTabDarkTheme = false;
        public static string TabColoredColor = MainColor;

        public static bool SetTabIsTitledWithText = true;

        //Bypass Web Erros 
        ///*********************************************************
        public static bool TurnTrustFailureOnWebException = true;
        public static bool TurnSecurityProtocolType3072On = true;

        //*********************************************************
        public static bool RenderPriorityFastPostLoad = true;
         
        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// <uses-permission android:name="com.android.vending.BILLING" />
        /// </summary>
        public static bool ShowInAppBilling = false; //#New 

        public static bool ShowPaypal = true; //#New 
        public static bool ShowBankTransfer = true; //#New 
    }
} 