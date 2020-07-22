using System;
using Android.App;
using Android.Widget;
using Com.Luseen.Autolinklibrary;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;

namespace DeepSound.Activities.Songs
{
   public class DialogInfoSong
   {
        #region Variables Basic

       private readonly Activity ActivityContext;
       private Dialog InfoSongWindow;
       private ImageView ImageSong;
       private TextView IconClose, TxtNameSong, IconGenres, TxtGenres, TxtPublisherName, TxtDate;
       private AutoLinkTextView TxtAbout, TxtTags, TxtLyrics;
       private TextView IconLike, CountLike, IconStars, CountStars, IconViews, CountViews, IconShare, CountShare, IconComment, CountComment;
       //private LinearLayout LayoutGenres, LayoutPublisher, LayoutAddedOn;
       private SoundDataObject DataObject;

       #endregion
          
        public DialogInfoSong(Activity activity)
        {
            try
            {
                ActivityContext = activity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
       public void Display(SoundDataObject dataObject)
       {
           try
           {
               DataObject = dataObject;

               InfoSongWindow = new Dialog(ActivityContext, AppSettings.SetTabDarkTheme ? Resource.Style.MyDialogThemeDark : Resource.Style.MyDialogTheme);
               InfoSongWindow.SetContentView(Resource.Layout.DialogInfoSongLayout);

               InitComponent();
               AddOrRemoveEvent(true); 
               SetDataSong();

               InfoSongWindow.Show();
           }
           catch (Exception e)
           {
               Console.WriteLine(e);
           }
        }
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                IconClose = InfoSongWindow.FindViewById<TextView>(Resource.Id.IconColse);
                ImageSong = InfoSongWindow.FindViewById<ImageView>(Resource.Id.image);

                TxtNameSong = InfoSongWindow.FindViewById<TextView>(Resource.Id.nameSong);

                //LayoutGenres = InfoSongWindow.FindViewById<LinearLayout>(Resource.Id.LayoutGenres);
                IconGenres = InfoSongWindow.FindViewById<TextView>(Resource.Id.IconGenres);
                TxtGenres = InfoSongWindow.FindViewById<TextView>(Resource.Id.GenresText);

                //LayoutPublisher = InfoSongWindow.FindViewById<LinearLayout>(Resource.Id.LayoutPublisher);
                TxtPublisherName = InfoSongWindow.FindViewById<TextView>(Resource.Id.publisherText);
               

                //LayoutAddedOn = InfoSongWindow.FindViewById<LinearLayout>(Resource.Id.LayoutAddedOn);
                TxtDate = InfoSongWindow.FindViewById<TextView>(Resource.Id.dateText);

                IconLike = InfoSongWindow.FindViewById<TextView>(Resource.Id.iconLike);
                CountLike = InfoSongWindow.FindViewById<TextView>(Resource.Id.textView_songLike);
                IconStars = InfoSongWindow.FindViewById<TextView>(Resource.Id.iconStars);
                CountStars = InfoSongWindow.FindViewById<TextView>(Resource.Id.textView_totalrate_songlist);
                IconViews = InfoSongWindow.FindViewById<TextView>(Resource.Id.iconViews);
                CountViews = InfoSongWindow.FindViewById<TextView>(Resource.Id.textView_views);
                IconShare = InfoSongWindow.FindViewById<TextView>(Resource.Id.iconShare);
                CountShare = InfoSongWindow.FindViewById<TextView>(Resource.Id.textView_share);
                IconComment = InfoSongWindow.FindViewById<TextView>(Resource.Id.iconComment);
                CountComment = InfoSongWindow.FindViewById<TextView>(Resource.Id.textView_comment);

                TxtAbout = InfoSongWindow.FindViewById<AutoLinkTextView>(Resource.Id.aboutText);
                TxtTags = InfoSongWindow.FindViewById<AutoLinkTextView>(Resource.Id.tagText);
                TxtLyrics = InfoSongWindow.FindViewById<AutoLinkTextView>(Resource.Id.lyricsText);
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconClose, FontAwesomeIcon.Times); 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconGenres, FontAwesomeIcon.LayerGroup);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconLike, IonIconsFonts.Heart);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconStars, IonIconsFonts.AndroidStar);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconViews, IonIconsFonts.Play);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconShare, IonIconsFonts.AndroidShareAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconComment, FontAwesomeIcon.CommentDots);
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
                    IconClose.Click += IconCloseOnClick;
                    TxtPublisherName.Click += TxtPublisherNameOnClick;
                }
                else
                {
                    IconClose.Click -= IconCloseOnClick;
                    TxtPublisherName.Click -= TxtPublisherNameOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

       //Open profile 
        private void TxtPublisherNameOnClick(object sender, EventArgs e)
        {
            try
            {
                InfoSongWindow.Hide();
                InfoSongWindow.Dismiss();

                if (DataObject.Publisher.Id != null)
                    HomeActivity.GetInstance()?.OpenProfile(DataObject.Publisher.Id, DataObject.Publisher);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Close
        private void IconCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                InfoSongWindow.Hide();
                InfoSongWindow.Dismiss();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        private void SetDataSong()
        {
            try
            { 
                if (DataObject != null)
                {
                    GlideImageLoader.LoadImage(ActivityContext, DataObject.Thumbnail, ImageSong, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    TxtNameSong.Text = DataObject.Title;
                    TxtGenres.Text = DataObject.CategoryName;
                    TxtDate.Text = DataObject.TimeFormatted;
                    TxtPublisherName.Text = DeepSoundTools.GetNameFinal(DataObject.Publisher);

                    CountLike.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(DataObject.CountLikes));
                    CountStars.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(DataObject.CountFavorite));
                    CountViews.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(DataObject.CountViews.Replace("K", "").Replace("M", "")));
                    CountShare.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(DataObject.CountShares));
                    CountComment.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(DataObject.CountComment));

                    TextSanitizer aboutSanitizer = new TextSanitizer(TxtAbout, ActivityContext);
                    aboutSanitizer.Load(Methods.FunString.DecodeString(DataObject.Description));

                    TextSanitizer tagsSanitizer = new TextSanitizer(TxtTags, ActivityContext);
                    tagsSanitizer.Load(ActivityContext.GetText(Resource.String.Lbl_Tags) + " : \n \n" + Methods.FunString.DecodeString(DataObject.Tags.Replace(","," #")));
                     
                    TextSanitizer lyricsSanitizer = new TextSanitizer(TxtLyrics, ActivityContext);
                    lyricsSanitizer.Load(ActivityContext.GetText(Resource.String.Lbl_Lyrics) + " : \n \n" + Methods.FunString.DecodeString(DataObject.Lyrics));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
   }
}