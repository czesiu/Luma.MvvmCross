using System.Collections.Generic;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Xamarin.BetterPickers;
using DatePicker = Xamarin.BetterPickers.DatePicker.DatePicker;

namespace com.doomonafireball.betterpickers.datepicker
{
    //using Activity = android.app.Activity;
    //using ColorStateList = android.content.res.ColorStateList;
    //using TypedArray = android.content.res.TypedArray;
    //using Bundle = android.os.Bundle;
    //using DialogFragment = android.support.v4.app.DialogFragment;
    //using Fragment = android.support.v4.app.Fragment;
    //using LayoutInflater = android.view.LayoutInflater;
    //using View = android.view.View;
    //using ViewGroup = android.view.ViewGroup;
    //using Button = android.widget.Button;

    /// <summary>
    /// Dialog to set alarm time.
    /// </summary>
    public class DatePickerDialogFragment : DialogFragment
    {

        private const string REFERENCE_KEY = "DatePickerDialogFragment_ReferenceKey";
        private const string THEME_RES_ID_KEY = "DatePickerDialogFragment_ThemeResIdKey";
        private const string MONTH_KEY = "DatePickerDialogFragment_MonthKey";
        private const string DAY_KEY = "DatePickerDialogFragment_DayKey";
        private const string YEAR_KEY = "DatePickerDialogFragment_YearKey";

        private Button mSet, mCancel;
        private DatePicker mPicker;

        private int mMonthOfYear = -1;
        private int mDayOfMonth = 0;
        private int mYear = 0;

        private int mReference = -1;
        private int mTheme = -1;
        private View mDividerOne, mDividerTwo;
        private Color mDividerColor;
        private ColorStateList mTextColor;
        private int mButtonBackgroundResId;
        private int mDialogBackgroundResId;
        private List<DatePickerDialogHandler> mDatePickerDialogHandlers = new List<DatePickerDialogHandler>();

