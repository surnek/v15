//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) DeepSound 25/04/2019 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using System;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;

namespace DeepSound.Helpers.MediaPlayerController
{ 
    public class SoundDownloadAsyncController
    {
        private readonly DownloadManager DownloadManager;
        private readonly DownloadManager.Request Request;

        private readonly string FilePath = Methods.Path.FolderDcimSound;
        private readonly string Filename;
        private long DownloadId;
        private string FromActivity;
        private SoundDataObject Sound;
        private readonly HomeActivity ActivityContext;
         
        public SoundDownloadAsyncController(string url, string filename, Context contextActivity)
        {
            try
            {
                ActivityContext = (HomeActivity)contextActivity;
                
                if (!Directory.Exists(FilePath))
                    Directory.CreateDirectory(FilePath);

                if (!filename.Contains(".mp3"))
                    Filename = filename + ".mp3";
                else
                    Filename = filename;
                 
                DownloadManager = (DownloadManager)Application.Context.GetSystemService(Context.DownloadService);
                Request = new DownloadManager.Request(Android.Net.Uri.Parse(url));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void StartDownloadManager(string title, SoundDataObject sound, string fromActivity)
        {
            try
            {
                if (sound != null && !string.IsNullOrEmpty(title))
                {
                    Sound = sound;
                    FromActivity = fromActivity;
                    Console.WriteLine(FromActivity);

                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.InsertOrUpdate_LatestDownloadsSound(Sound);
                    sqlEntity.Dispose();

                    Request.SetTitle(title);
                    Request.SetAllowedNetworkTypes(DownloadNetwork.Wifi | DownloadNetwork.Mobile);
                    Request.SetDestinationInExternalPublicDir("/" + AppSettings.ApplicationName + "/Sound/", Filename);
                    Request.SetNotificationVisibility(DownloadVisibility.Visible);
                    Request.SetAllowedOverRoaming(true);
                    DownloadId = DownloadManager.Enqueue(Request);


                    OnDownloadComplete onDownloadComplete = new OnDownloadComplete
                    {
                        ActivityContext = ActivityContext, TypeActivity = fromActivity,
                        Sound = Sound
                    };

                    Application.Context.ApplicationContext.RegisterReceiver(onDownloadComplete, new IntentFilter(DownloadManager.ActionDownloadComplete));
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Download_failed), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void StopDownloadManager()
        {
            try
            {
                DownloadManager.Remove(DownloadId);
                RemoveDiskSoundFile(Filename);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public bool RemoveDiskSoundFile(string filename)
        {
            try
            {
                string path = Methods.Path.FolderDcimSound + filename;
                if (File.Exists(path))
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.Remove_LatestDownloadsSound(int.Parse(filename.Replace(".mp3", "")));
                    sqlEntity.Dispose();

                    File.Delete(path);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }

        public bool CheckDownloadLinkIfExits()
        {
            try
            {
                if (File.Exists(FilePath + Filename))
                    return true;

                return false;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }

        public static string GetDownloadedDiskSoundUri(string url)
        {
            try
            {
                string filename = url.Split('/').Last();

                var fullpaths = "file://" + Android.Net.Uri.Parse(Methods.Path.FolderDcimSound + filename + ".mp3");
                if (File.Exists(fullpaths))
                    return fullpaths;

                var fullpath2 = Methods.Path.FolderDcimSound + filename + ".mp3";
                if (File.Exists(fullpath2))
                    return fullpath2;

                
                var fullpath3 = Methods.Path.FolderDcimSound + filename + ".mp3";
                if (File.Exists(fullpath3))
                    return fullpath3;
                
                return null;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        [BroadcastReceiver()]
        [IntentFilter(new[] { DownloadManager.ActionDownloadComplete })]
        private class OnDownloadComplete : BroadcastReceiver
        {
            public Context ActivityContext;
            public string TypeActivity;
            public SoundDataObject Sound;
             
            public override void OnReceive(Context context, Intent intent)
            {
                try
                { 
                    if (intent.Action == DownloadManager.ActionDownloadComplete )
                    {
                        if (ActivityContext == null)
                            return;

                        DownloadManager downloadManagerExcuter = (DownloadManager)Application.Context.GetSystemService(Context.DownloadService);
                        long downloadId = intent.GetLongExtra(DownloadManager.ExtraDownloadId, -1);
                        DownloadManager.Query query = new DownloadManager.Query();
                        query.SetFilterById(downloadId);
                        ICursor c = downloadManagerExcuter.InvokeQuery(query);
                        var sqlEntity = new SqLiteDatabase();

                        if (c.MoveToFirst())
                        {
                            int columnIndex = c.GetColumnIndex(DownloadManager.ColumnStatus);
                            if (c.GetInt(columnIndex) == (int)DownloadStatus.Successful)
                            {
                                string downloadedPath = c.GetString(c.GetColumnIndex(DownloadManager.ColumnLocalUri));

                                ActivityManager.RunningAppProcessInfo appProcessInfo = new ActivityManager.RunningAppProcessInfo();
                                ActivityManager.GetMyMemoryState(appProcessInfo);
                                if (appProcessInfo.Importance == Importance.Foreground ||  appProcessInfo.Importance == Importance.Background)
                                {
                                    sqlEntity.InsertOrUpdate_LatestDownloadsSound(Sound.Id, downloadedPath);
                                    if (TypeActivity == "Main")
                                    {
                                        if (ActivityContext is HomeActivity tabbedMain)
                                        { 
                                            tabbedMain.SoundController.BtnIconDownload.Tag = "Downloaded";
                                            tabbedMain.SoundController.BtnIconDownload.SetImageResource(Resource.Drawable.ic_check_circle);
                                            tabbedMain.SoundController.BtnIconDownload.SetColorFilter(Color.Red);

                                            tabbedMain.SoundController.ProgressBarDownload.Visibility = ViewStates.Invisible;
                                            tabbedMain.SoundController.BtnIconDownload.Visibility = ViewStates.Visible;

                                            tabbedMain.LibrarySynchronizer.AddToLatestDownloads(Sound);
                                        }
                                    }
                                }
                                else
                                {
                                    sqlEntity.InsertOrUpdate_LatestDownloadsSound(Sound.Id, downloadedPath);
                                }
                            }
                        }

                        sqlEntity.Dispose();
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
    }
}