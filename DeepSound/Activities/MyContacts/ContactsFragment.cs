using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Util;
using DeepSound.Activities.MyContacts.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Exception = System.Exception;
using Fragment = Android.Support.V4.App.Fragment;

namespace DeepSound.Activities.MyContacts
{
    public class ContactsFragment : Fragment
    {
        #region Variables Basic

       public ContactsAdapter MAdapter;
       private HomeActivity GlobalContext;
       private SwipeRefreshLayout SwipeRefreshLayout;
       private RecyclerView MRecycler;
       private LinearLayoutManager LayoutManager;
       private ViewStub EmptyStateLayout;
       private View Inflated;
       private RecyclerViewOnScrollListener MainScrollEvent;
       private string UserType = "", UserId = "";
       private AdsGoogle.AdMobRewardedVideo RewardedVideoAd;
       private AdView MAdView;
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
                View view = inflater.Inflate(Resource.Layout.RecyclerDefaultLayout, container, false);

                return view;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            { 
                var id = Arguments.GetString("UserId") ?? "Data not available";
                if (id != "Data not available" && !string.IsNullOrEmpty(id)) UserId = id;

                var type = Arguments.GetString("UserType") ?? "Data not available";
                if (type != "Data not available" && !string.IsNullOrEmpty(type)) UserType = type;

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                StartApiService();

                RewardedVideoAd = AdsGoogle.Ad_RewardedVideo(Context);
 
                base.OnViewCreated(view, savedInstanceState);
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

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                MAdView?.Resume();
                RewardedVideoAd?.OnResume(Context);
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
                MAdView?.Pause();
                RewardedVideoAd?.OnPause(Context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnDestroy()
        {
            try
            {
                MAdView?.Destroy();
                RewardedVideoAd?.OnDestroy(Context);
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                MAdView = view.FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitToolbar(View view)
        {
            try
            {
                var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                string title = "";
                switch (UserType)
                {
                    case "Following":
                        title = Context.GetText(Resource.String.Lbl_Following);  
                        break;
                    case "Followers":
                        title = Context.GetText(Resource.String.Lbl_Followers);
                        break;
                }

                GlobalContext.SetToolBar(toolbar, title);
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
                MAdapter = new ContactsAdapter(Activity) { UsersList = new ObservableCollection<UserDataObject>() };
                MAdapter.OnItemClick += MAdapterOnOnItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<UserDataObject>(Activity, MAdapter, sizeProvider, 10);
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

         
        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            try
            {
                switch (item.ItemId)
                {
                    case Android.Resource.Id.Home:
                        GlobalContext.FragmentNavigatorBack();
                        return true;
                }
                return base.OnOptionsItemSelected(item);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }

        #endregion

        #region Event

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.UsersList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id.ToString()); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open profile  
        private void MAdapterOnOnItemClick(object sender, ContactsAdapterClickEventArgs e)
        {
            try
            { 
                var item = MAdapter.GetItem(e.Position);
                if (item?.Id != null) GlobalContext.OpenProfile(item.Id, item);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.UsersList.Clear();
                MAdapter.NotifyDataSetChanged();

                MRecycler.Visibility = ViewStates.Visible;
                EmptyStateLayout.Visibility = ViewStates.Gone;
                MainScrollEvent.IsLoading = false;

                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Load Contacts 

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadContacts(offset) });
        }

        private async Task LoadContacts(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                switch (UserType)
                {
                    case "Following":
                        await LoadUserFollowingAsync(offset);
                        break;
                    case "Followers":
                        await LoadUserFollowersAsync(offset);
                        break;
                }
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

                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            }
        }
         
        private async Task LoadUserFollowingAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            MainScrollEvent.IsLoading = true;

            int countList = MAdapter.UsersList.Count;
            (int apiStatus, var respond) = await RequestsAsync.User.GetFollowingAsync(UserId, "15", offset);
            if (apiStatus == 200)
            {
                if (respond is GetUserObject result)
                {
                    var respondList = result.Data?.UserList?.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data?.UserList let check = MAdapter.UsersList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                MAdapter.UsersList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.UsersList.Count - countList); });
                        }
                        else
                        {
                            MAdapter.UsersList = new ObservableCollection<UserDataObject>(result.Data?.UserList);
                            Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.UsersList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreUser), ToastLength.Short).Show();
                    }
                }
            }
            else
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }

            Activity.RunOnUiThread(ShowEmptyPage);
        }

        private async Task LoadUserFollowersAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            MainScrollEvent.IsLoading = true;

            int countList = MAdapter.UsersList.Count;
            (int apiStatus, var respond) = await RequestsAsync.User.GetFollowerAsync(UserId, "15", offset);
            if (apiStatus == 200)
            {
                if (respond is GetUserObject result)
                {
                    var respondList = result.Data?.UserList?.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data?.UserList let check = MAdapter.UsersList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                MAdapter.UsersList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.UsersList.Count - countList); });
                        }
                        else
                        {
                            MAdapter.UsersList = new ObservableCollection<UserDataObject>(result.Data?.UserList);
                            Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.UsersList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreUser), ToastLength.Short).Show();
                    }
                }
            }
            else
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }

            Activity.RunOnUiThread(ShowEmptyPage);
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.UsersList.Count > 0)
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
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoUsers);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
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

    }
}