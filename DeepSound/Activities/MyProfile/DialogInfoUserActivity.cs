using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.MyContacts;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;
using Newtonsoft.Json;

namespace DeepSound.Activities.MyProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class DialogInfoUserActivity : AppCompatActivity
    {
        #region Variables Basic

        private ImageView Image;
        private TextView IconBack, Username, IconCountry, CountryText, CountTracks, CountFollowers, CountFollowing, IconEmail, EmailText, IconGender, GenderText, IconWebsite, WebsiteText, IconFacebook, FacebookText;
        private LinearLayout LayoutFollowing, LayoutFollowers, LayoutWebsite, LayoutFacebook, LayoutEmail;
        private UserDataObject DataUser;
        private Details Details;

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
                SetContentView(Resource.Layout.DialogInfoUserLayout);

                //Get Value  
                InitComponent();

                SetData();
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
                IconBack = FindViewById<TextView>(Resource.Id.IconBack);
                Image = FindViewById<ImageView>(Resource.Id.image);
                Username = FindViewById<TextView>(Resource.Id.username);
                IconCountry = FindViewById<TextView>(Resource.Id.IconCountry);
                CountryText = FindViewById<TextView>(Resource.Id.CountryText);
                CountTracks = FindViewById<TextView>(Resource.Id.CountTracks);
                LayoutFollowers = FindViewById<LinearLayout>(Resource.Id.followersLayout);
                CountFollowers = FindViewById<TextView>(Resource.Id.countFollowers);
                LayoutFollowing = FindViewById<LinearLayout>(Resource.Id.followingLayout);
                CountFollowing = FindViewById<TextView>(Resource.Id.countFollowing);
                LayoutEmail = FindViewById<LinearLayout>(Resource.Id.LayoutEmail);
                IconEmail = FindViewById<TextView>(Resource.Id.IconEmail);
                EmailText = FindViewById<TextView>(Resource.Id.EmailText);
                IconGender = FindViewById<TextView>(Resource.Id.IconGender);
                GenderText = FindViewById<TextView>(Resource.Id.GenderText);
                LayoutWebsite = FindViewById<LinearLayout>(Resource.Id.LayoutWebsite);
                IconWebsite = FindViewById<TextView>(Resource.Id.IconWebsite);
                WebsiteText = FindViewById<TextView>(Resource.Id.WebsiteText);
                LayoutFacebook = FindViewById<LinearLayout>(Resource.Id.LayoutFacebook);
                IconFacebook = FindViewById<TextView>(Resource.Id.IconFacebook);
                FacebookText = FindViewById<TextView>(Resource.Id.FacebookText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, IonIconsFonts.IosArrowBack);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconCountry, IonIconsFonts.Pin);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconEmail, IonIconsFonts.AndroidMail);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconWebsite, IonIconsFonts.AndroidGlobe);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconFacebook, IonIconsFonts.SocialFacebookOutline);
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
                    IconBack.Click += IconBackOnClick;
                    LayoutWebsite.Click += LayoutWebsiteOnClick;
                    LayoutFacebook.Click += LayoutFacebookOnClick;
                    LayoutFollowers.Click += LayoutFollowersOnClick;
                    LayoutFollowing.Click += LayoutFollowingOnClick;
                }
                else
                {
                    IconBack.Click -= IconBackOnClick;
                    LayoutWebsite.Click -= LayoutWebsiteOnClick;
                    LayoutFacebook.Click -= LayoutFacebookOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Finish();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //open Facebook
        private void LayoutFacebookOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    new IntentController(this).OpenFacebookIntent(this, DataUser.Facebook);
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //open Website
        private void LayoutWebsiteOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    Methods.App.OpenbrowserUrl(this, DataUser.Website);
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open My Contact >> Following
        private void LayoutFollowingOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("UserId", DataUser.Id.ToString());
                bundle.PutString("UserType", "Following");

                ContactsFragment contactsFragment = new ContactsFragment
                {
                    Arguments = bundle
                };


                HomeActivity.GetInstance().FragmentBottomNavigator.DisplayFragmentOnSamePage(contactsFragment);

                Finish();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void LayoutFollowersOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("UserId", DataUser.Id.ToString());
                bundle.PutString("UserType", "Followers");

                ContactsFragment contactsFragment = new ContactsFragment
                {
                    Arguments = bundle
                };


                HomeActivity.GetInstance().FragmentBottomNavigator.DisplayFragmentOnSamePage(contactsFragment);

                Finish();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

    
        #endregion

        private void SetData()
        {
            try
            {
                DataUser = JsonConvert.DeserializeObject<UserDataObject>(Intent.GetStringExtra("ItemDataUser"));
                if (DataUser != null)
                {
                    GlideImageLoader.LoadImage(this, DataUser.Avatar, Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    Username.Text = DeepSoundTools.GetNameFinal(DataUser);
                    CountryText.Text = DataUser.CountryId == 0 ? GetText(Resource.String.Lbl_Unknown) : DeepSoundTools.GetCountry(DataUser.CountryId - 1) ?? DataUser.CountryName;

                    if (AppSettings.ShowEmail)
                    {
                        LayoutEmail.Visibility = ViewStates.Visible;
                        EmailText.Text = DataUser.Email;
                    }
                    else
                    {
                        LayoutEmail.Visibility = ViewStates.Gone;
                    }
                   

                    GenderText.Text = DeepSoundTools.GetGender(DataUser.Gender);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconGender, DataUser.Gender.Contains("male") ? IonIconsFonts.Man : IonIconsFonts.Woman);

                    if (!string.IsNullOrEmpty(DataUser.Website))
                    {
                        LayoutWebsite.Visibility = ViewStates.Visible;
                        WebsiteText.Text = DataUser.Website;
                    }
                    else
                    {
                        LayoutWebsite.Visibility = ViewStates.Gone;
                    }

                    if (!string.IsNullOrEmpty(DataUser.Facebook))
                    {
                        LayoutFacebook.Visibility = ViewStates.Visible;
                        FacebookText.Text = DataUser.Facebook;
                    }
                    else
                    {
                        LayoutFacebook.Visibility = ViewStates.Gone;
                    }
                }

                Details = JsonConvert.DeserializeObject<Details>(Intent.GetStringExtra("ItemDataDetails"));
                if (Details != null)
                { 
                    CountFollowers.Text = Methods.FunString.FormatPriceValue(Details.Followers);
                    CountFollowing.Text = Methods.FunString.FormatPriceValue(Details.Following);
                    CountTracks.Text = Methods.FunString.FormatPriceValue(Details.LatestSongs);
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}