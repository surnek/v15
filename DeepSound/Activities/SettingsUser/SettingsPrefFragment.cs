using System;
using System.Collections.Generic;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Preferences;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Artists;
using DeepSound.Activities.Blog;
using DeepSound.Activities.Genres;
using DeepSound.Activities.MyProfile;
using DeepSound.Activities.SettingsUser.General;
using DeepSound.Activities.SettingsUser.Security;
using DeepSound.Activities.SettingsUser.Support;
using DeepSound.Activities.Upgrade;
using DeepSound.Helpers.Controller;
using DeepSoundClient;
using Java.Lang;
using Exception = System.Exception;

namespace DeepSound.Activities.SettingsUser
{
    public class SettingsPrefFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private Preference GeneralPref, MyAccountPref , ArticlePref;
        private Preference PasswordPref, BlockedUsersPref, HelpPref, InterestPref, AboutPref, DeleteAccountPref, LogoutPref, GoProPref, WithdrawalsPref;
        private Preference NightMode, TwoFactorPref, ManageSessionsPref;
        private readonly SettingsActivity ActivityContext;
        private string SNightModePref, DialogType;
        #endregion

        #region General

        public SettingsPrefFragment(SettingsActivity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.SettingsThemeDark) : new ContextThemeWrapper(Activity, Resource.Style.SettingsTheme);

                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = base.OnCreateView(localInflater, container, savedInstanceState);

