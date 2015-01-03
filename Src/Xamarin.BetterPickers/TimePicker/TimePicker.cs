using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Text;
using Xamarin.BetterPickers;
using Xamarin.BetterPickers.TimePicker;
using View = Android.Views.View;

namespace com.doomonafireball.betterpickers.timepicker
{
    //using Context = android.content.Context;
    //using ColorStateList = android.content.res.ColorStateList;
    //using Resources = android.content.res.Resources;
    //using TypedArray = android.content.res.TypedArray;
    //using Bundle = android.os.Bundle;
    //using Parcel = android.os.Parcel;
    //using Parcelable = android.os.Parcelable;
    //using IAttributeSet = android.util.IAttributeSet;
    //using HapticFeedbackConstants = android.view.HapticFeedbackConstants;
    //using LayoutInflater = android.view.LayoutInflater;
    //using View = android.view.View;
    //using Button = android.widget.Button;
    //using ImageButton = android.widget.ImageButton;
    //using LinearLayout = android.widget.LinearLayout;
    //using TextView = android.widget.TextView;

    public class TimePicker : LinearLayout, View.IOnClickListener, View.IOnLongClickListener
    {
        private bool InstanceFieldsInitialized;

        private void InitializeInstanceFields()
        {
            mInput = new int[mInputSize];
        }


        protected internal int mInputSize = 4;
        protected internal readonly Button[] mNumbers = new Button[10];
        protected internal int[] mInput;
        protected internal int mInputPointer = -1;
        protected internal Button mLeft, mRight;
        protected internal ImageButton mDelete;
        protected internal TimerView mEnteredTime;
        protected internal readonly Context mContext;

        private TextView mAmPmLabel;
        private string[] mAmpm;
        private readonly string mNoAmPmLabel;
        private int mAmPmState;
        private Button mSetButton;
        private bool mIs24HoursMode = false;

        private const int AMPM_NOT_SELECTED = 0;
        private const int PM_SELECTED = 1;
        private const int AM_SELECTED = 2;
        private const int HOURS24_MODE = 3;

        private const string TIME_PICKER_SAVED_BUFFER_POINTER = "timer_picker_saved_buffer_pointer";
        private const string TIME_PICKER_SAVED_INPUT = "timer_picker_saved_input";
        private const string TIME_PICKER_SAVED_AMPM = "timer_picker_saved_ampm";

        protected internal View mDivider;
        private ColorStateList mTextColor;
        private int mKeyBackgroundResId;
        private int mButtonBackgroundResId;
        private Color mDividerColor;
        private int mDeleteDrawableSrcResId;
        private int mTheme = -1;

