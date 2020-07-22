using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.OS;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using DeepSound.Activities.Playlist;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Playlist;
using Newtonsoft.Json;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Tabbes.Adapters
{
    public class PlayListViewPagerAdapter : PagerAdapter
    {
        private readonly Activity ActivityContext;
        private readonly ObservableCollection<PlaylistDataObject> PlaylistList;
        private readonly LayoutInflater Inflater;
        private readonly RequestBuilder FullGlideRequestBuilder;

        public PlayListViewPagerAdapter(Activity context, ObservableCollection<PlaylistDataObject> playlistList)
        {
            try
            {
                ActivityContext = context;
                PlaylistList = playlistList;
                Inflater = LayoutInflater.From(context);
                var glideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
                FullGlideRequestBuilder = Glide.With(context).AsBitmap().Apply(glideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override Object InstantiateItem(ViewGroup view, int position)
        {
            try
            {
                View layout = Inflater.Inflate(Resource.Layout.PlayListCoursalLayoutView, view, false);
                var mainFeaturedImage = layout.FindViewById<ImageView>(Resource.Id.image);
                var title = layout.FindViewById<TextView>(Resource.Id.titleText);
                var seconderText = layout.FindViewById<TextView>(Resource.Id.seconderyText);
                var thirdText = layout.FindViewById<TextView>(Resource.Id.thirdText);

                if (PlaylistList[position] != null)
                {
                    var d = PlaylistList[position].Name.Replace("<br>", "");
                    title.Text = Methods.FunString.DecodeString(d);
                    seconderText.Text = PlaylistList[position].Songs + " " + ActivityContext.GetText(Resource.String.Lbl_Songs) + " "; 


                    if (PlaylistList[position].Privacy == 0)
                        thirdText.Text = ActivityContext.GetText(Resource.String.Lbl_Public);
                    else
                        thirdText.Text = ActivityContext.GetText(Resource.String.Lbl_Private);

                    var imageUrl = string.Empty;

                    if (!string.IsNullOrEmpty(PlaylistList[position].ThumbnailReady))
                    {
                        if (!PlaylistList[position].ThumbnailReady.Contains(DeepSoundClient.Client.WebsiteUrl))
                            imageUrl = DeepSoundClient.Client.WebsiteUrl + "/" + PlaylistList[position].ThumbnailReady;
                        else
                            imageUrl = PlaylistList[position].ThumbnailReady;
                    }

                    if (string.IsNullOrEmpty(imageUrl))
                        imageUrl = PlaylistList[position].Thumbnail;

                    FullGlideRequestBuilder.Load(imageUrl).Into(mainFeaturedImage);
                }

                if (!layout.HasOnClickListeners)
                {
                    layout.Click += (sender, args) =>
                    {
                        try
                        {
                            var item = PlaylistList[position];
                            if (item != null)
                            {
                                Bundle bundle = new Bundle();
                                bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                                bundle.PutString("PlaylistId", item.Id.ToString());

                                var playlistProfileFragment = new PlaylistProfileFragment
                                {
                                    Arguments = bundle
                                };

                                ((HomeActivity)ActivityContext)?.FragmentBottomNavigator.DisplayFragment(playlistProfileFragment);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    };
                }

                view.AddView(layout);

                return layout;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
         
        public override bool IsViewFromObject(View view, Object @object)
        {
            return view.Equals(@object);
        }

        public override int Count => PlaylistList?.Count ?? 0;

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            try
            {
                View view = (View)@object;
                container.RemoveView(view);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
        } 
    }
}