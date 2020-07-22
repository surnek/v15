using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AFollestad.MaterialDialogs;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Com.Sothree.Slidinguppanel;
using DeepSound.Activities.Comments;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.Songs;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Refractored.Controls;
using Console = System.Console;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace DeepSound.Helpers.MediaPlayerController
{
    public class SoundController : Object, SeekBar.IOnSeekBarChangeListener, Animator.IAnimatorListener, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private AppCompatSeekBar SeekSongProgressbar;
        private ProgressBar TopSeekSongProgressbar;
        public FloatingActionButton BtPlay;
        private TextView  TvTitleSound, TvDescriptionSound;
        private TextView TvSongCurrentDuration, TvSongTotalDuration, TxtArtistName, TxtArtistAbout , TxtPlaybackSpeed;
        private ImageView BtnIconComments, BtnIconFavorite, BtnIconLike, BtnIconDislike;
        public ImageView BtnIconDownload;
        public ProgressBar ProgressBarDownload;
        private ImageView BtnIconAddTo, BtnIconShare, IconInfo;
        private FrameLayout LinearAddTo, LinearShare, LinearComments, LinearFavorite, LinearLike, LinearDislike;
        private RelativeLayout LinearDownload;
        private ImageView ImageCover;
        public ImageView BackIcon, BtnPlayImage;
        private ImageButton BtnSkipPrev, BtnNext, BtnRepeat, BtnShuffle, BtnBackward, BtnForward;
        private CircleImageView ArtistImageView;
        private Timer Timer;
        private readonly Activity ActivityContext;
        private readonly HomeActivity GlobalContext;
        public readonly SocialIoClickListeners ClickListeners;
        private string TotalIdPlaylistChecked;
        private SoundDownloadAsyncController SoundDownload;
        private RowSoundAdapter Adapter;
        private RequestBuilder FullGlideRequestBuilder;
        private RequestOptions GlideRequestOptions;

        #endregion

        #region General

        public SoundController(Activity activity)
        {
            try
            {
                ActivityContext = activity;
                GlobalContext = (HomeActivity)activity ?? HomeActivity.GetInstance();

                ClickListeners = new SocialIoClickListeners(activity);
                 
                PlayerService.ActionSeekTo = ActivityContext.PackageName + ".action.ACTION_SEEK_TO";
                PlayerService.ActionPlay = ActivityContext.PackageName + ".action.ACTION_PLAY";
                PlayerService.ActionPause = ActivityContext.PackageName + ".action.PAUSE";
                PlayerService.ActionStop = ActivityContext.PackageName + ".action.STOP";
                PlayerService.ActionSkip = ActivityContext.PackageName + ".action.SKIP";
                PlayerService.ActionRewind = ActivityContext.PackageName + ".action.REWIND";
                PlayerService.ActionToggle = ActivityContext.PackageName + ".action.ACTION_TOGGLE";
                PlayerService.ActionBackward = ActivityContext.PackageName + ".action.ACTION_BACKWARD";
                PlayerService.ActionForward = ActivityContext.PackageName + ".action.ACTION_FORWARD";
                PlayerService.ActionPlaybackSpeed = ActivityContext.PackageName + ".action.ACTION_PLAYBACK_SPEED";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void InitializeUi()
        {
            try
            {
                SeekSongProgressbar = ActivityContext.FindViewById<AppCompatSeekBar>(Resource.Id.seek_song_progressbar);
                TopSeekSongProgressbar = ActivityContext.FindViewById<ProgressBar>(Resource.Id.seek_song_progressbar2);
                BtPlay = ActivityContext.FindViewById<FloatingActionButton>(Resource.Id.bt_play);
                BtnSkipPrev = ActivityContext.FindViewById<ImageButton>(Resource.Id.bt_skipPrev);
                BtnNext = ActivityContext.FindViewById<ImageButton>(Resource.Id.bt_next);
                BtnRepeat = ActivityContext.FindViewById<ImageButton>(Resource.Id.bt_repeat);
                BtnShuffle = ActivityContext.FindViewById<ImageButton>(Resource.Id.bt_shuffle);
                BtnBackward = ActivityContext.FindViewById<ImageButton>(Resource.Id.btn_Backward);
                BtnForward = ActivityContext.FindViewById<ImageButton>(Resource.Id.btn_Forward);

                TvSongCurrentDuration = ActivityContext.FindViewById<TextView>(Resource.Id.tv_song_current_duration);
                TvSongTotalDuration = ActivityContext.FindViewById<TextView>(Resource.Id.tv_song_total_duration);
                TxtArtistName = ActivityContext.FindViewById<TextView>(Resource.Id.artist_name);
                TxtArtistAbout = ActivityContext.FindViewById<TextView>(Resource.Id.artist_about);
                ArtistImageView = ActivityContext.FindViewById<CircleImageView>(Resource.Id.image);
                ImageCover = ActivityContext.FindViewById<ImageView>(Resource.Id.image_Cover);
                BtnPlayImage = ActivityContext.FindViewById<ImageView>(Resource.Id.play_button);
                BackIcon = ActivityContext.FindViewById<ImageView>(Resource.Id.BackIcon);
                
                LinearAddTo = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_playlist);
                BtnIconAddTo = ActivityContext.FindViewById<ImageView>(Resource.Id.bottombar_addtoplay);
                LinearShare = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_share);
                BtnIconShare = ActivityContext.FindViewById<ImageView>(Resource.Id.bottombar_shareicon);
                LinearComments = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_comments);
                BtnIconComments = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_comments);
                LinearDownload = ActivityContext.FindViewById<RelativeLayout>(Resource.Id.ll_download);
                BtnIconDownload = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_download);
                ProgressBarDownload = ActivityContext.FindViewById<ProgressBar>(Resource.Id.progressBar);
                ProgressBarDownload.Visibility = ViewStates.Invisible;

                LinearFavorite = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_fav);
                BtnIconFavorite = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_fav);
                LinearLike = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_like);
                BtnIconLike = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_like);
                LinearDislike = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_Dislike);
                BtnIconDislike = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_Dislike);
                IconInfo = ActivityContext.FindViewById<ImageView>(Resource.Id.info);
                TvTitleSound = ActivityContext.FindViewById<TextView>(Resource.Id.titleSound);
                TvDescriptionSound = ActivityContext.FindViewById<TextView>(Resource.Id.descriptionSound);
                TxtPlaybackSpeed = ActivityContext.FindViewById<TextView>(Resource.Id.bt_playbackSpeed);



                GlideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
                FullGlideRequestBuilder = Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));

                BtnIconDownload.Tag = "Download";
                BtnSkipPrev.Tag = "no";
                BtnNext.Tag = "no";
                BtnRepeat.Tag = "no";
                BtnShuffle.Tag = "no";
                BtnBackward.Tag = "no";
                BtnForward.Tag = "no";

                if (!AppSettings.ShowForwardTrack)
                    BtnForward.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowBackwardTrack)
                    BtnBackward.Visibility = ViewStates.Gone;

                SetProgress();

                // set Event 
                if (!BtnPlayImage.HasOnClickListeners)
                {
                    BtnPlayImage.Click += BtPlayOnClick;
                    IconInfo.Click += IconInfoOnClick;

                    BtPlay.Click += BtPlayOnClick;
                    BackIcon.Click += BackIconOnClick;

                    BtnSkipPrev.Click += BtnSkipPrevOnClick;
                    BtnNext.Click += BtnNextOnClick;
                    BtnRepeat.Click += BtnRepeatOnClick;
                    BtnShuffle.Click += BtnShuffleOnClick;
                    BtnBackward.Click += BtnBackwardOnClick;
                    BtnForward.Click += BtnForwardOnClick;

                    LinearAddTo.Click += LinearAddToOnClick;
                    LinearShare.Click += LinearShareOnClick;
                    LinearComments.Click += LinearCommentsOnClick;
                    LinearDownload.Click += LinearDownloadOnClick;
                    LinearFavorite.Click += LinearFavoriteOnClick;
                    LinearLike.Click += LinearLikeOnClick;
                    LinearDislike.Click += LinearDislikeOnClick;
                    TxtPlaybackSpeed.Click += TxtPlaybackSpeedOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Destroy()
        {
            try
            {
                if (Timer != null)
                {
                    Timer?.Stop();
                    Timer?.Dispose();
                }

                if (GlobalContext.SlidingUpPanel.GetPanelState() != SlidingUpPanelLayout.PanelState.Hidden)
                    GlobalContext.SlidingUpPanel?.SetPanelState(SlidingUpPanelLayout.PanelState.Hidden);

                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null) item.IsPlay = false;

                Adapter?.NotifyItemChanged(Constant.PlayPos);

                if (Constant.Player == null)
                    return;

                if (Constant.Player.PlayWhenReady)
                {
                    Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                    intent.SetAction(PlayerService.ActionStop);
                    GlobalContext.StartService(intent);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Event Click

        //Show info Data song 
        private void IconInfoOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    new DialogInfoSong(ActivityContext).Display(item);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //click play Sound
        private void BtPlayOnClick(object sender, EventArgs e)
        {
            try
            {
                // check for already playing
                StartOrPausePlayer();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //click Back
        private void BackIconOnClick(object sender, EventArgs e)
        {
            try
            {
                if (GlobalContext.SlidingUpPanel != null && (GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded || GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Anchored))
                {
                    GlobalContext.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                }
                else if (GlobalContext.SlidingUpPanel != null && GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    StopFragmentSound();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private FloatingActionButton PlayAllButton;

        public void SetPlayAllButton(FloatingActionButton playAllButton)
        {
            PlayAllButton = playAllButton;
        }

        public void StopFragmentSound()
        {
            try
            {
                if (GlobalContext.SlidingUpPanel != null && GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    ReleaseSound();
                    Destroy();
                    PlayerService.GetPlayerService()?.RemoveNoti();
                    GlobalContext.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Hidden); 
                }

                if (PlayAllButton != null)
                {
                    PlayAllButton.SetImageResource(Resource.Drawable.icon_play_action_small_vector);
                    PlayAllButton.Tag = "play";
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Shuffle Sound
        private void BtnShuffleOnClick(object sender, EventArgs e)
        {
            try
            {
                Constant.IsSuffle = !Constant.IsSuffle;
                ToggleButtonColor(BtnShuffle);

                if (BtnRepeat.Tag.ToString() == "selected")
                {
                    Constant.IsRepeat = false;
                    ToggleButtonColor(BtnRepeat); // clear 
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Repeat Sound
        private void BtnRepeatOnClick(object sender, EventArgs e)
        {
            try
            {
                Constant.IsRepeat = !Constant.IsRepeat;
                ToggleButtonColor(BtnRepeat);

                if (BtnShuffle.Tag.ToString() == "selected")
                {
                    Constant.IsSuffle = !Constant.IsSuffle;
                    ToggleButtonColor(BtnShuffle);  // clear 
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Forward 10 Sec
        private void BtnForwardOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionForward);
                GlobalContext.StartService(intent);

                if (Timer != null)
                {
                    // update timer progress again
                    Timer.Enabled = true;
                    Timer.Start();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Backward 10 Sec
        private void BtnBackwardOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionBackward);
                GlobalContext.StartService(intent);

                if (Timer != null)
                {
                    // update timer progress again
                    Timer.Enabled = true;
                    Timer.Start();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        
        //Run Next Sound
        private void BtnNextOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                {
                    var item = Constant.ArrayListPlay[Constant.PlayPos];
                    if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && item.AudioLocation.Contains("http"))
                    { 
                        item.IsPlay = false; 
                        Adapter?.NotifyItemChanged(Constant.PlayPos);

                        if (!Constant.IsOnline || Methods.CheckConnectivity())
                        {
                            Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                            intent.SetAction(PlayerService.ActionSkip);
                            GlobalContext.StartService(intent);
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        }
                    }
                    else if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && (item.AudioLocation.Contains("file://") || item.AudioLocation.Contains("content://") ||
                                                                                           item.AudioLocation.Contains("storage") || item.AudioLocation.Contains("/data/user/0/")))
                    {
                        item.IsPlay = false;
                        Adapter?.NotifyItemChanged(Constant.PlayPos);

                        Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                        intent.SetAction(PlayerService.ActionSkip);
                        GlobalContext.StartService(intent);
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoSongSelected), ToastLength.Long).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Skip Prev 
        private void BtnSkipPrevOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                {
                    var item = Constant.ArrayListPlay[Constant.PlayPos];
                    if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && item.AudioLocation.Contains("http"))
                    {
                        if (!Constant.IsOnline || Methods.CheckConnectivity())
                        {
                            Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                            intent.SetAction(PlayerService.ActionRewind);
                            GlobalContext.StartService(intent);
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        }
                    }
                    else if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && (item.AudioLocation.Contains("file://") || item.AudioLocation.Contains("content://") ||
                                                                                           item.AudioLocation.Contains("storage") || item.AudioLocation.Contains("/data/user/0/")))
                    {
                        Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                        intent.SetAction(PlayerService.ActionRewind);
                        GlobalContext.StartService(intent);
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoSongSelected), ToastLength.Long).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Change Like
        private void LinearLikeOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                   ClickListeners.OnLikeSongsClick(new LikeSongsClickEventArgs() { LikeButton = BtnIconLike, SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Change Dislike
        private void LinearDislikeOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    ClickListeners.OnDislikeSongsClick(new LikeSongsClickEventArgs() { LikeButton = BtnIconDislike, SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        //Add Or remove Favorite 
        private void LinearFavoriteOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    ClickListeners.OnFavoriteSongsClick(new FavSongsClickEventArgs() { FavButton = BtnIconFavorite, SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Comments
        private void LinearCommentsOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    new DialogComment(ActivityContext).Display(item, TvSongCurrentDuration.Text);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Download
        private void LinearDownloadOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                {
                    Methods.Path.Chack_MyFolder();

                    var item = Constant.ArrayListPlay[Constant.PlayPos];
                    if (item != null)
                    { 
                        if (!Directory.Exists(Methods.Path.FolderDcimSound))
                            Directory.CreateDirectory(Methods.Path.FolderDcimSound);

                        string getFile = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimSound, item.Title);
                        if (getFile != "File Dont Exists")
                        {
                            var dialog = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                            dialog.Title(Resource.String.Lbl_DeleteSong);
                            dialog.Content(ActivityContext.GetText(Resource.String.Lbl_Do_You_want_to_remove_Song));
                            dialog.PositiveText(ActivityContext.GetText(Resource.String.Lbl_Yes)).OnPositive((o, args) =>
                            {
                                try
                                {
                                    SoundDownload = new SoundDownloadAsyncController(item.AudioLocation, item.Title, ActivityContext);
                                    SoundDownload?.RemoveDiskSoundFile(item.Title);

                                    BtnIconDownload.Tag = "Download";
                                    BtnIconDownload.SetImageResource(Resource.Drawable.icon_player_download);
                                    BtnIconDownload.SetColorFilter(Color.White);
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine(exception);
                                }
                            });
                            dialog.NegativeText(ActivityContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                            dialog.AlwaysCallSingleChoiceCallback();
                            dialog.Build().Show();
                        }
                        else
                        {
                            SoundDownload = new SoundDownloadAsyncController(item.AudioLocation, item.Title, ActivityContext);

                            if (!SoundDownload.CheckDownloadLinkIfExits())
                            {
                                SoundDownload.StartDownloadManager(item.Title, item, "Main");

                                ProgressBarDownload.Visibility = ViewStates.Visible;
                                BtnIconDownload.Visibility = ViewStates.Invisible;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Share
        private void LinearShareOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    ClickListeners.OnShareClick(new ShareSongClickEventArgs { SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Add To Playlist
        private void LinearAddToOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(ActivityContext, null, "Login");
                    dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Login), ActivityContext.GetText(Resource.String.Lbl_Message_Sorry_signin), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                var arrayAdapter = new List<string>();
                var arrayIndexAdapter = new int[] { };
                var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                 
                if (ListUtils.PlaylistList?.Count > 0) arrayAdapter.AddRange(ListUtils.PlaylistList.Select(playlistDataObject => Methods.FunString.DecodeString(playlistDataObject.Name)));

                dialogList.Title(ActivityContext.GetText(Resource.String.Lbl_SelectPlaylist))
                    .Items(arrayAdapter)
                    .ItemsCallbackMultiChoice(arrayIndexAdapter, OnSelection)
                    .AlwaysCallMultiChoiceCallback()
                    .NegativeText(ActivityContext.GetText(Resource.String.Lbl_Close)).OnNegative(this)
                    .PositiveText(ActivityContext.GetText(Resource.String.Lbl_Done)).OnPositive(this)
                    .NeutralText(ActivityContext.GetText(Resource.String.Lbl_Create)).OnNeutral(this)
                    .Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //speed playback (1x, 1.5x, 2x) 
        private void TxtPlaybackSpeedOnClick(object sender, EventArgs e)
        {
            try
            { 
                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionPlaybackSpeed);

                if (TxtPlaybackSpeed.Text == "1x")
                {
                    intent.PutExtra("PlaybackSpeed", "Medium");
                    TxtPlaybackSpeed.Text = "1.5x";
                }
                else if (TxtPlaybackSpeed.Text == "1.5x")
                {
                    intent.PutExtra("PlaybackSpeed", "High");
                    TxtPlaybackSpeed.Text = "2x";
                }
                else if (TxtPlaybackSpeed.Text == "2x")
                {
                    intent.PutExtra("PlaybackSpeed", "Normal");
                    TxtPlaybackSpeed.Text = "1x";
                }

                TxtPlaybackSpeed.SetTextColor(Color.ParseColor(AppSettings.MainColor));

                GlobalContext.StartService(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Fun >> SeekBar

        // set Progress bar values
        public void SetProgress()
        {
            try
            {
                // Run timer
                Timer = new Timer
                {
                    Interval = 1000
                };
                Timer.Elapsed += TimerOnElapsed;

                SeekSongProgressbar.Max = MusicUtils.MaxProgress;
                SeekSongProgressbar.SetOnSeekBarChangeListener(this);

                TopSeekSongProgressbar.Max = MusicUtils.MaxProgress;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    SeekSongProgressbar.SetProgress(0, true);
                    TopSeekSongProgressbar.SetProgress(0, true);
                }
                else
                {
                    try
                    {
                        // For API < 24 
                        SeekSongProgressbar.Progress = 0;
                        TopSeekSongProgressbar.Progress = 0;
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {

        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            try
            {
                if (Timer != null)
                {
                    // remove message Handler from updating progress bar
                    Timer.Enabled = false;
                    Timer.Stop();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                long progress = seekBar.Progress;

                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionSeekTo);
                intent.PutExtra("seekTo", progress);
                GlobalContext.StartService(intent);

                if (Timer != null)
                {
                    // update timer progress again
                    Timer.Enabled = true;
                    Timer.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Fun Player

        public void StartPlaySound(SoundDataObject soundObject, ObservableCollection<SoundDataObject> listSound, RowSoundAdapter adapter = null)
        {
            try
            {
                Adapter = adapter;
                GlobalContext?.SetWakeLock();

                if (listSound.Count > 0)
                {
                    Constant.IsPlayed = false;
                    Constant.IsOnline = true;
                    Constant.ArrayListPlay = new ObservableCollection<SoundDataObject>(listSound);
                }

                if (soundObject != null)
                {
                    LoadSoundData(soundObject , false);

                    ReleaseSound();

                    //Play Song  
                    if (GlobalContext?.SlidingUpPanel != null && GlobalContext?.SlidingUpPanel != null)
                    {
                        StartOrPausePlayer();

                        BackIcon.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                        BackIcon.Tag = "Open";

                        if (GlobalContext?.SlidingUpPanel.GetPanelState() != SlidingUpPanelLayout.PanelState.Collapsed)
                            GlobalContext?.SlidingUpPanel?.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void ReleaseSound()
        {
            try
            {
                if (Constant.Player != null)
                {
                    if (Constant.Player.PlayWhenReady)
                    {
                        Constant.Player.Stop();
                        Constant.Player.Release();
                        Constant.Player = null;
                    }
                }

                if (GlobalContext?.Timer != null)
                {
                    GlobalContext.Timer.Enabled = false;
                    GlobalContext.Timer.Stop();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartOrPausePlayer()
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                {
                    var item = Constant.ArrayListPlay[Constant.PlayPos];
                    Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                    if (Constant.IsPlayed)
                    {
                        if (Constant.Player.PlayWhenReady)
                        {
                            if (item != null) item.IsPlay = false;
                            intent.SetAction(PlayerService.ActionPause);
                            GlobalContext.StartService(intent);


                        }
                        else
                        {
                            if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && item.AudioLocation.Contains("http"))
                            {
                                if (!Constant.IsOnline || Methods.CheckConnectivity())
                                {
                                    item.IsPlay = true;
                                    intent.SetAction(PlayerService.ActionPlay);
                                    GlobalContext.StartService(intent);
                                }
                                else
                                {
                                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                }
                            }
                            else if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && (item.AudioLocation.Contains("file://") || item.AudioLocation.Contains("content://") || item.AudioLocation.Contains("storage") || item.AudioLocation.Contains("/data/user/0/")))
                            {
                                item.IsPlay = true;
                                intent.SetAction(PlayerService.ActionPlay);
                                GlobalContext.StartService(intent);
                            }
                        }
                    }
                    else
                    {
                        if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && item.AudioLocation.Contains("http"))
                        {
                            if (!Constant.IsOnline || Methods.CheckConnectivity())
                            {
                                item.IsPlay = true;
                                intent.SetAction(PlayerService.ActionPlay);
                                GlobalContext.StartService(intent);
                            }
                            else
                            {
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                            }
                        }
                        else if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && (item.AudioLocation.Contains("file://") || item.AudioLocation.Contains("content://") || item.AudioLocation.Contains("storage") || item.AudioLocation.Contains("/data/user/0/")))
                        {
                            item.IsPlay = true;
                            intent.SetAction(PlayerService.ActionPlay);
                            GlobalContext.StartService(intent);
                        }
                    }

                    Adapter?.NotifyItemChanged(Constant.PlayPos);
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoSongSelected), ToastLength.Long).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void LoadSoundData(SoundDataObject soundObject , bool isPlay)
        {
            try
            {
                GlobalContext.RunOnUiThread(() =>
                {
                    try
                    {
                        if (isPlay)
                        {
                            try
                            {
                                var list = Adapter?.SoundsList.Where(sound => sound.IsPlay).ToList();
                                if (list?.Count > 0)
                                    foreach (var all in list)
                                        all.IsPlay = false;

                                var item = Adapter?.GetItem(Constant.PlayPos);
                                if (item != null)
                                    item.IsPlay = true;

                                Adapter?.NotifyDataSetChanged();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }

                        var imageUrl = string.Empty;
                        if (!string.IsNullOrEmpty(soundObject.ThumbnailOriginal))
                        {
                            if (!soundObject.ThumbnailOriginal.Contains(DeepSoundClient.Client.WebsiteUrl))
                                imageUrl = DeepSoundClient.Client.WebsiteUrl + "/" + soundObject.ThumbnailOriginal;
                            else
                                imageUrl = soundObject.ThumbnailOriginal;
                        }

                        if (string.IsNullOrEmpty(imageUrl))
                            imageUrl = soundObject.Thumbnail;

                        FullGlideRequestBuilder.Load(imageUrl).Into(ImageCover); 
                        GlideImageLoader.LoadImage(ActivityContext, imageUrl, ArtistImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                         
                        TxtArtistName.Text = Methods.FunString.DecodeString(soundObject.Publisher.Name);

                        var d = soundObject.Title.Replace("<br>", "");
                        TxtArtistAbout.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(d), 30);

                        TvTitleSound.Text = Methods.FunString.DecodeString(d);

                        TvDescriptionSound.Text = string.IsNullOrEmpty(soundObject.AlbumName)
                            ? Methods.FunString.DecodeString(soundObject.CategoryName) + " " + ActivityContext.GetText(Resource.String.Lbl_Music)
                            : Methods.FunString.DecodeString(soundObject.CategoryName)+ " " + ActivityContext.GetText(Resource.String.Lbl_Music) + ", " + ActivityContext.GetText(Resource.String.Lbl_InAlbum) + " " + Methods.FunString.DecodeString(soundObject.AlbumName);

                        TvSongTotalDuration.Text = soundObject.Duration;
                        TvSongCurrentDuration.Text = "0:00";

                        BtnIconLike.Tag = soundObject.IsLiked != null && soundObject.IsLiked.Value ? "Like" : "Liked" ?? "Liked";
                        ClickListeners.SetLike(BtnIconLike);

                        BtnIconDislike.Tag = soundObject.IsDisLiked != null && soundObject.IsDisLiked.Value ? "Dislike" : "Disliked" ?? "Disliked";
                        ClickListeners.SetDislike(BtnIconDislike);

                        BtnIconFavorite.Tag = soundObject.IsFavoriated != null && soundObject.IsFavoriated.Value ? "Add" : "Added";
                        ClickListeners.SetFav(BtnIconFavorite);

                        //Add CreateCacheMediaSource if have or no 
                        //var fileSplit = soundObject.Id.Split('/').Last();

                        if (soundObject.IsOwner != null && (soundObject.AllowDownloads == 0 && !soundObject.IsOwner.Value))
                            LinearDownload.Visibility = ViewStates.Gone;
                        else
                            LinearDownload.Visibility = ViewStates.Visible;

                        TxtPlaybackSpeed.Text = "1x";
                        TxtPlaybackSpeed.SetTextColor(Color.ParseColor("#999999"));
                          
                        var sqlEntity = new SqLiteDatabase();
                        var dataSound = sqlEntity.Get_LatestDownloadsSound(soundObject.Id);
                        if (dataSound != null)
                        {
                            if (!string.IsNullOrEmpty(dataSound.AudioLocation) && (dataSound.AudioLocation.Contains("file://") || dataSound.AudioLocation.Contains("content://") || dataSound.AudioLocation.Contains("storage") || dataSound.AudioLocation.Contains("/data/user/0/")))
                            {
                                if (!Directory.Exists(Methods.Path.FolderDcimSound))
                                    Directory.CreateDirectory(Methods.Path.FolderDcimSound);

                                var title = dataSound.AudioLocation.Split("/Sound/").Last();
                                string getFile = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimSound, title);
                                if (getFile != "File Dont Exists")
                                {
                                    BtnIconDownload.Tag = "Downloaded";
                                    BtnIconDownload.SetImageResource(Resource.Drawable.ic_check_circle);
                                    BtnIconDownload.SetColorFilter(Color.Red);
                                }
                                else
                                {
                                    BtnIconDownload.Tag = "Download";
                                    BtnIconDownload.SetImageResource(Resource.Drawable.icon_player_download);
                                    BtnIconDownload.SetColorFilter(Color.White); 
                                }
                            }
                            else
                            {
                                BtnIconDownload.Tag = "Download";
                                BtnIconDownload.SetImageResource(Resource.Drawable.icon_player_download);
                                BtnIconDownload.SetColorFilter(Color.White);
                            }
                        }
                        else
                        {
                            BtnIconDownload.Tag = "Download";
                            BtnIconDownload.SetImageResource(Resource.Drawable.icon_player_download);
                            BtnIconDownload.SetColorFilter(Color.White);
                        }

                        ProgressBarDownload.Visibility = ViewStates.Invisible;
                        BtnIconDownload.Visibility = ViewStates.Visible;

                        sqlEntity.Dispose(); 
                    } 
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ChangePlayPauseIcon()
        {
            try
            {
                GlobalContext.RunOnUiThread(() =>
                {
                    // check for already playing
                    if (Constant.Player.PlayWhenReady)
                    {
                        // Changing button image to pause button
                        BtPlay.SetImageResource(Resource.Drawable.icon_player_pause);
                        BtnPlayImage.SetImageResource(Resource.Drawable.icon_player_pause);

                        if (Timer != null)
                        {
                            Timer.Enabled = true;
                            Timer.Start();
                        }
                    }
                    else
                    {
                        // Changing button image to play button
                        BtPlay.SetImageResource(Resource.Drawable.icon_player_play);
                        BtnPlayImage.SetImageResource(Resource.Drawable.icon_player_play);

                        if (Timer != null)
                        {
                            Timer.Enabled = false;
                            Timer.Stop();
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SeekUpdate()
        {
            try
            {
                ActivityContext.RunOnUiThread(() =>
                {
                    try
                    {
                        var totalDuration = Constant.Player.Duration;
                        var currentDuration = Constant.Player.CurrentPosition;

                        // Displaying Total Duration time
                        TvSongTotalDuration.Text = MusicUtils.MilliSecondsToTimer(totalDuration);
                        // Displaying time completed playing
                        TvSongCurrentDuration.Text = MusicUtils.MilliSecondsToTimer(currentDuration);

                        // Updating progress bar
                        int progress = MusicUtils.GetProgressSeekBar(currentDuration, totalDuration);

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        {
                            SeekSongProgressbar.SetProgress(progress, true);
                            TopSeekSongProgressbar.SetProgress(progress, true);
                        }
                        else
                        {
                            try
                            {
                                // For API < 24 
                                SeekSongProgressbar.Progress = progress;
                                TopSeekSongProgressbar.Progress = progress;
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception);
                            }
                        }

                        if (currentDuration >= totalDuration)
                        {
                            if (Constant.IsRepeat)
                            {
                                Constant.Player.SeekTo(0);
                                Constant.Player.PlayWhenReady = true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Runnable

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //Running this thread after 10 milliseconds
                SeekUpdate();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Animation Image 

        public void RotateImageAlbum()
        {
            try
            {
                if (!Constant.Player.PlayWhenReady) return;
                ArtistImageView.Animate().SetDuration(100).Rotation(ArtistImageView.Rotation + 2f).SetListener(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnAnimationCancel(Animator animation)
        {

        }

        public void OnAnimationEnd(Animator animation)
        {
            try
            {
                RotateImageAlbum();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnAnimationRepeat(Animator animation)
        {

        }

        public void OnAnimationStart(Animator animation)
        {

        }

        #endregion

        private void ToggleButtonColor(ImageButton bt)
        {
            try
            {
                if (bt != null)
                {
                    string selected = bt.Tag.ToString();
                    if (selected == "selected")
                    {
                        // selected
                        bt.SetColorFilter(Color.ParseColor("#999999"));
                        bt.Tag = "no";
                    }
                    else
                    {
                        bt.Tag = "selected";
                        bt.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetUiSliding(bool show)
        {
            try
            {
                var soundObject = Constant.ArrayListPlay[Constant.PlayPos];
                if (soundObject != null)
                {
                    if (show)
                    {
                        IconInfo.Visibility = ViewStates.Gone;
                        BtnPlayImage.Visibility = ViewStates.Visible;
                        TopSeekSongProgressbar.Visibility = ViewStates.Visible;

                        TxtArtistAbout.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(soundObject.Title), 30);
                    }
                    else
                    {
                        IconInfo.Visibility = ViewStates.Visible;
                        BtnPlayImage.Visibility = ViewStates.Gone;
                        TopSeekSongProgressbar.Visibility = ViewStates.Invisible;

                        TxtArtistAbout.Text = soundObject.TimeFormatted;
                    }

                    if (GlobalContext?.FragmentBottomNavigator?.PageNumber == 4)
                    {
                        GlobalContext.MoreMultiButtons.Visibility = GlobalContext?.SlidingUpPanel != null && GlobalContext?.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded ? ViewStates.Gone : ViewStates.Visible;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region MaterialDialog >> Add To Playlist Async

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                    TotalIdPlaylistChecked = TotalIdPlaylistChecked.Remove(TotalIdPlaylistChecked.Length - 1, 1);
                    if (Methods.CheckConnectivity())
                    {
                        var item = Constant.ArrayListPlay[Constant.PlayPos];
                        if (item != null)
                        {
                            //Sent Api
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Playlist.AddToPlaylistAsync(item.Id.ToString(), TotalIdPlaylistChecked) });
                        }
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    }
                }
                else if (p1 == DialogAction.Neutral)
                {
                    ActivityContext.StartActivity(new Intent(ActivityContext, typeof(CreatePlaylistActivity)));
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

        private bool OnSelection(MaterialDialog dialog, int[] which, string[] text)
        {
            try
            {
                var list = ListUtils.PlaylistList;
                if (list?.Count > 0)
                {
                    for (int i = 0; i < which.Length; i++)
                    {
                        TotalIdPlaylistChecked += list[i].Id + ",";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return true;
            }
            return true;
        }

        #endregion

    }
}