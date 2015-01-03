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

using Android.OS;
using Android.Runtime;
using Android.Text.Format;
using Android.Views;
using Java.Lang;
using Java.Sql;
using Xamarin.NineOldAndroids.Animations;
using Xamarin.NineOldAndroids.Views.Animations;
using DateFormat = Java.Text.DateFormat;
using Time = Android.Text.Format.Time;

namespace Xamarin.BetterPickers
{

    //using Keyframe = com.nineoldandroids.animation.Keyframe;
    //using ObjectAnimator = com.nineoldandroids.animation.ObjectAnimator;
    //using PropertyValuesHolder = com.nineoldandroids.animation.PropertyValuesHolder;
    //using AnimatorProxy = com.nineoldandroids.view.animation.AnimatorProxy;

    //using SuppressLint = android.annotation.SuppressLint;
    //using Build = android.os.Build;
    //using Time = android.text.format.Time;
    //using View = android.view.View;

	/// <summary>
	/// Utility helper functions for time and date pickers.
	/// </summary>
	public class Utils
	{

		public static readonly int MONDAY_BEFORE_JULIAN_EPOCH = Time.EpochJulianDay - 3;
		public const int PULSE_ANIMATOR_DURATION = 544;

		// Alpha level for time picker selection.
		public const int SELECTED_ALPHA = 51;
		public const int SELECTED_ALPHA_THEME_DARK = 102;
		// Alpha level for fully opaque.
		public const int FULL_ALPHA = 255;


		internal const string SHARED_PREFS_NAME = "com.android.calendar_preferences";

		public static bool JellybeanOrLater
		{
			get
			{
				return Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean;
			}
		}

		/// <summary>
		/// Try to speak the specified text, for accessibility. Only available on JB or later. </summary>
		/// <param name="text"> Text to announce. </param>
		//@SuppressLint("NewApi") public static void tryAccessibilityAnnounce(android.view.View view, CharSequence text)
		public static void tryAccessibilityAnnounce(View view, string text)
		{
			if (JellybeanOrLater && view != null && text != null)
			{
				view.AnnounceForAccessibility(text);
			}
		}

		public static int getDaysInMonth(int month, int year)
		{
			switch (month)
			{
				case 1:
				case 3:
				case 5:
				case 7:
				case 8:
				case 10:
				case 12:
					return 31;
				case 4:
				case 6:
				case 9:
				case 11:
					return 30;
				case 2:
					return (year % 4 == 0) ? 29 : 28;
				default:
					throw new System.ArgumentException("Invalid Month");
			}
		}

		/// <summary>
		/// Takes a number of weeks since the epoch and calculates the Julian day of
		/// the Monday for that week.
		/// 
		/// This assumes that the week containing the <seealso cref="android.text.format.Time#EPOCH_JULIAN_DAY"/>
		/// is considered week 0. It returns the Julian day for the Monday
		/// {@code week} weeks after the Monday of the week containing the epoch.
		/// </summary>
		/// <param name="week"> Number of weeks since the epoch </param>
		/// <returns> The julian day for the Monday of the given week since the epoch </returns>
		public static int getJulianMondayFromWeeksSinceEpoch(int week)
		{
			return MONDAY_BEFORE_JULIAN_EPOCH + week * 7;
		}

		/// <summary>
		/// Returns the week since <seealso cref="android.text.format.Time#EPOCH_JULIAN_DAY"/> (Jan 1, 1970)
		/// adjusted for first day of week.
		/// 
		/// This takes a julian day and the week start day and calculates which
		/// week since <seealso cref="android.text.format.Time#EPOCH_JULIAN_DAY"/> that day occurs in, starting
		/// at 0. *Do not* use this to compute the ISO week number for the year.
		/// </summary>
		/// <param name="julianDay"> The julian day to calculate the week number for </param>
		/// <param name="firstDayOfWeek"> Which week day is the first day of the week,
		///          see <seealso cref="android.text.format.Time#SUNDAY"/> </param>
		/// <returns> Weeks since the epoch </returns>
		public static int getWeeksSinceEpochFromJulianDay(int julianDay, int firstDayOfWeek)
		{
            var diff = (int)DayOfWeek.Thursday - firstDayOfWeek;
			if (diff < 0)
			{
				diff += 7;
			}
			int refDay = Time.EpochJulianDay - diff;
			return (julianDay - refDay) / 7;
		}

		/// <summary>
		/// Render an animator to pulsate a view in place. </summary>
		/// <param name="labelToAnimate"> the view to pulsate. </param>
		/// <returns> The animator object. Use .start() to begin. </returns>
		public static ObjectAnimator getPulseAnimator(View labelToAnimate, float decreaseRatio, float increaseRatio)
		{
			var k0 = Keyframe.OfFloat(0f, 1f);
            var k1 = Keyframe.OfFloat(0.275f, decreaseRatio);
            var k2 = Keyframe.OfFloat(0.69f, increaseRatio);
            var k3 = Keyframe.OfFloat(1f, 1f);

			var scaleX = PropertyValuesHolder.OfKeyframe("scaleX", k0, k1, k2, k3);
            var scaleY = PropertyValuesHolder.OfKeyframe("scaleY", k0, k1, k2, k3);
			var pulseAnimator = ObjectAnimator.OfPropertyValuesHolder(AnimatorProxy.NeedsProxy ? AnimatorProxy.Wrap(labelToAnimate) : (Object)labelToAnimate, scaleX, scaleY);
			pulseAnimator.SetDuration(PULSE_ANIMATOR_DURATION);

			return pulseAnimator;
		}
	}

}