                return view;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            try
            {
                // Load the preferences from an XML resource
                AddPreferencesFromResource(Resource.Xml.SettingsPrefs);

                SharedPref.SharedData = PreferenceManager.SharedPreferences;

                InitComponent();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public override void OnResume()
        {
            try
            {
                base.OnResume();
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                PreferenceScreen.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(false);
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
                SharedPref.SharedData = PreferenceManager.SharedPreferences;
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

                GeneralPref = FindPreference("general_key");
                MyAccountPref = FindPreference("myAccount_key");
                ArticlePref = FindPreference("article_key");
                PasswordPref = FindPreference("editPassword_key");
                BlockedUsersPref = FindPreference("blocked_key");
                TwoFactorPref = FindPreference("Twofactor_key");
                ManageSessionsPref = FindPreference("ManageSessions_key");
                NightMode = FindPreference("Night_Mode_key");
                InterestPref = FindPreference("interest_key");
                HelpPref = FindPreference("help_key");
                AboutPref = FindPreference("about_key");
                DeleteAccountPref = FindPreference("deleteAccount_key");
                LogoutPref = FindPreference("logout_key");

                GoProPref = FindPreference("goPro_key");
                WithdrawalsPref = FindPreference("Withdrawals_key");

                OnSharedPreferenceChanged(SharedPref.SharedData, "Night_Mode_key");

                //Delete Preference
                //============== Account_Profile_key ===================
                PreferenceCategory mCategoryAccount = (PreferenceCategory)FindPreference("Account_Profile_key");

                if (!AppSettings.ShowWithdrawals)
                    mCategoryAccount.RemovePreference(WithdrawalsPref);

                if (!AppSettings.ShowGoPro)
                    mCategoryAccount.RemovePreference(GoProPref);
                 
                if (!AppSettings.ShowBlog)
                    mCategoryAccount.RemovePreference(ArticlePref);

                if (!AppSettings.ShowBlockedUsers)
                    mCategoryAccount.RemovePreference(BlockedUsersPref);
                 
                //============== SecurityAccount_key ===================
                var mCategorySecurity = (PreferenceCategory)FindPreference("SecurityAccount_key");
                if (!AppSettings.ShowEditPassword)
                    mCategorySecurity.RemovePreference(PasswordPref);

                if (!AppSettings.ShowSettingsTwoFactor)
                    mCategorySecurity.RemovePreference(TwoFactorPref);

                if (!AppSettings.ShowSettingsManageSessions)
                    mCategorySecurity.RemovePreference(ManageSessionsPref);
                 
                 
                NightMode.IconSpaceReserved = false;
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
                    GeneralPref.PreferenceClick += GeneralPrefOnPreferenceClick;
                    MyAccountPref.PreferenceClick += MyAccountPrefOnPreferenceClick;
                    ArticlePref.PreferenceClick += ArticlePrefOnPreferenceClick;
                    PasswordPref.PreferenceClick += PasswordPrefOnPreferenceClick;
                    BlockedUsersPref.PreferenceClick += BlockedUsersPrefOnPreferenceClick;
                    InterestPref.PreferenceClick += InterestPrefOnPreferenceClick;
                    HelpPref.PreferenceClick += HelpPrefOnPreferenceClick;
                    AboutPref.PreferenceClick += AboutPrefOnPreferenceClick;
                    DeleteAccountPref.PreferenceClick += DeleteAccountPrefOnPreferenceClick;
                    LogoutPref.PreferenceClick += LogoutPrefOnPreferenceClick;
                    GoProPref.PreferenceClick += GoProPrefOnPreferenceClick;
                    WithdrawalsPref.PreferenceClick += WithdrawalsPrefOnPreferenceClick;
                    ManageSessionsPref.PreferenceClick += ManageSessionsPrefOnPreferenceClick;
                    TwoFactorPref.PreferenceClick += TwoFactorPrefOnPreferenceClick;
                }
                else
                {
                    GeneralPref.PreferenceClick -= GeneralPrefOnPreferenceClick;
                    MyAccountPref.PreferenceClick -= MyAccountPrefOnPreferenceClick;
                    ArticlePref.PreferenceClick -= ArticlePrefOnPreferenceClick;
                    PasswordPref.PreferenceClick -= PasswordPrefOnPreferenceClick;
                    BlockedUsersPref.PreferenceClick -= BlockedUsersPrefOnPreferenceClick;
                    InterestPref.PreferenceClick-= InterestPrefOnPreferenceClick;
                    HelpPref.PreferenceClick -= HelpPrefOnPreferenceClick;
                    AboutPref.PreferenceClick -= AboutPrefOnPreferenceClick;
                    DeleteAccountPref.PreferenceClick -= DeleteAccountPrefOnPreferenceClick;
                    LogoutPref.PreferenceClick -= LogoutPrefOnPreferenceClick;
                    GoProPref.PreferenceClick -= GoProPrefOnPreferenceClick;
                    WithdrawalsPref.PreferenceClick -= WithdrawalsPrefOnPreferenceClick;
                    ManageSessionsPref.PreferenceClick -= ManageSessionsPrefOnPreferenceClick;
                    TwoFactorPref.PreferenceClick -= TwoFactorPrefOnPreferenceClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ArticlePrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        { 
            try
            {
                Intent intent = new Intent(ActivityContext, typeof(BlogActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion
         
        private void InterestPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                Intent intent = new Intent(ActivityContext, typeof(GenresActivity));
                intent.PutExtra("Event", "Save");
                ActivityContext.StartActivity(intent); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
          
        #region Event Support

        //Logout
        private void LogoutPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                DialogType = "Logout";
                var dialog = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                dialog.Title(Resource.String.Lbl_Warning);
                dialog.Content(GetText(Resource.String.Lbl_Are_you_logout));
                dialog.PositiveText(GetText(Resource.String.Lbl_Ok)).OnPositive(this);
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //DeleteAccount
        private void DeleteAccountPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                ActivityContext.StartActivity(new Intent(ActivityContext, typeof(DeleteAccountActivity))); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //About
        private void AboutPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                var intent = new Intent(Context, typeof(AboutAppActivity));
                ActivityContext.StartActivity(intent); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Help
        private void HelpPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                var intent = new Intent(Context, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", Client.WebsiteUrl + "/contact"); 
                intent.PutExtra("Type", GetText(Resource.String.Lbl_Help));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Event General

        private void GeneralPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                ActivityContext.StartActivity(new Intent(ActivityContext, typeof(EditProfileInfoActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //BlockedUsers
        private void BlockedUsersPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                ActivityContext.StartActivity(new Intent(ActivityContext, typeof(BlockedUsersActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
  
        //MyAccount
        private void MyAccountPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                ActivityContext.StartActivity(new Intent(ActivityContext, typeof(MyAccountActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void GoProPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                ActivityContext.StartActivity(new Intent(ActivityContext, typeof(GoProActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void WithdrawalsPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                ActivityContext.StartActivity(new Intent(ActivityContext, typeof(WithdrawalsActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Event Security

        //Change Password
        private void PasswordPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                ActivityContext.StartActivity(new Intent(ActivityContext, typeof(PasswordActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //TwoFactor
        private void TwoFactorPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                ActivityContext.StartActivity(new Intent(ActivityContext, typeof(TwoFactorAuthActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Manage Sessions
        private void ManageSessionsPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                ActivityContext.StartActivity(new Intent(ActivityContext, typeof(ManageSessionsActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        #endregion
    
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            try
            {
                if (key.Equals("Night_Mode_key"))
                {
                    // Set summary to be the user-description for the selected value
                    Preference etp = FindPreference("Night_Mode_key");

                    string getValue = SharedPref.SharedData.GetString("Night_Mode_key", string.Empty);
                    if (getValue == SharedPref.LightMode)
                    {
                        etp.Summary = ActivityContext.GetString(Resource.String.Lbl_Light);
                    }
                    else if (getValue == SharedPref.DarkMode)
                    {
                        etp.Summary = ActivityContext.GetString(Resource.String.Lbl_Dark);
                    }
                    else if (getValue == SharedPref.DefaultMode)
                    {
                        etp.Summary = ActivityContext.GetString(Resource.String.Lbl_SetByBattery);
                    }
                    else
                    {
                        etp.Summary = getValue;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public override bool OnPreferenceTreeClick(Preference preference)
        {
            try
            {
                if (preference.Key == "Night_Mode_key")
                {
                    DialogType = "NightMode";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    dialogList.Title(Resource.String.Lbl_Night_Mode);

                    arrayAdapter.Add(GetText(Resource.String.Lbl_Light));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Dark));

                    if ((int)Build.VERSION.SdkInt >= 29)
                        arrayAdapter.Add(GetText(Resource.String.Lbl_SetByBattery));

                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }

                return base.OnPreferenceTreeClick(preference);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return base.OnPreferenceTreeClick(preference);
            }
        }
          
        #region MaterialDialog

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (DialogType == "Logout")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        Toast.MakeText(ActivityContext, GetText(Resource.String.Lbl_You_will_be_logged), ToastLength.Long).Show();
                        ApiRequest.Logout(ActivityContext);
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    } 
                }
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();

                string getValue = SharedPref.SharedData.GetString("Night_Mode_key", string.Empty);

                if (text == GetString(Resource.String.Lbl_Light) && getValue != SharedPref.LightMode)
                {
                    //Set Light Mode   
                    NightMode.Summary = ActivityContext.GetString(Resource.String.Lbl_Light);

                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                    AppSettings.SetTabDarkTheme = false;
                    SharedPref.SharedData.Edit().PutString("Night_Mode_key", SharedPref.LightMode).Commit();

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        ActivityContext.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        ActivityContext.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    }

                    Intent intent = new Intent(ActivityContext, typeof(SplashScreenActivity));
                    intent.AddCategory(Intent.CategoryHome);
                    intent.SetAction(Intent.ActionMain);
                    intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    ActivityContext.StartActivity(intent);
                    ActivityContext.FinishAffinity();
                }
                else if (text == GetString(Resource.String.Lbl_Dark) && getValue != SharedPref.DarkMode)
                {
                    NightMode.Summary = ActivityContext.GetString(Resource.String.Lbl_Dark);

                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                    AppSettings.SetTabDarkTheme = true;
                    SharedPref.SharedData.Edit().PutString("Night_Mode_key", SharedPref.DarkMode).Commit();

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        ActivityContext.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        ActivityContext.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    }

                    Intent intent = new Intent(ActivityContext, typeof(SplashScreenActivity));
                    intent.AddCategory(Intent.CategoryHome);
                    intent.SetAction(Intent.ActionMain);
                    intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    ActivityContext.StartActivity(intent);
                    ActivityContext.FinishAffinity();
                }
                else if (text == GetString(Resource.String.Lbl_SetByBattery) && getValue != SharedPref.DefaultMode)
                {
                    NightMode.Summary = ActivityContext.GetString(Resource.String.Lbl_SetByBattery);
                    SharedPref.SharedData.Edit().PutString("Night_Mode_key", SharedPref.DefaultMode).Commit();

                    if ((int)Build.VERSION.SdkInt >= 29)
                    {
                        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;

                        var currentNightMode = Resources.Configuration.UiMode & UiMode.NightMask;
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
                    }
                    else
                    {
                        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightAuto;

                        var currentNightMode = Resources.Configuration.UiMode & UiMode.NightMask;
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

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                        {
                            ActivityContext.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                            ActivityContext.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        }

                        Intent intent = new Intent(ActivityContext, typeof(SplashScreenActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        ActivityContext.StartActivity(intent);
                        ActivityContext.FinishAffinity();
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