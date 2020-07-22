using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Com.Luseen.Autolinklibrary;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;
using DeepSoundClient;
using DeepSoundClient.Classes.Chat;
using Path = System.IO.Path;
using Uri = Android.Net.Uri;

namespace DeepSound.Activities.Chat.Adapters
{
    public class UserMessagesAdapter : RecyclerView.Adapter
    {
        #region Variables Basic

        private readonly MessagesBoxActivity ActivityContext;
        public ObservableCollection<ChatMessagesDataObject> MessageList = new ObservableCollection<ChatMessagesDataObject>();
        private readonly RequestOptions Options;

        #endregion

        public UserMessagesAdapter(MessagesBoxActivity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
                Options = new RequestOptions().Apply(RequestOptions.CircleCropTransform()
                    .CenterCrop()
                    .SetPriority(Priority.High).Override(200)
                    .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                    .Error(Resource.Drawable.ImagePlacholder)
                    .Placeholder(Resource.Drawable.ImagePlacholder));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> 
                var itemView = MessageList[viewType];
                if (itemView != null)
                {
                    if (itemView.ApiPosition == ApiPosition.Right && itemView.ApiType == ApiType.Text)
                    {
                        View row = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Right_MS_view, parent, false);
                        TextViewHolder textViewHolder = new TextViewHolder(row, ActivityContext);
                        return textViewHolder;
                    }

                    if (itemView.ApiPosition == ApiPosition.Left && itemView.ApiType == ApiType.Text)
                    {
                        View row = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Left_MS_view, parent, false);
                        TextViewHolder textViewHolder = new TextViewHolder(row, ActivityContext);
                        return textViewHolder;
                    }
                    if (itemView.ApiPosition == ApiPosition.Right && itemView.ApiType == ApiType.Image)
                    {
                        View row = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Right_MS_image, parent, false);
                        ImageViewHolder imageViewHolder = new ImageViewHolder(row);
                        return imageViewHolder;
                    }
                    if (itemView.ApiPosition == ApiPosition.Left && itemView.ApiType == ApiType.Image)
                    {
                        View row = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Left_MS_image, parent, false);
                        ImageViewHolder imageViewHolder = new ImageViewHolder(row);
                        return imageViewHolder;
                    }

                    return null;
                }

                return null;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            try
            {
                int type = GetItemViewType(position);
                var item = MessageList[type];
                if (item == null) return;
                switch (item.ApiType)
                {
                    case ApiType.Text :
                        {
                            TextViewHolder holder = vh as TextViewHolder;
                            LoadTextOfChatItem(holder, position, item);
                            break;
                        }
                    case ApiType.Image:
                        {
                            ImageViewHolder holder = vh as ImageViewHolder;
                            LoadImageOfChatItem(holder, position, item);
                            break;
                        } 
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Load Holder

        private void LoadTextOfChatItem(TextViewHolder holder, int position, ChatMessagesDataObject item)
        {
            try
            {
                Console.WriteLine(position);
                holder.Time.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.Time));


                holder.TextSanitizerAutoLink.Load(Methods.FunString.DecodeString(item.Text), item.ApiPosition);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void LoadImageOfChatItem(ImageViewHolder holder, int position, ChatMessagesDataObject message)
        {
            try
            {
                Console.WriteLine(position);
                string imageUrl = message.Image.Contains("upload/chat/") && !message.Image.Contains(Client.WebsiteUrl) ? Client.WebsiteUrl + "/" + message.Image : message.Image;

                string fileSavedPath;

                holder.Time.Text = Methods.Time.TimeAgo(Convert.ToInt32(message.Time));

                if (imageUrl.Contains("http://") || imageUrl.Contains("https://"))
                {
                    var fileName = imageUrl.Split('/').Last();
                    string imageFile = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileName);
                    if (imageFile == "File Dont Exists")
                    {
                        holder.LoadingProgressView.Indeterminate = false;
                        holder.LoadingProgressView.Visibility = ViewStates.Visible;

                        string filePath = Path.Combine(Methods.Path.FolderDcimImage);
                        string mediaFile = filePath + "/" + fileName;
                        fileSavedPath = mediaFile;

                        WebClient webClient = new WebClient();

                        webClient.DownloadDataAsync(new System.Uri(imageUrl));
                        //webClient.DownloadProgressChanged += (sender, args) =>
                        //{
                        //    var progress = args.ProgressPercentage;
                        //};

                        webClient.DownloadDataCompleted += (s, e) =>
                        {
                            try
                            {
                                if (!Directory.Exists(filePath))
                                    Directory.CreateDirectory(filePath);

                                File.WriteAllBytes(mediaFile, e.Result);

                                //var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                                //mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(mediaFile)));
                                //ActivityContext.SendBroadcast(mediaScanIntent);

                                // Tell the media scanner about the new file so that it is
                                // immediately available to the user.
                                MediaScannerConnection.ScanFile(Application.Context, new[] { mediaFile }, null, null);

                                var file = Uri.FromFile(new Java.IO.File(mediaFile));
                                Glide.With(ActivityContext).Load(file.Path).Apply(Options).Into(holder.ImageView);

                                holder.LoadingProgressView.Indeterminate = false;
                                holder.LoadingProgressView.Visibility = ViewStates.Gone; 
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            } 
                        };
                    }
                    else
                    {
                        fileSavedPath = imageFile;

                        var file = Uri.FromFile(new Java.IO.File(imageFile));
                        Glide.With(ActivityContext).Load(file.Path).Apply(Options).Into(holder.ImageView);

                        holder.LoadingProgressView.Indeterminate = false;
                        holder.LoadingProgressView.Visibility = ViewStates.Gone;
                    }
                }
                else
                {
                    fileSavedPath = imageUrl;

                    var file = Uri.FromFile(new Java.IO.File(imageUrl));
                    Glide.With(ActivityContext).Load(file.Path).Apply(Options).Into(holder.ImageView);

                    //GlideImageLoader.LoadImage(ActivityContext, imageUrl, holder.ImageView, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    holder.LoadingProgressView.Indeterminate = false;
                    holder.LoadingProgressView.Visibility = ViewStates.Gone;
                }

                if (!holder.ImageView.HasOnClickListeners)
                {
                    holder.ImageView.Click += (sender, args) =>
                    {
                        try
                        {
                            string imageFile = Methods.MultiMedia.CheckFileIfExits(fileSavedPath);
                            if (imageFile != "File Dont Exists")
                            {
                                Java.IO.File file2 = new Java.IO.File(fileSavedPath);
                                var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                                Intent intent = new Intent();
                                intent.SetAction(Intent.ActionView);
                                intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                                intent.SetDataAndType(photoUri, "image/*");
                                ActivityContext.StartActivity(intent);
                            }
                            else
                            {
                                Intent intent = new Intent(Intent.ActionView, Uri.Parse(imageUrl));
                                ActivityContext.StartActivity(intent);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    };
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion

        public override int ItemCount => MessageList?.Count ?? 0;
 
        public ChatMessagesDataObject GetItem(int position)
        {
            return MessageList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        } 
    }

    public class UserMessagesAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        #endregion

        public UserMessagesAdapterViewHolder(View itemView, Action<UserMessagesAdapterClickEventArgs> clickListener,
            Action<UserMessagesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new UserMessagesAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new UserMessagesAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }

    public class TextViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public LinearLayout LytParent { get; set; }
        public TextView Time { get; private set; }
        public View MainView { get; private set; }
        public AutoLinkTextView AutoLinkTextView { get; private set; }
        public TextSanitizer TextSanitizerAutoLink { get; private set; }

        #endregion

        public TextViewHolder(View itemView, Activity activity) : base(itemView)
        {
            try
            {
                MainView = itemView;

                LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                AutoLinkTextView = itemView.FindViewById<AutoLinkTextView>(Resource.Id.active);
                Time = itemView.FindViewById<TextView>(Resource.Id.time);

                AutoLinkTextView.SetTextIsSelectable(true);

                if (TextSanitizerAutoLink == null)
                {
                    TextSanitizerAutoLink = new TextSanitizer(AutoLinkTextView, activity);
                }

                //itemView.Click += (sender, e) => clickListener(new UserMessagesAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                //itemView.LongClick += (sender, e) => longClickListener(new UserMessagesAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "Error");
            }
        }
    }

    public class ImageViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public LinearLayout LytParent { get; private set; }
        public ImageView ImageView { get; private set; }
        public ProgressBar LoadingProgressView { get; private set; }
        public TextView Time { get; private set; }

        #endregion

        public ImageViewHolder(View itemView) : base(itemView)
        {
            try
            {
                MainView = itemView;
                LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                ImageView = itemView.FindViewById<ImageView>(Resource.Id.imgDisplay);
                LoadingProgressView = itemView.FindViewById<ProgressBar>(Resource.Id.loadingProgressview);
                Time = itemView.FindViewById<TextView>(Resource.Id.time);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
     
    public class UserMessagesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}