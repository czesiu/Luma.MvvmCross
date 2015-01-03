using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.BetterPickers;
using Java.Lang;
using Java.Text;
using Java.Util;
using DateFormat = Android.Text.Format.DateFormat;

namespace Xamarin.BetterPickers.DatePicker
{
    //using Context = android.content.Context;
    //using ColorStateList = android.content.res.ColorStateList;
    //using Resources = android.content.res.Resources;
    //using TypedArray = android.content.res.TypedArray;
    //using Parcel = android.os.Parcel;
    //using Parcelable = android.os.Parcelable;
    //using PagerAdapter = android.support.v4.view.PagerAdapter;
    //using ViewPager = android.support.v4.view.ViewPager;
    //using DateFormat = android.text.format.DateFormat;
    //using IAttributeSet = android.util.IAttributeSet;
    //using HapticFeedbackConstants = android.view.HapticFeedbackConstants;
    //using LayoutInflater = android.view.LayoutInflater;
    //using View = android.view.View;
    //using ViewGroup = android.view.ViewGroup;
    //using Button = android.widget.Button;
    //using ImageButton = android.widget.ImageButton;
    //using LinearLayout = android.widget.LinearLayout;
    //using TextView = android.widget.TextView;



    public class DatePicker : LinearLayout, View.IOnClickListener, View.IOnLongClickListener
    {
        private bool InstanceFieldsInitialized;

        private void InitializeInstanceFields()
        {
            mDateInput = new int[mDateInputSize];
            mYearInput = new int[mYearInputSize];
        }


        protected int mDateInputSize = 2;
        protected int mYearInputSize = 4;
        protected int mMonthInput = -1;
        protected int[] mDateInput;
        protected int[] mYearInput;
        protected int mDateInputPointer = -1;
        protected int mYearInputPointer = -1;
        protected readonly Button[] mMonths = new Button[12];
        protected readonly Button[] mDateNumbers = new Button[10];
        protected readonly Button[] mYearNumbers = new Button[10];
        protected Button mDateLeft;
        protected Button mYearLeft, mYearRight;
        protected ImageButton mDateRight;
        protected UnderlinePageIndicatorPicker mKeyboardIndicator;
        protected ViewPager mKeyboardPager;
        protected KeyboardPagerAdapter mKeyboardPagerAdapter;
        protected ImageButton mDelete;
        protected DateView mEnteredDate;
        protected string[] mMonthAbbreviations;
        protected readonly Context mContext;
        private char[] mDateFormatOrder;

        private const string KEYBOARD_MONTH = "month";
        private const string KEYBOARD_DATE = "date";
        private const string KEYBOARD_YEAR = "year";

        private static int sMonthKeyboardPosition = -1;
        private static int sDateKeyboardPosition = -1;
        private static int sYearKeyboardPosition = -1;

        private Button mSetButton;

        protected internal View mDivider;
        private ColorStateList mTextColor;
        private int mKeyBackgroundResId;
        private int mButtonBackgroundResId;
        private Color mTitleDividerColor;
        private Color mKeyboardIndicatorColor;
        private int mCheckDrawableSrcResId;
        private int mDeleteDrawableSrcResId;
        private int mTheme = -1;

