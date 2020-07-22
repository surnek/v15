using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Genres;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient;
using DeepSoundClient.Classes.Auth;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;

namespace DeepSound.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class RegisterActivity : AppCompatActivity
    {
        #region Variables Basic

        private EditText FirstNameEditText, LastNameEditText, EmailEditText, UsernameEditText, PasswordEditText, ConfirmPasswordEditText;
        private Button RegisterButton;
        private LinearLayout TermsLayout, SignLayout;
        private ProgressBar ProgressBar;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                View mContentView = Window.DecorView;
                var uiOptions = (int)mContentView.SystemUiVisibility;
                var newUiOptions = uiOptions;

                newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.HideNavigation;
                mContentView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;

                Window.AddFlags(WindowManagerFlags.Fullscreen);

                base.OnCreate(savedInstanceState);

                // Create your application here
                SetContentView(Resource.Layout.RegisterLayout);

                //Get Value And Set Toolbar
                InitComponent();

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

        #region Functions

        private void InitComponent()
        {
            try
            {
                FirstNameEditText = FindViewById<EditText>(Resource.Id.edt_firstname);
                LastNameEditText = FindViewById<EditText>(Resource.Id.edt_lastname);
                EmailEditText = FindViewById<EditText>(Resource.Id.edt_email);
                UsernameEditText = FindViewById<EditText>(Resource.Id.edt_username);
                PasswordEditText = FindViewById<EditText>(Resource.Id.edt_password);
                ConfirmPasswordEditText = FindViewById<EditText>(Resource.Id.edt_Confirmpassword);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                RegisterButton = FindViewById<Button>(Resource.Id.SignInButton);
                TermsLayout = FindViewById<LinearLayout>(Resource.Id.termsLayout);
                SignLayout = FindViewById<LinearLayout>(Resource.Id.SignLayout);
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
                    RegisterButton.Click += RegisterButtonOnClick;
                    TermsLayout.Click += TermsLayoutOnClick;
                    SignLayout.Click += SignLayoutOnClick;
                }
                else
                {
                    RegisterButton.Click -= RegisterButtonOnClick;
                    TermsLayout.Click -= TermsLayoutOnClick;
                    SignLayout.Click -= SignLayoutOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        //Open Login Activity
        private void SignLayoutOnClick(object sender, EventArgs e)
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

        //Open Terms of Service
        private void TermsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var url = Client.WebsiteUrl + "/terms/terms";
                Methods.App.OpenbrowserUrl(this, url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        //Register QuickDate
        private async void RegisterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (!string.IsNullOrEmpty(EmailEditText.Text.Replace(" ", "")) || !string.IsNullOrEmpty(UsernameEditText.Text.Replace(" ", "")) ||
                        !string.IsNullOrEmpty(PasswordEditText.Text) || !string.IsNullOrEmpty(ConfirmPasswordEditText.Text))
                    {
                        var check = Methods.FunString.IsEmailValid(EmailEditText.Text.Replace(" ", ""));
                        if (!check)
                        {
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                        }
                        else
                        {
                            if (PasswordEditText.Text != ConfirmPasswordEditText.Text)
                            {
                                ProgressBar.Visibility = ViewStates.Gone;
                                RegisterButton.Visibility = ViewStates.Visible;
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_Register_password), GetText(Resource.String.Lbl_Ok));
                            }
                            else
                            {
                                ProgressBar.Visibility = ViewStates.Visible;
                                RegisterButton.Visibility = ViewStates.Gone;

                                var name = FirstNameEditText.Text + " " +  LastNameEditText.Text;
                                (int apiStatus, var respond) = await RequestsAsync.Auth.RegisterAsync(name, EmailEditText.Text.Replace(" ", ""), UsernameEditText.Text.Replace(" ", ""), PasswordEditText.Text, ConfirmPasswordEditText.Text, UserDetails.DeviceId);
                                if (apiStatus == 200)
                                {
                                    if (respond is RegisterObject auth)
                                    {
                                        if (auth.WaitValidation == 0)
                                        {
                                            SetDataRegister(auth);

                                            Intent intent = new Intent(this, typeof(GenresActivity));
                                            intent.PutExtra("Event", "Continue");
                                            StartActivity(intent);

                                            FinishAffinity();
                                        }
                                        else
                                        {
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_VerifyRegistration), GetText(Resource.String.Lbl_Ok));
                                        }
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
                                            case "This username is already taken":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister2), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            case "Username length must be between 5 / 32":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister3), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            case "Invalid username characters":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister4), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            case "This e-mail is already taken":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister5), GetText(Resource.String.Lbl_Ok));
                                                break; 
                                            case "This e-mail is invalid":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister6), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            case "Passwords don't match":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister7), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            case "Password is too short":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister8), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            default:
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                                break;
                                        }
                                    }

                                    ProgressBar.Visibility = ViewStates.Gone;
                                    RegisterButton.Visibility = ViewStates.Visible;
                                }
                                else if (apiStatus == 404)
                                {
                                    ProgressBar.Visibility = ViewStates.Gone;
                                    RegisterButton.Visibility = ViewStates.Visible;
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                                }
                            }
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        RegisterButton.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    RegisterButton.Visibility = ViewStates.Visible;
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ProgressBar.Visibility = ViewStates.Gone;
                RegisterButton.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), ex.Message, GetText(Resource.String.Lbl_Ok));

            }
        }

        private void SetDataRegister(RegisterObject auth)
        {
            try
            {
                if (auth != null)
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
                    UserDetails.IsLogin = true;

                    ListUtils.DataUserLoginList.Clear();
                    ListUtils.DataUserLoginList.Add(user);

                    var dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdateLogin_Credentials(user);

                    if (auth.Data != null)
                    {
                        ListUtils.MyUserInfoList.Add(auth.Data);
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetInfoData(this, UserDetails.UserId.ToString()) });
                    }
                      
                    dbDatabase.Dispose();
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