        /// <summary>
        /// Create an instance of the Picker (used internally)
        /// </summary>
        /// <param name="reference"> an (optional) user-defined reference, helpful when tracking multiple Pickers </param>
        /// <param name="themeResId"> the style resource ID for theming </param>
        /// <param name="monthOfYear"> (optional) zero-indexed month of year to pre-set </param>
        /// <param name="dayOfMonth"> (optional) day of month to pre-set </param>
        /// <param name="year"> (optional) year to pre-set </param>
        /// <returns> a Picker! </returns>
        public static DatePickerDialogFragment newInstance(int reference, int themeResId, int? monthOfYear, int? dayOfMonth, int? year)
        {
            var frag = new DatePickerDialogFragment();
            var args = new Bundle();
            args.PutInt(REFERENCE_KEY, reference);
            args.PutInt(THEME_RES_ID_KEY, themeResId);
            if (monthOfYear != null)
            {
                args.PutInt(MONTH_KEY, monthOfYear.Value);
            }
            if (dayOfMonth != null)
            {
                args.PutInt(DAY_KEY, dayOfMonth.Value);
            }
            if (year != null)
            {
                args.PutInt(YEAR_KEY, year.Value);
            }
            frag.Arguments = args;
            return frag;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var args = Arguments;
            if (args != null && args.ContainsKey(REFERENCE_KEY))
            {
                mReference = args.GetInt(REFERENCE_KEY);
            }
            if (args != null && args.ContainsKey(THEME_RES_ID_KEY))
            {
                mTheme = args.GetInt(THEME_RES_ID_KEY);
            }
            if (args != null && args.ContainsKey(MONTH_KEY))
            {
                mMonthOfYear = args.GetInt(MONTH_KEY);
            }
            if (args != null && args.ContainsKey(DAY_KEY))
            {
                mDayOfMonth = args.GetInt(DAY_KEY);
            }
            if (args != null && args.ContainsKey(YEAR_KEY))
            {
                mYear = args.GetInt(YEAR_KEY);
            }

            SetStyle(StyleNoTitle, 0);

            // Init defaults
            mTextColor = Resources.GetColorStateList(Resource.Color.dialog_text_color_holo_dark);
            mButtonBackgroundResId = Resource.Drawable.button_background_dark;
            mDividerColor = Resources.GetColor(Resource.Color.default_divider_color_dark);
            mDialogBackgroundResId = Resource.Drawable.dialog_full_holo_dark;

            if (mTheme != -1)
            {

                TypedArray a = Activity.ApplicationContext.ObtainStyledAttributes(mTheme, Resource.Styleable.BetterPickersDialogFragment);

                mTextColor = a.GetColorStateList(Resource.Styleable.BetterPickersDialogFragment_bpTextColor);
                mButtonBackgroundResId = a.GetResourceId(Resource.Styleable.BetterPickersDialogFragment_bpButtonBackground, mButtonBackgroundResId);
                mDividerColor = a.GetColor(Resource.Styleable.BetterPickersDialogFragment_bpDividerColor, mDividerColor);
                mDialogBackgroundResId = a.GetResourceId(Resource.Styleable.BetterPickersDialogFragment_bpDialogBackground, mDialogBackgroundResId);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            View v = inflater.Inflate(Resource.Layout.date_picker_dialog, null);
            mSet = (Button)v.FindViewById(Resource.Id.set_button);
            mCancel = (Button)v.FindViewById(Resource.Id.cancel_button);
            mCancel.SetOnClickListener(new OnClickListenerAnonymousInnerClassHelper(this));
            mPicker = (DatePicker)v.FindViewById(Resource.Id.date_picker);
            mPicker.SetButton = mSet;
            mPicker.setDate(mYear, mMonthOfYear, mDayOfMonth);
            mSet.SetOnClickListener(new OnClickListenerAnonymousInnerClassHelper2(this));

            mDividerOne = v.FindViewById(Resource.Id.divider_1);
            mDividerTwo = v.FindViewById(Resource.Id.divider_2);
            mDividerOne.SetBackgroundColor(mDividerColor);
            mDividerTwo.SetBackgroundColor(mDividerColor);
            mSet.SetTextColor(mTextColor);
            mSet.SetBackgroundResource(mButtonBackgroundResId);
            mCancel.SetTextColor(mTextColor);
            mCancel.SetBackgroundResource(mButtonBackgroundResId);
            mPicker.Theme = mTheme;
            Dialog.Window.SetBackgroundDrawableResource(mDialogBackgroundResId);

            return v;
        }

        private class OnClickListenerAnonymousInnerClassHelper : Java.Lang.Object, View.IOnClickListener
        {
            private readonly DatePickerDialogFragment outerInstance;

            public OnClickListenerAnonymousInnerClassHelper(DatePickerDialogFragment outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public void OnClick(View view)
            {
                outerInstance.Dismiss();
            }
        }

        private class OnClickListenerAnonymousInnerClassHelper2 : Java.Lang.Object, View.IOnClickListener
        {
            private readonly DatePickerDialogFragment outerInstance;

            public OnClickListenerAnonymousInnerClassHelper2(DatePickerDialogFragment outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public void OnClick(View view)
            {
                foreach (DatePickerDialogHandler handler in outerInstance.mDatePickerDialogHandlers)
                {
                    handler.onDialogDateSet(outerInstance.mReference, outerInstance.mPicker.Year, outerInstance.mPicker.MonthOfYear, outerInstance.mPicker.DayOfMonth);
                }

                var activity = outerInstance.Activity;
                var fragment = outerInstance.TargetFragment;
                if (activity is DatePickerDialogHandler)
                {
                    DatePickerDialogHandler act = (DatePickerDialogHandler)activity;
                    act.onDialogDateSet(outerInstance.mReference, outerInstance.mPicker.Year, outerInstance.mPicker.MonthOfYear, outerInstance.mPicker.DayOfMonth);
                }
                else if (fragment is DatePickerDialogHandler)
                {
                    DatePickerDialogHandler frag = (DatePickerDialogHandler)fragment;
                    frag.onDialogDateSet(outerInstance.mReference, outerInstance.mPicker.Year, outerInstance.mPicker.MonthOfYear, outerInstance.mPicker.DayOfMonth);
                }
                outerInstance.Dismiss();
            }
        }

        /// <summary>
        /// This interface allows objects to register for the Picker's set action.
        /// </summary>
        public interface DatePickerDialogHandler
        {

            void onDialogDateSet(int reference, int year, int monthOfYear, int dayOfMonth);
        }

        /// <summary>
        /// Attach a Vector of handlers to be notified in addition to the Fragment's Activity and target Fragment.
        /// </summary>
        /// <param name="handlers"> a Vector of handlers </param>
        public virtual List<DatePickerDialogHandler> DatePickerDialogHandlers
        {
            set
            {
                mDatePickerDialogHandlers = value;
            }
        }
    }
}