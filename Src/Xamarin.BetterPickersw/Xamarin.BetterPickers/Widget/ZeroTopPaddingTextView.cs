/*
 * Copyright (C) 2012 The Android Open Source Project
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
 * limitations under the License
 */

using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Widget;

namespace Xamarin.BetterPickers
{
	/// <summary>
	/// Displays text with no padding at the top.
	/// </summary>
	public class ZeroTopPaddingTextView : TextView
	{

		private const float NORMAL_FONT_PADDING_RATIO = 0.328f;
		// the bold fontface has less empty space on the top
		private const float BOLD_FONT_PADDING_RATIO = 0.208f;

		private const float NORMAL_FONT_BOTTOM_PADDING_RATIO = 0.25f;
		// the bold fontface has less empty space on the top
		private const float BOLD_FONT_BOTTOM_PADDING_RATIO = 0.208f;

		// pre-ICS (Droid Sans) has weird empty space on the bottom
		private const float PRE_ICS_BOTTOM_PADDING_RATIO = 0.233f;

		private static readonly Typeface SAN_SERIF_BOLD = Typeface.Create("san-serif", TypefaceStyle.Bold);
        private static readonly Typeface SAN_SERIF_CONDENSED_BOLD = Typeface.Create("sans-serif-condensed", TypefaceStyle.Bold);

		private int mPaddingRight;

		private string decimalSeperator = "";
		private string timeSeperator = "";

		public ZeroTopPaddingTextView(Context context)
            : this(context, null) { }
		public ZeroTopPaddingTextView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0) { }

		public ZeroTopPaddingTextView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            init();
			SetIncludeFontPadding(false);
			updatePadding();
		}

		private void init()
		{
			decimalSeperator = Resources.GetString(Resource.String.number_picker_seperator);
            timeSeperator = Resources.GetString(Resource.String.time_picker_time_seperator);
		}

		public virtual void updatePadding()
		{
			var paddingRatio = NORMAL_FONT_PADDING_RATIO;
			var bottomPaddingRatio = NORMAL_FONT_BOTTOM_PADDING_RATIO;
			if (Paint.Typeface != null && Paint.Typeface.Equals(Typeface.DefaultBold))
			{
				paddingRatio = BOLD_FONT_PADDING_RATIO;
				bottomPaddingRatio = BOLD_FONT_BOTTOM_PADDING_RATIO;
			}
			if (Typeface != null && Typeface.Equals(SAN_SERIF_BOLD))
			{
				paddingRatio = BOLD_FONT_PADDING_RATIO;
				bottomPaddingRatio = BOLD_FONT_BOTTOM_PADDING_RATIO;
			}
			if (Typeface != null && Typeface.Equals(SAN_SERIF_CONDENSED_BOLD))
			{
				paddingRatio = BOLD_FONT_PADDING_RATIO;
				bottomPaddingRatio = BOLD_FONT_BOTTOM_PADDING_RATIO;
			}
			if (Build.VERSION.SdkInt < BuildVersionCodes.IceCreamSandwich && Text != null && (Text.Equals(decimalSeperator) || Text.Equals(timeSeperator)))
			{
				bottomPaddingRatio = PRE_ICS_BOTTOM_PADDING_RATIO;
			}
			// no need to scale by display density because getTextSize() already returns the font
			// height in px
			SetPadding(0, (int)(-paddingRatio * TextSize), mPaddingRight, (int)(-bottomPaddingRatio * TextSize));
		}

		public virtual void updatePaddingForBoldDate()
		{
			var paddingRatio = BOLD_FONT_PADDING_RATIO;
			var bottomPaddingRatio = BOLD_FONT_BOTTOM_PADDING_RATIO;
			// no need to scale by display density because getTextSize() already returns the font
			// height in px
            SetPadding(0, (int)(-paddingRatio * TextSize), mPaddingRight, (int)(-bottomPaddingRatio * TextSize));
		}

		public virtual int PaddingRight
		{
			set
			{
				mPaddingRight = value;
				updatePadding();
			}
		}
	}
}