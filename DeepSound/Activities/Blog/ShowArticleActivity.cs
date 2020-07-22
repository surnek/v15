using System;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Blog;
using Java.Lang;
using Newtonsoft.Json;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace DeepSound.Activities.Blog
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class ShowArticleActivity : AppCompatActivity , MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView ImageBlog;
        private TextView TxtTitle, TxtViews;
        private WebView TxtHtml;
        private ImageButton BtnMore;
        private ArticleObject ArticleData; 
       //private CoordinatorLayout RootView;
        private TextView /*LoadMore,*/ CategoryName, ClockIcon, DateTimeTextView;
        private string ArticleId;
     
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                 
                Window.SetSoftInputMode(SoftInput.AdjustResize);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.ArticlesViewLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                GetDataArticles(); 
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
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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
                TxtTitle = FindViewById<TextView>(Resource.Id.title);
                ImageBlog = FindViewById<ImageView>(Resource.Id.imageBlog);
                ClockIcon = FindViewById<TextView>(Resource.Id.ClockIcon);
                DateTimeTextView = FindViewById<TextView>(Resource.Id.DateTime);
                CategoryName = FindViewById<TextView>(Resource.Id.CategoryName);
                TxtHtml = FindViewById<WebView>(Resource.Id.LocalWebView);
                TxtViews = FindViewById<TextView>(Resource.Id.views);
                BtnMore = FindViewById<ImageButton>(Resource.Id.more);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ClockIcon, IonIconsFonts.AndroidTime);

                CategoryName.Visibility = ViewStates.Gone; //wael get cat from api settings next version 
                BtnMore.Visibility = ViewStates.Gone; //wael get url next version 
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
                Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = "";
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
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
                   // BtnMore.Click+= BtnMoreOnClick;
                }
                else
                {
                   // BtnMore.Click -= BtnMoreOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        //#region Events

        //private void BtnMoreOnClick(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var arrayAdapter = new List<string>();
        //        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

        //        arrayAdapter.Add(GetString(Resource.String.Lbl_Copy));
        //        arrayAdapter.Add(GetString(Resource.String.Lbl_Share));

        //        dialogList.Items(arrayAdapter);
        //        dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
        //        dialogList.AlwaysCallSingleChoiceCallback();
        //        dialogList.ItemsCallback(this).Build().Show();
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.WriteLine(exception);
        //    }
        //}


        ////Event Menu >> Copy Link
        //private void CopyLinkEvent()
        //{
        //    try
        //    {
        //        //var clipboardManager = (ClipboardManager)GetSystemService(ClipboardService);

        //        //var clipData = ClipData.NewPlainText("text", ArticleData.Url);
        //        //clipboardManager.PrimaryClip = clipData;

        //        //Toast.MakeText(this, GetText(Resource.String.Lbl_Text_copied), ToastLength.Short).Show();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //}

        ////Event Menu >> Share
        //private async void ShareEvent()
        //{
        //    try
        //    {
        //        ////Share Plugin same as video
        //        //if (!CrossShare.IsSupported) return;

        //        //await CrossShare.Current.Share(new ShareMessage
        //        //{
        //        //    Title = ArticleData.Title,
        //        //    Text = " ",
        //        //    Url = ArticleData.Url
        //        //});
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //}

        //#endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                //string text = itemString.ToString();
                //if (text == GetString(Resource.String.Lbl_Copy))
                //{
                //    CopyLinkEvent();
                //}
                //else if (text == GetString(Resource.String.Lbl_Share))
                //{
                //    ShareEvent();
                //}
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
        private void GetDataArticles()
        {
            try
            {
                ArticleData = JsonConvert.DeserializeObject<ArticleObject>(Intent.GetStringExtra("itemObject"));
                if (ArticleData != null)
                {
                    ArticleId = ArticleData.Id;

                    GlideImageLoader.LoadImage(this, ArticleData.Thumbnail, ImageBlog, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    SupportActionBar.Title = Methods.FunString.DecodeString(ArticleData.Title);

                    TxtTitle.Text = Methods.FunString.DecodeString(ArticleData.Title);
                    TxtViews.Text = ArticleData.View + " " + GetText(Resource.String.Lbl_Views);

                    //SharedCount.Text = ArticleData.Shared.ToString();
                    DateTimeTextView.Text = ArticleData.CreatedAt;

                    //CategoryName.Text = GetText(Resource.String.Lbl_Category) + " : "  + CategoriesController.GetCategoryName(ArticleData.Category, "");
                      
                    string style = AppSettings.SetTabDarkTheme ? "<style type='text/css'>body{color: #fff; background-color: #444;}</style>" : "<style type='text/css'>body{color: #444; background-color: #fff;}</style>";

                    var content = Html.FromHtml(ArticleData.Content, FromHtmlOptions.ModeCompact).ToString();
                    string data = "<!DOCTYPE html>";
                    data += "<head><title></title>" + style + "</head>";
                    data += "<body>" + content + "</body>";
                    data += "</html>";

                    TxtHtml.SetWebViewClient(new WebViewClient());
                    TxtHtml.Settings.LoadsImagesAutomatically = true;
                    TxtHtml.Settings.JavaScriptEnabled = true;
                    TxtHtml.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                    //TxtHtml.Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.NarrowColumns);
                    TxtHtml.Settings.DomStorageEnabled = true;
                    TxtHtml.Settings.AllowFileAccess = true;
                    TxtHtml.Settings.DefaultTextEncodingName = "utf-8";

                    TxtHtml.LoadDataWithBaseURL(null, data, "text/html", "UTF-8", null); 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}   