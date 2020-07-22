using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content;
using Android.OS;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient;
using DeepSoundClient.Classes.Chat;
using DeepSoundClient.Requests;
using Java.Lang;
using Newtonsoft.Json;
using Exception = System.Exception;

namespace DeepSound.Activities.Chat.Service
{
    [Service]
    public class ScheduledApiService : Android.App.Service
    {
        private static Handler MainHandler = new Handler();
        private ResultReceiver ResultSender;
        private ApiPostUpdaterHelper PostUpdater;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                PostUpdater = new ApiPostUpdaterHelper(new Handler(), ResultSender);

                MainHandler ??= new Handler();
                MainHandler.PostDelayed(PostUpdater, AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);
            try
            {
                var rec = intent.GetParcelableExtra("receiverTag");
                ResultSender = (ResultReceiver)rec;
                if (PostUpdater != null)
                    PostUpdater.ResultSender = ResultSender;
                else
                    MainHandler.PostDelayed(new ApiPostUpdaterHelper(new Handler(), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            //MainHandler.PostDelayed(new PostUpdaterHelper(Application.Context, new Handler(), ResultSender), AppSettings.RefreshChatActivitiesSeconds);

            return StartCommandResult.Sticky;
        }
    }

    public class ApiPostUpdaterHelper : Java.Lang.Object, IRunnable
    {
        private static Handler MainHandler;
        public ResultReceiver ResultSender;

        public ApiPostUpdaterHelper(Handler mainHandler, ResultReceiver resultSender)
        {
            MainHandler = mainHandler;
            ResultSender = resultSender;
        }

        public async void Run()
        {
            try
            {
                if (string.IsNullOrEmpty(Methods.AppLifecycleObserver.AppState))
                    Methods.AppLifecycleObserver.AppState = "Background";

                //Toast.MakeText(Application.Context, "Started", ToastLength.Short).Show(); 
                if (Methods.AppLifecycleObserver.AppState == "Background")
                {
                    try
                    {
                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        var login = dbDatabase.Get_data_Login_Credentials();
                        Console.WriteLine(login);

                        if (string.IsNullOrEmpty(Current.AccessToken))
                            return;

                        (int apiStatus, var respond) = await RequestsAsync.Chat.GetConversationListAsync("35"); 
                        if (apiStatus != 200 || !(respond is GetConversationListObject result))
                        {
                            // Methods.DisplayReportResult(Activity, respond);
                        }
                        else
                        {
                            //Toast.MakeText(Application.Context, "ResultSender 1 \n" + data, ToastLength.Short).Show();
                             
                            if (result.Data.Count > 0)
                            {
                                ListUtils.ChatList = new ObservableCollection<DataConversation>(result.Data);
                                //Insert All data users to database
                                dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
                            }
                        }
                        dbDatabase.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        // Toast.MakeText(Application.Context, "Exception  " + e, ToastLength.Short).Show();
                    }
                }
                else
                { 
                    (int apiStatus, var respond) = await RequestsAsync.Chat.GetConversationListAsync("35");
                    if (apiStatus != 200 || !(respond is GetConversationListObject result))
                    {
                       // Methods.DisplayReportResult(Activity, respond);
                    }
                    else
                    {
                        var b = new Bundle();
                        b.PutString("Json", JsonConvert.SerializeObject(result));
                        ResultSender.Send(0, b);

                        //Toast.MakeText(Application.Context, "ResultSender 2 \n" + data, ToastLength.Short).Show();

                        Console.WriteLine("Allen Post + started");
                    }
                }

                MainHandler.PostDelayed(new ApiPostUpdaterHelper(new Handler(), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                //Toast.MakeText(Application.Context, "ResultSender failed", ToastLength.Short).Show();
                MainHandler.PostDelayed(new ApiPostUpdaterHelper(new Handler(), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
                Console.WriteLine(e);
                Console.WriteLine("Allen Post + failed");
            }
        }
    }
} 