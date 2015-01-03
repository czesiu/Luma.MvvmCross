using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.BetterPickers;
using Xamarin.BetterPickers.Widget;

namespace Xamarin.BetterPickers.TimePicker
{
    public class TimerView : LinearLayout
    {

        private ZeroTopPaddingTextView mHoursOnes, mMinutesOnes;
        private ZeroTopPaddingTextView mHoursTens, mMinutesTens;
        private readonly Typeface mAndroidClockMonoThin;
        private Typeface mOriginalHoursTypeface;

        private ZeroTopPaddingTextView mHoursSeperator;
        private ColorStateList mTextColor;

        /// <summary>
        /// Instantiates a TimerView
        /// </summary>
        /// <param name="context"> the Context in which to inflate the View </param>
        public TimerView(Context context)
            : this(context, null)
        {
        }

        /// <summary>
        /// Instantiates a TimerView
        /// </summary>
        /// <param name="context"> the Context in which to inflate the View </param>
        /// <param name="attrs"> attributes that define the text color </param>
        public TimerView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {

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
            if (mHoursOnes != null)
            {
                mHoursOnes.SetTextColor(mTextColor);
            }
            if (mMinutesOnes != null)
            {
                mMinutesOnes.SetTextColor(mTextColor);
            }
            if (mHoursTens != null)
            {
                mHoursTens.SetTextColor(mTextColor);
            }
            if (mMinutesTens != null)
            {
                mMinutesTens.SetTextColor(mTextColor);
            }
            if (mHoursSeperator != null)
            {
                mHoursSeperator.SetTextColor(mTextColor);
            }
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();

            mHoursTens = (ZeroTopPaddingTextView)FindViewById(Resource.Id.hours_tens);
            mMinutesTens = (ZeroTopPaddingTextView)FindViewById(Resource.Id.minutes_tens);
            mHoursOnes = (ZeroTopPaddingTextView)FindViewById(Resource.Id.hours_ones);
            mMinutesOnes = (ZeroTopPaddingTextView)FindViewById(Resource.Id.minutes_ones);
            mHoursSeperator = (ZeroTopPaddingTextView)FindViewById(Resource.Id.hours_seperator);
            if (mHoursOnes != null)
            {
                mOriginalHoursTypeface = mHoursOnes.Typeface;
            }
            // Set the lowest time unit with thin font (excluding hundredths)
            if (mMinutesTens != null)
            {
                mMinutesTens.Typeface = mAndroidClockMonoThin;
                mMinutesTens.updatePadding();
            }
            if (mMinutesOnes != null)
            {
                mMinutesOnes.Typeface = mAndroidClockMonoThin;
                mMinutesOnes.updatePadding();
            }
        }

        /// <summary>
        /// Set the time shown
        /// </summary>
        /// <param name="hoursTensDigit"> the tens digit of the hours </param>
        /// <param name="hoursOnesDigit"> the ones digit of the hours </param>
        /// <param name="minutesTensDigit"> the tens digit of the minutes </param>
        /// <param name="minutesOnesDigit"> the ones digit of the minutes </param>
        public virtual void setTime(int hoursTensDigit, int hoursOnesDigit, int minutesTensDigit, int minutesOnesDigit)
        {
            if (mHoursTens != null)
            {
                // Hide digit
                if (hoursTensDigit == -2)
                {
                    mHoursTens.Visibility = ViewStates.Invisible;
                }
                else if (hoursTensDigit == -1)
                {
                    mHoursTens.Text = "-";
                    mHoursTens.Typeface = mAndroidClockMonoThin;
                    mHoursTens.Enabled = false;
                    mHoursTens.updatePadding();
                    mHoursTens.Visibility = ViewStates.Visible;
                }
                else
                {
                    mHoursTens.Text = string.Format("{0:D}", hoursTensDigit);
                    mHoursTens.Typeface = mOriginalHoursTypeface;
                    mHoursTens.Enabled = true;
                    mHoursTens.updatePaddingForBoldDate();
                    mHoursTens.Visibility = ViewStates.Visible;
                }
            }
            if (mHoursOnes != null)
            {
                if (hoursOnesDigit == -1)
                {
                    mHoursOnes.Text = "-";
                    mHoursOnes.Typeface = mAndroidClockMonoThin;
                    mHoursOnes.Enabled = false;
                    mHoursOnes.updatePadding();
                }
                else
                {
                    mHoursOnes.Text = string.Format("{0:D}", hoursOnesDigit);
                    mHoursOnes.Typeface = mOriginalHoursTypeface;
                    mHoursOnes.Enabled = true;
                    mHoursOnes.updatePaddingForBoldDate();
                }
            }
            if (mMinutesTens != null)
            {
                if (minutesTensDigit == -1)
                {
                    mMinutesTens.Text = "-";
                    mMinutesTens.Enabled = false;
                }
                else
                {
                    mMinutesTens.Enabled = true;
                    mMinutesTens.Text = string.Format("{0:D}", minutesTensDigit);
                }
            }
            if (mMinutesOnes != null)
            {
                if (minutesOnesDigit == -1)
                {
                    mMinutesOnes.Text = "-";
                    mMinutesOnes.Enabled = false;
                }
                else
                {
                    mMinutesOnes.Text = string.Format("{0:D}", minutesOnesDigit);
                    mMinutesOnes.Enabled = true;
                }
            }
        }
    }
}