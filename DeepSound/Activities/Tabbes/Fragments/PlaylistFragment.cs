using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Library;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.Playlist.Adapters;
using DeepSound.Activities.Tabbes.Adapters;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Requests;
using Liaoinstan.SpringViewLib.Widgets;
using Me.Relex;
using Newtonsoft.Json;
using DefaultHeader = DeepSound.Helpers.PullSwipeStyles.DefaultHeader;

namespace DeepSound.Activities.Tabbes.Fragments
{
    public class PlaylistFragment : Fragment, SpringView.IOnFreshListener
    {
        #region Variables Basic

        private HPlaylistAdapter PublicPlaylistAdapter;
        private HomeActivity GlobalContext;
        private SpringView SwipeRefreshLayout;
        private ViewStub EmptyStateLayout, PublicPlaylistViewStub;
        private View Inflated, PublicPlaylistInflated;
        private RecyclerViewOnScrollListener PublicPlaylistScrollEvent;
        private PlaylistProfileFragment PlaylistProfileFragment;
        private ProgressBar ProgressBar;
        private FloatingActionButton BtnAdd;
        public MyPlaylistFragment MyPlaylistFragment;
        private TemplateRecyclerInflater RecyclerInflaterPublicPlaylist;
        private ViewPager ViewPagerView;
        private CircleIndicator ViewPagerCircleIndicator;
        private PlayListViewPagerAdapter PlayListViewPagerAdapter;
        private LinearLayout MyPlaylistLinear;

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
                View view = inflater.Inflate(Resource.Layout.TPlaylistLayout, container, false); 
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
                ProgressBar = view.FindViewById<ProgressBar>(Resource.Id.progress);
                ProgressBar.Visibility = ViewStates.Visible;

                PublicPlaylistViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubPublicePlaylist);
                EmptyStateLayout = (ViewStub)view.FindViewById(Resource.Id.viewStub);

                MyPlaylistLinear = view.FindViewById<LinearLayout>(Resource.Id.myPlaylistLinear);
                MyPlaylistLinear.Click += MyPlaylistLinearOnClick;
                MyPlaylistLinear.Visibility = ViewStates.Gone;

                ViewPagerView = view.FindViewById<ViewPager>(Resource.Id.viewpager3);
                ViewPagerCircleIndicator = (CircleIndicator)view.FindViewById(Resource.Id.indicator1);
                ViewPagerView.PageMargin = 6;
                ViewPagerView.SetClipChildren(false);
                ViewPagerView.SetPageTransformer(true, new CarouselEffectTransformer2(Activity));

                SwipeRefreshLayout = (SpringView)view.FindViewById(Resource.Id.material_style_ptr_frame);
                SwipeRefreshLayout.SetType(SpringView.Type.Overlap);
                SwipeRefreshLayout.Header = new DefaultHeader(Activity);
                SwipeRefreshLayout.Footer = new Helpers.PullSwipeStyles.DefaultFooter(Activity);
                SwipeRefreshLayout.Enable = true;
                SwipeRefreshLayout.SetListener(this);

                BtnAdd = (FloatingActionButton)view.FindViewById(Resource.Id.floatingAdd); 
                BtnAdd.Visibility = UserDetails.IsLogin ? ViewStates.Visible : ViewStates.Gone;
                BtnAdd.Click += BtnAddOnClick;
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
                //Public Playlist RecyclerView
                PublicPlaylistAdapter = new HPlaylistAdapter(Activity) { PlaylistList = new ObservableCollection<PlaylistDataObject>() };
                PublicPlaylistAdapter.OnItemClick += PublicPlaylistAdapterOnOnItemClick;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
          
        #endregion

        #region Refresh

