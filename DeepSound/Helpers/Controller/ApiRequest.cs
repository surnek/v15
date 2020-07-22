using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Auth.Api;
using Android.Views;
using DeepSound.Activities.Albums;
using DeepSound.Activities.Default;
using DeepSound.Activities.SettingsUser;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.OneSignal;
using DeepSound.SQLite;
using DeepSoundClient;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Console = System.Console;
using Exception = System.Exception;

namespace DeepSound.Helpers.Controller
{
    public static class ApiRequest
    { 
        public static async Task GetSettings_Api(Activity context)
        {
            if (Methods.CheckConnectivity())
            {
                (int apiStatus, var respond) = await Current.GetOptionsAsync();
                if (apiStatus == 200)
                {
                    if (respond is OptionsObject result)
                    {
                        ListUtils.SettingsSiteList = null;
                        ListUtils.SettingsSiteList = result.DataOptions;

                        AppSettings.OneSignalAppId = result.DataOptions.AndroidMPushId;
                        OneSignalNotification.RegisterNotificationDevice();

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.InsertOrUpdateSettings(result.DataOptions);
                        dbDatabase.Dispose();
                    }
                    else Methods.DisplayReportResult(context, respond);
                }
            }
        }
         
        public static async Task<ProfileObject> GetInfoData(Activity context,string userId)
        {
            if (!UserDetails.IsLogin || userId == "0")
                return null;

            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.User.ProfileAsync(userId, "stations,albums,playlists,activities,latest_songs,store").ConfigureAwait(false);
                if (apiStatus == 200)
                {
                    if (respond is ProfileObject result)
                    {
                        if (userId == UserDetails.UserId.ToString())
                        {
                            if (result.Data != null)
                            {
                                UserDetails.Avatar = result.Data.Avatar;
                                UserDetails.Username = result.Data.Username;
                                UserDetails.IsPro = result.Data.IsPro.ToString();
                                UserDetails.Url = result.Data.Url;
                                UserDetails.FullName = result.Data.Name;

                                ListUtils.MyUserInfoList.Clear();
                                ListUtils.MyUserInfoList.Add(result.Data);

                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.InsertOrUpdate_DataMyInfo(result.Data);
                                dbDatabase.Dispose();

                                HomeActivity.GetInstance().RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        if (AppSettings.ShowGoPro && result.Data.IsPro != 1) return;
                                        var mainFragmentProIcon = HomeActivity.GetInstance()?.MainFragment?.ProIcon;
                                        if (mainFragmentProIcon != null)
                                            mainFragmentProIcon.Visibility = ViewStates.Gone;
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                });

                            }

                            return result;
                        }

                        return result;
                    }
                }
                else Methods.DisplayReportResult(context, respond);
            }

            return null;
        }
         
        public static async Task<(int?, int?)> GetCountNotifications()
        {
            var (respondCode, respondString) = await RequestsAsync.Common.GetCountNotificationsAsync(UserDetails.DeviceId).ConfigureAwait(false);
            if (respondCode.Equals(200))
            {
                if (respondString is NotificationsCountObject notifications)
                {
                    return (notifications.Count, notifications.Msgs);
                }
            }
            return (0, 0);
        }
         
        public static async Task GetGenres_Api()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    (int apiStatus, var respond) = await RequestsAsync.User.GenresAsync();
                    if (apiStatus == 200)
                    {
                        if (respond is GenresObject result)
                        {
                            ListUtils.GenresList.Clear();
                            ListUtils.GenresList = new ObservableCollection<GenresObject.DataGenres>(result.Data);
                             
                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrUpdate_Genres(ListUtils.GenresList);
                            dbDatabase.Dispose();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task GetMyPlaylist_Api()
        {
            try
            {
                if (!UserDetails.IsLogin)
                    return;

                if (Methods.CheckConnectivity())
                { 
                    (int apiStatus, var respond) = await RequestsAsync.Playlist.GetPlaylistAsync(UserDetails.UserId.ToString());
                    if (apiStatus.Equals(200))
                    {
                        if (respond is PlaylistObject result)
                        {
                            ListUtils.PlaylistList = new ObservableCollection<PlaylistDataObject>(result.Playlist);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public static async Task GetPrices_Api()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    (int apiStatus, var respond) = await RequestsAsync.Common.GetPricesAsync();
                    if (apiStatus == 200)
                    {
                        if (respond is PricesObject result)
                        {
                            ListUtils.PriceList.Clear();
                            ListUtils.PriceList = new ObservableCollection<PricesObject.DataPrice>(result.Data);
                             
                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrUpdate_Price(ListUtils.PriceList);
                            dbDatabase.Dispose();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
          
        public static async Task<string> GetTimeZoneAsync()
        {
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync("http://ip-api.com/json/");
                string json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<TimeZoneObject>(json);
                return data?.Timezone ?? "UTC";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "UTC";
            }
        }


        private static bool RunLogout;

        public static async void Delete(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    RunLogout = true;

                    await RemoveData("Delete");

                    context.RunOnUiThread(() =>
                    {
                        Methods.Path.DeleteAll_MyFolderDisk();

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();

                        Runtime.GetRuntime().RunFinalization();
                        Runtime.GetRuntime().Gc();
                        TrimCache(context);

                        dbDatabase.ClearAll();
                        dbDatabase.DropAll();

                        ListUtils.ClearAllList();

                        UserDetails.ClearAllValueUserDetails();

                        dbDatabase.CheckTablesStatus();
                        dbDatabase.Dispose();

                        SharedPref.SharedData.Edit().Clear().Commit();

                        Intent intent = new Intent(context, typeof(FirstActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        context.StartActivity(intent);
                        context.FinishAffinity();
                        context.Finish();
                    });

                    RunLogout = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public static async void Logout(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    RunLogout = true;

                    await RemoveData("Logout");

                    context.RunOnUiThread(() =>
                    {
                        Methods.Path.DeleteAll_MyFolderDisk();

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();

                        Runtime.GetRuntime().RunFinalization();
                        Runtime.GetRuntime().Gc();
                        TrimCache(context);

                        dbDatabase.ClearAll();
                        dbDatabase.DropAll();

                        ListUtils.ClearAllList();

                        UserDetails.ClearAllValueUserDetails();

                        dbDatabase.CheckTablesStatus();
                        dbDatabase.Dispose();

                        SharedPref.SharedData.Edit().Clear().Commit(); 

                        Intent intent = new Intent(context, typeof(FirstActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        context.StartActivity(intent);
                        context.FinishAffinity();
                        context.Finish();
                    });

                    RunLogout = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void TrimCache(Activity context)
        {
            try
            {
                File dir = context.CacheDir;
                if (dir != null && dir.IsDirectory)
                {
                    DeleteDir(dir);
                }

                context.DeleteDatabase("DeepSoundMusic.db");
                context.DeleteDatabase(SqLiteDatabase.PathCombine);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static bool DeleteDir(File dir)
        {
            try
            {
                if (dir == null || !dir.IsDirectory) return dir != null && dir.Delete();
                string[] children = dir.List();
                if (children.Select(child => DeleteDir(new File(dir, child))).Any(success => !success))
                {
                    return false;
                }

                // The directory is now empty so delete it
                return dir.Delete();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private static async Task RemoveData(string type)
        {
            try
            {
                if (type == "Logout")
                {
                    if (Methods.CheckConnectivity())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { RequestsAsync.Auth.LogoutAsync });
                    }
                }
                else if (type == "Delete")
                {
                    Methods.Path.DeleteAll_MyFolder();

                    if (Methods.CheckConnectivity())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.DeleteAccountAsync(UserDetails.UserId.ToString(), UserDetails.Password) });
                    }
                }

                await Task.Delay(500);

                try
                {
                    if (AppSettings.ShowGoogleLogin && LoginActivity.MGoogleApiClient != null)
                        if (Auth.GoogleSignInApi != null)
                        {
                            Auth.GoogleSignInApi.SignOut(LoginActivity.MGoogleApiClient);
                            LoginActivity.MGoogleApiClient = null;
                        }

                    if (AppSettings.ShowFacebookLogin)
                    {
                        var accessToken = AccessToken.CurrentAccessToken;
                        var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                        if (isLoggedIn && Profile.CurrentProfile != null)
                        { 
                            LoginManager.Instance.LogOut();
                        }
                    }

                    AlbumsFragment.MAdapter?.SoundsList?.Clear(); 

                    OneSignalNotification.UnRegisterNotificationDevice(); 
                    UserDetails.ClearAllValueUserDetails(); 
                   
                    if (HomeActivity.GetInstance()?.Timer != null)
                    {
                        HomeActivity.GetInstance().Timer.Stop();
                        HomeActivity.GetInstance().Timer = null;
                    }

                    Constant.Player?.Release();

                    GC.Collect();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}