using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Common;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Notification.Adapters
{
    public class NotificationsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<NotificationsAdapterClickEventArgs> OnItemClick;
        public event EventHandler<NotificationsAdapterClickEventArgs> OnItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<NotificationsObject.Notifiation> NotificationsList = new ObservableCollection<NotificationsObject.Notifiation>();

        public NotificationsAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Notifications_view
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_NotificationView, parent, false);
                var vh = new NotificationsAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is NotificationsAdapterViewHolder holder)
                {
                    var item = NotificationsList[position];
                    if (item != null)
                    {
                        holder.UserNameNoitfy.Text = DeepSoundTools.GetNameFinal(item.UserData);

                        GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        if (item.NType == "follow_user")
                        {
                            if (holder.IconNotify.Text != IonIconsFonts.PersonAdd)
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.PersonAdd);

                            holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_FollowUser);
                        }
                        else if (item.NType == "liked_track")
                        {
                            if (holder.IconNotify.Text != IonIconsFonts.Thumbsup)
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.Thumbsup);
                            holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_LikedTrack);
                        }
                        else if (item.NType == "liked_comment")
                        {
                            if (holder.IconNotify.Text != IonIconsFonts.Pricetag)
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.Pricetag);

                            holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_LikedComment);
                        }
                        else if (item.NType == "purchased")
                        {
                            if (holder.IconNotify.Text != IonIconsFonts.Cash)
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.Cash);

                            holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_PurchasedYourSong);   
                        }
                        else if (item.NType == "approved_artist")
                        {
                            if (holder.IconNotify.Text != IonIconsFonts.CheckmarkCircled)
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.CheckmarkCircled);

                            holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_ApprovedArtist);
                        } 
                        else if (item.NType == "decline_artist")
                        {
                            if (holder.IconNotify.Text != IonIconsFonts.SadOutline)
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.SadOutline);

                            holder.Description.Text =ActivityContext.GetText(Resource.String.Lbl_DeclineArtist);
                        }
                        else if (item.NType == "new_track")
                        {
                            if (holder.IconNotify.Text != IonIconsFonts.AndroidAdd)
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.AndroidAdd);

                            holder.Description.Text =ActivityContext.GetText(Resource.String.Lbl_UploadNewTrack);
                        }
                        else
                        {
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify, IonIconsFonts.AndroidNotifications);
                            holder.Description.Text = item.NText;
                        } 
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => NotificationsList?.Count ?? 0;

        public NotificationsObject.Notifiation GetItem(int position)
        {
            return NotificationsList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        void Click(NotificationsAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(NotificationsAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = NotificationsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (!string.IsNullOrEmpty(item.UserData?.Avatar))
                {
                    d.Add(item.UserData.Avatar);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return Glide.With(ActivityContext).Load(p0.ToString())
                .Apply(new RequestOptions().CircleCrop());
        } 
    }

    public class NotificationsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView ImageUser { get; private set; }
        public View CircleIcon { get; set; }
        public TextView IconNotify { get; private set; }
        public TextView UserNameNoitfy { get; private set; }
        public TextView Description { get; private set; }

        #endregion

        public NotificationsAdapterViewHolder(View itemView, Action<NotificationsAdapterClickEventArgs> clickListener, Action<NotificationsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                ImageUser = (ImageView)MainView.FindViewById(Resource.Id.ImageUser);
                CircleIcon = MainView.FindViewById<View>(Resource.Id.CircleIcon);
                IconNotify = (TextView)MainView.FindViewById(Resource.Id.IconNotifications);
                UserNameNoitfy = (TextView)MainView.FindViewById(Resource.Id.NotificationsName);
                Description = (TextView)MainView.FindViewById(Resource.Id.NotificationsText);

                FontUtils.SetFont(UserNameNoitfy, Fonts.SfRegular);
                FontUtils.SetFont(Description, Fonts.SfMedium);
                 
                //Create an Event
                itemView.Click += (sender, e) => clickListener(new NotificationsAdapterClickEventArgs { View = itemView, Position = AdapterPosition, Image = ImageUser });
                itemView.LongClick += (sender, e) => longClickListener(new NotificationsAdapterClickEventArgs { View = itemView, Position = AdapterPosition, Image = ImageUser });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class NotificationsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public ImageView Image { get; set; }
    }
}