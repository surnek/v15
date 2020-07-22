using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Activities.Library.Listeners;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Albums;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Albums.Adapters
{
    public class RowAlbumsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        private readonly Activity ActivityContext;
        public event EventHandler<AlbumsAdapterClickEventArgs> OnItemClick;
        public event EventHandler<AlbumsAdapterClickEventArgs> OnItemLongClick;

        public ObservableCollection<DataAlbumsObject> AlbumsList = new ObservableCollection<DataAlbumsObject>();
        private readonly LibrarySynchronizer LibrarySynchronizer;
        public RowAlbumsAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
                LibrarySynchronizer = new LibrarySynchronizer(context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => AlbumsList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_AlbumsView
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_AlbumsView, parent, false);
                var vh = new AlbumsAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (!(viewHolder is AlbumsAdapterViewHolder holder)) return;

                var item = AlbumsList[position];

                if (item == null) return;

                GlideImageLoader.LoadImage(ActivityContext, item.Thumbnail, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                holder.TxtTitle.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.Title), 25);

                var count = !string.IsNullOrEmpty(item.CountSongs) ? item.CountSongs : item.SongsCount ?? "0";
                if (Math.Abs(item.Price) > 0)
                {
                    var currencySymbol = ListUtils.SettingsSiteList?.CurrencySymbol ?? "$"; 
                    holder.TxtSeconderyText.Text = DeepSoundTools.GetNameFinal(item.Publisher ?? item.UserData) + " - " + count + " " + ActivityContext.GetText(Resource.String.Lbl_Songs) + " - " + currencySymbol + item.Price;
                }
                else
                {
                    holder.TxtSeconderyText.Text = DeepSoundTools.GetNameFinal(item.Publisher ?? item.UserData) + " - " + count + " " + ActivityContext.GetText(Resource.String.Lbl_Songs);
                }

                if (!holder.MoreButton.HasOnClickListeners)
                    holder.MoreButton.Click += (sender, e) => LibrarySynchronizer.AlbumsOnMoreClick(new MoreAlbumsClickEventArgs { View = holder.MainView, AlbumsClass = item });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public DataAlbumsObject GetItem(int position)
        {
            return AlbumsList[position];
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
         
        private void OnClick(AlbumsAdapterClickEventArgs args)
        {
            OnItemClick?.Invoke(this, args);
        }

        private void OnLongClick(AlbumsAdapterClickEventArgs args)
        {
            OnItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = AlbumsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Thumbnail != "")
                {
                    d.Add(item.Thumbnail);
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

    public class AlbumsAdapterViewHolder : RecyclerView.ViewHolder
    {
        public View MainView { get; private set; }
        public ImageView Image { get; private set; }
        public TextView TxtTitle { get; private set; }
        public TextView TxtSeconderyText { get; private set; }
        public ImageButton MoreButton { get; private set; }

        public AlbumsAdapterViewHolder(View itemView, Action<AlbumsAdapterClickEventArgs> clickListener, Action<AlbumsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = MainView.FindViewById<ImageView>(Resource.Id.Image);
                TxtTitle = MainView.FindViewById<TextView>(Resource.Id.title);
                TxtSeconderyText = MainView.FindViewById<TextView>(Resource.Id.brief);
                MoreButton = MainView.FindViewById<ImageButton>(Resource.Id.more);

                //Event
                itemView.Click += (sender, e) => clickListener(new AlbumsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new AlbumsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


    }

    public class AlbumsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}