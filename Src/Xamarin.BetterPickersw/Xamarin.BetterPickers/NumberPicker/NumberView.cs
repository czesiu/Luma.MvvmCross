using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.BetterPickers.Widget;

namespace Xamarin.BetterPickers
{

    //using ZeroTopPaddingTextView = ZeroTopPaddingTextView;

    //using Context = android.content.Context;
    //using ColorStateList = android.content.res.ColorStateList;
    //using TypedArray = android.content.res.TypedArray;
    //using Typeface = android.graphics.Typeface;
    //using IAttributeSet = android.util.AttributeSet;
    //using View = android.view.View;
    //using LinearLayout = android.widget.LinearLayout;

	public class NumberView : LinearLayout
	{

		private ZeroTopPaddingTextView mNumber, mDecimal;
		private ZeroTopPaddingTextView mDecimalSeperator;
		private ZeroTopPaddingTextView mMinusLabel;
		private readonly Typeface mAndroidClockMonoThin;
		private Typeface mOriginalNumberTypeface;

		private ColorStateList mTextColor;

		/// <summary>
		/// Instantiate a NumberView
		/// </summary>
		/// <param name="context"> the Context in which to inflate the View </param>
		public NumberView(Context context)
            : this(context, null) { }

		/// <summary>
		/// Instantiate a NumberView
		/// </summary>
		/// <param name="context"> the Context in which to inflate the View </param>
		/// <param name="attrs"> attributes that define the title color </param>
		public NumberView(Context context, IAttributeSet attrs)
            : base(context, attrs)
		{
            var fonts = context.Assets.List("fonts");
		    var x = context.Assets.List("");
            var xx = context.Assets.Open("fonts/AndroidClockMono-Thin.ttf");
			mAndroidClockMonoThin = Typeface.CreateFromAsset(context.Assets, "fonts/AndroidClockMono-Thin.ttf");

			// Init defaults
			mTextColor = Resources.GetColorStateList(Resource.Color.dialog_text_color_holo_dark);
		}

		/// <summary>
		/// Set a theme and restyle the views. This View will change its title color.
		/// </summary>
		/// <param name="themeResId"> the resource ID for theming </param>
		public virtual int Theme
		{
			set
			{
				if (value != -1)
				{
					TypedArray a = Context.ObtainStyledAttributes(value, Resource.Styleable.BetterPickersDialogFragment);

                    mTextColor = a.GetColorStateList(Resource.Styleable.BetterPickersDialogFragment_bpTextColor);
				}
    
				restyleViews();
			}
		}

		private void restyleViews()
		{
			if (mNumber != null)
			{
				mNumber.SetTextColor(mTextColor);
			}
			if (mDecimal != null)
			{
                mDecimal.SetTextColor(mTextColor);
			}
			if (mDecimalSeperator != null)
			{
                mDecimalSeperator.SetTextColor(mTextColor);
			}
			if (mMinusLabel != null)
			{
                mMinusLabel.SetTextColor(mTextColor);
			}
		}

		protected override void OnFinishInflate()
		{
            base.OnFinishInflate();

			mNumber = (ZeroTopPaddingTextView) FindViewById(Resource.Id.number);
			mDecimal = (ZeroTopPaddingTextView) FindViewById(Resource.Id.@decimal);
			mDecimalSeperator = (ZeroTopPaddingTextView) FindViewById(Resource.Id.decimal_separator);
			mMinusLabel = (ZeroTopPaddingTextView) FindViewById(Resource.Id.minus_label);
			if (mNumber != null)
			{
				mOriginalNumberTypeface = mNumber.Typeface;
			}
			// Set the lowest time unit with thin font
			if (mNumber != null)
			{
				mNumber.Typeface = mAndroidClockMonoThin;
				mNumber.updatePadding();
			}
			if (mDecimal != null)
			{
				mDecimal.Typeface = mAndroidClockMonoThin;
				mDecimal.updatePadding();
			}

			restyleViews();
		}

		/// <summary>
		/// Set the number shown
		/// </summary>
		/// <param name="numbersDigit"> the non-decimal digits </param>
		/// <param name="decimalDigit"> the decimal digits </param>
		/// <param name="showDecimal"> whether it's a decimal or not </param>
		/// <param name="isNegative"> whether it's positive or negative </param>
		public virtual void setNumber(string numbersDigit, string decimalDigit, bool showDecimal, bool isNegative)
		{
            mMinusLabel.Visibility = isNegative ? ViewStates.Visible : ViewStates.Gone;
			if (mNumber != null)
			{
				if (numbersDigit.Equals(""))
				{
					// Set to -
					mNumber.Text = "-";
					mNumber.Typeface = mAndroidClockMonoThin;
					mNumber.Enabled = false;
					mNumber.updatePadding();
                    mNumber.Visibility = ViewStates.Visible;
				}
				else if (showDecimal)
				{
					// Set to bold
					mNumber.Text = numbersDigit;
					mNumber.Typeface = mOriginalNumberTypeface;
					mNumber.Enabled = true;
					mNumber.updatePaddingForBoldDate();
                    mNumber.Visibility = ViewStates.Gone;
				}
				else
				{
					// Set to thin
					mNumber.Text = numbersDigit;
					mNumber.Typeface = mAndroidClockMonoThin;
					mNumber.Enabled = true;
					mNumber.updatePadding();
                    mNumber.Visibility = ViewStates.Visible;
				}
			}
			if (mDecimal != null)
			{
				// Hide digit
				if (decimalDigit.Equals(""))
				{
                    mDecimal.Visibility = ViewStates.Gone;
				}
				else
				{
					mDecimal.Text = decimalDigit;
					mDecimal.Typeface = mAndroidClockMonoThin;
					mDecimal.Enabled = true;
					mDecimal.updatePadding();
                    mDecimal.Visibility = ViewStates.Visible;
				}
			}
			if (mDecimalSeperator != null)
			{
				// Hide separator
                mDecimalSeperator.Visibility = showDecimal ? ViewStates.Visible : ViewStates.Gone;
			}
		}
	}
}