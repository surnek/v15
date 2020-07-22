using System;
using System.Collections.Generic;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Java.Lang;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace DeepSound.Activities.SettingsUser.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MyAccountActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private TextView BackIcon, UsernameIcon, EmailIcon, GenderIcon, AgeIcon, CountryIcon;
        private EditText EdtUsername, EdtEmail, EdtAge, EdtCountry;
        private RadioButton RadioMale, RadioFemale;
        private Button BtnSave;
        private Toolbar Toolbar;
        private string Country,IdGender;
        private AdView MAdView;

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
                SetContentView(Resource.Layout.MyAccountLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                
                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, null);

                GetMyInfoData();
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
                MAdView?.Destroy();
                base.OnDestroy();
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
                BackIcon = FindViewById<TextView>(Resource.Id.IconBack);

                UsernameIcon = FindViewById<TextView>(Resource.Id.IconUsername);
                EdtUsername = FindViewById<EditText>(Resource.Id.UsernameEditText);

                EmailIcon = FindViewById<TextView>(Resource.Id.IconEmail);
                EdtEmail = FindViewById<EditText>(Resource.Id.EmailEditText);

                GenderIcon = FindViewById<TextView>(Resource.Id.IconGender);
                RadioMale = FindViewById<RadioButton>(Resource.Id.radioMale);
                RadioFemale = FindViewById<RadioButton>(Resource.Id.radioFemale);

                AgeIcon = FindViewById<TextView>(Resource.Id.IconAge);
                EdtAge = FindViewById<EditText>(Resource.Id.AgeEditText);

                CountryIcon = FindViewById<TextView>(Resource.Id.IconCountry);
                EdtCountry = FindViewById<EditText>(Resource.Id.CountryEditText);
                 
                BtnSave = FindViewById<Button>(Resource.Id.ApplyButton);

                Methods.SetColorEditText(EdtUsername, AppSettings.SetTabDarkTheme ? Color.White : Color.Black); 
                Methods.SetColorEditText(EdtEmail, AppSettings.SetTabDarkTheme ? Color.White : Color.Black); 
                Methods.SetColorEditText(EdtAge, AppSettings.SetTabDarkTheme ? Color.White : Color.Black); 
                Methods.SetColorEditText(EdtCountry, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
              
                RadioMale.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                RadioFemale.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black); 
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, BackIcon, IonIconsFonts.ChevronLeft);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, GenderIcon, FontAwesomeIcon.Transgender);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, UsernameIcon, FontAwesomeIcon.Users);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, EmailIcon, FontAwesomeIcon.At);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, AgeIcon, FontAwesomeIcon.BirthdayCake);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, CountryIcon, FontAwesomeIcon.Flag);

                Methods.SetFocusable(EdtCountry);
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
                    Toolbar.Title = GetString(Resource.String.Lbl_MyAccount);
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
                    RadioMale.CheckedChange += RadioMaleOnCheckedChange;
                    RadioFemale.CheckedChange += RadioFemaleOnCheckedChange;
                    BtnSave.Click += BtnSaveOnClick;
                    EdtCountry.Touch += EdtCountryOnClick;
                }
                else
                {
                    RadioMale.CheckedChange -= RadioMaleOnCheckedChange;
                    RadioFemale.CheckedChange -= RadioFemaleOnCheckedChange;
                    BtnSave.Click -= BtnSaveOnClick;
                    EdtCountry.Touch -= EdtCountryOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion

        #region Events

        private void RadioFemaleOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                bool isChecked = RadioFemale.Checked;
                if (isChecked)
                {
                    RadioMale.Checked = false;
                    RadioFemale.Checked = true;
                    IdGender = "female";
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void RadioMaleOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                bool isChecked = RadioMale.Checked;
                if (isChecked)
                {
                    RadioMale.Checked = true;
                    RadioFemale.Checked = false;
                    IdGender = "male";
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

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
                        {"username", EdtUsername.Text},
                        {"email", EdtEmail.Text},
                        {"gender",IdGender},
                        {"country",Country},
                        {"age", EdtAge.Text},
                    };
                     
                    (int apiStatus, var respond) = await RequestsAsync.User.UpdateGeneralAsync(UserDetails.UserId.ToString() , dictionary);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            Console.WriteLine(result.Message);
                            var local = ListUtils.MyUserInfoList.FirstOrDefault();
                            if (local != null)
                            {
                                local.Username = EdtUsername.Text;
                                local.Email = EdtEmail.Text;
                                local.Age = Convert.ToInt32(EdtAge.Text);
                                local.CountryId = Convert.ToInt32(Country);
                                local.CountryName = EdtCountry.Text;

                                SqLiteDatabase database = new SqLiteDatabase();
                                database.InsertOrUpdate_DataMyInfo(local);
                                database.Dispose(); 
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Short).Show();
                            AndHUD.Shared.Dismiss(this);
                             
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

        //Country
        private void EdtCountryOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event.Action != MotionEventActions.Down) return;


                string[] countriesArray = Resources.GetStringArray(Resource.Array.countriesArray);

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                var arrayAdapter = countriesArray.ToList();

                dialogList.Title(GetText(Resource.String.Lbl_Location));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
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
                    EdtUsername.Text = dataUser.Username;
                    EdtEmail.Text = dataUser.Email;

                    if (dataUser.Age != 0)
                        EdtAge.Text = dataUser.Age.ToString();

                    EdtCountry.Text = dataUser.CountryId == 0 ? GetText(Resource.String.Lbl_ChooseYourCountry) : DeepSoundTools.GetCountry(dataUser.CountryId - 1) ?? dataUser.CountryName;

                    switch (dataUser.Gender)
                    {
                        case "male":
                            RadioMale.Checked = true;
                            RadioFemale.Checked = false;
                            IdGender = "male";
                            break;
                        case "female":
                            RadioMale.Checked = false;
                            RadioFemale.Checked = true;
                            IdGender = "female";
                            break;
                    } 
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region MaterialDialog

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
                var id = itemId + 1;
                var text = itemString.ToString();
                 
                Country = id.ToString();
                EdtCountry.Text = text;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

    }
}