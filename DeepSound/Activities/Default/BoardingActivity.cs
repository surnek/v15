using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Com.CodeMyBrainsOut.Onboarder;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;

namespace DeepSound.Activities.Default
{ 
    [Activity(Icon = "@mipmap/icon", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.Orientation)]
    public class BoardingActivity : AhoyOnboarderActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                var onBoarderCard1 = new AhoyOnboarderCard(GetString(Resource.String.Lbl_Title_page1), GetString(Resource.String.Lbl_Description_page1), Resource.Drawable.Onboarding_icon1)
                {
                    BackgroundColor = Resource.Color.accent_transparent
                };

                var onBoarderCard2 = new AhoyOnboarderCard(GetString(Resource.String.Lbl_Title_page2), GetString(Resource.String.Lbl_Description_page2), Resource.Drawable.Onboarding_icon2)
                {
                    BackgroundColor = Resource.Color.accent_transparent
                };

                var onBoarderCard3 = new AhoyOnboarderCard(GetString(Resource.String.Lbl_Title_page3), GetString(Resource.String.Lbl_Description_page3), Resource.Drawable.Onboarding_icon3)
                {
                    BackgroundColor = Resource.Color.accent_transparent
                };

                onBoarderCard1.TitleColor = Resource.Color.white;
                onBoarderCard2.TitleColor = Resource.Color.white;
                onBoarderCard3.TitleColor = Resource.Color.white;

                onBoarderCard1.DescriptionColor = Resource.Color.white;
                onBoarderCard2.DescriptionColor = Resource.Color.white;
                onBoarderCard3.DescriptionColor = Resource.Color.white;

                SetGradientBackground();

                SetFinishButtonTitle(GetString(Resource.String.Lbl_FinishButton_WalkTroutPage));

                var pages = new List<AhoyOnboarderCard>
                {
                    onBoarderCard1, onBoarderCard2, onBoarderCard3
                };

                SetOnboardPages(pages);

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetGenres_Api });

                if (AppSettings.ShowPrice)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetPrices_Api });   
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnFinishButtonPressed()
        {
            try
            {
                var data = ListUtils.DataUserLoginList.FirstOrDefault();
                if (data != null)
                {
                    //data.Status = "Active";
                    //UserDetails.Status = "Active";

                    //SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    //dbDatabase.InsertOrUpdateLogin_Credentials(data);
                    //dbDatabase.Dispose();

                    StartActivity(new Intent(this, typeof(HomeActivity)));
                    Finish();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}