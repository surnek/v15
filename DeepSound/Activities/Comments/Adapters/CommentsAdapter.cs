using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Luseen.Autolinklibrary;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Comments;
using DeepSoundClient.Requests;
using Java.Util;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Comments.Adapters
{
    public class CommentsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<AvatarCommentAdapterClickEventArgs> OnAvatarClick;
        public event EventHandler<CommentAdapterClickEventArgs> OnItemClick;
        public event EventHandler<CommentAdapterClickEventArgs> OnItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<CommentsDataObject> CommentList = new ObservableCollection<CommentsDataObject>();

        public CommentsAdapter(Activity context)
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
                //Setup your layout here >> Style_PageCircle_view
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_CommentView, parent, false); 
                var vh = new CommentAdapterViewHolder(itemView, Click,LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is CommentAdapterViewHolder holder)
                {
                    var item = CommentList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                         
                        TextSanitizer changer = new TextSanitizer(holder.CommentText, ActivityContext);
                        changer.Load(Methods.FunString.DecodeString(item.Value));

                        holder.TimeTextView.Text = item.SecondsFormated;

                        holder.LikeNumber.Text = Methods.FunString.FormatPriceValue(item.CountLiked);

                        holder.LikeiconView.Tag = item.IsLikedComment != null && item.IsLikedComment.Value ? "Like" : "Liked";
                        SetLike(holder.LikeiconView);

                        holder.LikeButton.Tag = item.IsLikedComment != null && item.IsLikedComment.Value ? "1" : "0";
                         
                        if (!holder.Image.HasOnClickListeners)
                            holder.Image.Click += (sender, e) => AvatarClick(new AvatarCommentAdapterClickEventArgs { Class = item, Position = position, View = holder.MainView });

                        if (!holder.LikeButton.HasOnClickListeners)
                            holder.LikeButton.Click += (sender, e) => OnLikeButtonClick(holder, new CommentAdapterClickEventArgs { Class = item, Position = position, View = holder.MainView });
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void SetLike(TextView likeButton)
        {
            try
            {
                if (likeButton.Tag.ToString() == "Liked")
                {
                    likeButton.SetTextColor(Color.White);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, likeButton, IonIconsFonts.IosHeartOutline);
                    likeButton.Tag = "Like";
                }
                else
                {
                    likeButton.SetTextColor(Color.ParseColor("#ed4856"));
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, likeButton, IonIconsFonts.IosHeart);
                    likeButton.Tag = "Liked";
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void OnLikeButtonClick(CommentAdapterViewHolder holder, CommentAdapterClickEventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(ActivityContext, null, "Login");
                    dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Login), ActivityContext.GetText(Resource.String.Lbl_Message_Sorry_signin), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    if (e.Class != null)
                    {
                        if (holder.LikeButton.Tag.ToString() == "1")
                        {
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.LikeiconView, IonIconsFonts.IosHeartOutline);
                            holder.LikeiconView.SetTextColor(Color.White);
                            holder.LikeButton.Tag = "0";
                            e.Class.IsLikedComment = false;

                            if (!holder.LikeNumber.Text.Contains("K") && !holder.LikeNumber.Text.Contains("M"))
                            {
                                double x = Convert.ToDouble(holder.LikeNumber.Text);
                                if (x > 0)
                                    x--;
                                else
                                    x = 0;
                                holder.LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
                                e.Class.CountLiked = Convert.ToInt32(x);
                            }
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.LikeUnLikeCommentAsync(e.Class.Id.ToString(), false) });
                        }
                        else
                        {
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.LikeiconView, IonIconsFonts.IosHeart);

                            holder.LikeiconView.SetTextColor(Color.ParseColor("#ed4856"));
                            holder.LikeButton.Tag = "1";
                            e.Class.IsLikedComment =true;

                            if (!holder.LikeNumber.Text.Contains("K") && !holder.LikeNumber.Text.Contains("M"))
                            {
                                double x = Convert.ToDouble(holder.LikeNumber.Text);
                                x++;
                                holder.LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
                                e.Class.CountLiked = Convert.ToInt32(x);
                            }

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.LikeUnLikeCommentAsync(e.Class.Id.ToString(), true) });
                        } 
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public override int ItemCount => CommentList?.Count ?? 0;

        public CommentsDataObject GetItem(int position)
        {
            return CommentList[position];
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
        void AvatarClick(AvatarCommentAdapterClickEventArgs args) => OnAvatarClick?.Invoke(this, args);
        void Click(CommentAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(CommentAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = CommentList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.UserData.Avatar != "")
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

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return Glide.With(ActivityContext).Load(p0.ToString())
                .Apply(new RequestOptions().CircleCrop());
        }
    }

    public class CommentAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public ImageView Image { get; private set; }
        public AutoLinkTextView CommentText { get; private set; }

        public TextView TimeTextView { get; private set; }

        public TextView LikeiconView { get; private set; }
        public TextView LikeNumber { get; private set; }
        public LinearLayout LikeButton { get; private set; }
     
        #endregion

        public CommentAdapterViewHolder(View itemView, Action<CommentAdapterClickEventArgs> clickListener, Action<CommentAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                CommentText = MainView.FindViewById<AutoLinkTextView>(Resource.Id.active);
                TimeTextView = MainView.FindViewById<TextView>(Resource.Id.time);
                LikeiconView = MainView.FindViewById<TextView>(Resource.Id.Likeicon);
                LikeNumber = MainView.FindViewById<TextView>(Resource.Id.LikeNumber);
                LikeButton = MainView.FindViewById<LinearLayout>(Resource.Id.LikeButton);
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, LikeiconView, IonIconsFonts.IosHeartOutline);

                //Event
                itemView.Click += (sender, e) => clickListener(new CommentAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new CommentAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class CommentAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public CommentsDataObject Class { get; set; }
    }

    public class AvatarCommentAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public CommentsDataObject Class { get; set; }
    } 
}