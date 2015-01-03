using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Text.Format;
using Android.Util;
using Android.Views;
using Xamarin.BetterPickers;
using Xamarin.BetterPickers.Widget;

namespace Xamarin.BetterPickers.DatePicker
{
	public class DateView : PickerLinearLayout
	{

		private ZeroTopPaddingTextView mMonth;
		private ZeroTopPaddingTextView mDate;
		private ZeroTopPaddingTextView mYearLabel;
		private readonly Typeface mAndroidClockMonoThin;
		private Typeface mOriginalNumberTypeface;
		private UnderlinePageIndicatorPicker mUnderlinePageIndicatorPicker;

		private ColorStateList mTitleColor;

		/// <summary>
		/// Instantiate a DateView
		/// </summary>
		/// <param name="context"> the Context in which to inflate the View </param>
		public DateView(Context context)
            : this(context, null) { }

		/// <summary>
		/// Instantiate a DateView
		/// </summary>
		/// <param name="context"> the Context in which to inflate the View </param>
		/// <param name="attrs"> attributes that define the title color </param>
		public DateView(Context context, IAttributeSet attrs)
            : base(context, attrs)
		{

			mAndroidClockMonoThin = Typeface.CreateFromAsset(context.Assets, "fonts/AndroidClockMono-Thin.ttf");
            mOriginalNumberTypeface = Typeface.CreateFromAsset(context.Assets, "fonts/Roboto-Bold.ttf");

			// Init defaults
			mTitleColor = Resources.GetColorStateList(Resource.Color.dialog_text_color_holo_dark);

			SetWillNotDraw(false);
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

                    mTitleColor = a.GetColorStateList(Resource.Styleable.BetterPickersDialogFragment_bpTitleColor);
				}
    
				restyleViews();
			}
		}

		private void restyleViews()
		{
			if (mMonth != null)
			{
				mMonth.SetTextColor(mTitleColor);
			}
			if (mDate != null)
			{
				mDate.SetTextColor(mTitleColor);
			}
			if (mYearLabel != null)
			{
				mYearLabel.SetTextColor(mTitleColor);
			}
		}

		protected override void OnFinishInflate()
		{
            base.OnFinishInflate();

			mMonth = (ZeroTopPaddingTextView) FindViewById(Resource.Id.month);
			mDate = (ZeroTopPaddingTextView) FindViewById(Resource.Id.date);
			mYearLabel = (ZeroTopPaddingTextView) FindViewById(Resource.Id.year_label);
			// Reorder based on locale
			char[] dateFormatOrder = DateFormat.GetDateFormatOrder(Context);
			RemoveAllViews();
			for (var i = 0; i < dateFormatOrder.Length; i++)
			{
				switch (dateFormatOrder[i])
				{
					case DateFormat.Date:
						AddView(mDate);
						break;
					case DateFormat.Month:
                        AddView(mMonth);
						break;
					case DateFormat.Year:
                        AddView(mYearLabel);
						break;
				}
			}

			if (mMonth != null)
			{
				//mOriginalNumberTypeface = mMonth.getTypeface();
			}
			// Set both TextViews with thin font (for hyphen)
			if (mDate != null)
			{
				mDate.Typeface = mAndroidClockMonoThin;
				mDate.updatePadding();
			}
			if (mMonth != null)
			{
				mMonth.Typeface = mAndroidClockMonoThin;
				mMonth.updatePadding();
			}

			restyleViews();
		}

		/// <summary>
		/// Set the date shown
		/// </summary>
		/// <param name="month"> a String representing the month of year </param>
		/// <param name="dayOfMonth"> an int representing the day of month </param>
		/// <param name="year"> an int representing the year </param>
		public virtual void setDate(string month, int dayOfMonth, int year)
		{
			if (mMonth != null)
			{
				if (month.Equals(""))
				{
					mMonth.Text = "-";
					mMonth.Typeface = mAndroidClockMonoThin;
					mMonth.Enabled = false;
					mMonth.updatePadding();
				}
				else
				{
					mMonth.Text = month;
					mMonth.Typeface = mOriginalNumberTypeface;
					mMonth.Enabled = true;
					mMonth.updatePaddingForBoldDate();
				}
			}
			if (mDate != null)
			{
				if (dayOfMonth <= 0)
				{
					mDate.Text = "-";
					mDate.Enabled = false;
					mDate.updatePadding();
				}
				else
				{
					mDate.Text = Convert.ToString(dayOfMonth);
					mDate.Enabled = true;
					mDate.updatePadding();
				}
			}
			if (mYearLabel != null)
			{
				if (year <= 0)
				{
					mYearLabel.Text = "----";
					mYearLabel.Enabled = false;
					mYearLabel.updatePadding();
				}
				else
				{
					string yearString = Convert.ToString(year);
					// Pad to 4 digits
					while (yearString.Length < 4)
					{
						yearString = "-" + yearString;
					}
					mYearLabel.Text = yearString;
					mYearLabel.Enabled = true;
					mYearLabel.updatePadding();
				}
			}
		}

		/// <summary>
		/// Allow attachment of the UnderlinePageIndicator
		/// </summary>
		/// <param name="indicator"> the indicator to attach </param>
		public virtual UnderlinePageIndicatorPicker UnderlinePage
		{
			set
			{
				mUnderlinePageIndicatorPicker = value;
			}
		}

		protected override void OnDraw(Canvas canvas)
		{
            base.OnDraw(canvas);

			mUnderlinePageIndicatorPicker.TitleView = this;
		}

		/// <summary>
		/// Set an onClickListener for notification
		/// </summary>
		/// <param name="mOnClickListener"> an OnClickListener from the parent </param>
		public virtual IOnClickListener OnClick
		{
			set
			{
				mDate.SetOnClickListener(value);
                mMonth.SetOnClickListener(value);
                mYearLabel.SetOnClickListener(value);
			}
		}

		/// <summary>
		/// Get the date TextView
		/// </summary>
		/// <returns> the date TextView </returns>
		public virtual ZeroTopPaddingTextView Date
		{
			get
			{
				return mDate;
			}
		}

		/// <summary>
		/// Get the month TextView
		/// </summary>
		/// <returns> the month TextView </returns>
		public virtual ZeroTopPaddingTextView Month
		{
			get
			{
				return mMonth;
			}
		}

		/// <summary>
		/// Get the year TextView
		/// </summary>
		/// <returns> the year TextView </returns>
		public virtual ZeroTopPaddingTextView Year
		{
			get
			{
				return mYearLabel;
			}
		}

		public override View GetViewAt(int index)
		{
			return GetChildAt(index);
		}
	}
}