        /// <summary>
        /// Instantiates a DatePicker object
        /// </summary>
        /// <param name="context"> the Context required for creation </param>
        public DatePicker(Context context)
            : this(context, null)
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }
        }

        /// <summary>
        /// Instantiates a DatePicker object
        /// </summary>
        /// <param name="context"> the Context required for creation </param>
        /// <param name="attrs"> additional attributes that define custom colors, selectors, and backgrounds. </param>
        public DatePicker(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }
            mContext = context;
            mDateFormatOrder = DateFormat.GetDateFormatOrder(mContext);
            mMonthAbbreviations = makeLocalizedMonthAbbreviations();
            LayoutInflater layoutInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            layoutInflater.Inflate(LayoutId, this);

            // Init defaults
            mTextColor = Resources.GetColorStateList(Resource.Color.dialog_text_color_holo_dark);
            mKeyBackgroundResId = Resource.Drawable.key_background_dark;
            mButtonBackgroundResId = Resource.Drawable.button_background_dark;
            mTitleDividerColor = Resources.GetColor(Resource.Color.default_divider_color_dark);
            mKeyboardIndicatorColor = Resources.GetColor(Resource.Color.default_keyboard_indicator_color_dark);
            mDeleteDrawableSrcResId = Resource.Drawable.ic_backspace_dark;
            mCheckDrawableSrcResId = Resource.Drawable.ic_check_dark;
        }

        protected internal virtual int LayoutId
        {
            get
            {
                return Resource.Layout.date_picker_view;
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
                    var a = Context.ObtainStyledAttributes(value, Resource.Styleable.BetterPickersDialogFragment);

                    mTextColor = a.GetColorStateList(Resource.Styleable.BetterPickersDialogFragment_bpTextColor);
                    mKeyBackgroundResId = a.GetResourceId(Resource.Styleable.BetterPickersDialogFragment_bpKeyBackground, mKeyBackgroundResId);
                    mButtonBackgroundResId = a.GetResourceId(Resource.Styleable.BetterPickersDialogFragment_bpButtonBackground, mButtonBackgroundResId);
                    mCheckDrawableSrcResId = a.GetResourceId(Resource.Styleable.BetterPickersDialogFragment_bpCheckIcon, mCheckDrawableSrcResId);
                    mTitleDividerColor = a.GetColor(Resource.Styleable.BetterPickersDialogFragment_bpTitleDividerColor, mTitleDividerColor);
                    mKeyboardIndicatorColor = a.GetColor(Resource.Styleable.BetterPickersDialogFragment_bpKeyboardIndicatorColor, mKeyboardIndicatorColor);
                    mDeleteDrawableSrcResId = a.GetResourceId(Resource.Styleable.BetterPickersDialogFragment_bpDeleteIcon, mDeleteDrawableSrcResId);
                }

                restyleViews();
            }
        }

        private void restyleViews()
        {
            foreach (Button month in mMonths)
            {
                if (month != null)
                {
                    month.SetTextColor(mTextColor);
                    month.SetBackgroundResource(mKeyBackgroundResId);
                }
            }
            foreach (Button dateNumber in mDateNumbers)
            {
                if (dateNumber != null)
                {
                    dateNumber.SetTextColor(mTextColor);
                    dateNumber.SetBackgroundResource(mKeyBackgroundResId);
                }
            }
            foreach (Button yearNumber in mYearNumbers)
            {
                if (yearNumber != null)
                {
                    yearNumber.SetTextColor(mTextColor);
                    yearNumber.SetBackgroundResource(mKeyBackgroundResId);
                }
            }
            if (mKeyboardIndicator != null)
            {
                mKeyboardIndicator.SelectedColor = mKeyboardIndicatorColor;
            }
            if (mDivider != null)
            {
                mDivider.SetBackgroundColor(mTitleDividerColor);
            }
            if (mDateLeft != null)
            {
                mDateLeft.SetTextColor(mTextColor);
                mDateLeft.SetBackgroundResource(mKeyBackgroundResId);
            }
            if (mDateRight != null)
            {
                mDateRight.SetBackgroundResource(mKeyBackgroundResId);
                mDateRight.SetImageDrawable(Resources.GetDrawable(mCheckDrawableSrcResId));
            }
            if (mDelete != null)
            {
                mDelete.SetBackgroundResource(mButtonBackgroundResId);
                mDelete.SetImageDrawable(Resources.GetDrawable(mDeleteDrawableSrcResId));
            }
            if (mYearLeft != null)
            {
                mYearLeft.SetTextColor(mTextColor);
                mYearLeft.SetBackgroundResource(mKeyBackgroundResId);
            }
            if (mYearRight != null)
            {
                mYearRight.SetTextColor(mTextColor);
                mYearRight.SetBackgroundResource(mKeyBackgroundResId);
            }
            if (mEnteredDate != null)
            {
                mEnteredDate.Theme = mTheme;
            }
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();

            mDivider = FindViewById(Resource.Id.divider);

            for (int i = 0; i < mDateInput.Length; i++)
            {
                mDateInput[i] = 0;
            }
            for (int i = 0; i < mYearInput.Length; i++)
            {
                mYearInput[i] = 0;
            }

            mKeyboardIndicator = FindViewById<UnderlinePageIndicatorPicker>(Resource.Id.keyboard_indicator);
            mKeyboardPager = FindViewById<ViewPager>(Resource.Id.keyboard_pager);
            mKeyboardPager.OffscreenPageLimit = 2;
            mKeyboardPagerAdapter = new KeyboardPagerAdapter(this, (LayoutInflater)mContext.GetSystemService(Context.LayoutInflaterService));
            mKeyboardPager.Adapter = mKeyboardPagerAdapter;
            mKeyboardIndicator.ViewPager = mKeyboardPager;
            mKeyboardPager.CurrentItem = 0;

            mEnteredDate = (DateView)FindViewById(Resource.Id.date_text);
            mEnteredDate.Theme = mTheme;
            mEnteredDate.UnderlinePage = mKeyboardIndicator;
            mEnteredDate.OnClick = this;

            mDelete = (ImageButton)FindViewById(Resource.Id.delete);
            mDelete.SetOnClickListener(this);
            mDelete.SetOnLongClickListener(this);

            setLeftRightEnabled();
            updateDate();
            updateKeypad();
        }

        public class KeyboardPagerAdapter : PagerAdapter
        {
            private readonly DatePicker outerInstance;


            internal LayoutInflater mInflater;

            public KeyboardPagerAdapter(DatePicker outerInstance, LayoutInflater inflater)
            {
                this.outerInstance = outerInstance;
                mInflater = inflater;
            }

            /// <summary>
            /// Based on the Locale, inflate the day, month, or year keyboard
            /// </summary>
            /// <param name="collection"> the ViewPager collection group </param>
            /// <param name="position"> the position within the ViewPager </param>
            /// <returns> an inflated View representing the keyboard for this position </returns>
            public virtual object instantiateItem(ViewGroup collection, int position)
            {
                View view;
                Resources res = outerInstance.mContext.Resources;
                if (outerInstance.mDateFormatOrder[position] == DateFormat.Month)
                {
                    // Months
                    sMonthKeyboardPosition = position;
                    view = mInflater.Inflate(Resource.Layout.keyboard_text_with_header, null);
                    View v1 = view.FindViewById(Resource.Id.first);
                    View v2 = view.FindViewById(Resource.Id.second);
                    View v3 = view.FindViewById(Resource.Id.third);
                    View v4 = view.FindViewById(Resource.Id.fourth);
                    var header = (TextView)view.FindViewById(Resource.Id.header);

                    header.SetText(Resource.String.month_c);

                    outerInstance.mMonths[0] = (Button)v1.FindViewById(Resource.Id.key_left);
                    outerInstance.mMonths[1] = (Button)v1.FindViewById(Resource.Id.key_middle);
                    outerInstance.mMonths[2] = (Button)v1.FindViewById(Resource.Id.key_right);

                    outerInstance.mMonths[3] = (Button)v2.FindViewById(Resource.Id.key_left);
                    outerInstance.mMonths[4] = (Button)v2.FindViewById(Resource.Id.key_middle);
                    outerInstance.mMonths[5] = (Button)v2.FindViewById(Resource.Id.key_right);

                    outerInstance.mMonths[6] = (Button)v3.FindViewById(Resource.Id.key_left);
                    outerInstance.mMonths[7] = (Button)v3.FindViewById(Resource.Id.key_middle);
                    outerInstance.mMonths[8] = (Button)v3.FindViewById(Resource.Id.key_right);

                    outerInstance.mMonths[9] = (Button)v4.FindViewById(Resource.Id.key_left);
                    outerInstance.mMonths[10] = (Button)v4.FindViewById(Resource.Id.key_middle);
                    outerInstance.mMonths[11] = (Button)v4.FindViewById(Resource.Id.key_right);

                    for (int i = 0; i < 12; i++)
                    {
                        outerInstance.mMonths[i].SetOnClickListener(outerInstance);
                        outerInstance.mMonths[i].Text = outerInstance.mMonthAbbreviations[i];
                        outerInstance.mMonths[i].SetTextColor(outerInstance.mTextColor);
                        outerInstance.mMonths[i].SetBackgroundResource(outerInstance.mKeyBackgroundResId);
                        outerInstance.mMonths[i].SetTag(Resource.Id.date_keyboard, KEYBOARD_MONTH);
                        outerInstance.mMonths[i].SetTag(Resource.Id.date_month_int, i);
                    }
                }
                else if (outerInstance.mDateFormatOrder[position] == DateFormat.Date)
                {
                    // Date
                    sDateKeyboardPosition = position;
                    view = mInflater.Inflate(Resource.Layout.keyboard_right_drawable_with_header, null);
                    View v1 = view.FindViewById(Resource.Id.first);
                    View v2 = view.FindViewById(Resource.Id.second);
                    View v3 = view.FindViewById(Resource.Id.third);
                    View v4 = view.FindViewById(Resource.Id.fourth);
                    TextView header = (TextView)view.FindViewById(Resource.Id.header);

                    header.SetText(Resource.String.day_c);

                    outerInstance.mDateNumbers[1] = (Button)v1.FindViewById(Resource.Id.key_left);
                    outerInstance.mDateNumbers[2] = (Button)v1.FindViewById(Resource.Id.key_middle);
                    outerInstance.mDateNumbers[3] = (Button)v1.FindViewById(Resource.Id.key_right);

                    outerInstance.mDateNumbers[4] = (Button)v2.FindViewById(Resource.Id.key_left);
                    outerInstance.mDateNumbers[5] = (Button)v2.FindViewById(Resource.Id.key_middle);
                    outerInstance.mDateNumbers[6] = (Button)v2.FindViewById(Resource.Id.key_right);

                    outerInstance.mDateNumbers[7] = (Button)v3.FindViewById(Resource.Id.key_left);
                    outerInstance.mDateNumbers[8] = (Button)v3.FindViewById(Resource.Id.key_middle);
                    outerInstance.mDateNumbers[9] = (Button)v3.FindViewById(Resource.Id.key_right);

                    outerInstance.mDateLeft = (Button)v4.FindViewById(Resource.Id.key_left);
                    outerInstance.mDateLeft.SetTextColor(outerInstance.mTextColor);
                    outerInstance.mDateLeft.SetBackgroundResource(outerInstance.mKeyBackgroundResId);
                    outerInstance.mDateNumbers[0] = (Button)v4.FindViewById(Resource.Id.key_middle);
                    outerInstance.mDateRight = (ImageButton)v4.FindViewById(Resource.Id.key_right);

                    for (int i = 0; i < 10; i++)
                    {
                        outerInstance.mDateNumbers[i].SetOnClickListener(outerInstance);
                        outerInstance.mDateNumbers[i].Text = string.Format("{0:D}", i);
                        outerInstance.mDateNumbers[i].SetTextColor(outerInstance.mTextColor);
                        outerInstance.mDateNumbers[i].SetBackgroundResource(outerInstance.mKeyBackgroundResId);
                        outerInstance.mDateNumbers[i].SetTag(Resource.Id.date_keyboard, KEYBOARD_DATE);
                        outerInstance.mDateNumbers[i].SetTag(Resource.Id.numbers_key, i);
                    }

                    outerInstance.mDateRight.SetImageDrawable(res.GetDrawable(outerInstance.mCheckDrawableSrcResId));
                    outerInstance.mDateRight.SetBackgroundResource(outerInstance.mKeyBackgroundResId);
                    outerInstance.mDateRight.SetOnClickListener(outerInstance);
                }
                else if (outerInstance.mDateFormatOrder[position] == DateFormat.Year)
                {
                    // Year
                    sYearKeyboardPosition = position;
                    view = mInflater.Inflate(Resource.Layout.keyboard_with_header, null);
                    View v1 = view.FindViewById(Resource.Id.first);
                    View v2 = view.FindViewById(Resource.Id.second);
                    View v3 = view.FindViewById(Resource.Id.third);
                    View v4 = view.FindViewById(Resource.Id.fourth);
                    TextView header = (TextView)view.FindViewById(Resource.Id.header);

                    header.SetText(Resource.String.year_c);

                    outerInstance.mYearNumbers[1] = (Button)v1.FindViewById(Resource.Id.key_left);
                    outerInstance.mYearNumbers[2] = (Button)v1.FindViewById(Resource.Id.key_middle);
                    outerInstance.mYearNumbers[3] = (Button)v1.FindViewById(Resource.Id.key_right);

                    outerInstance.mYearNumbers[4] = (Button)v2.FindViewById(Resource.Id.key_left);
                    outerInstance.mYearNumbers[5] = (Button)v2.FindViewById(Resource.Id.key_middle);
                    outerInstance.mYearNumbers[6] = (Button)v2.FindViewById(Resource.Id.key_right);

                    outerInstance.mYearNumbers[7] = (Button)v3.FindViewById(Resource.Id.key_left);
                    outerInstance.mYearNumbers[8] = (Button)v3.FindViewById(Resource.Id.key_middle);
                    outerInstance.mYearNumbers[9] = (Button)v3.FindViewById(Resource.Id.key_right);

                    outerInstance.mYearLeft = (Button)v4.FindViewById(Resource.Id.key_left);
                    outerInstance.mYearLeft.SetTextColor(outerInstance.mTextColor);
                    outerInstance.mYearLeft.SetBackgroundResource(outerInstance.mKeyBackgroundResId);
                    outerInstance.mYearNumbers[0] = (Button)v4.FindViewById(Resource.Id.key_middle);
                    outerInstance.mYearRight = (Button)v4.FindViewById(Resource.Id.key_right);
                    outerInstance.mYearRight.SetTextColor(outerInstance.mTextColor);
                    outerInstance.mYearRight.SetBackgroundResource(outerInstance.mKeyBackgroundResId);

                    for (int i = 0; i < 10; i++)
                    {
                        outerInstance.mYearNumbers[i].SetOnClickListener(outerInstance);
                        outerInstance.mYearNumbers[i].Text = string.Format("{0:D}", i);
                        outerInstance.mYearNumbers[i].SetTextColor(outerInstance.mTextColor);
                        outerInstance.mYearNumbers[i].SetBackgroundResource(outerInstance.mKeyBackgroundResId);
                        outerInstance.mYearNumbers[i].SetTag(Resource.Id.date_keyboard, KEYBOARD_YEAR);
                        outerInstance.mYearNumbers[i].SetTag(Resource.Id.numbers_key, i);
                    }
                }
                else
                {
                    view = new View(outerInstance.mContext);
                }
                outerInstance.setLeftRightEnabled();
                outerInstance.updateDate();
                outerInstance.updateKeypad();
                collection.AddView(view, 0);

                return view;
            }

            public void DestroyItem(ViewGroup container, int position, object @object)
            {
                container.RemoveView((View)@object);
            }

            public override int Count
            {
                get
                {
                    return 3;
                }
            }

            public override bool IsViewFromObject(View view, Object o)
            {
                return view == o;
            }
        }

        /// <summary>
        /// Update the delete button to determine whether it is able to be clicked.
        /// </summary>
        public virtual void updateDeleteButton()
        {
            bool enabled = mMonthInput != -1 || mDateInputPointer != -1 || mYearInputPointer != -1;
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

        protected virtual void doOnClick(View v)
        {
            if (v == mDelete)
            {
                // Delete is dependent on which keyboard
                switch (mDateFormatOrder[mKeyboardPager.CurrentItem])
                {
                    case DateFormat.Month:
                        if (mMonthInput != -1)
                        {
                            mMonthInput = -1;
                        }
                        break;
                    case DateFormat.Date:
                        if (mDateInputPointer >= 0)
                        {
                            for (int i = 0; i < mDateInputPointer; i++)
                            {
                                mDateInput[i] = mDateInput[i + 1];
                            }
                            mDateInput[mDateInputPointer] = 0;
                            mDateInputPointer--;
                        }
                        else if (mKeyboardPager.CurrentItem > 0)
                        {
                            mKeyboardPager.SetCurrentItem(mKeyboardPager.CurrentItem - 1, true);
                        }
                        break;
                    case DateFormat.Year:
                        if (mYearInputPointer >= 0)
                        {
                            for (int i = 0; i < mYearInputPointer; i++)
                            {
                                mYearInput[i] = mYearInput[i + 1];
                            }
                            mYearInput[mYearInputPointer] = 0;
                            mYearInputPointer--;
                        }
                        else if (mKeyboardPager.CurrentItem > 0)
                        {
                            mKeyboardPager.SetCurrentItem(mKeyboardPager.CurrentItem - 1, true);
                        }
                        break;
                }
            }
            else if (v == mDateRight)
            {
                onDateRightClicked();
            }
            else if (v == mEnteredDate.Date)
            {
                mKeyboardPager.CurrentItem = sDateKeyboardPosition;
            }
            else if (v == mEnteredDate.Month)
            {
                mKeyboardPager.CurrentItem = sMonthKeyboardPosition;
            }
            else if (v == mEnteredDate.Year)
            {
                mKeyboardPager.CurrentItem = sYearKeyboardPosition;
            }
            else if (v.GetTag(Resource.Id.date_keyboard).Equals(KEYBOARD_MONTH))
            {
                // A month was pressed
                mMonthInput = (int)v.GetTag(Resource.Id.date_month_int);
                if (mKeyboardPager.CurrentItem < 2)
                {
                    mKeyboardPager.SetCurrentItem(mKeyboardPager.CurrentItem + 1, true);
                }
            }
            else if (v.GetTag(Resource.Id.date_keyboard).Equals(KEYBOARD_DATE))
            {
                // A date number was pressed
                addClickedDateNumber((int)v.GetTag(Resource.Id.numbers_key));
            }
            else if (v.GetTag(Resource.Id.date_keyboard).Equals(KEYBOARD_YEAR))
            {
                // A year number was pressed
                addClickedYearNumber((int)v.GetTag(Resource.Id.numbers_key));
            }
            updateKeypad();
        }

        public bool OnLongClick(View v)
        {
            v.PerformHapticFeedback(FeedbackConstants.LongPress);
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
            updateDate();
            // enable/disable the "set" key
            enableSetButton();
            // Update the backspace button
            updateDeleteButton();
            updateMonthKeys();
            updateDateKeys();
            updateYearKeys();
        }

        /// <summary>
        /// Reset all inputs and dates, and scroll to the first shown keyboard.
        /// </summary>
        public virtual void reset()
        {
            for (int i = 0; i < mDateInputSize; i++)
            {
                mDateInput[i] = 0;
            }
            for (int i = 0; i < mYearInputSize; i++)
            {
                mYearInput[i] = 0;
            }
            mDateInputPointer = -1;
            mYearInputPointer = -1;
            mMonthInput = -1;
            mKeyboardPager.SetCurrentItem(0, true);
            updateDate();
        }

        protected internal virtual void updateDate()
        {
            string month;
            if (mMonthInput < 0)
            {
                month = "";
            }
            else
            {
                month = mMonthAbbreviations[mMonthInput];
            }
            mEnteredDate.setDate(month, DayOfMonth, Year);
        }

        protected virtual void setLeftRightEnabled()
        {
            if (mDateLeft != null)
            {
                mDateLeft.Enabled = false;
            }
            if (mDateRight != null)
            {
                mDateRight.Enabled = canGoToYear();
            }
            if (mYearLeft != null)
            {
                mYearLeft.Enabled = false;
            }
            if (mYearRight != null)
            {
                mYearRight.Enabled = false;
            }
        }

        private void addClickedDateNumber(int val)
        {
            if (mDateInputPointer < mDateInputSize - 1)
            {
                for (int i = mDateInputPointer; i >= 0; i--)
                {
                    mDateInput[i + 1] = mDateInput[i];
                }
                mDateInputPointer++;
                mDateInput[0] = val;
            }
            if (DayOfMonth >= 4 || (MonthOfYear == 1 && DayOfMonth >= 3))
            {
                if (mKeyboardPager.CurrentItem < 2)
                {
                    mKeyboardPager.SetCurrentItem(mKeyboardPager.CurrentItem + 1, true);
                }
            }
        }

        private void addClickedYearNumber(int val)
        {
            if (mYearInputPointer < mYearInputSize - 1)
            {
                for (int i = mYearInputPointer; i >= 0; i--)
                {
                    mYearInput[i + 1] = mYearInput[i];
                }
                mYearInputPointer++;
                mYearInput[0] = val;
            }
            // Move to the next keyboard if the year is >= 1000 (not in every case)
            if (Year >= 1000 && mKeyboardPager.CurrentItem < 2)
            {
                mKeyboardPager.SetCurrentItem(mKeyboardPager.CurrentItem + 1, true);
            }
        }

        /// <summary>
        /// Clicking on the date right button advances
        /// </summary>
        private void onDateRightClicked()
        {
            if (mKeyboardPager.CurrentItem < 2)
            {
                mKeyboardPager.SetCurrentItem(mKeyboardPager.CurrentItem + 1, true);
            }
        }

        /// <summary>
        /// Enable/disable keys on the month key pad according to the data entered
        /// </summary>
        private void updateMonthKeys()
        {
            int date = DayOfMonth;
            for (int i = 0; i < mMonths.Length; i++)
            {
                if (mMonths[i] != null)
                {
                    mMonths[i].Enabled = true;
                }
            }
            if (date > 29)
            {
                // Disable February
                if (mMonths[1] != null)
                {
                    mMonths[1].Enabled = false;
                }
            }
            if (date > 30)
            {
                // Disable April, June, September, November
                if (mMonths[3] != null)
                {
                    mMonths[3].Enabled = false;
                }
                if (mMonths[5] != null)
                {
                    mMonths[5].Enabled = false;
                }
                if (mMonths[8] != null)
                {
                    mMonths[8].Enabled = false;
                }
                if (mMonths[10] != null)
                {
                    mMonths[10].Enabled = false;
                }
            }
        }

        /// <summary>
        /// Enable/disable keys on the date key pad according to the data entered
        /// </summary>
        private void updateDateKeys()
        {
            int date = DayOfMonth;
            if (date >= 4)
            {
                DateKeyRange = -1;
            }
            else if (date >= 3)
            {
                if (mMonthInput == 1)
                {
                    // February
                    DateKeyRange = -1;
                }
                else if (mMonthInput == 3 || mMonthInput == 5 || mMonthInput == 8 || mMonthInput == 10)
                {
                    // April, June, September, Novemeber have 30 days
                    DateKeyRange = 0;
                }
                else
                {
                    DateKeyRange = 1;
                }
            }
            else if (date >= 2)
            {
                DateKeyRange = 9;
            }
            else if (date >= 1)
            {
                DateKeyRange = 9;
            }
            else
            {
                DateMinKeyRange = 1;
            }
        }

        /// <summary>
        /// Enable/disable keys on the year key pad according to the data entered
        /// </summary>
        private void updateYearKeys()
        {
            int year = Year;
            if (year >= 1000)
            {
                YearKeyRange = -1;
            }
            else if (year >= 1)
            {
                YearKeyRange = 9;
            }
            else
            {
                YearMinKeyRange = 1;
            }
        }

        /// <summary>
        /// Enables a range of numeric keys from zero to maxKey. The rest of the keys will be disabled
        /// </summary>
        /// <param name="maxKey"> the maximum key number that can be pressed </param>
        private int DateKeyRange
        {
            set
            {
                for (int i = 0; i < mDateNumbers.Length; i++)
                {
                    if (mDateNumbers[i] != null)
                    {
                        mDateNumbers[i].Enabled = i <= value;
                    }
                }
            }
        }

        /// <summary>
        /// Enables a range of numeric keys from minKey up. The rest of the keys will be disabled
        /// </summary>
        /// <param name="minKey"> the minimum key number that can be pressed </param>
        private int DateMinKeyRange
        {
            set
            {
                for (int i = 0; i < mDateNumbers.Length; i++)
                {
                    if (mDateNumbers[i] != null)
                    {
                        mDateNumbers[i].Enabled = i >= value;
                    }
                }
            }
        }

        /// <summary>
        /// Enables a range of numeric keys from zero to maxKey. The rest of the keys will be disabled
        /// </summary>
        /// <param name="maxKey"> the maximum key that can be pressed </param>
        private int YearKeyRange
        {
            set
            {
                for (int i = 0; i < mYearNumbers.Length; i++)
                {
                    if (mYearNumbers[i] != null)
                    {
                        mYearNumbers[i].Enabled = i <= value;
                    }
                }
            }
        }

        /// <summary>
        /// Enables a range of numeric keys from minKey up. The rest of the keys will be disabled
        /// </summary>
        /// <param name="minKey"> the minimum key that can be pressed </param>
        private int YearMinKeyRange
        {
            set
            {
                for (int i = 0; i < mYearNumbers.Length; i++)
                {
                    if (mYearNumbers[i] != null)
                    {
                        mYearNumbers[i].Enabled = i >= value;
                    }
                }
            }
        }

        /// <summary>
        /// Check if a user can move to the year keyboard
        /// </summary>
        /// <returns> true or false whether the user can move to the year keyboard </returns>
        private bool canGoToYear()
        {
            return DayOfMonth > 0;
        }

        private void updateLeftRightButtons()
        {
            if (mDateRight != null)
            {
                mDateRight.Enabled = canGoToYear();
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
            mSetButton.Enabled = DayOfMonth > 0 && Year > 0 && MonthOfYear >= 0;
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
        /// Returns the year as currently inputted by the user.
        /// </summary>
        /// <returns> the inputted year </returns>
        public virtual int Year
        {
            get
            {
                return mYearInput[3] * 1000 + mYearInput[2] * 100 + mYearInput[1] * 10 + mYearInput[0];
            }
        }

        /// <summary>
        /// Returns the zero-indexed month of year as currently inputted by the user.
        /// </summary>
        /// <returns> the zero-indexed inputted month </returns>
        public virtual int MonthOfYear
        {
            get
            {
                return mMonthInput;
            }
        }

        /// <summary>
        /// Returns the day of month as currently inputted by the user.
        /// </summary>
        /// <returns> the inputted day of month </returns>
        public virtual int DayOfMonth
        {
            get
            {
                return mDateInput[1] * 10 + mDateInput[0];
            }
        }

        /// <summary>
        /// Set the date shown in the date picker
        /// </summary>
        /// <param name="year"> the new year to set </param>
        /// <param name="monthOfYear"> the new zero-indexed month to set </param>
        /// <param name="dayOfMonth"> the new day of month to set </param>
        public virtual void setDate(int year, int monthOfYear, int dayOfMonth)
        {
            mMonthInput = monthOfYear;
            mYearInput[3] = year / 1000;
            mYearInput[2] = (year % 1000) / 100;
            mYearInput[1] = (year % 100) / 10;
            mYearInput[0] = year % 10;
            if (year >= 1000)
            {
                mYearInputPointer = 3;
            }
            else if (year >= 100)
            {
                mYearInputPointer = 2;
            }
            else if (year >= 10)
            {
                mYearInputPointer = 1;
            }
            else if (year > 0)
            {
                mYearInputPointer = 0;
            }
            mDateInput[1] = dayOfMonth / 10;
            mDateInput[0] = dayOfMonth % 10;
            if (dayOfMonth >= 10)
            {
                mDateInputPointer = 1;
            }
            else if (dayOfMonth > 0)
            {
                mDateInputPointer = 0;
            }
            for (int i = 0; i < mDateFormatOrder.Length; i++)
            {
                char c = mDateFormatOrder[i];
                if (c == DateFormat.Month && monthOfYear == -1)
                {
                    mKeyboardPager.SetCurrentItem(i, true);
                    break;
                }
                else if (c == DateFormat.Date && dayOfMonth <= 0)
                {
                    mKeyboardPager.SetCurrentItem(i, true);
                    break;
                }
                else if (c == DateFormat.Year && year <= 0)
                {
                    mKeyboardPager.SetCurrentItem(i, true);
                    break;
                }
            }
            updateKeypad();
        }

        /// <summary>
        /// Create a String array with all the months abbreviations localized with the default Locale.
        /// </summary>
        /// <returns> a String array with all localized month abbreviations like JAN, FEB, etc. </returns>
        public static string[] makeLocalizedMonthAbbreviations()
        {
            return makeLocalizedMonthAbbreviations(Locale.Default);
        }

        /// <summary>
        /// Create a String array with all the months abbreviations localized with the specified Locale.
        /// </summary>
        /// <param name="locale"> the Locale to use for localization, or null to use the default one </param>
        /// <returns> a String array with all localized month abbreviations like JAN, FEB, etc. </returns>
        public static string[] makeLocalizedMonthAbbreviations(Locale locale)
        {
            bool hasLocale = locale != null;
            SimpleDateFormat monthAbbreviationFormat = hasLocale ? new SimpleDateFormat("MMM", locale) : new SimpleDateFormat("MMM");
            var date = hasLocale ? new GregorianCalendar(locale) : new GregorianCalendar();
            date.Set(CalendarField.Year, 0);
            date.Set(CalendarField.DayOfMonth, 1);
            date.Set(CalendarField.HourOfDay, 0);
            date.Set(CalendarField.Minute, 0);
            date.Set(CalendarField.Second, 0);
            date.Set(CalendarField.Millisecond, 0);

            var months = new string[12];

            for (var i = 0; i < months.Length; i++)
            {
                date.Set(CalendarField.Month, i);
                months[i] = monthAbbreviationFormat.Format(date).ToUpper();
            }

            return months;
        }

        protected override IParcelable OnSaveInstanceState()
        {
            var parcel = base.OnSaveInstanceState();

            var state = new SavedState(parcel);
            state.mMonthInput = mMonthInput;
            state.mDateInput = mDateInput;
            state.mDateInputPointer = mDateInputPointer;
            state.mYearInput = mYearInput;
            state.mYearInputPointer = mYearInputPointer;
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

            mDateInputPointer = savedState.mDateInputPointer;
            mYearInputPointer = savedState.mYearInputPointer;
            mDateInput = savedState.mDateInput;
            mYearInput = savedState.mYearInput;
            if (mDateInput == null)
            {
                mDateInput = new int[mDateInputSize];
                mDateInputPointer = -1;
            }
            if (mYearInput == null)
            {
                mYearInput = new int[mYearInputSize];
                mYearInputPointer = -1;
            }
            mMonthInput = savedState.mMonthInput;
            updateKeypad();
        }

        private class SavedState : BaseSavedState
        {
            internal int mDateInputPointer;
            internal int mYearInputPointer;
            internal int[] mDateInput;
            internal int[] mYearInput;
            internal int mMonthInput;

            public SavedState(IParcelable superState)
                : base(superState) { }

            internal SavedState(Parcel @in)
                : base(@in)
            {
                mDateInputPointer = @in.ReadInt();
                mYearInputPointer = @in.ReadInt();
                @in.ReadIntArray(mDateInput);
                @in.ReadIntArray(mYearInput);
                mMonthInput = @in.ReadInt();
            }

            public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
            {
                base.WriteToParcel(dest, flags);
                dest.WriteInt(mDateInputPointer);
                dest.WriteInt(mYearInputPointer);
                dest.WriteIntArray(mDateInput);
                dest.WriteIntArray(mYearInput);
                dest.WriteInt(mMonthInput);
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