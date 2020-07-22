using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Fonts;
using DeepSoundClient.Classes.User;
using Java.Util;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Genres.Adapters
{
    public class GenresCheckerAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<GenresCheckerAdapterClickEventArgs> OnItemClick;
        public event EventHandler<GenresCheckerAdapterClickEventArgs> OnItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<GenresObject.DataGenres> GenresList = new ObservableCollection<GenresObject.DataGenres>();

        public GenresCheckerAdapter(Activity context)
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
                //Setup your layout here >> Style_GenresSoundView
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_GenresView, parent, false);
                var vh = new GenresCheckerAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is GenresCheckerAdapterViewHolder holder)
                {
                    var item = GenresList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.BackgroundThumb, holder.GenresImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        holder.GenresImage.ClearColorFilter();
                        holder.GenresImage.SetColorFilter(Color.ParseColor(item.Color), PorterDuff.Mode.Lighten);

                        holder.TxtName.Text = item.CateogryName;

                        if (item.Checked)
                        { 
                            holder.TxtCheck.Visibility = ViewStates.Visible;
                            holder.TxtName.Visibility = ViewStates.Gone; 
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => GenresList?.Count ?? 0;

        public GenresObject.DataGenres GetItem(int position)
        {
            return GenresList[position];
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

        void Click(GenresCheckerAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(GenresCheckerAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = GenresList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.BackgroundThumb != "")
                {
                    d.Add(item.BackgroundThumb);
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

    public class GenresCheckerAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public ImageView GenresImage { get; private set; }
        public TextView TxtName { get; private set; }
        public TextView TxtCheck { get; private set; }

        #endregion

        public GenresCheckerAdapterViewHolder(View itemView, Action<GenresCheckerAdapterClickEventArgs> clickListener, Action<GenresCheckerAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                GenresImage = (ImageView)MainView.FindViewById(Resource.Id.image);
                TxtName = MainView.FindViewById<TextView>(Resource.Id.titleText);
                TxtCheck = MainView.FindViewById<TextView>(Resource.Id.CheckText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtCheck, IonIconsFonts.Checkmark);

                //Event
                itemView.Click += (sender, e) => clickListener(new GenresCheckerAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new GenresCheckerAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class GenresCheckerAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; } 
    }
}