﻿using System;
using System.Collections.Generic;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using Exception = System.Exception;

namespace DeepSound.Activities.Search
{
    public class SearchFilterBottomDialogFragment : BottomSheetDialogFragment, MaterialDialog.ISingleButtonCallback 
    {
        #region Variables Basic

        private TextView IconBack, IconPrice, IconGenres , BtnSelectAll;
        private TextView TxtTitlePage, TxtPrice, TxtGenres;
        private RelativeLayout PriceLayout, GenresLayout;
        private Button ButtonApply; 
        //private HomeActivity GlobalContext;
        private string TypeDialog, CurrencySymbol, TotalIdGenresChecked = "", TotalIdPriceChecked = "";

        #endregion

        #region General

        //public override void OnCreate(Bundle savedInstanceState)
        //{
        //    base.OnCreate(savedInstanceState);
        //    // Create your fragment here
        //    //GlobalContext = (HomeActivity)Activity;
        //}
         
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark_Base) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Base);

                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater.Inflate(Resource.Layout.ButtomSheetSearchFilter, container, false);
                 
                //Get Value And Set Toolbar
                InitComponent(view);

                CurrencySymbol = ListUtils.SettingsSiteList?.CurrencySymbol ?? "$";

                return view;
            }
            catch (Exception e)
            {
                Console.WriteLine(e); 
                return null;
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                IconBack = view.FindViewById<TextView>(Resource.Id.IconBack); 
                IconPrice = view.FindViewById<TextView>(Resource.Id.IconPrice); 
                IconGenres = view.FindViewById<TextView>(Resource.Id.IconGenres);

                TxtTitlePage = view.FindViewById<TextView>(Resource.Id.titlepage);
                TxtPrice = view.FindViewById<TextView>(Resource.Id.PricePlace);
                TxtGenres = view.FindViewById<TextView>(Resource.Id.GenresPlace);

                PriceLayout = view.FindViewById<RelativeLayout>(Resource.Id.LayoutPrice);
                GenresLayout = view.FindViewById<RelativeLayout>(Resource.Id.LayoutGenres);
                 
                BtnSelectAll = view.FindViewById<TextView>(Resource.Id.SelectAllbutton);
                ButtonApply = view.FindViewById<Button>(Resource.Id.ApplyButton);
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, IonIconsFonts.ChevronLeft);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconPrice, IonIconsFonts.SocialUsd);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconGenres, IonIconsFonts.SocialBuffer);

                FontUtils.SetFont(TxtTitlePage, Fonts.SfMedium);
                FontUtils.SetFont(TxtGenres, Fonts.SfRegular);
                FontUtils.SetFont(TxtPrice, Fonts.SfRegular);

                TxtPrice.Text = "";
                TxtGenres.Text = "";

                IconBack.Click += IconBackOnClick;
                PriceLayout.Click += PriceLayoutOnClick;
                GenresLayout.Click += GenresLayoutOnClick;
                ButtonApply.Click += ButtonApplyOnClick;
                BtnSelectAll.Click += BtnSelectAllOnClick;

                if (!AppSettings.ShowPrice)
                {
                    PriceLayout.Visibility = ViewStates.Gone; 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        //Choose Genres
        private void GenresLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "Genres";

                var arrayIndexAdapter = new int[] { };
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                var arrayAdapter = ListUtils.GenresList.Select(item => item.CateogryName).ToList();

                dialogList.Title(Context.GetText(Resource.String.Lbl_ChooseGenres))
                    .Items(arrayAdapter)
                    .ItemsCallbackMultiChoice(arrayIndexAdapter, OnSelection)
                    .AlwaysCallMultiChoiceCallback()
                    .PositiveText(Context.GetText(Resource.String.Lbl_Close)).OnPositive(this)
                    .Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Choose Price
        private void PriceLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "Price";

                var arrayAdapter = new List<string>();
                var arrayIndexAdapter = new int[] { };
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                foreach (var item in ListUtils.PriceList)
                    if (item.Price == "0.00" || item.Price == "0")
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Free));
                    else
                        arrayAdapter.Add(CurrencySymbol + item.Price);

                dialogList.Title(Context.GetText(Resource.String.Lbl_ChoosePrice))
                    .Items(arrayAdapter)
                    .ItemsCallbackMultiChoice(arrayIndexAdapter, OnSelection)
                    .AlwaysCallMultiChoiceCallback()
                    .PositiveText(Context.GetText(Resource.String.Lbl_Close)).OnPositive(this)
                    .Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        //Save and sent data and set new search 
        private void ButtonApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                UserDetails.FilterGenres = TotalIdGenresChecked.Remove(TotalIdGenresChecked.Length - 1, 1);
                UserDetails.FilterPrice = TotalIdPriceChecked.Remove(TotalIdPriceChecked.Length - 1, 1);

                Dismiss(); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Dismiss();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Select all data 
        private void BtnSelectAllOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ListUtils.GenresList.Count > 0)
                {
                    //Get all id 
                    foreach (var item in ListUtils.GenresList)
                    {
                        TotalIdGenresChecked += item.Id + ",";
                    } 
                    UserDetails.FilterGenres = TotalIdGenresChecked.Remove(TotalIdGenresChecked.Length - 1, 1);

                    TxtGenres.Text = GetString(Resource.String.Lbl_SelectAll);
                }

                if (ListUtils.PriceList.Count > 0)
                {
                    //Get all id 
                    foreach (var item in ListUtils.PriceList)
                    {
                        TotalIdPriceChecked += item.Id + ",";
                    }
                   
                    UserDetails.FilterPrice = TotalIdPriceChecked.Remove(TotalIdPriceChecked.Length - 1, 1);

                    TxtPrice.Text = GetString(Resource.String.Lbl_SelectAll);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);  
            }
        }
       
        #endregion
         
        #region MaterialDialog

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                    switch (TypeDialog)
                    {
                        case "Genres" when TotalIdGenresChecked != "":
                            TxtGenres.Text = Context.GetText(Resource.String.Lbl_Selected);
                            break;
                        case "Price" when TotalIdPriceChecked != "":
                            TxtPrice.Text = Context.GetText(Resource.String.Lbl_Selected);
                            break;
                    }
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

        private bool OnSelection(MaterialDialog dialog, int[] which, string[] text)
        {
            try
            {
                if (TypeDialog == "Genres")
                {
                    TotalIdGenresChecked = "";
                    for (int i = 0; i < which.Length; i++)
                    {
                        TotalIdGenresChecked += ListUtils.GenresList[i].Id.ToString() + ",";
                    } 
                }
                else if (TypeDialog == "Price")
                {
                    TotalIdPriceChecked = "";
                    for (int i = 0; i < which.Length; i++)
                    {
                        TotalIdPriceChecked += ListUtils.PriceList[i].Id.ToString() + ",";
                    } 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return true;
            } 
             return true;
        }
         
        #endregion
         
    }
}