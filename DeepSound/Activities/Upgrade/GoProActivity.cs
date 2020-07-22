using System;
using System.Collections.Generic;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Payment;
using DeepSound.PaymentGoogle;
using DeepSound.SQLite;
using DeepSoundClient;
using DeepSoundClient.Requests;
using Java.Lang;
using Xamarin.PayPal.Android;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace DeepSound.Activities.Upgrade
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
    public class GoProActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private LinearLayout OptionLinerLayout, MainLayout;
        private Button UpgradeButton;
        private TextView HeadText, PriceText;
        private InitPayPalPayment InitPayPalPayment;
        private InitInAppBillingPayment BillingPayment;

        private TextDecorator TextDecorator;
        private RelativeLayout RelativeLayout;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.Go_Pro_Layout);

                InitPayPalPayment = new InitPayPalPayment(this);
                BillingPayment = new InitInAppBillingPayment(this);
                TextDecorator = new TextDecorator();

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                InitPayPalPayment?.StopPayPalService();
                BillingPayment?.DisconnectInAppBilling();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                OptionLinerLayout = FindViewById<LinearLayout>(Resource.Id.OptionLinerLayout);
                UpgradeButton = FindViewById<Button>(Resource.Id.UpgradeButton);
                HeadText = FindViewById<TextView>(Resource.Id.headText);
                PriceText = FindViewById<TextView>(Resource.Id.priceTextView);
                MainLayout = FindViewById<LinearLayout>(Resource.Id.mainLayout);
                RelativeLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);

                HeadText.Text = GetText(Resource.String.Lbl_Title_Pro1) + " " + AppSettings.ApplicationName + " " + GetText(Resource.String.Lbl_Title_Pro2);

                if (AppSettings.SetTabDarkTheme)
                {
                    MainLayout.SetBackgroundResource(Resource.Drawable.ShadowLinerLayoutDark);
                    RelativeLayout.SetBackgroundResource(Resource.Drawable.price_gopro_item_style_dark);
                }

                var list = ListUtils.SettingsSiteList;
                if (list != null)
                {
                    PriceText.Text = list.ProPrice + list.CurrencySymbol;
                }

                Typeface font = Typeface.CreateFromAsset(Application.Context.Resources.Assets, "ionicons.ttf");
                string name = "go_pro_array";
                int resourceId = Resources.GetIdentifier(name, "array", ApplicationInfo.PackageName);
                if (resourceId == 0)
                {
                    return;
                }

                string[] planArray = Resources.GetStringArray(resourceId);
                if (planArray != null)
                {
                    foreach (string options in planArray)
                    {
                        if (!string.IsNullOrEmpty(options))
                        {
                            TextView text = new TextView(this)
                            {
                                Text = options,
                                TextSize = 13
                            };
                            text.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#444444"));
                            text.Gravity = GravityFlags.CenterHorizontal;
                            text.SetTypeface(font, TypefaceStyle.Normal);
                            TextDecorator.Content = options;
                            TextDecorator.DecoratedContent = new Android.Text.SpannableString(options);
                            TextDecorator.SetTextColor(IonIconsFonts.Checkmark, "#43a735");
                            TextDecorator.SetTextColor(IonIconsFonts.Close, "#e13c4c");

                            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);//height and width are inpixel
                            layoutParams.SetMargins(0, 30, 0, 5);

                            text.LayoutParameters = layoutParams;
                            OptionLinerLayout.AddView(text);
                            TextDecorator.Build(text, TextDecorator.DecoratedContent);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_Go_Pro);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    UpgradeButton.Click += UpgradeButtonOnClick;
                }
                else
                {
                    UpgradeButton.Click -= UpgradeButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void UpgradeButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                if (AppSettings.ShowInAppBilling && Client.IsExtended)
                    arrayAdapter.Add(GetString(Resource.String.Btn_GooglePlay));

                if (AppSettings.ShowPaypal)
                    arrayAdapter.Add(GetString(Resource.String.Btn_Paypal));

                if (AppSettings.ShowBankTransfer)
                    arrayAdapter.Add(GetString(Resource.String.Lbl_BankTransfer));

                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            } 
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                BillingPayment?.Handler?.HandleActivityResult(requestCode, resultCode, data);

                if (requestCode == InitPayPalPayment?.PayPalDataRequestCode)
                {
                    switch (resultCode)
                    {
                        case Result.Ok:
                            var confirmObj = data.GetParcelableExtra(PaymentActivity.ExtraResultConfirmation);
                            PaymentConfirmation configuration = Android.Runtime.Extensions.JavaCast<PaymentConfirmation>(confirmObj);
                            if (configuration != null)
                            {
                                //string createTime = configuration.ProofOfPayment.CreateTime;
                                //string intent = configuration.ProofOfPayment.Intent;
                                //string paymentId = configuration.ProofOfPayment.PaymentId;
                                //string state = configuration.ProofOfPayment.State;
                                //string transactionId = configuration.ProofOfPayment.TransactionId;

                                if (Methods.CheckConnectivity())
                                {
                                    (int apiStatus, var respond) = await RequestsAsync.User.UpgradeMembershipAsync(UserDetails.UserId.ToString());
                                    if (apiStatus == 200)
                                    {
                                        var dataUser = ListUtils.MyUserInfoList.FirstOrDefault();
                                        if (dataUser != null)
                                        {
                                            dataUser.IsPro = 1;

                                            var sqlEntity = new SqLiteDatabase();
                                            sqlEntity.InsertOrUpdate_DataMyInfo(dataUser);
                                            sqlEntity.Dispose();

                                            if (AppSettings.ShowGoPro && dataUser.IsPro != 1) return;
                                            var mainFragmentProIcon = HomeActivity.GetInstance()?.MainFragment?.ProIcon;
                                            if (mainFragmentProIcon != null)
                                                mainFragmentProIcon.Visibility = ViewStates.Gone;
                                        }

                                        Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                        Finish();
                                    }
                                    else Methods.DisplayReportResult(this, respond);
                                }
                                else
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                }
                            }

                            break;
                        case Result.Canceled:
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long).Show();
                            break;
                    }
                }
                else if (requestCode == PaymentActivity.ResultExtrasInvalid)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Invalid), ToastLength.Long).Show();
                }
                else if (requestCode == 1001 && resultCode == Result.Ok)
                {
                    if (Methods.CheckConnectivity())
                    {
                        (int apiStatus, var respond) = await RequestsAsync.User.UpgradeMembershipAsync(UserDetails.UserId.ToString());
                        if (apiStatus == 200)
                        {
                            var dataUser = ListUtils.MyUserInfoList.FirstOrDefault();
                            if (dataUser != null)
                            {
                                dataUser.IsPro = 1;

                                var sqlEntity = new SqLiteDatabase();
                                sqlEntity.InsertOrUpdate_DataMyInfo(dataUser);
                                sqlEntity.Dispose();
                                 
                                var mainFragmentProIcon = HomeActivity.GetInstance()?.MainFragment?.ProIcon;
                                if (mainFragmentProIcon != null)
                                    mainFragmentProIcon.Visibility = ViewStates.Gone;
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                            Finish();
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    }
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion


        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == GetString(Resource.String.Btn_Paypal))
                {
                    InitPayPalPayment.BtnPaypalOnClick(ListUtils.SettingsSiteList?.ProPrice, "membership");
                }
                else if (text == GetString(Resource.String.Btn_GooglePlay))
                {
                    var price = ListUtils.SettingsSiteList?.ProPrice;

                    BillingPayment.SetConnInAppBilling();
                    BillingPayment.InitInAppBilling(price, "membership");
                }
                else if (text == GetString(Resource.String.Lbl_BankTransfer))
                {
                    OpenIntentBankTransfer();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {

                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OpenIntentBankTransfer()
        {
            try
            {
                var price = ListUtils.SettingsSiteList?.ProPrice;

                Intent intent = new Intent(this, typeof(PaymentLocalActivity));
                intent.PutExtra("Price", price);
                intent.PutExtra("payType", "membership");
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}