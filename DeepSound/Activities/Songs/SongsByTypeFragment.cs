using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Util;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace DeepSound.Activities.Songs
{
    public class SongsByTypeFragment : Fragment
    {
        #region Variables Basic

        public RowSoundAdapter MAdapter;
        private HomeActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private string SongsType;
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

                SongsType = Arguments.GetString("SongsType") ?? "";

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                GetSongsByType();

                AdsGoogle.Ad_Interstitial(Context);

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
                base.OnDestroy();
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
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
              
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


                switch (SongsType)
                {
                    case "Popular":
                    {
                        GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_Popular_Title));
                        break;
                    }
                    case "RecentlyPlayed":
                    {
                        GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_RecentlyPlayed));
                        break;
                    }
                    case "NewReleases":
                    {
                        GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_LatestSongs_Title));
                        break;
                    }
                    case "BrowseTopSongs":
                    {
                        GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_TopSongs_Title));
                        break;
                    }
                    case "MyProfileStore":
                    {
                        GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_Store_Title));
                        break;
                    }
                    case "MyProfileTopSongs":
                    {
                        GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_TopSongs_Title));
                        break;
                    }
                    case "MyProfileLatestSongs":
                    {
                        GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_LatestSongs_Title));
                        break;
                    }
                    default:
                        GlobalContext.SetToolBar(toolbar, SongsType);
                        break;
                } 
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
                MAdapter = new RowSoundAdapter(Activity, "SongsByTypeFragment") {SoundsList = new ObservableCollection<SoundDataObject>()};
                MAdapter.OnItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<SoundDataObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
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
         
        //Start Play Sound 
        private void MAdapterOnItemClick(object sender, RowSoundAdapterClickEventArgs e)
        {
            try
            {
                var list = MAdapter.SoundsList.Where(sound => sound.IsPlay).ToList();
                if (list.Count > 0)
                {
                    foreach (var all in list)
                    {
                        all.IsPlay = false;

                        var index = MAdapter.SoundsList.IndexOf(all);
                        MAdapter.NotifyItemChanged(index);
                    }
                }

                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    item.IsPlay = true;
                    MAdapter.NotifyItemChanged(e.Position);

                    Constant.PlayPos = e.Position;
                    GlobalContext?.SoundController?.StartPlaySound(item, MAdapter.SoundsList, MAdapter);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        #endregion

        #region Load Data Soungs

        private void GetSongsByType()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    switch (SongsType)
                    {
                        case "Popular":
                        {
                            if (GlobalContext?.MainFragment?.PopularSoundAdapter?.SoundsList?.Count > 0)
                            {
                                MAdapter.SoundsList = GlobalContext?.MainFragment?.PopularSoundAdapter?.SoundsList;
                                MAdapter.NotifyDataSetChanged();
                            } 
                            break;
                        }
                        case "RecentlyPlayed":
                        {
                            if (GlobalContext?.MainFragment?.RecentlyPlayedSoundAdapter?.SoundsList?.Count > 0)
                            {
                                MAdapter.SoundsList = GlobalContext?.MainFragment?.RecentlyPlayedSoundAdapter?.SoundsList;
                                MAdapter.NotifyDataSetChanged();
                            } 
                            break;
                        }
                        case "NewReleases":
                        {
                            if (GlobalContext?.MainFragment?.NewReleasesSoundAdapter?.SoundsList?.Count > 0)
                            {
                                MAdapter.SoundsList = GlobalContext?.MainFragment?.NewReleasesSoundAdapter?.SoundsList;
                                MAdapter.NotifyDataSetChanged();
                            } 
                            break;
                        }
                        case "BrowseTopSongs":
                        {
                            if (GlobalContext?.BrowseFragment?.TopSongsSoundAdapter?.SoundsList?.Count > 0)
                            {
                                MAdapter.SoundsList = GlobalContext?.BrowseFragment?.TopSongsSoundAdapter?.SoundsList;
                                MAdapter.NotifyDataSetChanged();
                            } 
                            break;
                        }
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
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                GetSongsByType();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion 
    }
}