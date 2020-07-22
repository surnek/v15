using AFollestad.MaterialDialogs;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using DeepSound.Activities.MyContacts;
using DeepSound.Activities.MyProfile;
using DeepSound.Activities.SettingsUser;
using DeepSound.Activities.UserProfile.Fragments;
using DeepSound.Adapters;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using Java.Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using DeepSound.Helpers.Controller;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Refractored.Controls;
using Exception = System.Exception;
using Fragment = Android.Support.V4.App.Fragment;

namespace DeepSound.Activities.Tabbes.Fragments
{
    public class ProfileFragment : Fragment, AppBarLayout.IOnOffsetChangedListener,  MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private HomeActivity GlobalContext;
        private CollapsingToolbarLayout CollapsingToolbar;
        private AppBarLayout AppBarLayout;
        private ImageView IconPro, IconMore, IconInfo;
        public ImageView ImageCover;
        public CircleImageView ImageAvatar;
        private TextView TxtFullName ,TxtCountFollowers , TxtFollowers , TxtCountFollowing , TxtFollowing;
        private Button EditButton;
        private ViewPager ViewPagerView;
        private TabLayout Tabs;
        private FloatingActionButton BtnEdit;
        private RequestBuilder FullGlideRequestBuilder;
        private RequestOptions GlideRequestOptions;
        public UserActivitiesFragment ActivitiesFragment;
        public UserAlbumsFragment AlbumsFragment;
        public UserLikedFragment LikedFragment;
        public UserPlaylistFragment PlaylistFragment;
        public UserSongsFragment SongsFragment;
        public UserStationsFragment StationsFragment;
        public UserStoreFragment StoreFragment;
        public ContactsFragment ContactsFragment;
        public Details DetailsCounter;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TProfileLayout, container, false);

                InitComponent(view);
                SetUpViewPager();

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                    Activity.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                }

                GetMyInfoData(); 
                return view;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
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
      
        private void InitComponent(View view)
        {
            try
            {
                CollapsingToolbar = (CollapsingToolbarLayout)view.FindViewById(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = "";
                
                AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(true);
                AppBarLayout.AddOnOffsetChangedListener(this);
                
                ImageCover = (ImageView)view.FindViewById(Resource.Id.imageCover);

                IconPro = (ImageView)view.FindViewById(Resource.Id.pro);
                IconPro.Visibility = ViewStates.Invisible;

                IconMore = (ImageView)view.FindViewById(Resource.Id.more);
                IconMore.Click += ButtonMoreOnClick;

                IconInfo = (ImageView)view.FindViewById(Resource.Id.info);
                IconInfo.Click += IconInfoOnClick;

                ImageAvatar = (CircleImageView)view.FindViewById(Resource.Id.imageAvatar);
                ImageAvatar.Click += ImageAvatarOnClick;

                TxtFullName = (TextView)view.FindViewById(Resource.Id.fullNameTextView);

                TxtCountFollowers = (TextView)view.FindViewById(Resource.Id.countFollowersTextView);
                TxtFollowers = (TextView)view.FindViewById(Resource.Id.FollowersTextView);
                TxtFollowers.Click += TxtFollowersOnClick;
                TxtCountFollowers.Click += TxtFollowersOnClick;

                TxtCountFollowing = (TextView)view.FindViewById(Resource.Id.countFollowingTextView);
                TxtFollowing = (TextView)view.FindViewById(Resource.Id.FollowingTextView);
                TxtFollowing.Click += TxtFollowingOnClick;
                TxtCountFollowing.Click += TxtFollowingOnClick;

                EditButton = (Button)view.FindViewById(Resource.Id.EditButton);
                EditButton.Click += BtnEditOnClick;

                ViewPagerView = (ViewPager)view.FindViewById(Resource.Id.profileViewPager);
                Tabs = (TabLayout)view.FindViewById(Resource.Id.sectionsTabs);

                BtnEdit = (FloatingActionButton)view.FindViewById(Resource.Id.fab);
                BtnEdit.Click += BtnEditOnClick;

                GlideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
                FullGlideRequestBuilder = Glide.With(this).AsBitmap().Apply(GlideRequestOptions);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Set Tab

        private void SetUpViewPager()
        {
            try
            {
                ActivitiesFragment = new UserActivitiesFragment();
                AlbumsFragment = new UserAlbumsFragment();
                LikedFragment = new UserLikedFragment();
                PlaylistFragment = new UserPlaylistFragment();
                SongsFragment = new UserSongsFragment();
                StationsFragment = new UserStationsFragment();
                StoreFragment = new UserStoreFragment();

                Bundle bundle = new Bundle();
                bundle.PutString("UserId", UserDetails.UserId.ToString());

                ActivitiesFragment.Arguments = bundle;
                AlbumsFragment.Arguments = bundle;
                LikedFragment.Arguments = bundle;
                PlaylistFragment.Arguments = bundle;
                SongsFragment.Arguments = bundle;
                StationsFragment.Arguments = bundle;
                StoreFragment.Arguments = bundle;

                MainTabAdapter adapter = new MainTabAdapter(Activity.SupportFragmentManager);
                adapter.AddFragment(ActivitiesFragment, GetText(Resource.String.Lbl_Activities_Title));
                adapter.AddFragment(AlbumsFragment, GetText(Resource.String.Lbl_Albums));
                adapter.AddFragment(LikedFragment, GetText(Resource.String.Lbl_Liked));
                adapter.AddFragment(PlaylistFragment, GetText(Resource.String.Lbl_Playlist));
                adapter.AddFragment(SongsFragment, GetText(Resource.String.Lbl_Songs));
                adapter.AddFragment(StoreFragment, GetText(Resource.String.Lbl_Store_Title));
                adapter.AddFragment(StationsFragment, GetText(Resource.String.Lbl_Stations));

                //ViewPagerView.CurrentItem = adapter.Count;
                ViewPagerView.OffscreenPageLimit = adapter.Count;
                ViewPagerView.PageSelected += ViewPagerViewOnPageSelected;
                ViewPagerView.Adapter = adapter;

                Tabs.SetupWithViewPager(ViewPagerView);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ViewPagerViewOnPageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            try
            { 
                var p = e.Position;
                var number = GetSelectedTab(p);
                switch (number)
                {
                    //Activities
                    case 1:
                        ActivitiesFragment?.StartApiServiceWithOffset();
                        break;
                    //Albums
                    case 2:
                        AlbumsFragment?.StartApiServiceWithOffset();
                        break;
                    //Liked
                    case 3:
                        LikedFragment?.StartApiServiceWithOffset();
                        break;
                    //Playlist
                    case 4:
                        PlaylistFragment?.StartApiServiceWithOffset();
                        break;
                    //Songs
                    case 5:
                        SongsFragment?.StartApiServiceWithOffset();
                        break;
                    //Store
                    case 6:
                        StoreFragment?.StartApiServiceWithOffset();
                        break;
                    //Stations
                    case 7:
                        StationsFragment?.StartApiServiceWithOffset();
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            } 
        }
          
        private int GetSelectedTab(int number)
        {
            try
            {
                var tabName = Tabs.GetTabAt(number).Text;
                if (tabName == Resources.GetText(Resource.String.Lbl_Activities_Title))
                {
                    return 1;
                } 
                if (tabName == Resources.GetText(Resource.String.Lbl_Albums))
                {
                    return 2;
                } 
                if (tabName == Resources.GetText(Resource.String.Lbl_Liked))
                {
                    return 3;
                }
                if (tabName == Resources.GetText(Resource.String.Lbl_Playlist))
                {
                    return 4;
                }
                if (tabName == Resources.GetText(Resource.String.Lbl_Songs))
                {
                    return 5;
                }
                if (tabName == Resources.GetText(Resource.String.Lbl_Store_Title))
                {
                    return 6;
                }
                if (tabName == Resources.GetText(Resource.String.Lbl_Stations))
                {
                    return 7;
                }
                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            } 
        }

        #endregion

        #region Event
          
        //Open Edit page profile
        private void BtnEditOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(Activity, typeof(EditProfileInfoActivity));
                StartActivityForResult(intent, 200); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //More 
        private void ButtonMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                arrayAdapter.Add(Context.GetText(Resource.String.Lbl_ChangeCoverImage));
                arrayAdapter.Add(Context.GetText(Resource.String.Lbl_Settings));
                arrayAdapter.Add(Context.GetText(Resource.String.Lbl_CopyLinkToProfile));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(Context.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Show Info User
        private void IconInfoOnClick(object sender, EventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList.FirstOrDefault();
                if (dataUser != null)
                {
                    Intent intent = new Intent(Activity, typeof(DialogInfoUserActivity));
                    intent.PutExtra("ItemDataUser", JsonConvert.SerializeObject(dataUser));
                    intent.PutExtra("ItemDataDetails", JsonConvert.SerializeObject(DetailsCounter));
                    Activity.StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open My Contact >> Following
        private void TxtFollowingOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("UserId", UserDetails.UserId.ToString());
                bundle.PutString("UserType", "Following");

                ContactsFragment = new ContactsFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(ContactsFragment);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open My Contact >> Followers
        private void TxtFollowersOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("UserId", UserDetails.UserId.ToString());
                bundle.PutString("UserType", "Followers");

                ContactsFragment = new ContactsFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(ContactsFragment);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Update Avatar Async
        private void ImageAvatarOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.OpenDialogGallery("Avatar");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Result

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 200 && resultCode == (int)Result.Ok)
                {
                    if (!string.IsNullOrEmpty(data.GetStringExtra("name")))
                        if (!data.GetStringExtra("name").Equals(TxtFullName.Text))
                            TxtFullName.Text = data.GetStringExtra("name");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }



        #endregion

        #region Load Data User

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
                LoadDataUser(dataUser); 
                StartApiService();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void LoadDataUser(UserDataObject dataUser)
        {
            try
            { 
                if (dataUser != null)
                {
                    CollapsingToolbar.Title = DeepSoundTools.GetNameFinal(dataUser);

                    FullGlideRequestBuilder.Load(dataUser.Cover).Into(ImageCover);
                    FullGlideRequestBuilder.Load(dataUser.Avatar).Into(ImageAvatar);

                    TxtFullName.Text = DeepSoundTools.GetNameFinal(dataUser); 

                    IconPro.Visibility = dataUser.IsPro == 1 ? ViewStates.Visible : ViewStates.Gone;

                    if (dataUser.Verified == 1)
                        TxtFullName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_small_vector, 0);

                    if (ActivitiesFragment?.IsCreated == true)
                        ActivitiesFragment.PopulateData(dataUser.Activities);

                    if (AlbumsFragment?.IsCreated == true)
                        AlbumsFragment.PopulateData(dataUser.Albums);

                    if (LikedFragment?.IsCreated == true)
                        LikedFragment.PopulateData(dataUser.Liked);

                    if (PlaylistFragment?.IsCreated == true)
                        PlaylistFragment.PopulateData(dataUser.Playlists);

                    if (SongsFragment?.IsCreated == true)
                        SongsFragment.PopulateData(dataUser.TopSongs);

                    if (StationsFragment?.IsCreated == true)
                        StationsFragment.PopulateData(dataUser.Stations);  

                    if (StoreFragment?.IsCreated == true)
                        StoreFragment.PopulateData(dataUser.Store);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartApiService()
        {

            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartApiFetch });
        }

        private async Task StartApiFetch()
        {
            var (apiStatus, respond) = await RequestsAsync.User.ProfileAsync(UserDetails.UserId.ToString(), "stations,albums,playlists,liked,activities,latest_songs,top_songs,store");
            if (apiStatus.Equals(200))
            {
                if (respond is ProfileObject result)
                {
                    if (result.Data != null)
                    {
                        Activity.RunOnUiThread(() =>
                        {
                            try
                            {
                                UserDetails.Avatar = result.Data.Avatar;
                                UserDetails.Username = result.Data.Username;
                                UserDetails.IsPro = result.Data.IsPro.ToString();
                                UserDetails.Url = result.Data.Url;
                                UserDetails.FullName = result.Data.Name;

                                ListUtils.MyUserInfoList.Clear();
                                ListUtils.MyUserInfoList.Add(result.Data);

                                LoadDataUser(result.Data);

                                if (result.Details != null)
                                {
                                    DetailsCounter = result.Details;

                                    TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(result.Details.Followers);
                                    TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(result.Details.Following);
                                    //TxtCountTracks.Text = Methods.FunString.FormatPriceValue(result.Details.LatestSongs);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }); 
                    }
                }
            } 
            else
            {
                Methods.DisplayReportResult(Activity, respond);
            } 
        }
         
        #endregion

        #region appBarLayout

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            try
            {
                int minHeight = ViewCompat.GetMinimumHeight(CollapsingToolbar) * 2;
                float scale = (float)(minHeight + verticalOffset) / minHeight;

                BtnEdit.ScaleX = scale >= 0 ? scale : 0;
                BtnEdit.ScaleY = scale >= 0 ? scale : 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == Context.GetText(Resource.String.Lbl_ChangeCoverImage))
                {
                    GlobalContext.OpenDialogGallery("Cover");
                }
                else if (text == Context.GetText(Resource.String.Lbl_Settings))
                {
                    //open settings
                    Context.StartActivity(new Intent(Context, typeof(SettingsActivity)));
                }
                else if (text == Context.GetText(Resource.String.Lbl_CopyLinkToProfile))
                {
                    string url = ListUtils.MyUserInfoList.FirstOrDefault()?.Url;
                    GlobalContext.SoundController.ClickListeners.OnMenuCopyOnClick(url);
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