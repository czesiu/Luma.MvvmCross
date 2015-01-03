using System.Collections.Generic;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using Fragment = Android.Support.V4.App.Fragment;

namespace Xamarin.BetterPickers
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
	public class NumberPickerDialogFragment : DialogFragment
	{

		private const string REFERENCE_KEY = "NumberPickerDialogFragment_ReferenceKey";
		private const string THEME_RES_ID_KEY = "NumberPickerDialogFragment_ThemeResIdKey";
		private const string MIN_NUMBER_KEY = "NumberPickerDialogFragment_MinNumberKey";
		private const string MAX_NUMBER_KEY = "NumberPickerDialogFragment_MaxNumberKey";
		private const string PLUS_MINUS_VISIBILITY_KEY = "NumberPickerDialogFragment_PlusMinusVisibilityKey";
		private const string DECIMAL_VISIBILITY_KEY = "NumberPickerDialogFragment_DecimalVisibilityKey";
		private const string LABEL_TEXT_KEY = "NumberPickerDialogFragment_LabelTextKey";

		private Button mSet, mCancel;
		private NumberPicker mPicker;

		private View mDividerOne, mDividerTwo;
		private int mReference = -1;
		private int mTheme = -1;
		private Color mDividerColor;
		private ColorStateList mTextColor;
		private string mLabelText = "";
		private int mButtonBackgroundResId;
		private int mDialogBackgroundResId;

		private int? mMinNumber = null;
		private int? mMaxNumber = null;
        private ViewStates mPlusMinusVisibility = ViewStates.Visible;
        private ViewStates mDecimalVisibility = ViewStates.Visible;
		private List<NumberPickerDialogHandler> mNumberPickerDialogHandlers = new List<NumberPickerDialogHandler>();

		/// <summary>
		/// Create an instance of the Picker (used internally)
		/// </summary>
		/// <param name="reference"> an (optional) user-defined reference, helpful when tracking multiple Pickers </param>
		/// <param name="themeResId"> the style resource ID for theming </param>
		/// <param name="minNumber"> (optional) the minimum possible number </param>
		/// <param name="maxNumber"> (optional) the maximum possible number </param>
		/// <param name="plusMinusVisibility"> (optional) View.VISIBLE, View.INVISIBLE, or View.GONE </param>
		/// <param name="decimalVisibility"> (optional) View.VISIBLE, View.INVISIBLE, or View.GONE </param>
		/// <param name="labelText"> (optional) text to add as a label </param>
		/// <returns> a Picker! </returns>
		public static NumberPickerDialogFragment newInstance(int reference, int themeResId, int? minNumber, int? maxNumber, int? plusMinusVisibility, int? decimalVisibility, string labelText)
		{
			var frag = new NumberPickerDialogFragment();
			var args = new Bundle();
			args.PutInt(REFERENCE_KEY, reference);
            args.PutInt(THEME_RES_ID_KEY, themeResId);
			if (minNumber != null)
			{
                args.PutInt(MIN_NUMBER_KEY, minNumber.Value);
			}
			if (maxNumber != null)
			{
                args.PutInt(MAX_NUMBER_KEY, maxNumber.Value);
			}
			if (plusMinusVisibility != null)
			{
                args.PutInt(PLUS_MINUS_VISIBILITY_KEY, plusMinusVisibility.Value);
			}
			if (decimalVisibility != null)
			{
                args.PutInt(DECIMAL_VISIBILITY_KEY, decimalVisibility.Value);
			}
			if (labelText != null)
			{
				args.PutString(LABEL_TEXT_KEY, labelText);
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
            if (args != null && args.ContainsKey(PLUS_MINUS_VISIBILITY_KEY))
			{
                mPlusMinusVisibility = (ViewStates)args.GetInt(PLUS_MINUS_VISIBILITY_KEY);
			}
            if (args != null && args.ContainsKey(DECIMAL_VISIBILITY_KEY))
			{
                mDecimalVisibility = (ViewStates)args.GetInt(DECIMAL_VISIBILITY_KEY);
			}
            if (args != null && args.ContainsKey(MIN_NUMBER_KEY))
			{
                mMinNumber = args.GetInt(MIN_NUMBER_KEY);
			}
            if (args != null && args.ContainsKey(MAX_NUMBER_KEY))
			{
				mMaxNumber = args.GetInt(MAX_NUMBER_KEY);
			}
            if (args != null && args.ContainsKey(LABEL_TEXT_KEY))
			{
				mLabelText = args.GetString(LABEL_TEXT_KEY);
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
			View v = inflater.Inflate(Resource.Layout.number_picker_dialog, null);
			mSet = (Button) v.FindViewById(Resource.Id.set_button);
			mCancel = (Button) v.FindViewById(Resource.Id.cancel_button);
			mCancel.SetOnClickListener(new OnClickListenerAnonymousInnerClassHelper(this));
			mPicker = (NumberPicker) v.FindViewById(Resource.Id.number_picker);
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

			mPicker.DecimalVisibility = mDecimalVisibility;
			mPicker.PlusMinusVisibility = mPlusMinusVisibility;
			mPicker.LabelText = mLabelText;
			if (mMinNumber != null)
			{
				mPicker.Min = mMinNumber.Value;
			}
			if (mMaxNumber != null)
			{
				mPicker.Max = mMaxNumber.Value;
			}

			return v;
		}

		private class OnClickListenerAnonymousInnerClassHelper : Java.Lang.Object, View.IOnClickListener
		{
			private readonly NumberPickerDialogFragment outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(NumberPickerDialogFragment outerInstance)
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
			private readonly NumberPickerDialogFragment outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(NumberPickerDialogFragment outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void OnClick(View view)
			{
				double number = outerInstance.mPicker.EnteredNumber;
				if (outerInstance.mMinNumber != null && outerInstance.mMaxNumber != null && (number < outerInstance.mMinNumber || number > outerInstance.mMaxNumber))
				{
                    string errorText = string.Format(outerInstance.GetString(Resource.String.min_max_error), outerInstance.mMinNumber, outerInstance.mMaxNumber);
					outerInstance.mPicker.ErrorView.Text = errorText;
					outerInstance.mPicker.ErrorView.Show();
					return;
				}
				else if (outerInstance.mMinNumber != null && number < outerInstance.mMinNumber)
				{
                    string errorText = string.Format(outerInstance.GetString(Resource.String.min_error), outerInstance.mMinNumber);
					outerInstance.mPicker.ErrorView.Text = errorText;
					outerInstance.mPicker.ErrorView.Show();
					return;
				}
				else if (outerInstance.mMaxNumber != null && number > outerInstance.mMaxNumber)
				{
                    string errorText = string.Format(outerInstance.GetString(Resource.String.max_error), outerInstance.mMaxNumber);
					outerInstance.mPicker.ErrorView.Text = errorText;
					outerInstance.mPicker.ErrorView.Show();
					return;
				}
				foreach (NumberPickerDialogHandler handler in outerInstance.mNumberPickerDialogHandlers)
				{
					handler.onDialogNumberSet(outerInstance.mReference, outerInstance.mPicker.Number, outerInstance.mPicker.Decimal, outerInstance.mPicker.IsNegative, number);
				}

				Activity activity = outerInstance.Activity;
                Fragment fragment = outerInstance.TargetFragment;
				if (activity is NumberPickerDialogHandler)
				{
					NumberPickerDialogHandler act = (NumberPickerDialogHandler) activity;
					act.onDialogNumberSet(outerInstance.mReference, outerInstance.mPicker.Number, outerInstance.mPicker.Decimal, outerInstance.mPicker.IsNegative, number);
				}
				else if (fragment is NumberPickerDialogHandler)
				{
					NumberPickerDialogHandler frag = (NumberPickerDialogHandler) fragment;
					frag.onDialogNumberSet(outerInstance.mReference, outerInstance.mPicker.Number, outerInstance.mPicker.Decimal, outerInstance.mPicker.IsNegative, number);
				}
                outerInstance.Dismiss();
			}
		}

		/// <summary>
		/// This interface allows objects to register for the Picker's set action.
		/// </summary>
		public interface NumberPickerDialogHandler
		{

			void onDialogNumberSet(int reference, int number, double @decimal, bool isNegative, double fullNumber);
		}

		/// <summary>
		/// Attach a Vector of handlers to be notified in addition to the Fragment's Activity and target Fragment.
		/// </summary>
		/// <param name="handlers"> a Vector of handlers </param>
		public virtual List<NumberPickerDialogHandler> NumberPickerDialogHandlers
		{
			set
			{
				mNumberPickerDialogHandlers = value;
			}
		}
	}
}