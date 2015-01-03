using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Xamarin.BetterPickers
{
    //using Context = android.content.Context;
    //using ColorStateList = android.content.res.ColorStateList;
    //using Resources = android.content.res.Resources;
    //using TypedArray = android.content.res.TypedArray;
    //using Parcel = android.os.Parcel;
    //using Parcelable = android.os.Parcelable;
    //using IAttributeSet = android.util.AttributeSet;
    //using HapticFeedbackConstants = android.view.HapticFeedbackConstants;
    //using LayoutInflater = android.view.LayoutInflater;
    //using View = android.view.View;
    //using Button = android.widget.Button;
    //using ImageButton = android.widget.ImageButton;
    //using LinearLayout = android.widget.LinearLayout;
    //using TextView = android.widget.TextView;

	public class NumberPicker : LinearLayout, View.IOnClickListener, View.IOnLongClickListener
	{
		private bool InstanceFieldsInitialized;

		private void InitializeInstanceFields()
		{
			mInput = new int[mInputSize];
		}


		protected int mInputSize = 20;
		protected readonly Button[] mNumbers = new Button[10];
		protected int[] mInput;
		protected int mInputPointer = -1;
		protected Button mLeft, mRight;
		protected ImageButton mDelete;
		protected NumberView mEnteredNumber;
		protected readonly Context mContext;

		private TextView mLabel;
		private NumberPickerErrorTextView mError;
		private int mSign;
		private string mLabelText = "";
		private Button mSetButton;
		private const int CLICKED_DECIMAL = 10;

		private const int SIGN_POSITIVE = 0;
		private const int SIGN_NEGATIVE = 1;

		protected internal View mDivider;
		private ColorStateList mTextColor;
		private int mKeyBackgroundResId;
		private int mButtonBackgroundResId;
		private Color mDividerColor;
		private int mDeleteDrawableSrcResId;
		private int mTheme = -1;

		private int? mMinNumber = null;
		private int? mMaxNumber = null;

		/// <summary>
		/// Instantiates a NumberPicker object
		/// </summary>
		/// <param name="context"> the Context required for creation </param>
		public NumberPicker(Context context)
            : this(context, null)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		/// <summary>
		/// Instantiates a NumberPicker object
		/// </summary>
		/// <param name="context"> the Context required for creation </param>
		/// <param name="attrs"> additional attributes that define custom colors, selectors, and backgrounds. </param>
		public NumberPicker(Context context, IAttributeSet attrs)
            : base(context, attrs)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			mContext = context;
			LayoutInflater layoutInflater = (LayoutInflater) context.GetSystemService(Context.LayoutInflaterService);
			layoutInflater.Inflate(LayoutId, this);

			// Init defaults
			mTextColor = Resources.GetColorStateList(Resource.Color.dialog_text_color_holo_dark);
			mKeyBackgroundResId = Resource.Drawable.key_background_dark;
            mButtonBackgroundResId = Resource.Drawable.button_background_dark;
            mDeleteDrawableSrcResId = Resource.Drawable.ic_backspace_dark;
			mDividerColor = Resources.GetColor(Resource.Color.default_divider_color_dark);
		}

		protected internal virtual int LayoutId
		{
			get
			{
				return Resource.Layout.number_picker_view;
			}
		}

		/// <summary>
		/// Change the theme of the Picker
		/// </summary>
		/// <param name="themeResId"> the resource ID of the new style </param>
		public virtual int Theme
		{
			set
			{
				mTheme = value;
				if (mTheme != -1)
				{
					TypedArray a = Context.ObtainStyledAttributes(value, Resource.Styleable.BetterPickersDialogFragment);

                    mTextColor = a.GetColorStateList(Resource.Styleable.BetterPickersDialogFragment_bpTextColor);
                    mKeyBackgroundResId = a.GetResourceId(Resource.Styleable.BetterPickersDialogFragment_bpKeyBackground, mKeyBackgroundResId);
                    mButtonBackgroundResId = a.GetResourceId(Resource.Styleable.BetterPickersDialogFragment_bpButtonBackground, mButtonBackgroundResId);
                    mDividerColor = a.GetColor(Resource.Styleable.BetterPickersDialogFragment_bpDividerColor, mDividerColor);
                    mDeleteDrawableSrcResId = a.GetResourceId(Resource.Styleable.BetterPickersDialogFragment_bpDeleteIcon, mDeleteDrawableSrcResId);
				}
    
				restyleViews();
			}
		}

		private void restyleViews()
		{
			foreach (Button number in mNumbers)
			{
				if (number != null)
				{
					number.SetTextColor(mTextColor);
					number.SetBackgroundResource(mKeyBackgroundResId);
				}
			}
			if (mDivider != null)
			{
				mDivider.SetBackgroundColor(mDividerColor);
			}
			if (mLeft != null)
			{
                mLeft.SetTextColor(mTextColor);
				mLeft.SetBackgroundResource(mKeyBackgroundResId);
			}
			if (mRight != null)
			{
				mRight.SetTextColor(mTextColor);
				mRight.SetBackgroundResource(mKeyBackgroundResId);
			}
			if (mDelete != null)
			{
				mDelete.SetBackgroundResource(mButtonBackgroundResId);
				mDelete.SetImageDrawable(Resources.GetDrawable(mDeleteDrawableSrcResId));
			}
			if (mEnteredNumber != null)
			{
				mEnteredNumber.Theme = mTheme;
			}
			if (mLabel != null)
			{
				mLabel.SetTextColor(mTextColor);
			}
		}

		protected override void OnFinishInflate()
		{
            base.OnFinishInflate();

			mDivider = FindViewById(Resource.Id.divider);
			mError = (NumberPickerErrorTextView) FindViewById(Resource.Id.error);

			for (int i = 0; i < mInput.Length; i++)
			{
				mInput[i] = -1;
			}

			View v1 = FindViewById(Resource.Id.first);
			View v2 = FindViewById(Resource.Id.second);
			View v3 = FindViewById(Resource.Id.third);
			View v4 = FindViewById(Resource.Id.fourth);
            mEnteredNumber = FindViewById <NumberView>(Resource.Id.number_text);
			mDelete = (ImageButton) FindViewById(Resource.Id.delete);
			mDelete.SetOnClickListener(this);
			mDelete.SetOnLongClickListener(this);

			mNumbers[1] = (Button) v1.FindViewById(Resource.Id.key_left);
			mNumbers[2] = (Button) v1.FindViewById(Resource.Id.key_middle);
			mNumbers[3] = (Button) v1.FindViewById(Resource.Id.key_right);

			mNumbers[4] = (Button) v2.FindViewById(Resource.Id.key_left);
			mNumbers[5] = (Button) v2.FindViewById(Resource.Id.key_middle);
			mNumbers[6] = (Button) v2.FindViewById(Resource.Id.key_right);

			mNumbers[7] = (Button) v3.FindViewById(Resource.Id.key_left);
			mNumbers[8] = (Button) v3.FindViewById(Resource.Id.key_middle);
			mNumbers[9] = (Button) v3.FindViewById(Resource.Id.key_right);

			mLeft = (Button) v4.FindViewById(Resource.Id.key_left);
			mNumbers[0] = (Button) v4.FindViewById(Resource.Id.key_middle);
			mRight = (Button) v4.FindViewById(Resource.Id.key_right);
			setLeftRightEnabled();

			for (int i = 0; i < 10; i++)
			{
                mNumbers[i].SetOnClickListener(this);
				mNumbers[i].Text = string.Format("{0:D}", i);
				mNumbers[i].SetTag(Resource.Id.numbers_key, new int?(i));
			}
			updateNumber();

			Resources res = mContext.Resources;
			mLeft.Text = res.GetString(Resource.String.number_picker_plus_minus);
            mRight.Text = res.GetString(Resource.String.number_picker_seperator);
			mLeft.SetOnClickListener(this);
            mRight.SetOnClickListener(this);
			mLabel = (TextView) FindViewById(Resource.Id.label);
			mSign = SIGN_POSITIVE;

			// Set the correct label state
			showLabel();

			restyleViews();
			updateKeypad();
		}

		/// <summary>
		/// Using View.GONE, View.VISIBILE, or View.INVISIBLE, set the visibility of the plus/minus indicator
		/// </summary>
		/// <param name="visiblity"> an int using Android's View.* convention </param>
		public virtual ViewStates PlusMinusVisibility
		{
			set
			{
				if (mLeft != null)
				{
					mLeft.Visibility = value;
				}
			}
		}

		/// <summary>
		/// Using View.GONE, View.VISIBILE, or View.INVISIBLE, set the visibility of the decimal indicator
		/// </summary>
		/// <param name="visiblity"> an int using Android's View.* convention </param>
        public virtual ViewStates DecimalVisibility
		{
			set
			{
				if (mRight != null)
				{
					mRight.Visibility = value;
				}
			}
		}

		/// <summary>
		/// Set a minimum required number
		/// </summary>
		/// <param name="min"> the minimum required number </param>
		public virtual int Min
		{
			set
			{
				mMinNumber = value;
			}
		}

		/// <summary>
		/// Set a maximum required number
		/// </summary>
		/// <param name="max"> the maximum required number </param>
		public virtual int Max
		{
			set
			{
				mMaxNumber = value;
			}
		}

		/// <summary>
		/// Update the delete button to determine whether it is able to be clicked.
		/// </summary>
		public virtual void updateDeleteButton()
		{
			bool enabled = mInputPointer != -1;
			if (mDelete != null)
			{
				mDelete.Enabled = enabled;
			}
		}

		/// <summary>
		/// Expose the NumberView in order to set errors
		/// </summary>
		/// <returns> the NumberView </returns>
		public virtual NumberPickerErrorTextView ErrorView
		{
			get
			{
				return mError;
			}
		}

		public void OnClick(View v)
		{
			v.PerformHapticFeedback(FeedbackConstants.VirtualKey);
			mError.HideImmediately();
			DoOnClick(v);
			updateDeleteButton();
		}

		protected virtual void DoOnClick(View v)
		{
			var val = (int?) v.GetTag(Resource.Id.numbers_key);
			if (val != null)
			{
				// A number was pressed
				addClickedNumber(val.Value);
			}
			else if (v == mDelete)
			{
				if (mInputPointer >= 0)
				{
					for (int i = 0; i < mInputPointer; i++)
					{
						mInput[i] = mInput[i + 1];
					}
					mInput[mInputPointer] = -1;
					mInputPointer--;
				}
			}
			else if (v == mLeft)
			{
				onLeftClicked();
			}
			else if (v == mRight)
			{
				onRightClicked();
			}
			updateKeypad();
		}

		public bool OnLongClick(View v)
		{
			v.PerformHapticFeedback(FeedbackConstants.LongPress);
			mError.HideImmediately();
			if (v == mDelete)
			{
				mDelete.Pressed = false;
				reset();
				updateKeypad();
				return true;
			}
			return false;
		}

		private void updateKeypad()
		{
			// Update state of keypad
			// Update the number
			updateLeftRightButtons();
			updateNumber();
			// enable/disable the "set" key
			enableSetButton();
			// Update the backspace button
			updateDeleteButton();
		}

		/// <summary>
		/// Set the text displayed in the small label
		/// </summary>
		/// <param name="labelText"> the String to set as the label </param>
		public virtual string LabelText
		{
			set
			{
				mLabelText = value;
				showLabel();
			}
		}

		private void showLabel()
		{
			if (mLabel != null)
			{
				mLabel.Text = mLabelText;
			}
		}

		/// <summary>
		/// Reset all inputs.
		/// </summary>
		public virtual void reset()
		{
			for (int i = 0; i < mInputSize; i++)
			{
				mInput[i] = -1;
			}
			mInputPointer = -1;
			updateNumber();
		}

		// Update the number displayed in the picker:
		protected internal virtual void updateNumber()
		{
			string numberString = EnteredNumberString;
			numberString = numberString.Replace("\\-", "");
			string[] split = numberString.Split("\\.", true);
			if (split.Length >= 2)
			{
				if (split[0].Equals(""))
				{
					mEnteredNumber.setNumber("0", split[1], containsDecimal(), mSign == SIGN_NEGATIVE);
				}
				else
				{
					mEnteredNumber.setNumber(split[0], split[1], containsDecimal(), mSign == SIGN_NEGATIVE);
				}
			}
			else if (split.Length == 1)
			{
				mEnteredNumber.setNumber(split[0], "", containsDecimal(), mSign == SIGN_NEGATIVE);
			}
			else if (numberString.Equals("."))
			{
				mEnteredNumber.setNumber("0", "", true, mSign == SIGN_NEGATIVE);
			}
		}

		protected internal virtual void setLeftRightEnabled()
		{
			mLeft.Enabled = true;
			mRight.Enabled = canAddDecimal();
			if (!canAddDecimal())
			{
				mRight.ContentDescription = null;
			}
		}

		private void addClickedNumber(int val)
		{
			if (mInputPointer < mInputSize - 1)
			{
				// For 0 we need to check if we have a value of zero or not
				if (mInput[0] == 0 && mInput[1] == -1 && !containsDecimal() && val != CLICKED_DECIMAL)
				{
					mInput[0] = val;
				}
				else
				{
					for (int i = mInputPointer; i >= 0; i--)
					{
						mInput[i + 1] = mInput[i];
					}
					mInputPointer++;
					mInput[0] = val;
				}
			}
		}

		/// <summary>
		/// Clicking on the bottom left button will toggle the sign.
		/// </summary>
		private void onLeftClicked()
		{
			if (mSign == SIGN_POSITIVE)
			{
				mSign = SIGN_NEGATIVE;
			}
			else
			{
				mSign = SIGN_POSITIVE;
			}
		}

		/// <summary>
		/// Clicking on the bottom right button will add a decimal point.
		/// </summary>
		private void onRightClicked()
		{
			if (canAddDecimal())
			{
				addClickedNumber(CLICKED_DECIMAL);
			}
		}

		private bool containsDecimal()
		{
			bool containsDecimal_Renamed = false;
			foreach (int i in mInput)
			{
				if (i == 10)
				{
					containsDecimal_Renamed = true;
				}
			}
			return containsDecimal_Renamed;
		}

		/// <summary>
		/// Checks if the user allowed to click on the right button.
		/// </summary>
		/// <returns> true or false if the user is able to add a decimal or not </returns>
		private bool canAddDecimal()
		{
			return !containsDecimal();
		}

		private string EnteredNumberString
		{
			get
			{
				string value = "";
				for (int i = mInputPointer; i >= 0; i--)
				{
					if (mInput[i] == -1)
					{
						// Don't add
					}
					else if (mInput[i] == CLICKED_DECIMAL)
					{
						value += ".";
					}
					else
					{
						value += mInput[i];
					}
				}
				return value;
			}
		}

		/// <summary>
		/// Returns the number inputted by the user
		/// </summary>
		/// <returns> a double representing the entered number </returns>
		public virtual double EnteredNumber
		{
			get
			{
				string value = "0";
				for (int i = mInputPointer; i >= 0; i--)
				{
					if (mInput[i] == -1)
					{
						break;
					}
					else if (mInput[i] == CLICKED_DECIMAL)
					{
						value += ".";
					}
					else
					{
						value += mInput[i];
					}
				}
				if (mSign == SIGN_NEGATIVE)
				{
					value = "-" + value;
				}
				return double.Parse(value);
			}
		}

		private void updateLeftRightButtons()
		{
			mRight.Enabled = canAddDecimal();
		}

		/// <summary>
		/// Enable/disable the "Set" button
		/// </summary>
		private void enableSetButton()
		{
			if (mSetButton == null)
			{
				return;
			}

			// Nothing entered - disable
			if (mInputPointer == -1)
			{
				mSetButton.Enabled = false;
				return;
			}

			// If the user entered 1 digits or more
			mSetButton.Enabled = mInputPointer >= 0;
		}

		/// <summary>
		/// Expose the set button to allow communication with the parent Fragment.
		/// </summary>
		/// <param name="b"> the parent Fragment's "Set" button </param>
		public virtual Button SetButton
		{
			set
			{
				mSetButton = value;
				enableSetButton();
			}
		}

		/// <summary>
		/// Returns the number as currently inputted by the user
		/// </summary>
		/// <returns> an int representation of the number with no decimal </returns>
		public virtual int Number
		{
			get
			{
				string numberString = Convert.ToString(EnteredNumber);
				string[] split = numberString.Split("\\.", true);
				return int.Parse(split[0]);
			}
		}

		/// <summary>
		/// Returns the decimal following the number
		/// </summary>
		/// <returns> a double representation of the decimal value </returns>
		public virtual double Decimal
		{
			get
			{
                // Example: 1000,12 - this returns 0,12
                // TODO: Check is this working
                return Math.Truncate(EnteredNumber);
			}
		}

		/// <summary>
		/// Returns whether the number is positive or negative
		/// </summary>
		/// <returns> true or false whether the number is positive or negative </returns>
		public virtual bool IsNegative
		{
			get
			{
				return mSign == SIGN_NEGATIVE;
			}
		}

        protected override IParcelable OnSaveInstanceState()
		{
			var parcel = base.OnSaveInstanceState();
			var state = new SavedState(parcel);
			state.mInput = mInput;
			state.mSign = mSign;
			state.mInputPointer = mInputPointer;
			return state;
		}

        protected override void OnRestoreInstanceState(IParcelable state)
		{
			if (!(state is SavedState))
			{
				base.OnRestoreInstanceState(state);
				return;
			}
            
			var savedState = (SavedState) state;
            base.OnRestoreInstanceState(savedState.SuperState);

			mInputPointer = savedState.mInputPointer;
			mInput = savedState.mInput;
			if (mInput == null)
			{
				mInput = new int[mInputSize];
				mInputPointer = -1;
			}
			mSign = savedState.mSign;
			updateKeypad();
		}

		private class SavedState : BaseSavedState
		{
			internal int mInputPointer;
			internal int[] mInput;
			internal int mSign;

			public SavedState(IParcelable superState)
                : base(superState) { }

			internal SavedState(Parcel @in) : base(@in)
			{
				mInputPointer = @in.ReadInt();
				@in.ReadIntArray(mInput);
				mSign = @in.ReadInt();
			}

			public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
			{
				base.WriteToParcel(dest, flags);
				dest.WriteInt(mInputPointer);
				dest.WriteIntArray(mInput);
                dest.WriteInt(mSign);
			}

            //public static readonly Creator<SavedState> CREATOR = new CreatorAnonymousInnerClassHelper();

            //private class CreatorAnonymousInnerClassHelper : Creator<SavedState>
            //{
            //    public CreatorAnonymousInnerClassHelper()
            //    {
            //    }

            //    public virtual SavedState createFromParcel(Parcel @in)
            //    {
            //        return new SavedState(@in);
            //    }

            //    public virtual SavedState[] newArray(int size)
            //    {
            //        return new SavedState[size];
            //    }
            //}
		}
	}

}