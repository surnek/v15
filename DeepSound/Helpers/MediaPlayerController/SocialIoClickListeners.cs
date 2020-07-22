using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Albums;
using DeepSound.Activities.Tabbes;
using DeepSound.Activities.Upload;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Java.Lang;
using Newtonsoft.Json;
using Plugin.Share;
using Plugin.Share.Abstractions;
using Exception = System.Exception;

namespace DeepSound.Helpers.MediaPlayerController
{
    public class SocialIoClickListeners : Java.Lang.Object, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        private readonly Activity MainContext;
        private readonly HomeActivity GlobalContext;
        private string TypeDialog, NamePage;
        private MoreSongClickEventArgs MoreSongArgs;

        public SocialIoClickListeners(Activity context)
        {
            try
            {
                MainContext = context;
                GlobalContext = (HomeActivity)MainContext ?? HomeActivity.GetInstance();
                TypeDialog = string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Add Like Or Remove 
        public void OnLikeSongsClick(LikeSongsClickEventArgs e, string name = "")
        {
            try
            { 
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    var refs = SetLike(e.LikeButton);
                    e.SongsClass.IsLiked = refs;

                    //add to Liked
                    if (refs)
                        GlobalContext?.LibrarySynchronizer.AddToLiked(e.SongsClass);

                    if (name == "AlbumsFragment")
                    {
                        var list = AlbumsFragment.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            AlbumsFragment.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "PlaylistProfileFragment")
                    {
                        var list = GlobalContext?.PlaylistFragment?.MyPlaylistFragment?.PlaylistProfileFragment?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext.PlaylistFragment.MyPlaylistFragment?.PlaylistProfileFragment?.MAdapter?.NotifyItemChanged(index);
                        }
                    }

                    //Sent Api
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.LikeUnLikeTrackAsync(e.SongsClass.AudioId) });
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Add Dislike Or Remove 
        public void OnDislikeSongsClick(LikeSongsClickEventArgs e, string name = "")
        {
            try
            { 
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    var refs = SetDislike(e.LikeButton);
                    e.SongsClass.IsDisLiked = refs;

                    //add to Disliked
                    //if (refs)
                    //    GlobalContext?.LibrarySynchronizer.AddToLiked(e.SongsClass);

                    if (name == "AlbumsFragment")
                    {
                        var list = AlbumsFragment.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsDisLiked = refs;
                            int index = list.IndexOf(dataSong);
                            AlbumsFragment.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "PlaylistProfileFragment")
                    {
                        var list = GlobalContext?.PlaylistFragment?.MyPlaylistFragment?.PlaylistProfileFragment?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsDisLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext.PlaylistFragment.MyPlaylistFragment?.PlaylistProfileFragment?.MAdapter?.NotifyItemChanged(index);
                        }
                    }

                    //Sent Api 
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.DislikeUnDislikeTrackAsync(e.SongsClass.AudioId) });
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Add Favorite Or Remove 
        public void OnFavoriteSongsClick(FavSongsClickEventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }


