using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Util;
using DeepSound.Activities.Chat.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Chat;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace DeepSound.Activities.Chat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class LastChatActivity : AppCompatActivity 
    {
        #region Variables Basic

        public static LastChatAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private static Toolbar ToolBar;
        private AdView MAdView;
        private string UserId = "";
        private static LastChatActivity Instance;

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
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                 
                GetLastChatLocal();

                AdsGoogle.Ad_Interstitial(this);
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
                MAdView?.Resume();
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
                MAdView?.Pause();
                base.OnPause();
                AddOrRemoveEvent(false);
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
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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

        protected override void OnDestroy()
        {
            try
            {
                ListUtils.ChatList = MAdapter.UserList;

                MAdapter?.UserList.Clear();
                MAdapter?.NotifyDataSetChanged();

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
                var mainLayout = FindViewById<CoordinatorLayout>(Resource.Id.mainLayout);
                mainLayout.SetPadding(0, 0, 0, 0);

                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.SetPadding(5, 0, 0, 0);

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new LastChatAdapter(this);
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                //MRecycler.HasFixedSize = true;
                //MRecycler.SetItemViewCacheSize(10);
                //MRecycler.GetLayoutManager().ItemPrefetchEnabled = true; 
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<DataConversation>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
                  
                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
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
                ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = GetText(Resource.String.Lbl_Chats);
                    ToolBar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(ToolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    ToolBar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
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
                    MAdapter.OnItemClick += MAdapterOnOnItemClick;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    MAdapter.OnItemClick -= MAdapterOnOnItemClick;
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static LastChatActivity GetInstance()
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

        #endregion

        #region Scroll

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                //Event Scroll #LastChat
                var item = MAdapter.UserList.LastOrDefault();
                if (item != null && MAdapter.UserList.Count > 10)
                {
                    StartApiService(item.ChatTime.ToString());
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion
         
        #region Permissions

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 100)
                {
                    var item = MAdapter.UserList.FirstOrDefault(a => a.User.Id.ToString() == UserId);
                    if (item != null)
                    {
                        Intent intent = new Intent(this, typeof(MessagesBoxActivity));
                        intent.PutExtra("UserId", item.User.Id.ToString());
                        intent.PutExtra("UserItem", JsonConvert.SerializeObject(item.User));
                        StartActivity(intent);

                        MAdapter.NotifyItemChanged(MAdapter.UserList.IndexOf(item));

                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long).Show();
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                ListUtils.ChatList.Clear();

                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();

                SqLiteDatabase database = new SqLiteDatabase();
                database.ClearLastChat();
                database.ClearAll_Messages();
                database.Dispose();
                MainScrollEvent.IsLoading = false;

                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void MAdapterOnOnItemClick(object sender, LastChatAdapterClickEventArgs e)
        {
            try
            {
                HomeActivity.GetInstance()?.SetService();

                if (ToolBar.Visibility != ViewStates.Visible)
                    ToolBar.Visibility = ViewStates.Visible;

                // read the item which removes bold from the row >> event click open ChatBox by user id
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    UserId = item.User.Id.ToString();
                    item.GetCountSeen = 0;
                    item.GetLastMessage.Seen = 1;
                    Intent intent = new Intent(this, typeof(MessagesBoxActivity));
                    intent.PutExtra("UserId", item.User.Id.ToString());
                    intent.PutExtra("TypeChat", "LastChat");
                    intent.PutExtra("UserItem", JsonConvert.SerializeObject(item.User));

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        StartActivity(intent);
                        MAdapter.NotifyItemChanged(e.Position);
                    }
                    else
                    {
                        //Check to see if any permission in our group is available, if one, then all are
                        if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            StartActivity(intent);
                            MAdapter.NotifyItemChanged(e.Position);
                        }
                        else
                            new PermissionsController(this).RequestPermission(100);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Load Data Api 

        private void GetLastChatLocal()
        {
            try
            {
                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                ListUtils.ChatList = new ObservableCollection<DataConversation>();
                var list = dbDatabase.GetAllLastChat();
                if (list.Count > 0)
                {
                    ListUtils.ChatList = new ObservableCollection<DataConversation>(list);
                    MAdapter.UserList = ListUtils.ChatList;
                    MAdapter.NotifyDataSetChanged();
                }
                else
                {
                    SwipeRefreshLayout.Refreshing = true;

                    StartApiService();
                }

                dbDatabase.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
        }

        private async Task LoadDataAsync(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.UserList.Count;

                var (apiStatus, respond) = await RequestsAsync.Chat.GetConversationListAsync("15", offset);
                if (apiStatus != 200 || !(respond is GetConversationListObject result) || result.Data == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            LoadDataJsonLastChat(result);
                        }
                        else
                        {
                            ListUtils.ChatList = new ObservableCollection<DataConversation>(result.Data);
                            MAdapter.UserList = new ObservableCollection<DataConversation>(result.Data);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
                            dbDatabase.Dispose();
                        }
                    }
                    else
                    {
                        if (MAdapter.UserList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short).Show();
                    }
                }

                MainScrollEvent.IsLoading = false;
                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            }
        }

        public void LoadDataJsonLastChat(GetConversationListObject result)
        {
            try
            {
                if (MAdapter != null)
                {
                    if (MAdapter.UserList?.Count > 0)
                    {
                        foreach (var user in result.Data)
                        {
                            var checkUser = MAdapter.UserList.FirstOrDefault(a => a.User.Id == user.User.Id);
                            if (checkUser != null)
                            {
                                int index = MAdapter.UserList.IndexOf(checkUser);

                                //checkUser.Id = user.Id;
                                //if (checkUser.Owner != user.Owner) checkUser.Owner = user.Owner;
                                if (checkUser.GetLastMessage.Time != user.GetLastMessage.Time) checkUser.GetLastMessage.Time = user.GetLastMessage.Time;
                                if (checkUser.GetLastMessage.Seen != user.GetLastMessage.Seen) checkUser.GetLastMessage.Seen = user.GetLastMessage.Seen;
                                if (checkUser.GetCountSeen != user.GetCountSeen) checkUser.GetCountSeen = user.GetCountSeen;
                                if (checkUser.User != user.User) checkUser.User = user.User;

                                if (checkUser.GetLastMessage.ApiType != user.GetLastMessage.ApiType) continue;
                                checkUser.GetLastMessage.ApiType = user.GetLastMessage.ApiType;

                                if (checkUser.GetLastMessage.Text != user.GetLastMessage.Text)
                                {
                                    checkUser.GetLastMessage.Text = user.GetLastMessage.Text;

                                    if (index > -1)
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            MAdapter.UserList.Move(index, 0);
                                            MAdapter.NotifyItemMoved(index, 0);
                                        });
                                    }
                                }

                                if (checkUser.GetLastMessage.Image != user.GetLastMessage.Image)
                                {
                                    checkUser.GetLastMessage.Image = user.GetLastMessage.Image;

                                    if (index > -1)
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            MAdapter.UserList.Move(index, 0);
                                            MAdapter.NotifyItemMoved(index, 0);
                                        });
                                    }
                                } 
                            }
                            else
                            {
                                RunOnUiThread(() =>
                                {
                                    MAdapter.UserList.Insert(0, user);
                                    MAdapter.NotifyItemInserted(0);

                                    //var dataUser = MAdapter.UserList.IndexOf(MAdapter.UserList.FirstOrDefault(a => a.Id == user.Id));
                                    //if (dataUser > -1)
                                    //    MAdapter.NotifyItemChanged(dataUser);
                                });
                            }
                        }
                    }
                    else
                    {
                        MAdapter.UserList = new ObservableCollection<DataConversation>(result.Data);
                        MAdapter.NotifyDataSetChanged();
                    }

                    ListUtils.ChatList = MAdapter.UserList;
                }

                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
                dbDatabase.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.UserList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoMessage);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                Console.WriteLine(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        public override void OnBackPressed()
        {
            try
            {
                base.OnBackPressed();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 
    }
} 