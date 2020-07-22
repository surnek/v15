﻿using System;
using Xamarin.Facebook;
using Exception = System.Exception;


namespace DeepSound.Helpers.SocialLogins
{
    public class FbMyProfileTracker : ProfileTracker
    {
        public event EventHandler<ProfileChangedEventArgs> MOnProfileChanged;

        protected override void OnCurrentProfileChanged(Profile oldProfile, Profile currentProfile)
        {
            try
            {
                MOnProfileChanged?.Invoke(this, new ProfileChangedEventArgs(currentProfile));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }  
        }
    }

    public class ProfileChangedEventArgs : EventArgs
    {
        public Profile MProfile;
        public ProfileChangedEventArgs(Profile profile)
        {
            try
            {
                MProfile = profile;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }           
        }
        //Extract or delete HTML tags based on their name or whether or not they contain some attributes or content with the HTML editor pro online program.
    }
}