using System;
using Android.App;
using Android.Content;
using Java.Math;
using DeepSound.Helpers.Utils;
using DeepSoundClient;
using Xamarin.PayPal.Android;

namespace DeepSound.Payment
{
    public class InitPayPalPayment
    {
        private readonly Activity ActivityContext;
        private static PayPalConfiguration PayPalConfig;
        private PayPalPayment PayPalPayment;
        private Intent IntentService;
        public readonly int PayPalDataRequestCode = 7171;

        public InitPayPalPayment(Activity activity)
        {
            ActivityContext = activity;
        }

        //Paypal
        public void BtnPaypalOnClick(string price, string payType)
        {
            try
            {
                InitPayPal(price, payType);

                Intent intent = new Intent(ActivityContext, typeof(PaymentActivity));
                intent.PutExtra(PayPalService.ExtraPaypalConfiguration, PayPalConfig);
                intent.PutExtra(PaymentActivity.ExtraPayment, PayPalPayment);
                ActivityContext.StartActivityForResult(intent, PayPalDataRequestCode);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void InitPayPal(string price, string payType)
        {
            try
            {
                //PayerID
                string currency = "USD";
                string paypalClintId = "";
                var option = ListUtils.SettingsSiteList;
                if (option != null)
                {
                    currency = option.PaypalCurrency ?? "USD";
                    paypalClintId = option.PaypalId;
                }

                PayPalConfig = new PayPalConfiguration()
                    .ClientId(paypalClintId)
                    .LanguageOrLocale(AppSettings.Lang)
                    .MerchantName(AppSettings.ApplicationName)
                    .MerchantPrivacyPolicyUri(Android.Net.Uri.Parse(Client.WebsiteUrl + "/terms/privacy"));

                switch (option?.PaypalMode)
                {
                    case "sandbox":
                        PayPalConfig.Environment(PayPalConfiguration.EnvironmentSandbox);
                        break;
                    case "live":
                        PayPalConfig.Environment(PayPalConfiguration.EnvironmentProduction);
                        break;
                    default:
                        PayPalConfig.Environment(PayPalConfiguration.EnvironmentProduction);
                        break;
                }

                string text;

                switch (payType)
                {
                    case "PurchaseSong":
                        text = "Purchase the song";
                        break;
                    case "PurchaseAlbum":
                        text = "Purchase the album";
                        break;
                    case "membership":
                        text = "Upgrade";
                        break;
                    default:
                        text = "Purchase the song";
                        break;
                }
                 
                PayPalPayment = new PayPalPayment(new BigDecimal(price), currency, text, PayPalPayment.PaymentIntentSale);

                IntentService = new Intent(ActivityContext, typeof(PayPalService)); 
                IntentService.PutExtra(PayPalService.ExtraPaypalConfiguration, PayPalConfig);
                ActivityContext.StartService(IntentService);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void StopPayPalService()
        {
            try
            {
                ActivityContext.StopService(new Intent(ActivityContext, typeof(PayPalService)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 
    }
}