        /// <summary>
        /// Instantiates a TimePicker object
        /// </summary>
        /// <param name="context"> the Context required for creation </param>
        public TimePicker(Context context)
            : this(context, null)
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }
        }

        /// <summary>
        /// Instantiates a TimePicker object
        /// </summary>
        /// <param name="context"> the Context required for creation </param>
        /// <param name="attrs"> additional attributes that define custom colors, selectors, and backgrounds. </param>
        public TimePicker(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }
            mContext = context;
            mIs24HoursMode = get24HourMode(mContext);
            LayoutInflater layoutInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            layoutInflater.Inflate(LayoutId, this);
            mNoAmPmLabel = context.Resources.GetString(Resource.String.time_picker_ampm_label);

            // Init defaults
            mTextColor = Resources.GetColorStateList(Resource.Color.dialog_text_color_holo_dark);
            mKeyBackgroundResId = Resource.Drawable.key_background_dark;
            mButtonBackgroundResId = Resource.Drawable.button_background_dark;
            mDividerColor = Resources.GetColor(Resource.Color.default_divider_color_dark);
            mDeleteDrawableSrcResId = Resource.Drawable.ic_backspace_dark;
        }

        protected virtual int LayoutId
        {
            get
            {
                return Resource.Layout.time_picker_view;
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
            if (mAmPmLabel != null)
            {
                mAmPmLabel.SetTextColor(mTextColor);
                mAmPmLabel.SetBackgroundResource(mKeyBackgroundResId);
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
            if (mEnteredTime != null)
            {
                mEnteredTime.Theme = mTheme;
            }
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();

            View v1 = FindViewById(Resource.Id.first);
            View v2 = FindViewById(Resource.Id.second);
            View v3 = FindViewById(Resource.Id.third);
            View v4 = FindViewById(Resource.Id.fourth);
            mEnteredTime = (TimerView)FindViewById(Resource.Id.timer_time_text);
            mDelete = (ImageButton)FindViewById(Resource.Id.delete);
            mDelete.SetOnClickListener(this);
            mDelete.SetOnLongClickListener(this);

            mNumbers[1] = (Button)v1.FindViewById(Resource.Id.key_left);
            mNumbers[2] = (Button)v1.FindViewById(Resource.Id.key_middle);
            mNumbers[3] = (Button)v1.FindViewById(Resource.Id.key_right);

            mNumbers[4] = (Button)v2.FindViewById(Resource.Id.key_left);
            mNumbers[5] = (Button)v2.FindViewById(Resource.Id.key_middle);
            mNumbers[6] = (Button)v2.FindViewById(Resource.Id.key_right);

            mNumbers[7] = (Button)v3.FindViewById(Resource.Id.key_left);
            mNumbers[8] = (Button)v3.FindViewById(Resource.Id.key_middle);
            mNumbers[9] = (Button)v3.FindViewById(Resource.Id.key_right);

            mLeft = (Button)v4.FindViewById(Resource.Id.key_left);
            mNumbers[0] = (Button)v4.FindViewById(Resource.Id.key_middle);
            mRight = (Button)v4.FindViewById(Resource.Id.key_right);
            LeftRightEnabled = false;

            for (int i = 0; i < 10; i++)
            {
                mNumbers[i].SetOnClickListener(this);
                mNumbers[i].Text = string.Format("{0:D}", i);
                mNumbers[i].SetTag(Resource.Id.numbers_key, i);
            }
            updateTime();

            Resources res = mContext.Resources;
            mAmpm = (new DateFormatSymbols()).GetAmPmStrings();

            if (mIs24HoursMode)
            {
                mLeft.Text = res.GetString(Resource.String.time_picker_00_label);
                mRight.Text = res.GetString(Resource.String.time_picker_30_label);
            }
            else
            {
                mLeft.Text = mAmpm[0];
                mRight.Text = mAmpm[1];
            }
            mLeft.SetOnClickListener(this);
            mRight.SetOnClickListener(this);
            mAmPmLabel = (TextView)FindViewById(Resource.Id.ampm_label);
            mAmPmState = AMPM_NOT_SELECTED;
            mDivider = FindViewById(Resource.Id.divider);

            restyleViews();
            updateKeypad();
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

        public void OnClick(View v)
        {
            v.PerformHapticFeedback(FeedbackConstants.VirtualKey);
            doOnClick(v);
            updateDeleteButton();
        }

        protected internal virtual void doOnClick(View v)
        {
            var val = (int?)v.GetTag(Resource.Id.numbers_key);
            // A number was pressed
            if (val != null)
            {
                addClickedNumber(val.Value);
            }
            else if (v == mDelete)
            {
                // Pressing delete when AM or PM is selected, clears the AM/PM
                // selection
                if (!mIs24HoursMode && mAmPmState != AMPM_NOT_SELECTED)
                {
                    mAmPmState = AMPM_NOT_SELECTED;
                }
                else if (mInputPointer >= 0)
                {
                    for (int i = 0; i < mInputPointer; i++)
                    {
                        mInput[i] = mInput[i + 1];
                    }
                    mInput[mInputPointer] = 0;
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
            if (v == mDelete)
            {
                mDelete.Pressed = false;

                mAmPmState = AMPM_NOT_SELECTED;
                reset();
                updateKeypad();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reset all inputs .
        /// </summary>
        public virtual void reset()
        {
            for (int i = 0; i < mInputSize; i++)
            {
                mInput[i] = 0;
            }
            mInputPointer = -1;
            updateTime();
        }

        private void updateKeypad()
        {
            // Update state of keypad
            // Set the correct AM/PM state
            showAmPm();
            // Update the time
            updateLeftRightButtons();
            updateTime();
            // enable/disable numeric keys according to the numbers entered already
            updateNumericKeys();
            // enable/disable the "set" key
            enableSetButton();
            // Update the backspace button
            updateDeleteButton();

        }

        /// <summary>
        /// Update the time displayed in the picker:
        /// 
        /// Special cases:
        /// 
        /// 1. show "-" for digits not entered yet.
        /// 
        /// 2. hide the hours digits when it is not relevant
        /// </summary>
        protected internal virtual void updateTime()
        {
            // Put "-" in digits that was not entered by passing -1
            // Hide digit by passing -2 (for highest hours digit only);

            int hours1 = -1;
            int time = EnteredTime;
            // If the user entered 2 to 9 or 13 to 15 , there is no need for a 4th digit (AM/PM mode)
            // If the user entered 3 to 9 or 24 to 25 , there is no need for a 4th digit (24 hours mode)
            if (mInputPointer > -1)
            {
                // Test to see if the highest digit is 2 to 9 for AM/PM or 3 to 9 for 24 hours mode
                if (mInputPointer >= 0)
                {
                    int digit = mInput[mInputPointer];
                    if ((mIs24HoursMode && digit >= 3 && digit <= 9) || (!mIs24HoursMode && digit >= 2 && digit <= 9))
                    {
                        hours1 = -2;
                    }
                }
                // Test to see if the 2 highest digits are 13 to 15 for AM/PM or 24 to 25 for 24 hours
                // mode
                if (mInputPointer > 0 && mInputPointer < 3 && hours1 != -2)
                {
                    int digits = mInput[mInputPointer] * 10 + mInput[mInputPointer - 1];
                    if ((mIs24HoursMode && digits >= 24 && digits <= 25) || (!mIs24HoursMode && digits >= 13 && digits <= 15))
                    {
                        hours1 = -2;
                    }
                }
                // If we have a digit show it
                if (mInputPointer == 3)
                {
                    hours1 = mInput[3];
                }
            }
            else
            {
                hours1 = -1;
            }
            int hours2 = (mInputPointer < 2) ? -1 : mInput[2];
            int minutes1 = (mInputPointer < 1) ? -1 : mInput[1];
            int minutes2 = (mInputPointer < 0) ? -1 : mInput[0];
            mEnteredTime.setTime(hours1, hours2, minutes1, minutes2);
        }

        private void showAmPm()
        {
            if (!mIs24HoursMode)
            {
                switch (mAmPmState)
                {
                    case AMPM_NOT_SELECTED:
                        mAmPmLabel.Text = mNoAmPmLabel;
                        break;
                    case AM_SELECTED:
                        mAmPmLabel.Text = mAmpm[0];
                        break;
                    case PM_SELECTED:
                        mAmPmLabel.Text = mAmpm[1];
                        break;
                    default:
                        break;
                }
            }
            else
            {
                mAmPmLabel.Visibility = ViewStates.Invisible;
                mAmPmState = HOURS24_MODE;
            }
        }

        private void addClickedNumber(int val)
        {
            if (mInputPointer < mInputSize - 1)
            {
                for (int i = mInputPointer; i >= 0; i--)
                {
                    mInput[i + 1] = mInput[i];
                }
                mInputPointer++;
                mInput[0] = val;
            }
        }

        /// <summary>
        /// Clicking on the bottom left button will add "00" to the time
        /// 
        /// In AM/PM mode is will also set the time to AM.
        /// </summary>
        private void onLeftClicked()
        {
            int time = EnteredTime;
            if (!mIs24HoursMode)
            {
                if (canAddDigits())
                {
                    addClickedNumber(0);
                    addClickedNumber(0);
                }
                mAmPmState = AM_SELECTED;
            }
            else if (canAddDigits())
            {
                addClickedNumber(0);
                addClickedNumber(0);
            }
        }

        /// <summary>
        /// Clicking on the bottom right button will add "00" to the time in AM/PM mode and "30" is 24 hours mode.
        /// 
        /// In AM/PM mode is will also set the time to PM.
        /// </summary>
        private void onRightClicked()
        {
            int time = EnteredTime;
            if (!mIs24HoursMode)
            {
                if (canAddDigits())
                {
                    addClickedNumber(0);
                    addClickedNumber(0);
                }
                mAmPmState = PM_SELECTED;
            }
            else
            {
                if (canAddDigits())
                {
                    addClickedNumber(3);
                    addClickedNumber(0);
                }
            }
        }

        /// <summary>
        /// Checks if the user allowed to click on the left or right button that enters "00" or "30"
        /// </summary>
        /// <returns> true or false whether a user is allowed to click on the left or right </returns>
        private bool canAddDigits()
        {
            int time = EnteredTime;
            // For AM/PM mode , can add "00" if an hour between 1 and 12 was entered
            if (!mIs24HoursMode)
            {
                return (time >= 1 && time <= 12);
            }
            // For 24 hours mode , can add "00"/"30" if an hour between 0 and 23 was entered
            return (time >= 0 && time <= 23 && mInputPointer > -1 && mInputPointer < 2);
        }

        /// <summary>
        /// Enable/disable keys in the numeric key pad according to the data entered
        /// </summary>
        private void updateNumericKeys()
        {
            int time = EnteredTime;
            if (mIs24HoursMode)
            {
                if (mInputPointer >= 3)
                {
                    KeyRange = -1;
                }
                else if (time == 0)
                {
                    if (mInputPointer == -1 || mInputPointer == 0 || mInputPointer == 2)
                    {
                        KeyRange = 9;
                    }
                    else if (mInputPointer == 1)
                    {
                        KeyRange = 5;
                    }
                    else
                    {
                        KeyRange = -1;
                    }
                }
                else if (time == 1)
                {
                    if (mInputPointer == 0 || mInputPointer == 2)
                    {
                        KeyRange = 9;
                    }
                    else if (mInputPointer == 1)
                    {
                        KeyRange = 5;
                    }
                    else
                    {
                        KeyRange = -1;
                    }
                }
                else if (time == 2)
                {
                    if (mInputPointer == 2 || mInputPointer == 1)
                    {
                        KeyRange = 9;
                    }
                    else if (mInputPointer == 0)
                    {
                        KeyRange = 3;
                    }
                    else
                    {
                        KeyRange = -1;
                    }
                }
                else if (time <= 5)
                {
                    KeyRange = 9;
                }
                else if (time <= 9)
                {
                    KeyRange = 5;
                }
                else if (time >= 10 && time <= 15)
                {
                    KeyRange = 9;
                }
                else if (time >= 16 && time <= 19)
                {
                    KeyRange = 5;
                }
                else if (time >= 20 && time <= 25)
                {
                    KeyRange = 9;
                }
                else if (time >= 26 && time <= 29)
                {
                    KeyRange = -1;
                }
                else if (time >= 30 && time <= 35)
                {
                    KeyRange = 9;
                }
                else if (time >= 36 && time <= 39)
                {
                    KeyRange = -1;
                }
                else if (time >= 40 && time <= 45)
                {
                    KeyRange = 9;
                }
                else if (time >= 46 && time <= 49)
                {
                    KeyRange = -1;
                }
                else if (time >= 50 && time <= 55)
                {
                    KeyRange = 9;
                }
                else if (time >= 56 && time <= 59)
                {
                    KeyRange = -1;
                }
                else if (time >= 60 && time <= 65)
                {
                    KeyRange = 9;
                }
                else if (time >= 70 && time <= 75)
                {
                    KeyRange = 9;
                }
                else if (time >= 80 && time <= 85)
                {
                    KeyRange = 9;
                }
                else if (time >= 90 && time <= 95)
                {
                    KeyRange = 9;
                }
                else if (time >= 100 && time <= 105)
                {
                    KeyRange = 9;
                }
                else if (time >= 106 && time <= 109)
                {
                    KeyRange = -1;
                }
                else if (time >= 110 && time <= 115)
                {
                    KeyRange = 9;
                }
                else if (time >= 116 && time <= 119)
                {
                    KeyRange = -1;
                }
                else if (time >= 120 && time <= 125)
                {
                    KeyRange = 9;
                }
                else if (time >= 126 && time <= 129)
                {
                    KeyRange = -1;
                }
                else if (time >= 130 && time <= 135)
                {
                    KeyRange = 9;
                }
                else if (time >= 136 && time <= 139)
                {
                    KeyRange = -1;
                }
                else if (time >= 140 && time <= 145)
                {
                    KeyRange = 9;
                }
                else if (time >= 146 && time <= 149)
                {
                    KeyRange = -1;
                }
                else if (time >= 150 && time <= 155)
                {
                    KeyRange = 9;
                }
                else if (time >= 156 && time <= 159)
                {
                    KeyRange = -1;
                }
                else if (time >= 160 && time <= 165)
                {
                    KeyRange = 9;
                }
                else if (time >= 166 && time <= 169)
                {
                    KeyRange = -1;
                }
                else if (time >= 170 && time <= 175)
                {
                    KeyRange = 9;
                }
                else if (time >= 176 && time <= 179)
                {
                    KeyRange = -1;
                }
                else if (time >= 180 && time <= 185)
                {
                    KeyRange = 9;
                }
                else if (time >= 186 && time <= 189)
                {
                    KeyRange = -1;
                }
                else if (time >= 190 && time <= 195)
                {
                    KeyRange = 9;
                }
                else if (time >= 196 && time <= 199)
                {
                    KeyRange = -1;
                }
                else if (time >= 200 && time <= 205)
                {
                    KeyRange = 9;
                }
                else if (time >= 206 && time <= 209)
                {
                    KeyRange = -1;
                }
                else if (time >= 210 && time <= 215)
                {
                    KeyRange = 9;
                }
                else if (time >= 216 && time <= 219)
                {
                    KeyRange = -1;
                }
                else if (time >= 220 && time <= 225)
                {
                    KeyRange = 9;
                }
                else if (time >= 226 && time <= 229)
                {
                    KeyRange = -1;
                }
                else if (time >= 230 && time <= 235)
                {
                    KeyRange = 9;
                }
                else if (time >= 236)
                {
                    KeyRange = -1;
                }
            }
            else
            {
                // Selecting AM/PM disabled the keypad
                if (mAmPmState != AMPM_NOT_SELECTED)
                {
                    KeyRange = -1;
                }
                else if (time == 0)
                {
                    KeyRange = 9;
                    // If 0 was entered as the first digit in AM/PM mode, do not allow a second 0
                    //        if (mInputPointer == 0) {
                    mNumbers[0].Enabled = false;
                    //      }
                }
                else if (time <= 9)
                {
                    KeyRange = 5;
                }
                else if (time <= 95)
                {
                    KeyRange = 9;
                }
                else if (time >= 100 && time <= 105)
                {
                    KeyRange = 9;
                }
                else if (time >= 106 && time <= 109)
                {
                    KeyRange = -1;
                }
                else if (time >= 110 && time <= 115)
                {
                    KeyRange = 9;
                }
                else if (time >= 116 && time <= 119)
                {
                    KeyRange = -1;
                }
                else if (time >= 120 && time <= 125)
                {
                    KeyRange = 9;
                }
                else if (time >= 126)
                {
                    KeyRange = -1;
                }
            }
        }

        /// <summary>
        /// Returns the time already entered in decimal representation. if time is H1 H2 : M1 M2 the value retured is
        /// H1*1000+H2*100+M1*10+M2
        /// </summary>
        /// <returns> the time already entered in decimal representation </returns>
        private int EnteredTime
        {
            get
            {
                return mInput[3] * 1000 + mInput[2] * 100 + mInput[1] * 10 + mInput[0];
            }
        }

        /// <summary>
        /// enables a range of numeric keys from zero to maxKey. The rest of the keys will be disabled
        /// </summary>
        /// <param name="maxKey"> the maximum key that can be pressed </param>
        private int KeyRange
        {
            set
            {
                for (int i = 0; i < mNumbers.Length; i++)
                {
                    mNumbers[i].Enabled = i <= value;
                }
            }
        }

        private void updateLeftRightButtons()
        {
            int time = EnteredTime;
            if (mIs24HoursMode)
            {
                bool enable = canAddDigits();
                mLeft.Enabled = enable;
                mRight.Enabled = enable;
            }
            else
            {
                // You can use the AM/PM if time entered is 0 to 12 or it is 3 digits or more
                if ((time > 12 && time < 100) || time == 0 || mAmPmState != AMPM_NOT_SELECTED)
                {
                    mLeft.Enabled = false;
                    mRight.Enabled = false;
                }
                else
                {
                    mLeft.Enabled = true;
                    mRight.Enabled = true;
                }
            }
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
            // If the user entered 3 digits or more but not 060 to 095
            // it is a legal time and the set key should be enabled.
            if (mIs24HoursMode)
            {
                int time = EnteredTime;
                mSetButton.Enabled = mInputPointer >= 2 && (time < 60 || time > 95);
            }
            else
            {
                // If AM/PM mode , enable the set button if AM/PM was selected
                mSetButton.Enabled = mAmPmState != AMPM_NOT_SELECTED;
            }
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
        /// Get the hours as currently inputted by the user.
        /// </summary>
        /// <returns> the inputted hours </returns>
        public virtual int Hours
        {
            get
            {
                int hours = mInput[3] * 10 + mInput[2];
                if (hours == 12)
                {
                    switch (mAmPmState)
                    {
                        case PM_SELECTED:
                            return 12;
                        case AM_SELECTED:
                            return 0;
                        case HOURS24_MODE:
                            return hours;
                        default:
                            break;
                    }
                }
                return hours + (mAmPmState == PM_SELECTED ? 12 : 0);
            }
        }

        /// <summary>
        /// Get the minutes as currently inputted by the user
        /// </summary>
        /// <returns> the inputted minutes </returns>
        public virtual int Minutes
        {
            get
            {
                return mInput[1] * 10 + mInput[0];
            }
        }

        protected override IParcelable OnSaveInstanceState()
        {
            var parcel = base.OnSaveInstanceState();
            var state = new SavedState(parcel);
            state.mInput = mInput;
            state.mAmPmState = mAmPmState;
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

            var savedState = (SavedState)state;
            base.OnRestoreInstanceState(savedState.SuperState);

            mInputPointer = savedState.mInputPointer;
            mInput = savedState.mInput;
            if (mInput == null)
            {
                mInput = new int[mInputSize];
                mInputPointer = -1;
            }
            mAmPmState = savedState.mAmPmState;
            updateKeypad();
        }

        private class SavedState : BaseSavedState
        {

            internal int mInputPointer;
            internal int[] mInput;
            internal int mAmPmState;

            public SavedState(IParcelable superState)
                : base(superState)
            {
            }

            internal SavedState(Parcel @in)
                : base(@in)
            {
                mInputPointer = @in.ReadInt();
                @in.ReadIntArray(mInput);
                mAmPmState = @in.ReadInt();
            }

            public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
            {
                base.WriteToParcel(dest, flags);
                dest.WriteInt(mInputPointer);
                dest.WriteIntArray(mInput);
                dest.WriteInt(mAmPmState);
            }

            //public static readonly Parcelable.Creator<SavedState> CREATOR = new CreatorAnonymousInnerClassHelper();

            //private class CreatorAnonymousInnerClassHelper : Parcelable.Creator<SavedState>
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

        /// <summary>
        /// Return whether it is currently 24-hour mode on the system
        /// </summary>
        /// <param name="context"> a required Context </param>
        /// <returns> true or false whether it is 24-hour mode or not </returns>
        public static bool get24HourMode(Context context)
        {
            return Android.Text.Format.DateFormat.Is24HourFormat(context);
        }

        /// <summary>
        /// Get the time currently inputted by the user
        /// </summary>
        /// <returns> an int representing the current time </returns>
        public virtual int Time
        {
            get
            {
                return mInput[4] * 3600 + mInput[3] * 600 + mInput[2] * 60 + mInput[1] * 10 + mInput[0];
            }
        }

        public virtual void saveEntryState(Bundle outState, string key)
        {
            outState.PutIntArray(key, mInput);
        }

        public virtual void restoreEntryState(Bundle inState, string key)
        {
            int[] input = inState.GetIntArray(key);
            if (input != null && mInputSize == input.Length)
            {
                for (int i = 0; i < mInputSize; i++)
                {
                    mInput[i] = input[i];
                    if (mInput[i] != 0)
                    {
                        mInputPointer = i;
                    }
                }
                updateTime();
            }
        }

        protected virtual bool LeftRightEnabled
        {
            set
            {
                mLeft.Enabled = value;
                mRight.Enabled = value;
                if (!value)
                {
                    mLeft.ContentDescription = null;
                    mRight.ContentDescription = null;
                }
            }
        }
    }

}