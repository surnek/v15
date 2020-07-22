using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Artists;
using DeepSound.Activities.Artists.Adapters;
using DeepSound.Activities.Default;
using DeepSound.Activities.Genres.Adapters;
using DeepSound.Activities.Notification;
using DeepSound.Activities.Search;
using DeepSound.Activities.Songs;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Upgrade;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Liaoinstan.SpringViewLib.Widgets;
using Me.Relex;
using DefaultHeader = DeepSound.Helpers.PullSwipeStyles.DefaultHeader;

namespace DeepSound.Activities.Tabbes.Fragments
{
    public class MainFeedFragment : Fragment, SpringView.IOnFreshListener 
    {
        #region Variables Basic

        public ArtistsAdapter ArtistsAdapter;
        private GenresAdapter GenresAdapter;
        public HSoundAdapter NewReleasesSoundAdapter, RecentlyPlayedSoundAdapter, PopularSoundAdapter;
        private HomeActivity GlobalContext;
        private SpringView SwipeRefreshLayout;
        private ViewStub EmptyStateLayout, BrowseViewStub, NewReleasesViewStub, RecentlyPlayedViewStub, PopularViewStub,  ArtistsViewStub;
        private View Inflated, BrowseInflated, NewReleasesInflated, RecentlyPlayedInflated, PopularInflated, ArtistsInflated;       
      
        public TextView ProIcon, SearchIcon ,  NotificationIcon;
        private ViewPager ViewPagerView;
        private CircleIndicator ViewPagerCircleIndicator;
        private ObservableCollection<SoundDataObject> RecommendedList;
        private ProgressBar ProgressBar; 
        private RecyclerViewOnScrollListener ArtistsScrollEvent;
        public SongsByGenresFragment SongsByGenresFragment;
        public SongsByTypeFragment SongsByTypeFragment;
        private RelativeLayout MainAlert;

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
                View view = inflater.Inflate(Resource.Layout.TMainFeedLayout, container, false);

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
                InitComponent(view);
               // InitToolbar(view);
                SetRecyclerViewAdapters();

                StartApiService();

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

        #endregion
         
        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                ProgressBar = view.FindViewById<ProgressBar>(Resource.Id.progress);
                ProgressBar.Visibility = ViewStates.Visible;
                 
                BrowseViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubBrowse);
                NewReleasesViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubNewReleases);
                RecentlyPlayedViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubRecentlyPlayed);
                PopularViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubPopular);
                ArtistsViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubArtists);

                ViewPagerView = view.FindViewById<ViewPager>(Resource.Id.viewpager2);
                ViewPagerCircleIndicator = (CircleIndicator)view.FindViewById(Resource.Id.indicator);
                ViewPagerView.PageMargin = 6;
                ViewPagerView.SetClipChildren(false) ;
                ViewPagerView.SetPageTransformer(true, new CarouselEffectTransformer2(Activity));

                NotificationIcon = (TextView)view.FindViewById(Resource.Id.notificationIcon);
                SearchIcon = (TextView)view.FindViewById(Resource.Id.searchIcon);
                ProIcon = (TextView)view.FindViewById(Resource.Id.proIcon);
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, NotificationIcon, IonIconsFonts.AndroidNotifications);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, SearchIcon, IonIconsFonts.AndroidSearch);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, ProIcon, FontAwesomeIcon.Rocket);

                SwipeRefreshLayout = (SpringView)view.FindViewById(Resource.Id.material_style_ptr_frame);
                SwipeRefreshLayout.SetType(SpringView.Type.Overlap);
                SwipeRefreshLayout.Header = new DefaultHeader(Activity);
                SwipeRefreshLayout.Footer = new Helpers.PullSwipeStyles.DefaultFooter(Activity);
                SwipeRefreshLayout.Enable = true;
                SwipeRefreshLayout.SetListener(this);

                NotificationIcon.Click += NotificationIconOnClick;
                SearchIcon.Click += SearchIconOnClick;
                ProIcon.Click += ProIconOnClick;

                if (!AppSettings.ShowGoPro) 
                    ProIcon.Visibility = ViewStates.Gone;

                if (!UserDetails.IsLogin)
                    ProIcon.Visibility = ViewStates.Gone;
                 
                NotificationIcon.Visibility = UserDetails.IsLogin ? ViewStates.Visible : ViewStates.Gone;
                 
                MainAlert = (RelativeLayout)view.FindViewById(Resource.Id.mainAlert);
                MainAlert.Visibility = !UserDetails.IsLogin ? ViewStates.Visible : ViewStates.Gone;
                MainAlert.Click += MainAlertOnClick;
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
                RecommendedList = new ObservableCollection<SoundDataObject>(); 
                
                //Browse RecyclerView >> LinearLayoutManager.Horizontal
                GenresAdapter = new GenresAdapter(Activity) { GenresList = new ObservableCollection<GenresObject.DataGenres>() };
                GenresAdapter.GenresList = ListUtils.GenresList;
                GenresAdapter.OnItemClick += GenresAdapterOnOnItemClick;

                //New Releases RecyclerView >> LinearLayoutManager.Horizontal 
                NewReleasesSoundAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() }; 
                NewReleasesSoundAdapter.OnItemClick += NewReleasesSoundAdapterOnOnItemClick;

                // Recently Played RecyclerView >> LinearLayoutManager.Horizontal
                RecentlyPlayedSoundAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                RecentlyPlayedSoundAdapter.OnItemClick += RecentlyPlayedSoundAdapterOnOnItemClick;

                // Popular RecyclerView >> LinearLayoutManager.Horizontal
                PopularSoundAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                PopularSoundAdapter.OnItemClick += PopularSoundAdapterOnOnItemClick;

                ArtistsAdapter = new ArtistsAdapter(Activity);
                ArtistsAdapter.OnItemClick += ArtistsAdapterOnOnItemClick;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void MainAlertOnClick(object sender, EventArgs e)
        {
            try
            {
                Activity.StartActivity(new Intent(Activity, typeof(LoginActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //open sound from NewRelease
        private void NewReleasesSoundAdapterOnOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = NewReleasesSoundAdapter.GetItem(e.Position);
                    if (item == null)
                        return;
                    Constant.PlayPos = e.Position;
                    GlobalContext?.SoundController?.StartPlaySound(item, NewReleasesSoundAdapter.SoundsList);
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //open sound from Popular
        private void PopularSoundAdapterOnOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = PopularSoundAdapter.GetItem(e.Position);
                    if (item == null)
                        return;
                    Constant.PlayPos = e.Position; 
                    GlobalContext?.SoundController?.StartPlaySound(item, PopularSoundAdapter.SoundsList);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //open sound from RecentlyPlayed
        private void RecentlyPlayedSoundAdapterOnOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = RecentlyPlayedSoundAdapter.GetItem(e.Position);
                    if (item != null)
                    { 
                        Constant.PlayPos = e.Position;
                        GlobalContext?.SoundController?.StartPlaySound(item, RecentlyPlayedSoundAdapter.SoundsList);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //open Songs By Genres
        private void GenresAdapterOnOnItemClick(object sender, GenresAdapterClickEventArgs e)
        {
            try
            {
                var item = GenresAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("GenresId", item.Id.ToString());
                    bundle.PutString("GenresText", item.CateogryName);

                    SongsByGenresFragment = new SongsByGenresFragment
                    {
                        Arguments = bundle
                    };
                    GlobalContext.FragmentBottomNavigator.DisplayFragment(SongsByGenresFragment);
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open profile  
        private void ArtistsAdapterOnOnItemClick(object sender, ArtistsAdapterClickEventArgs e)
        {
            try
            { 
                var item = ArtistsAdapter.GetItem(e.Position);
                if (item?.Id != null) GlobalContext.OpenProfile(item.Id, item);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open go to pro Page  
        private void ProIconOnClick(object sender, EventArgs e)
        {
            try
            {
                Activity.StartActivity(new Intent(Activity, typeof(GoProActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open Notification Page
        private void NotificationIconOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.ShowOrHideBadgeViewIcon();
                 
                NotificationFragment notificationFragment = new NotificationFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(notificationFragment); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open Search Page
        private void SearchIconOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchFragment searchFragment = new SearchFragment();
                GlobalContext?.FragmentBottomNavigator.DisplayFragment(searchFragment);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void BrowseMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                //Show all Genres
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void PopularMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("SongsType", "Popular");

                SongsByTypeFragment = new SongsByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(SongsByTypeFragment);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void RecentlyPlayedMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("SongsType", "RecentlyPlayed");

                SongsByTypeFragment = new SongsByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(SongsByTypeFragment);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void NewReleasesMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("SongsType", "NewReleases");

                SongsByTypeFragment = new SongsByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(SongsByTypeFragment);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ArtistsMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                ArtistsFragment fragment = new ArtistsFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(fragment);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Artists Scroll
        private void ArtistsScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                if (ArtistsScrollEvent.IsLoading == false)
                {
                    ArtistsScrollEvent.IsLoading = true;
                    var item = ArtistsAdapter.ArtistsList.LastOrDefault();
                    if (item != null)
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadArtists(item.Id.ToString()) });
                        ArtistsScrollEvent.IsLoading = false;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Refresh

        public void OnRefresh()
        {
            try
            {
                NewReleasesSoundAdapter.SoundsList.Clear();
                NewReleasesSoundAdapter.NotifyDataSetChanged();

                RecentlyPlayedSoundAdapter.SoundsList.Clear();
                RecentlyPlayedSoundAdapter.NotifyDataSetChanged();

                PopularSoundAdapter.SoundsList.Clear();
                PopularSoundAdapter.NotifyDataSetChanged();

                GenresAdapter.GenresList.Clear();
                GenresAdapter.NotifyDataSetChanged();

                ArtistsAdapter.ArtistsList.Clear();
                ArtistsAdapter.NotifyDataSetChanged();

                RecommendedList.Clear();
                ViewPagerView.Adapter = null;

                EmptyStateLayout.Visibility = ViewStates.Gone;
                 
                StartApiService();                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnLoadMore()
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

        #region Load Discover Api

        private void StartApiService()
        { 
            if (Methods.CheckConnectivity())
            {
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> {LoadDiscover, ApiRequest.GetGenres_Api, () => LoadArtists()});
            }
            else
            {
                SwipeRefreshLayout.OnFinishFreshAndLoad();

                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
            }
        }

        private async Task LoadDiscover()
        {
            (int apiStatus, var respond) = await RequestsAsync.Common.GetDiscoverAsync().ConfigureAwait(false);
            if (apiStatus == 200)
            {
                if (respond is DiscoverObject result)
                {
                    if (result.randoms?.Recommended != null && result.randoms?.Recommended?.Count > 0)
                    {
                        RecommendedList = new ObservableCollection<SoundDataObject>(result.randoms?.Recommended);
                    }

                    if (result.newReleases != null && result.newReleases.Value.NewReleasesClass.Data?.Count > 0)
                    {
                        NewReleasesSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.newReleases.Value.NewReleasesClass.Data);
                    }

                    if (result.recentlyPlayed != null && result.recentlyPlayed.Value.RecentlyPlayedClass.Data?.Count > 0)
                    {
                        if (RecentlyPlayedSoundAdapter.SoundsList.Count > 0)
                        {
                            var newItemList = result.recentlyPlayed.Value.RecentlyPlayedClass.Data.Where(c => !RecentlyPlayedSoundAdapter.SoundsList.Select(fc => fc.Id).Contains(c.Id)).ToList();
                            if (newItemList.Count > 0)
                            {
                                ListUtils.AddRange(RecentlyPlayedSoundAdapter.SoundsList, newItemList);
                            } 
                        }
                        else
                        {                            
                            RecentlyPlayedSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.recentlyPlayed.Value.RecentlyPlayedClass.Data); 
                        }

                        var soundDataObjects = RecentlyPlayedSoundAdapter.SoundsList?.Reverse();
                        Console.WriteLine(soundDataObjects);

                        var list = RecentlyPlayedSoundAdapter.SoundsList.OrderBy(o => o.Views);
                        RecentlyPlayedSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(list);
                    }
                      
                    if (result.mostPopularWeek != null && result.mostPopularWeek.Value.MostPopularWeekClass?.Data?.Count > 0)
                    {
                        PopularSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.mostPopularWeek.Value.MostPopularWeekClass?.Data);
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity.RunOnUiThread(ShowEmptyPage);
        }

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.OnFinishFreshAndLoad();

                if (ProgressBar.Visibility == ViewStates.Visible)
                    ProgressBar.Visibility = ViewStates.Gone;

                if (RecommendedList?.Count > 0)
                {
                    if (ViewPagerView.Adapter == null)
                    { 
                        ViewPagerView.Adapter = new ImageCoursalViewPager(Activity, RecommendedList);
                        ViewPagerView.CurrentItem = 1;
                        ViewPagerCircleIndicator.SetViewPager(ViewPagerView); 
                    }
                    ViewPagerView.Adapter.NotifyDataSetChanged();
                }
                 
                if (NewReleasesSoundAdapter.SoundsList?.Count > 0)
                {
                    NewReleasesInflated ??= NewReleasesViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, NewReleasesInflated, NewReleasesSoundAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_LatestSongs_Title), Context.GetText(Resource.String.Lbl_LatestSongs_Description));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += NewReleasesMoreOnClick;
                    }
                }
                 
                if (RecentlyPlayedSoundAdapter.SoundsList?.Count > 0)
                { 
                    RecentlyPlayedInflated ??= RecentlyPlayedViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, RecentlyPlayedInflated, RecentlyPlayedSoundAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_RecentlyPlayed));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += RecentlyPlayedMoreOnClick;
                    }
                }
                 
                if (PopularSoundAdapter.SoundsList?.Count > 0)
                { 
                    PopularInflated ??= PopularViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, PopularInflated, PopularSoundAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Popular_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += PopularMoreOnClick;
                    }
                }

                if (GenresAdapter.GenresList.Count == 0)
                    GenresAdapter.GenresList = new ObservableCollection<GenresObject.DataGenres>(ListUtils.GenresList);

                if (GenresAdapter.GenresList.Count > 0)
                {
                    BrowseInflated ??= BrowseViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<GenresObject.DataGenres>(Activity, BrowseInflated, GenresAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Genres), Context.GetText(Resource.String.Lbl_Browse_Description));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += BrowseMoreOnClick;
                    } 
                }

                if (RecommendedList?.Count == 0 && NewReleasesSoundAdapter?.SoundsList?.Count == 0 && RecentlyPlayedSoundAdapter?.SoundsList?.Count == 0 &&
                    PopularSoundAdapter?.SoundsList?.Count == 0&& GenresAdapter?.GenresList?.Count == 0 && ArtistsAdapter.ArtistsList?.Count == 0)
                {
                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSound);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.OnFinishFreshAndLoad();
                if (ProgressBar.Visibility == ViewStates.Visible)
                    ProgressBar.Visibility = ViewStates.Gone;
                Console.WriteLine(e);
            }
        }
         
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    StartApiService();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async Task LoadArtists(string offsetArtists = "0")
        {
            int countList = ArtistsAdapter.ArtistsList.Count;
            (int apiStatus, var respond) = await RequestsAsync.User.GetArtistsAsync("20", offsetArtists).ConfigureAwait(false);
            if (apiStatus == 200)
            {
                if (respond is GetUserObject result)
                {
                    var respondList = result.Data?.UserList.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data?.UserList let check = ArtistsAdapter.ArtistsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                ArtistsAdapter.ArtistsList.Add(item);
                            }

                            Activity.RunOnUiThread(() =>
                            {
                                ArtistsAdapter.NotifyItemRangeInserted(countList, ArtistsAdapter.ArtistsList.Count - countList);
                            });
                        }
                        else
                        {
                            ArtistsAdapter.ArtistsList = new ObservableCollection<UserDataObject>(result.Data?.UserList);

                            Activity.RunOnUiThread(() =>
                            {
                                ArtistsInflated ??= ArtistsViewStub.Inflate();

                                TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                                recyclerInflater.InflateLayout<UserDataObject>(Activity, ArtistsInflated, ArtistsAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Artists));
                                if (!recyclerInflater.MainLinear.HasOnClickListeners)
                                {
                                    recyclerInflater.MainLinear.Click += null;
                                    recyclerInflater.MainLinear.Click += ArtistsMoreOnClick;
                                }

                                if (ArtistsScrollEvent == null)
                                {
                                    RecyclerViewOnScrollListener playlistRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(recyclerInflater.LayoutManager);
                                    ArtistsScrollEvent = playlistRecyclerViewOnScrollListener;
                                    ArtistsScrollEvent.LoadMoreEvent += ArtistsScrollEventOnLoadMoreEvent;
                                    recyclerInflater.Recyler.AddOnScrollListener(playlistRecyclerViewOnScrollListener);
                                    ArtistsScrollEvent.IsLoading = false;
                                }
                            }); 
                        }
                    }
                    else
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreArtists), ToastLength.Short).Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            //Activity.RunOnUiThread(ShowEmptyPage);
        }

        #endregion

    }
}