                if (Methods.CheckConnectivity())
                {
                    var refs = SetFav(e.FavButton);
                    e.SongsClass.IsFavoriated = refs;

                    //add to Favorites
                    if (refs)
                        GlobalContext?.LibrarySynchronizer.AddToFavorites(e.SongsClass);

                    //Sent Api
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.AddOrRemoveFavoriteTrackAsync(e.SongsClass.AudioId) });
                    
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Share
        public async void OnShareClick(ShareSongClickEventArgs args)
        {
            try
            {
                if (!CrossShare.IsSupported)
                    return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = args.SongsClass.Title,
                    Text = args.SongsClass.Description,
                    Url = args.SongsClass.Url
                });

                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdate_SharedSound(args.SongsClass);
                dbDatabase.Dispose();

                if (UserDetails.IsLogin)
                    GlobalContext?.LibrarySynchronizer?.AddToShareSong(args.SongsClass);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        //Event Show More :  DeleteSong , EditSong , GoSong , Copy Link  , Report .. 
        public void OnMoreClick(MoreSongClickEventArgs args, string namePage = "")
        {
            try
            {
                NamePage = namePage;
                MoreSongArgs = args;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                if (MoreSongArgs.SongsClass.UserId == UserDetails.UserId && UserDetails.IsLogin)
                {
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_DeleteSong));
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_EditSong));
                }

                if (UserDetails.IsLogin)
                {
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_ReportSong));
                   //arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_ReportCopyright));
                }

                arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_Copy));

                dialogList.Title(MainContext.GetText(Resource.String.Lbl_Songs));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(MainContext.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == MainContext.GetText(Resource.String.Lbl_DeleteSong))
                {
                    OnMenuDeleteSongOnClick(MoreSongArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_EditSong))
                {
                    OnMenuEditSongOnClick(MoreSongArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_ReportSong))
                {
                    OnMenuReportSongOnClick(MoreSongArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_ReportCopyright))
                {
                    OnMenuReportCopyrightSongOnClick(MoreSongArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_Copy))
                {
                    OnMenuCopyOnClick(MoreSongArgs.SongsClass.AudioLocation);
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
                if (TypeDialog == "DeleteSong")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        MainContext.RunOnUiThread(() =>
                        {
                            try
                            {
                                if (Methods.CheckConnectivity())
                                {
                                    SoundDataObject dataSong = null;
                                    dynamic mAdapter = null;

                                    switch (NamePage)
                                    {
                                        //Delete Song from list
                                        case "FavoritesFragment":
                                            dataSong = GlobalContext?.LibraryFragment?.FavoritesFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.LibraryFragment?.FavoritesFragment?.MAdapter;
                                            break;
                                        case "LatestDownloadsFragment":
                                            dataSong = GlobalContext?.LibraryFragment?.LatestDownloadsFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.LibraryFragment?.LatestDownloadsFragment?.MAdapter;
                                            break;
                                        case "LikedFragment":
                                            dataSong = GlobalContext?.LibraryFragment?.LikedFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.LibraryFragment?.LikedFragment?.MAdapter;
                                            break;
                                        case "RecentlyPlayedFragment":
                                            dataSong = GlobalContext?.LibraryFragment?.RecentlyPlayedFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.LibraryFragment?.RecentlyPlayedFragment?.MAdapter;
                                            break;
                                        case "SharedFragment":
                                            dataSong = GlobalContext?.LibraryFragment?.SharedFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.LibraryFragment?.SharedFragment?.MAdapter;
                                            break;
                                        case "PurchasesFragment":
                                            dataSong = GlobalContext?.LibraryFragment?.PurchasesFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.LibraryFragment?.PurchasesFragment?.MAdapter;
                                            break;
                                        case "SongsByGenresFragment":
                                            dataSong = GlobalContext?.MainFragment?.SongsByGenresFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.MainFragment?.SongsByGenresFragment?.MAdapter;
                                            break;
                                        case "SongsByTypeFragment":
                                            dataSong = GlobalContext?.MainFragment?.SongsByTypeFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.MainFragment?.SongsByTypeFragment?.MAdapter;
                                            break;
                                        case "SearchSongsFragment":
                                            dataSong = GlobalContext?.BrowseFragment?.SearchFragment?.SongsTab?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.BrowseFragment?.SearchFragment?.SongsTab?.MAdapter;
                                            break;
                                    }

                                    if (mAdapter != null)
                                    {
                                        if (dataSong is SoundDataObject data)
                                        {
                                            mAdapter.SoundsList.Remove(data);

                                            int index = mAdapter.SoundsList.IndexOf(data);
                                            if (index >= 0)
                                            {
                                                mAdapter.NotifyItemRemoved(index);
                                            }
                                        }
                                    }

                                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_SongSuccessfullyDeleted), ToastLength.Short).Show();

                                    //Sent Api >>
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.DeleteTrackAsync(MoreSongArgs.SongsClass.Id.ToString()) });
                                }
                                else
                                {
                                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                                } 
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        });
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else
                {
                    if (p1 == DialogAction.Positive)
                    {
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        //DeleteSong
        private void OnMenuDeleteSongOnClick(MoreSongClickEventArgs song)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }


                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "DeleteSong";
                    MoreSongArgs = song;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                    dialog.Title(MainContext.GetText(Resource.String.Lbl_DeleteSong));
                    dialog.Content(MainContext.GetText(Resource.String.Lbl_AreYouSureDeleteSong));
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Edit Song 
        private void OnMenuEditSongOnClick(MoreSongClickEventArgs song)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }


                Intent intent = new Intent(MainContext, typeof(EditSongActivity));
                intent.PutExtra("ItemDataSong", JsonConvert.SerializeObject(song.SongsClass));
                intent.PutExtra("NamePage", NamePage);
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //ReportSong
        private void OnMenuReportSongOnClick(MoreSongClickEventArgs song)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }
 
                if (Methods.CheckConnectivity())
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_YourReportSong), ToastLength.Short).Show();
                    //Sent Api >>
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.ReportUnReportTrackAsync(song.SongsClass.Id.ToString(), true) });
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //wael
        //Report Copyright Song
        private void OnMenuReportCopyrightSongOnClick(MoreSongClickEventArgs song)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }
 
                 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Copy
        public void OnMenuCopyOnClick(string urlClipboard)
        {
            try
            {
                ClipboardManager clipboard = (ClipboardManager)MainContext.GetSystemService(Context.ClipboardService);
                ClipData clip = ClipData.NewPlainText("clipboard", urlClipboard);
                clipboard.PrimaryClip = clip;

                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_Text_copied), ToastLength.Short).Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public bool SetLike(ImageView likeButton)
        {
            try
            { 
                if (likeButton.Tag.ToString() == "Liked")
                {
                    likeButton.SetImageResource(Resource.Drawable.icon_player_heart);
                    likeButton.SetColorFilter(Color.Argb(255, 255, 255, 255));
                    likeButton.Tag = "Like";
                    return false;
                }
                else
                {
                    likeButton.SetImageResource(Resource.Drawable.icon_heart_filled_post_vector);
                    likeButton.SetColorFilter(Color.ParseColor("#f55a4e"));
                    likeButton.Tag = "Liked";
                    return true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }
        
        public bool SetDislike(ImageView likeButton)
        {
            try
            { 
                if (likeButton.Tag.ToString() == "Disliked")
                {
                    likeButton.SetImageResource(Resource.Drawable.icon_player_dislike);
                    likeButton.SetColorFilter(Color.Argb(255, 255, 255, 255));
                    likeButton.Tag = "Dislike";
                    return false;
                }
                else
                {
                    likeButton.SetImageResource(Resource.Drawable.icon_player_dislike);
                    likeButton.SetColorFilter(Color.ParseColor("#f55a4e"), PorterDuff.Mode.Multiply);
                    likeButton.Tag = "Disliked";
                    return true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }

        public bool SetFav(ImageView favButton)
        {
            try
            {
                if (favButton.Tag.ToString() == "Added")
                {
                    favButton.SetImageResource(Resource.Drawable.icon_player_star);
                    favButton.SetColorFilter(Color.Argb(255, 255, 255, 255));
                    favButton.Tag = "Add";
                    return false;
                }
                else
                {
                    favButton.SetImageResource(Resource.Drawable.icon_star_filled_post_vector);
                    favButton.SetColorFilter(Color.ParseColor("#ffa142"));
                    favButton.Tag = "Added";
                    return true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }
    }

    public class MoreSongClickEventArgs
    {
        public View View { get; set; }
        public SoundDataObject SongsClass { get; set; }
    }

    public class ShareSongClickEventArgs
    {
        public View View { get; set; }
        public SoundDataObject SongsClass { get; set; }
    }
     
    public class FavSongsClickEventArgs : EventArgs
    {
        public SoundDataObject SongsClass { get; set; }
        public ImageView FavButton { get; set; }
    }

    public class CommentSongClickEventArgs
    {
        public View View { get; set; }
        public SoundDataObject SongsClass { get; set; }
    }

    public class LikeSongsClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public SoundDataObject SongsClass { get; set; }
        public ImageView LikeButton { get; set; }
    }
}