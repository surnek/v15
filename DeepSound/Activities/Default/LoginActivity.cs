using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.SocialLogins;
using DeepSound.Helpers.Utils;
using DeepSound.Library.OneSignal;
using DeepSound.SQLite;
using DeepSoundClient;
using DeepSoundClient.Classes.Auth;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Org.Json;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme",ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class LoginActivity : AppCompatActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener, IResultCallback
    { 
        #region Variables Basic

        private EditText EmailEditText, PasswordEditText;
        private ProgressBar ProgressBar;
        private FloatingActionButton BtnSignIn;
        private TextView ForgotPassTextView, RegisterTextView;
        private Button WoWonderSignInButton;
        private LoginButton FbLoginButton;
        private SignInButton GoogleSignInButton;

        private ICallbackManager MFbCallManager;
        private FbMyProfileTracker MprofileTracker;
        public static GoogleApiClient MGoogleApiClient;
        public static LoginActivity Instance;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                 
                //Set Full screen 
                View mContentView = Window.DecorView;
                var uiOptions = (int)mContentView.SystemUiVisibility;
                var newUiOptions = uiOptions;

                newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.HideNavigation;
                mContentView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;

                Window.AddFlags(WindowManagerFlags.Fullscreen);

                // Create your application here
                SetContentView(Resource.Layout.LoginLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitSocialLogins();

                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.RegisterNotificationDevice();
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

        protected override void OnStop()
        {
            try
            {
                base.OnStop();
                if (MGoogleApiClient.IsConnected) MGoogleApiClient.Disconnect();
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

        #region Functions

        private void InitComponent()
        {
            try
            {
                EmailEditText = FindViewById<EditText>(Resource.Id.edt_email);
                PasswordEditText = FindViewById<EditText>(Resource.Id.edt_password);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progress_bar);
                BtnSignIn = FindViewById<FloatingActionButton>(Resource.Id.fab);
                ForgotPassTextView = FindViewById<TextView>(Resource.Id.txt_forgot_pass);
                RegisterTextView = FindViewById<TextView>(Resource.Id.txt_Regsiter);
               
                ProgressBar.Visibility = ViewStates.Gone;
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
                    BtnSignIn.Click += BtnSignInOnClick;
                    ForgotPassTextView.Click += ForgotPassTextViewOnClick;
                    RegisterTextView.Click += LinearRegisterOnClick;
                }
                else
                {
                    BtnSignIn.Click -= BtnSignInOnClick;
                    ForgotPassTextView.Click -= ForgotPassTextViewOnClick;
                    RegisterTextView.Click -= LinearRegisterOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitSocialLogins()
        {
            try
            {
                //#Facebook
                if (AppSettings.ShowFacebookLogin)
                {
                    //FacebookSdk.SdkInitialize(this);

                    MprofileTracker = new FbMyProfileTracker();
                    MprofileTracker.MOnProfileChanged += MprofileTrackerOnMOnProfileChanged;
                    MprofileTracker.StartTracking();

                    FbLoginButton = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    FbLoginButton.Visibility = ViewStates.Visible;
                     FbLoginButton.SetPermissions(new List<string>
                    {
                        "email",
                        "public_profile"
                    });

                    MFbCallManager = CallbackManagerFactory.Create();
                    FbLoginButton.RegisterCallback(MFbCallManager, this);

                    //FB accessToken
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }

                    string hash = Methods.App.GetKeyHashesConfigured(this);
                    Console.WriteLine(hash);
                }
                else
                {
                    FbLoginButton = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    FbLoginButton.Visibility = ViewStates.Gone;
                }

                //#Google
                if (AppSettings.ShowGoogleLogin)
                { 
                    GoogleSignInButton = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    GoogleSignInButton.Click += GoogleSignInButtonOnClick;
                }
                else
                {
                    GoogleSignInButton = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    GoogleSignInButton.Visibility = ViewStates.Gone;
                }

                //#WoWonder 
                if (AppSettings.ShowWoWonderLogin)
                {
                    WoWonderSignInButton = FindViewById<Button>(Resource.Id.WoWonderLogin_button);
                    WoWonderSignInButton.Click += WoWonderSignInButtonOnClick;

                    WoWonderSignInButton.Text = GetString(Resource.String.Lbl_LoginWith) + " " + AppSettings.AppNameWoWonder;
                    WoWonderSignInButton.Visibility = ViewStates.Visible;
                }
                else
                {
                    WoWonderSignInButton = FindViewById<Button>(Resource.Id.WoWonderLogin_button);
                    WoWonderSignInButton.Visibility = ViewStates.Gone;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Login With Facebook
        private void MprofileTrackerOnMOnProfileChanged(object sender, ProfileChangedEventArgs e)
        {
            try
            {
                if (e.MProfile != null)
                {
                    //FbFirstName = e.MProfile.FirstName;
                    //FbLastName = e.MProfile.LastName;
                    //FbName = e.MProfile.Name;
                    //FbProfileId = e.MProfile.Id;

                    var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
                    var parameters = new Bundle();
                    parameters.PutString("fields", "id,name,age_range,email");
                    request.Parameters = parameters;
                    request.ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        //Login With Google
        private void GoogleSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                    .RequestIdToken(AppSettings.ClientId)
                    .RequestScopes(new Scope(Scopes.Profile))
                    .RequestScopes(new Scope(Scopes.PlusMe))
                    .RequestScopes(new Scope(Scopes.DriveAppfolder))
                    .RequestServerAuthCode(AppSettings.ClientId)
                    .RequestProfile().RequestEmail().Build();

                MGoogleApiClient ??= new GoogleApiClient.Builder(this, this, this)
                    .EnableAutoManage(this, this)
                    .AddApi(Auth.GOOGLE_SIGN_IN_API, gso)
                    .Build();
                        .EnableAutoManage(this, this)
                        .AddApi(Auth.GOOGLE_SIGN_IN_API, gso)
                        .Build();

                MGoogleApiClient.Connect();
                 
                var opr = Auth.GoogleSignInApi.SilentSignIn(MGoogleApiClient);
                if (opr.IsDone)
                {
                    // If the user's cached credentials are valid, the OptionalPendingResult will be "done"
                    // and the GoogleSignInResult will be available instantly.
                    Log.Debug("Login_Activity", "Got cached sign-in");
                    var result = opr.Get() as GoogleSignInResult;
                    HandleSignInResult(result);

                    //Auth.GoogleSignInApi.SignOut(mGoogleApiClient).SetResultCallback(this);
                }
                else
                {
                    // If the user has not previously signed in on this device or the sign-in has expired,
                    // this asynchronous branch will attempt to sign in the user silently.  Cross-device
                    // single sign-on will occur in this branch.
                    opr.SetResultCallback(new SignInResultCallback { Activity = this });
                }

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    if (!MGoogleApiClient.IsConnecting)
                        ResolveSignInError();
                    else if (MGoogleApiClient.IsConnected) MGoogleApiClient.Disconnect();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.GetAccounts) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.UseCredentials) == Permission.Granted)
                    {
                        if (!MGoogleApiClient.IsConnecting)
                            ResolveSignInError();
                        else if (MGoogleApiClient.IsConnected) MGoogleApiClient.Disconnect();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(106);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion

        #region Events

        //Login DeepSound
        private async void BtnSignInOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (!string.IsNullOrEmpty(EmailEditText.Text.Replace(" ", "")) || !string.IsNullOrEmpty(PasswordEditText.Text))
                    {
                        ProgressBar.Visibility = ViewStates.Visible;
                        BtnSignIn.Visibility = ViewStates.Gone;

                        (int apiStatus, var respond) = await RequestsAsync.Auth.LoginAsync(EmailEditText.Text.Replace(" ",""), PasswordEditText.Text, UserDetails.DeviceId);
                        if (apiStatus == 200)
                        {
                            if (respond is LoginObject auth)
                            {
                                SetDataLogin(auth);

                                StartActivity(new Intent(this, typeof(BoardingActivity)));
                                Finish();
                            }
                            else if (respond is LoginTwoFactorObject factorObject)
                            {
                                UserDetails.UserId = factorObject.UserId;
                                var intent = new Intent(this, typeof(VerificationAccountActivity));
                                intent.PutExtra("Type", "TwoFactor");
                                StartActivity(intent);

                                Finish();
                            }
                        }
                        else if (apiStatus == 400)
                        {
                            if (respond is ErrorObject error)
                            {
                                string errorText = error.Error;
                                switch (errorText)
                                {
                                    case "Please check your details":
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorPleaseCheckYourDetails), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case "Incorrect username or password":
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),GetText(Resource.String.Lbl_ErrorLogin2), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case "Your account is not activated yet, please check your inbox for the activation link":
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin3), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    default:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                        break;
                                }
                            }

                            ProgressBar.Visibility = ViewStates.Gone;
                            BtnSignIn.Visibility = ViewStates.Visible;
                        }
                        else if (apiStatus == 404)
                        {
                            ProgressBar.Visibility = ViewStates.Gone;
                            BtnSignIn.Visibility = ViewStates.Visible;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    BtnSignIn.Visibility = ViewStates.Visible;
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), ex.Message, GetText(Resource.String.Lbl_Ok));
            }
        }

        //Open Register
        private void LinearRegisterOnClick(object sender, EventArgs e)
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
          
        //Open Forgot Password
        private void ForgotPassTextViewOnClick(object sender, EventArgs e)
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
        
        public void SetDataLogin(LoginObject auth)
        {
            try
            {
                UserDetails.Username = EmailEditText.Text;
                UserDetails.FullName = EmailEditText.Text;
                UserDetails.Password = PasswordEditText.Text;
                UserDetails.AccessToken = auth.AccessToken;
               UserDetails.UserId = auth.Data.Id;
                UserDetails.Status = "Active";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = EmailEditText.Text;

                Current.AccessToken = auth.AccessToken;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId.ToString(),
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = EmailEditText.Text,
                    Password = PasswordEditText.Text,
                    Status = "Active",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId
                };
                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                UserDetails.IsLogin = true;

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);

                if (auth.Data != null)
                {
                    ListUtils.MyUserInfoList.Add(auth.Data);
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetInfoData(this ,UserDetails.UserId.ToString()) }); 
                }

                dbDatabase.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Social Logins

        private string FbAccessToken, GAccessToken, GServerCode;

        #region Facebook

        public void OnCancel()
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;

                SetResult(Result.Canceled);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnError(FacebookException error)
        {
            try
            {

                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;

                // Handle e
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

                SetResult(Result.Canceled);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnSuccess(Object result)
        {
            try
            {
                //var loginResult = result as LoginResult;
                //var id = AccessToken.CurrentAccessToken.UserId;

                ProgressBar.Visibility = ViewStates.Visible;
                BtnSignIn.Visibility = ViewStates.Gone;

                SetResult(Result.Ok);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async void OnCompleted(JSONObject json, GraphResponse response)
        {
            try
            {
                //var data = json.ToString();
                //var result = JsonConvert.DeserializeObject<FacebookResult>(data);
                //FbEmail = result.Email;

                ProgressBar.Visibility = ViewStates.Visible;
                BtnSignIn.Visibility = ViewStates.Gone;

                var accessToken = AccessToken.CurrentAccessToken;
                if (accessToken != null)
                {
                    FbAccessToken = accessToken.Token;

                    //Login Api 
                    var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(FbAccessToken, "facebook", UserDetails.DeviceId);
                    if (apiStatus == 200)
                    {
                        if (respond is LoginObject auth)
                        {
                            SetDataLogin(auth);

                            StartActivity(new Intent(this, typeof(BoardingActivity)));
                            Finish();
                        }
                    }
                    else if (apiStatus == 400)
                    {
                        if (respond is ErrorObject error)
                        {
                            string errorText = error.Error;
                            switch (errorText)
                            {
                                case "Please check your details":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorPleaseCheckYourDetails), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "Incorrect username or password":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin2), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "Your account is not activated yet, please check your inbox for the activation link":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin3), GetText(Resource.String.Lbl_Ok));
                                    break;
                                default:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                    }
                    else if (apiStatus == 404)
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception e)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), e.Message, GetText(Resource.String.Lbl_Ok));
                Console.WriteLine(e);
            }
        }

        #endregion

        //======================================================

        #region Google

        public void HandleSignInResult(GoogleSignInResult result)
        {
            try
            {
                Log.Debug("Login_Activity", "handleSignInResult:" + result.IsSuccess);
                if (result.IsSuccess)
                {
                    // Signed in successfully, show authenticated UI.
                    var acct = result.SignInAccount;
                    SetContentGoogle(acct);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ResolveSignInError()
        {
            try
            {
                if (MGoogleApiClient.IsConnecting) return;

                var signInIntent = Auth.GoogleSignInApi.GetSignInIntent(MGoogleApiClient);
                StartActivityForResult(signInIntent, 0);
            }
            catch (Exception e)
            {
                //The intent was cancelled before it was sent. Return to the default
                //state and attempt to connect to get an updated ConnectionResult
                Console.WriteLine(e);
                MGoogleApiClient.Connect();

            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            try
            {
                var opr = Auth.GoogleSignInApi.SilentSignIn(MGoogleApiClient);
                if (opr.IsCanceled)
                {
                    // If the user's cached credentials are valid, the OptionalPendingResult will be "done"
                    // and the GoogleSignInResult will be available instantly.
                    Log.Debug("Login_Activity", "Got cached sign-in");
                    var result = opr.Get() as GoogleSignInResult;
                    HandleSignInResult(result);
                }
                else
                {
                    // If the user has not previously signed in on this device or the sign-in has expired,
                    // this asynchronous branch will attempt to sign in the user silently.  Cross-device
                    // single sign-on will occur in this branch.

                    opr.SetResultCallback(new SignInResultCallback { Activity = this });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void SetContentGoogle(GoogleSignInAccount acct)
        {
            try
            {
                //Successful log in hooray!!
                if (acct != null)
                {
                    ProgressBar.Visibility = ViewStates.Visible;
                    BtnSignIn.Visibility = ViewStates.Gone;

                    //var GAccountName = acct.Account.Name;
                    //var GAccountType = acct.Account.Type;
                    //var GDisplayName = acct.DisplayName;
                    //var GFirstName = acct.GivenName;
                    //var GLastName = acct.FamilyName;
                    //var GProfileId = acct.Id;
                    //var GEmail = acct.Email;
                    //var GImg = acct.PhotoUrl.Path;
                    GAccessToken = acct.IdToken;
                    GServerCode = acct.ServerAuthCode;
                    Console.WriteLine(GServerCode);

                    if (!string.IsNullOrEmpty(GAccessToken))
                    {
                        //Login Api 
                        //string key = Methods.App.GetValueFromManifest(this, "com.google.android.geo.API_KEY");
                        (int apiStatus, var respond) = await RequestsAsync.Auth.SocialLoginAsync(GAccessToken, "google", UserDetails.DeviceId);
                        if (apiStatus == 200)
                        {
                            if (respond is LoginObject auth)
                            {
                                SetDataLogin(auth);

                                StartActivity(new Intent(this, typeof(BoardingActivity)));
                                Finish(); 
                            }
                        }
                        else if (apiStatus == 400)
                        {
                            if (respond is ErrorObject error)
                            {
                                string errorText = error.Error;
                                switch (errorText)
                                {
                                    case "Please check your details":
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorPleaseCheckYourDetails), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case "Incorrect username or password":
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin2), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case "Your account is not activated yet, please check your inbox for the activation link":
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin3), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    default:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                        break;
                                }
                            }

                            ProgressBar.Visibility = ViewStates.Gone;
                            BtnSignIn.Visibility = ViewStates.Visible;
                        }
                        else if (apiStatus == 404)
                        {
                            ProgressBar.Visibility = ViewStates.Gone;
                            BtnSignIn.Visibility = ViewStates.Visible;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), e.Message, GetText(Resource.String.Lbl_Ok));
                Console.WriteLine(e);
            }
        }

        public void OnConnectionSuspended(int cause)
        {
           
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            try
            {
                // An unresolvable error has occurred and Google APIs (including Sign-In) will not
                // be available.
                Log.Debug("Login_Activity", "onConnectionFailed:" + result);

                //The user has already clicked 'sign-in' so we attempt to resolve all
                //errors until the user is signed in, or the cancel
                ResolveSignInError();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnResult(Object result)
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        //======================================================
         
        #region WoWonder

        //Event Click login using WoWonder
        private void WoWonderSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(WoWonderLoginActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public async void LoginWoWonder(string woWonderAccessToken)
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Visible;
                BtnSignIn.Visibility = ViewStates.Gone;
                ForgotPassTextView.Visibility = ViewStates.Gone;

                if (!string.IsNullOrEmpty(woWonderAccessToken))
                {
                    //Login Api 
                    (int apiStatus, var respond) = await RequestsAsync.Auth.SocialLoginAsync(woWonderAccessToken, "wowonder", UserDetails.DeviceId);
                    if (apiStatus == 200)
                    {
                        if (respond is LoginObject auth)
                        {
                            SetDataLogin(auth);

                            StartActivity(new Intent(this, typeof(BoardingActivity)));
                            FinishAffinity();
                        }
                    }
                    else if (apiStatus == 400)
                    {
                        if (respond is ErrorObject error)
                        {
                            string errorText = error.Error;
                            switch (errorText)
                            {
                                case "Please check your details":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorPleaseCheckYourDetails), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "Incorrect username or password":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin2), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "Your account is not activated yet, please check your inbox for the activation link":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin3), GetText(Resource.String.Lbl_Ok));
                                    break;
                                default:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                        ForgotPassTextView.Visibility = ViewStates.Visible;
                    }
                    else if (apiStatus == 404)
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                        ForgotPassTextView.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 0)
                {
                    var result = Auth.GoogleSignInApi.GetSignInResultFromIntent(data);
                    HandleSignInResult(result);
                }
                else
                {
                    // Logins Facebook
                    MFbCallManager.OnActivityResult(requestCode, (int)resultCode, data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 106)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        if (!MGoogleApiClient.IsConnecting)
                            ResolveSignInError();
                        else if (MGoogleApiClient.IsConnected) MGoogleApiClient.Disconnect();
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