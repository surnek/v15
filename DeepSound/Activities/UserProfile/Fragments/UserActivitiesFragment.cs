﻿using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using DeepSound.Activities.Tabbes.Adapters;
using DeepSoundClient.Classes.User;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Activities.Tabbes;
using DeepSoundClient.Classes.Global;
using Bumptech.Glide.Util;
using Bumptech.Glide.Integration.RecyclerView;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Requests;
using Fragment = Android.Support.V4.App.Fragment;

namespace DeepSound.Activities.UserProfile.Fragments
{
    public class UserActivitiesFragment : Fragment
    {
        #region Variables Basic

        private HomeActivity GlobalContext;
        private View Inflated;
        private ViewStub EmptyStateLayout;  
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        public ActivitiesAdapter MAdapter;
        private LinearLayoutManager MLayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;
        public bool IsCreated;
        private string UserId;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.TemplateRecyclerViewLayout2, container, false);
                IsCreated = true;
                UserId = Arguments.GetString("UserId");

                InitComponent(view);
                SetRecyclerViewAdapters();
                return view;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
         
        #endregion

        #region Functions
         
        private void InitComponent(View view)
        {
            try
            {
                MRecycler = view.FindViewById<RecyclerView>(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
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
                MAdapter = new ActivitiesAdapter(Activity) { ActivityList = new ObservableCollection<ActivityDataObject>() };
                MAdapter.OnItemClick += MAdapterOnItemClick;
                MLayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(MLayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<ActivityDataObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(MLayoutManager);
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

        #region Event

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.ActivityList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.AId.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(item.AId.ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Start Play Sound 
        private void MAdapterOnItemClick(object sender, ActivitiesAdapterClickEventArgs e)
        {
            try
            { 
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Constant.PlayPos = e.Position;
                    GlobalContext?.SoundController?.StartPlaySound(item.TrackData, new ObservableCollection<SoundDataObject>() { item.TrackData });
                }
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
                MAdapter.ActivityList.Clear();
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

        #region Load Data 

        public void PopulateData(List<ActivityDataObject> list)
        {
            try
            {
                if (list?.Count > 0)
                {
                    MAdapter.ActivityList = new ObservableCollection<ActivityDataObject>(list);
                    MAdapter.NotifyDataSetChanged();
                    ShowEmptyPage();  
                }
                else
                {
                    ShowEmptyPage();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public void StartApiServiceWithOffset()
        {
            try
            {
                var item = MAdapter.ActivityList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.AId.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(item.AId.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadData(offset) });
        }

        private async Task LoadData(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = MAdapter.ActivityList.Count;
                (int apiStatus, var respond) = await RequestsAsync.User.GetUserActivitiesAsync(UserId, "15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetFeedObject result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.ActivityList.FirstOrDefault(a => a.AId == item.AId) where check == null select item)
                                {
                                    MAdapter.ActivityList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.ActivityList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.ActivityList = new ObservableCollection<ActivityDataObject>(result.Data);
                                Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.ActivityList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreActivities), ToastLength.Short).Show();
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
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.ActivityList.Count > 0)
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
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoActivity);
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