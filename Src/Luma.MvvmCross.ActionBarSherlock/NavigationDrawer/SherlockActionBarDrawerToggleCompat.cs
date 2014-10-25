/*
 * Copyright (C) 2013 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Android.App;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Lang.Reflect;
using Xamarin.ActionBarSherlock.App;
using ActionBar = Xamarin.ActionBarSherlock.App.ActionBar;

namespace Luma.MvvmCross.ActionBarSherlock.NavigationDrawer
{
    /// <summary>
    /// This class encapsulates some awful hacks.
    /// 
    /// Before JB-MR2 (API 18) it was not possible to change the home-as-up indicator glyph
    /// in an action bar without some really gross hacks. Since the MR2 SDK is not published as of
    /// this writing, the new API is accessed via reflection here if available.
    /// </summary>
    public class SherlockActionBarDrawerToggleCompat
    {
        private const string TAG = "SherlockActionBarDrawerToggleCompat";

        private static readonly int[] THEME_ATTRS = { Android.Resource.Attribute.HomeAsUpIndicator };

        public static object setActionBarUpIndicator(object info, Activity activity, Drawable drawable, int contentDescRes)
        {
            if (info == null)
            {
                info = new SetIndicatorInfo(activity);
            }

            SetIndicatorInfo sii = (SetIndicatorInfo)info;
            if (sii.setHomeAsUpIndicator != null)
            {
                try
                {
                    var actionBar = ((SherlockFragmentActivity)activity).SupportActionBar;
                    sii.setHomeAsUpIndicator.Invoke(actionBar, drawable);
                    sii.setHomeActionContentDescription.Invoke(actionBar, contentDescRes);
                }
                catch (Exception e)
                {
                    Log.Warn(TAG, "Couldn't set home-as-up indicator via JB-MR2 API", e);
                }
            }
            else if (sii.upIndicatorView != null)
            {
                sii.upIndicatorView.SetImageDrawable(drawable);
            }
            else
            {
                Log.Warn(TAG, "Couldn't set home-as-up indicator");
            }

            return info;
        }

        public static object setActionBarDescription(object info, Activity activity, int contentDescRes)
        {
            if (info == null)
            {
                info = new SetIndicatorInfo(activity);
            }

            SetIndicatorInfo sii = (SetIndicatorInfo)info;
            if (sii.setHomeAsUpIndicator != null)
            {
                try
                {
                    var actionBar = ((SherlockFragmentActivity)activity).SupportActionBar;
                    sii.setHomeActionContentDescription.Invoke(actionBar, contentDescRes);
                }
                catch (Exception e)
                {
                    Log.Warn(TAG, "Couldn't set content description via JB-MR2 API", e);
                }
            }
            return info;
        }

        public static Drawable getThemeUpIndicator(Activity activity)
        {
            TypedArray a = activity.ObtainStyledAttributes(THEME_ATTRS);
            Drawable result = a.GetDrawable(0);
            a.Recycle();
            return result;
        }

        private class SetIndicatorInfo
        {
            public Method setHomeAsUpIndicator;
            public Method setHomeActionContentDescription;
            public ImageView upIndicatorView;

            internal SetIndicatorInfo(Activity activity)
            {
                try
                {
                    var drawableClass = Object.GetObject<Class>(JNIEnv.FindClass(typeof (Drawable)), JniHandleOwnership.DoNotTransfer);
                    var actionBarClass = Object.GetObject<Class>(JNIEnv.FindClass(typeof (ActionBar)), JniHandleOwnership.DoNotTransfer);
                    setHomeAsUpIndicator = actionBarClass.GetDeclaredMethod("setHomeAsUpIndicator", drawableClass);
                    setHomeActionContentDescription = actionBarClass.GetDeclaredMethod("setHomeActionContentDescription", Integer.Type);

                    // If we got the method we won't need the stuff below.
                    return;
                }
                catch (NoSuchMethodException)
                {
                    // Oh well. We'll use the other mechanism below instead.
                }

                int homeRes = Android.Resource.Id.Home;
                View home = activity.FindViewById(homeRes);

                if (home == null)
                {
                    home = activity.FindViewById(Resource.Id.abs__home);
                    homeRes = Resource.Id.abs__home;
                }

                ViewGroup parent = (ViewGroup)home.Parent;

                int childCount = parent.ChildCount;
                if (childCount != 2)
                {
                    // No idea which one will be the right one, an OEM messed with things.
                    return;
                }

                View first = parent.GetChildAt(0);

                View second = parent.GetChildAt(1);

                View up = first.Id == Android.Resource.Id.Home ? second : first;

                if (up is ImageView)
                {
                    // Jackpot! (Probably...)
                    upIndicatorView = (ImageView)up;
                }
            }
        }
    }

}