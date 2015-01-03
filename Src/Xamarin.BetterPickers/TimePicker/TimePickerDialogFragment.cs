using System.Collections.Generic;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using Fragment = Android.Support.V4.App.Fragment;

namespace Xamarin.BetterPickers.TimePicker
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
    public class TimePickerDialogFragment : DialogFragment
    {

        private const string REFERENCE_KEY = "TimePickerDialogFragment_ReferenceKey";
        private const string THEME_RES_ID_KEY = "TimePickerDialogFragment_ThemeResIdKey";

        private Button mSet, mCancel;
        private com.doomonafireball.betterpickers.timepicker.TimePicker mPicker;

        private int mReference = -1;
        private int mTheme = -1;
        private View mDividerOne, mDividerTwo;
        private Color mDividerColor;
        private ColorStateList mTextColor;
        private int mButtonBackgroundResId;
        private int mDialogBackgroundResId;
        private List<TimePickerDialogHandler> mTimePickerDialogHandlers = new List<TimePickerDialogHandler>();

        /// <summary>
        /// Create an instance of the Picker (used internally)
        /// </summary>
        /// <param name="reference"> an (optional) user-defined reference, helpful when tracking multiple Pickers </param>
        /// <param name="themeResId"> the style resource ID for theming </param>
        /// <returns> a Picker! </returns>
        public static TimePickerDialogFragment newInstance(int reference, int themeResId)
        {
            var frag = new TimePickerDialogFragment();
            var args = new Bundle();
            args.PutInt(REFERENCE_KEY, reference);
            args.PutInt(THEME_RES_ID_KEY, themeResId);
            frag.Arguments = args;
            return frag;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Bundle args = Arguments;
            if (args != null && args.ContainsKey(REFERENCE_KEY))
            {
                mReference = args.GetInt(REFERENCE_KEY);
            }
            if (args != null && args.ContainsKey(THEME_RES_ID_KEY))
            {
                mTheme = args.GetInt(THEME_RES_ID_KEY);
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
            var v = inflater.Inflate(Resource.Layout.time_picker_dialog, null);
            mSet = (Button)v.FindViewById(Resource.Id.set_button);
            mCancel = (Button)v.FindViewById(Resource.Id.cancel_button);
            mCancel.SetOnClickListener(new OnClickListenerAnonymousInnerClassHelper(this));
            mPicker = (com.doomonafireball.betterpickers.timepicker.TimePicker)v.FindViewById(Resource.Id.time_picker);
            mPicker.SetButton = mSet;
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
            private readonly TimePickerDialogFragment outerInstance;

            public OnClickListenerAnonymousInnerClassHelper(TimePickerDialogFragment outerInstance)
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
            private readonly TimePickerDialogFragment outerInstance;

            public OnClickListenerAnonymousInnerClassHelper2(TimePickerDialogFragment outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public void OnClick(View view)
            {
                foreach (TimePickerDialogHandler handler in outerInstance.mTimePickerDialogHandlers)
                {
                    handler.onDialogTimeSet(outerInstance.mReference, outerInstance.mPicker.Hours, outerInstance.mPicker.Minutes);
                }

                var activity = outerInstance.Activity;
                var fragment = outerInstance.TargetFragment;
                if (activity is TimePickerDialogHandler)
                {
                    var act = (TimePickerDialogHandler)activity;
                    act.onDialogTimeSet(outerInstance.mReference, outerInstance.mPicker.Hours, outerInstance.mPicker.Minutes);
                }
                else if (fragment is TimePickerDialogHandler)
                {
                    var frag = (TimePickerDialogHandler)fragment;
                    frag.onDialogTimeSet(outerInstance.mReference, outerInstance.mPicker.Hours, outerInstance.mPicker.Minutes);
                }
                outerInstance.Dismiss();
            }
        }

        /// <summary>
        /// This interface allows objects to register for the Picker's set action.
        /// </summary>
        public interface TimePickerDialogHandler
        {

            void onDialogTimeSet(int reference, int hourOfDay, int minute);
        }

        /// <summary>
        /// Attach a Vector of handlers to be notified in addition to the Fragment's Activity and target Fragment.
        /// </summary>
        /// <param name="handlers"> a Vector of handlers </param>
        public virtual List<TimePickerDialogHandler> TimePickerDialogHandlers
        {
            set
            {
                mTimePickerDialogHandlers = value;
            }
        }
    }
}