        public void OnRefresh()
        {
            try
            {
                ViewPagerView.Adapter = null;
                ViewPagerView.CurrentItem = 0;
                
                PublicPlaylistAdapter.PlaylistList.Clear();
                PublicPlaylistAdapter.NotifyDataSetChanged();

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
                if (PublicPlaylistScrollEvent.IsLoading == false)
                {
                    PublicPlaylistScrollEvent.IsLoading = true;
                    var item = PublicPlaylistAdapter.PlaylistList.LastOrDefault();
                    if (item != null)
                    { 
                        GetPublicPlaylist(item.Id.ToString()).ConfigureAwait(false);
                        PublicPlaylistScrollEvent.IsLoading = false;
                    }
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        public void UpdateMyPlaylist()
        {
            try 
            {
                ViewPagerView.Adapter = null;
                ViewPagerView.CurrentItem = 0;
                 
                if (ViewPagerView.Adapter == null)
                {
                    PlayListViewPagerAdapter = new PlayListViewPagerAdapter(Activity, ListUtils.PlaylistList);
                    ViewPagerView.Adapter = PlayListViewPagerAdapter;
                    ViewPagerView.CurrentItem = 0;
                    ViewPagerCircleIndicator.SetViewPager(ViewPagerView);
                }
                ViewPagerView.Adapter.NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        #region Get Public Playlist Api 

        private void StartApiService()
        {
            if (Methods.CheckConnectivity())
            {
                PollyController.RunRetryPolicyFunction(UserDetails.IsLogin ? new List<Func<Task>> {() => GetPublicPlaylist(), () => GetPlaylist()} : new List<Func<Task>> {() => GetPublicPlaylist()});
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
        
        private async Task GetPublicPlaylist(string offsetPublicPlaylist = "0")
        {
            if (PublicPlaylistScrollEvent != null && PublicPlaylistScrollEvent.IsLoading)
                return;

            if (PublicPlaylistScrollEvent != null) PublicPlaylistScrollEvent.IsLoading = true;

            int countList = PublicPlaylistAdapter.PlaylistList.Count;
            (int apiStatus, var respond) = await RequestsAsync.Playlist.GetPublicPlaylistAsync("15", offsetPublicPlaylist);
            if (apiStatus.Equals(200))
            {
                if (respond is PlaylistObject result)
                {
                    var respondList = result.Playlist.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Playlist let check = PublicPlaylistAdapter.PlaylistList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            PublicPlaylistAdapter.PlaylistList.Add(item);
                        }

                        if (countList > 0)
                        {
                            Activity.RunOnUiThread(() => { PublicPlaylistAdapter.NotifyItemRangeInserted(countList - 1, PublicPlaylistAdapter.PlaylistList.Count - countList); });
                        }
                        else
                        {
                            Activity.RunOnUiThread(() =>
                            {
                                if (PublicPlaylistInflated == null)
                                    PublicPlaylistInflated = PublicPlaylistViewStub.Inflate();

                                RecyclerInflaterPublicPlaylist = new TemplateRecyclerInflater();
                                RecyclerInflaterPublicPlaylist.InflateLayout<PlaylistDataObject>(Activity, PublicPlaylistInflated, PublicPlaylistAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Activity.GetString(Resource.String.Lbl_Hot_Playlist));

                                if (PublicPlaylistScrollEvent == null)
                                {
                                    RecyclerViewOnScrollListener playlistRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(RecyclerInflaterPublicPlaylist.LayoutManager);
                                    PublicPlaylistScrollEvent = playlistRecyclerViewOnScrollListener;
                                    PublicPlaylistScrollEvent.LoadMoreEvent += PublicPlaylistScrollEventOnLoadMoreEvent;
                                    RecyclerInflaterPublicPlaylist.Recyler.AddOnScrollListener(playlistRecyclerViewOnScrollListener);
                                    PublicPlaylistScrollEvent.IsLoading = false;
                                }
                            }); 
                        }
                    }
                    else
                    {
                        if (RecyclerInflaterPublicPlaylist.Recyler != null)
                            if (PublicPlaylistAdapter.PlaylistList.Count > 10 && !RecyclerInflaterPublicPlaylist.Recyler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePlaylist), ToastLength.Short).Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity.RunOnUiThread(() => { ShowEmptyPage("PublicPlaylist"); });
        }

        private async Task GetPlaylist(string offsetPlaylist = "0")
        {
            if (!UserDetails.IsLogin)
                return;

            (int apiStatus, var respond) = await RequestsAsync.Playlist.GetPlaylistAsync(UserDetails.UserId.ToString(), "15", offsetPlaylist);
            if (apiStatus.Equals(200))
            {
                if (respond is PlaylistObject result)
                {
                    var respondList = result.Playlist.Count;
                    if (respondList > 0)
                    {
                        ListUtils.PlaylistList = new ObservableCollection<PlaylistDataObject>(result.Playlist);
                        if (ViewPagerView.Adapter == null)
                        {
                            PlayListViewPagerAdapter = new PlayListViewPagerAdapter(Activity, ListUtils.PlaylistList);
                            ViewPagerView.Adapter = PlayListViewPagerAdapter;
                            ViewPagerView.CurrentItem = 0;
                            ViewPagerCircleIndicator.SetViewPager(ViewPagerView);
                        }
                        ViewPagerView.Adapter.NotifyDataSetChanged();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity.RunOnUiThread(() => { ShowEmptyPage("MyPlaylist"); }); 
        }
         
        private void ShowEmptyPage(string type)
        {
            try
            {
                if (PublicPlaylistScrollEvent != null) PublicPlaylistScrollEvent.IsLoading = false;
            
                SwipeRefreshLayout.OnFinishFreshAndLoad();

                if (ProgressBar.Visibility == ViewStates.Visible)
                    ProgressBar.Visibility = ViewStates.Gone;

                if (type == "MyPlaylist")
                {
                    if (ViewPagerView.Adapter.Count > 0)
                    {
                        MyPlaylistLinear.Visibility = ViewStates.Visible; 
                    }
                }
                else
                {
                    if (PublicPlaylistAdapter?.PlaylistList?.Count == 0)
                    {
                        if (RecyclerInflaterPublicPlaylist.Recyler != null)
                            RecyclerInflaterPublicPlaylist.Recyler.Visibility = ViewStates.Gone;

                        if (Inflated == null)
                            Inflated = EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoPlaylist);
                        if (x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                        }

                        EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        if (RecyclerInflaterPublicPlaylist.Recyler != null)
                            RecyclerInflaterPublicPlaylist.Recyler.Visibility = ViewStates.Visible;

                        EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                }
              
            }
            catch (Exception e)
            {
                if (PublicPlaylistScrollEvent != null) PublicPlaylistScrollEvent.IsLoading = false;

                SwipeRefreshLayout.OnFinishFreshAndLoad();
                if (ProgressBar.Visibility == ViewStates.Visible)
                    ProgressBar.Visibility = ViewStates.Gone;
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

        #region Scroll

        private void PublicPlaylistScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = PublicPlaylistAdapter.PlaylistList.LastOrDefault(); 
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !PublicPlaylistScrollEvent.IsLoading)
                    GetPublicPlaylist(item.Id.ToString()).ConfigureAwait(false); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        #endregion

        #region Event 
         
        private void PublicPlaylistAdapterOnOnItemClick(object sender, PlaylistAdapterClickEventArgs e)
        {
            try
            {
                var item = PublicPlaylistAdapter.PlaylistList[e.Position];
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("PlaylistId", item.Id.ToString());

                    PlaylistProfileFragment = new PlaylistProfileFragment
                    {
                        Arguments = bundle
                    };

                    GlobalContext.FragmentBottomNavigator.DisplayFragment(PlaylistProfileFragment);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void MyPlaylistLinearOnClick(object sender, EventArgs e)
        {
            try
            {
                MyPlaylistFragment = new MyPlaylistFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(MyPlaylistFragment);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        // Create Playlist
        private void BtnAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                StartActivity(new Intent(Activity, typeof(CreatePlaylistActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion
    }
}