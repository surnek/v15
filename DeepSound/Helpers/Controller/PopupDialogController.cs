using System;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Text;
using Android.Views;
using DeepSound.Activities.Default;
using DeepSoundClient.Classes.Global;
using Java.Lang;
using Exception = System.Exception;

namespace DeepSound.Helpers.Controller
{
    public class PopupDialogController : Java.Lang.Object, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, MaterialDialog.IInputCallback
    {
        private readonly Activity ActivityContext;
        private SoundDataObject SoundData;
        private readonly string TypeDialog;

        public PopupDialogController(Activity activity, SoundDataObject soundData, string typeDialog)
        {
            ActivityContext = activity;
            SoundData = soundData;
            TypeDialog = typeDialog;
        }

       
        public void ShowNormalDialog(string title, string content = null, string positiveText = null, string negativeText = null)
        {
            try
            {
                MaterialDialog.Builder dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                if (!string.IsNullOrEmpty(title))
                    dialogList.Title(title);

                if (!string.IsNullOrEmpty(content))
                    dialogList.Content(content);

                if (!string.IsNullOrEmpty(negativeText))
                {
                    dialogList.NegativeText(negativeText);
                    dialogList.OnNegative(this);
                }

                if (!string.IsNullOrEmpty(positiveText))
                {
                    dialogList.PositiveText(positiveText);
                    dialogList.OnPositive(this);
                }

                dialogList.Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void ShowEditTextDialog(string title, string content = null, string positiveText = null, string negativeText = null)
        {
            try
            {
                MaterialDialog.Builder dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                if (!string.IsNullOrEmpty(title))
                    dialogList.Title(title);

                if (!string.IsNullOrEmpty(content))
                    dialogList.Content(content);

                if (!string.IsNullOrEmpty(negativeText))
                {
                    dialogList.NegativeText(negativeText);
                    dialogList.OnNegative(this);
                }

                if (!string.IsNullOrEmpty(positiveText))
                {
                    dialogList.PositiveText(positiveText);
                    dialogList.OnPositive(this);
                }

                dialogList.InputType(InputTypes.ClassText | InputTypes.TextFlagMultiLine);
                dialogList.Input("", "", this);
                dialogList.Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void OnSelection(MaterialDialog p0, View p1, int p2, ICharSequence selectedPlayListName)
        {
            try
            {
                
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
                if (TypeDialog == "Login")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        ActivityContext.StartActivity(new Intent(ActivityContext, typeof(LoginActivity)));
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnInput(MaterialDialog p0, ICharSequence p1)
        {
            try
            {
                if (TypeDialog == "Report")
                {
                    if (p1.Length() > 0)
                    {
                         
                    }
                    else
                    {
                        //Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_The_name_can_not_be_blank), ToastLength.Short).Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}