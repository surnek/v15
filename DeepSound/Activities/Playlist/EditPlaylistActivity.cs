using System;
using System.Linq;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AT.Markushi.UI;
using Com.Theartofdev.Edmodo.Cropper;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Requests;
using Java.IO;
using Newtonsoft.Json;
using Console = System.Console;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace DeepSound.Activities.Playlist
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class EditPlaylistActivity : AppCompatActivity
    {
        #region Variables Basic

        private ImageView PlaylistImage;
        private CircleButton BtnClose;
        private Button BtnSelectImage, BtnSave;
        private TextView TxtSubTitle ,IconName, Iconprivacy;
        private EditText NameEditText;
        private RadioButton RbPublic, RbPrivate;
        private AdView MAdView;
        private string Status = "0", PathImage = "";
        private PlaylistDataObject PlaylistObject;

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
                SetContentView(Resource.Layout.CreatePlaylistLayout);
                 
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                
                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, null);

                SetDataPlaylist();
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
                MAdView?.Resume();
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
                MAdView?.Pause();
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

        protected override void OnDestroy()
        {
            try
            {
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
                PlaylistImage = FindViewById<ImageView>(Resource.Id.Image);
                BtnClose = FindViewById<CircleButton>(Resource.Id.ImageCircle);
                BtnSelectImage = FindViewById<Button>(Resource.Id.btn_AddPhoto);
                TxtSubTitle = FindViewById<TextView>(Resource.Id.subTitle);
                IconName = FindViewById<TextView>(Resource.Id.IconName);
                NameEditText = FindViewById<EditText>(Resource.Id.NameEditText);
                Iconprivacy = FindViewById<TextView>(Resource.Id.Iconprivacy);
                RbPublic = FindViewById<RadioButton>(Resource.Id.radioPublic);
                RbPrivate = FindViewById<RadioButton>(Resource.Id.radioPrivate);
                BtnSave = FindViewById<Button>(Resource.Id.ApplyButton);
                BtnSave.Text = GetText(Resource.String.Lbl_Submit);

                TxtSubTitle.Visibility = ViewStates.Gone;

                RbPublic.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                RbPrivate.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetColorEditText(NameEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconName, FontAwesomeIcon.TextWidth);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, Iconprivacy, FontAwesomeIcon.ShieldAlt); 
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title =GetText(Resource.String.Lbl_EditPlaylist);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ?  Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
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
                    BtnClose.Click += BtnCloseOnClick;
                    BtnSelectImage.Click += BtnSelectImageOnClick;
                    RbPublic.CheckedChange += RadioPublicOnCheckedChange;
                    RbPrivate.CheckedChange += RadioPrivateOnCheckedChange;
                    BtnSave.Click += BtnSaveOnClick;
                }
                else
                {
                    BtnClose.Click -= BtnCloseOnClick;
                    BtnSelectImage.Click -= BtnSelectImageOnClick;
                    RbPublic.CheckedChange -= RadioPublicOnCheckedChange;
                    RbPrivate.CheckedChange -= RadioPrivateOnCheckedChange;
                    BtnSave.Click -= BtnSaveOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        //Click Save data Playlist
        private async void BtnSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
                else
                {
                    if (string.IsNullOrEmpty(NameEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterName), ToastLength.Short).Show();
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                    (int apiStatus, var respond) = await RequestsAsync.Playlist.UpdatePlaylistAsync(PlaylistObject.Id.ToString(),NameEditText.Text, Status, PathImage); //Sent api 
                    if (apiStatus.Equals(200))
                    {
                        if (respond is MessageObject result)
                        {
                            Console.WriteLine(result.Message);
                            Toast.MakeText(this, GetText(Resource.String.Lbl_PlaylistSuccessfullyUpdated), ToastLength.Short).Show();
                            AndHUD.Shared.Dismiss(this);

                            RunOnUiThread(() =>
                            {
                                try
                                {
                                    var playlistData = ListUtils.PlaylistList.FirstOrDefault(a => a.Id == PlaylistObject?.Id);
                                    if (playlistData != null)
                                    {
                                        playlistData.Name = NameEditText.Text;
                                        playlistData.Privacy = Convert.ToInt32(Status);

                                        if (!string.IsNullOrEmpty(PathImage))
                                            playlistData.ThumbnailReady = PathImage; 
                                    }

                                    var dataPlaylistFragment = HomeActivity.GetInstance()?.PlaylistFragment;
                                    dataPlaylistFragment?.UpdateMyPlaylist(); 

                                    var dataMyPlaylistFragment = HomeActivity.GetInstance()?.PlaylistFragment?.MyPlaylistFragment?.PlaylistAdapter;
                                    var list2 = dataMyPlaylistFragment?.PlaylistList;
                                    var dataMyPlaylist = list2?.FirstOrDefault(a => a.Id == PlaylistObject?.Id);
                                    if (dataMyPlaylist != null)
                                    {
                                        dataMyPlaylist.Name = NameEditText.Text;
                                        dataMyPlaylist.Privacy = Convert.ToInt32(Status);

                                        if (!string.IsNullOrEmpty(PathImage))
                                            dataMyPlaylist.ThumbnailReady = PathImage;
                                         
                                        int index = list2.IndexOf(dataMyPlaylist);
                                        if (index >= 0)
                                            dataMyPlaylistFragment.NotifyItemChanged(index);
                                    } 

                                    Finish();
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine(exception);
                                }
                            });
                        }
                    }
                    else  
                    {
                        if (respond is ErrorObject error)
                        {
                            var errorText = error.Error.Replace("&#039;", "'");
                            AndHUD.Shared.ShowError(this, errorText, MaskType.Clear, TimeSpan.FromSeconds(2));
                        }
                        Methods.DisplayReportResult(this, respond);
                    }

                    AndHUD.Shared.Dismiss(this);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        //Private
        private void RadioPrivateOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                bool isChecked = RbPrivate.Checked;
                if (isChecked)
                {
                    RbPublic.Checked = false;
                    Status = "0";
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Public
        private void RadioPublicOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                bool isChecked = RbPublic.Checked;
                if (isChecked)
                {
                    RbPrivate.Checked = false;
                    Status = "1";
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open Gallery 
        private void BtnSelectImageOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Remove image 
        private void BtnCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                PathImage = "";
                GlideImageLoader.LoadImage(this, "Grey_Offline", PlaylistImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                var result = CropImage.GetActivityResult(data);
                //If its from Camera or Gallery
                if (requestCode == CropImage.CropImageActivityRequestCode)
                {
                    if (resultCode == Result.Ok)
                    {
                        if (result.IsSuccessful)
                        {
                            var resultUri = result.Uri;

                            if (!string.IsNullOrEmpty(resultUri.Path))
                            {
                                PathImage = resultUri.Path;
                                var file = Uri.FromFile(new File(resultUri.Path));
                                GlideImageLoader.LoadImage(this, file.Path, PlaylistImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                            }
                        } 
                    } 
                }
                else if (requestCode == CropImage.CropImageActivityResultErrorCode)
                {
                    Exception error = result.Error;
                    Console.WriteLine(error);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        OpenDialogGallery();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long).Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        private void SetDataPlaylist()
        {
            try
            {
                PlaylistObject = JsonConvert.DeserializeObject<PlaylistDataObject>(Intent.GetStringExtra("ItemData") ?? "");
                if (PlaylistObject != null)
                {
                    PathImage = "";
                    GlideImageLoader.LoadImage(this, PlaylistObject.ThumbnailReady, PlaylistImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                     
                    NameEditText.Text = PlaylistObject.Name;

                    if (PlaylistObject.Privacy == 0)
                    {
                        RbPublic.Checked = true;
                        RbPrivate.Checked = false;
                        Status = "0";
                    }
                    else
                    {
                        RbPublic.Checked = false;
                        RbPrivate.Checked = true;
                        Status = "1";
                    } 
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OpenDialogGallery()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDcimImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Builder()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Done))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDcimImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Builder()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Done))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}