using System;
using Android.App;
using DeepSoundClient.Classes.Global;

namespace DeepSound.Helpers.Utils
{
    public static class DeepSoundTools
    {
        private static readonly string[] CountriesArray = Application.Context.Resources.GetStringArray(Resource.Array.countriesArray);

        public static string GetNameFinal(UserDataObject dataUser)
        {
            try
            {
                if (!string.IsNullOrEmpty(dataUser.Name) && !string.IsNullOrWhiteSpace(dataUser.Name))
                    return Methods.FunString.DecodeString(dataUser.Name);

                if (!string.IsNullOrEmpty(dataUser.Username) && !string.IsNullOrWhiteSpace(dataUser.Username))
                    return Methods.FunString.DecodeString(dataUser.Username);

                return Methods.FunString.DecodeString(dataUser.Username);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "";
            }
        }
         
        public static string GetAboutFinal(UserDataObject dataUser)
        {
            try
            {
                if (!string.IsNullOrEmpty(dataUser.AboutDecoded) && !string.IsNullOrWhiteSpace(dataUser.AboutDecoded))
                    return Methods.FunString.DecodeString(dataUser.AboutDecoded);

                return Application.Context.Resources.GetString(Resource.String.Lbl_HasNotAnyInfo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Application.Context.Resources.GetString(Resource.String.Lbl_HasNotAnyInfo);
            }
        }
         
        public static string GetCountry(long codeCountry)
        {
            try
            { 
                if (codeCountry > -1)
                {
                    string name = CountriesArray[codeCountry];
                    return name;
                }
                return "";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "";
            }
        }

        public static string GetGender(string type)
        {
            try
            {
                string text;
                switch (type)
                {
                    case "Male":
                    case "male":
                        text = Application.Context.GetText(Resource.String.Lbl_Male);
                        break;
                    case "Female":
                    case "female":
                        text = Application.Context.GetText(Resource.String.Lbl_Female);
                        break;
                    default:
                        text = "";
                        break;
                }
                return text;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "";
            }
        }

    }
}