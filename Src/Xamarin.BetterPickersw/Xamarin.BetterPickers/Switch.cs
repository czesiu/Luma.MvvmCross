/*
 * This source is part of the
 *      _____  ___   ____
 *  __ / / _ \/ _ | / __/___  _______ _
 * / // / , _/ __ |/ _/_/ _ \/ __/ _ `/
 * \___/_/|_/_/ |_/_/ (_)___/_/  \_, /
 *                              /___/
 * repository.
 *
 * Copyright (C) 2012 Benoit 'BoD' Lubek (BoD@JRAF.org)
 * Copyright (C) 2010 The Android Open Source Project
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
 * limitations under the License.
 */

using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Xamarin.BetterPickers
{
    /// <summary>
    /// A Switch is a two-state toggle switch widget that can select between two
    /// options. The user may drag the "thumb" back and forth to choose the selected option,
    /// or simply tap to toggle as if it were a checkbox. The <seealso cref="#setText(CharSequence) text"/> property controls the text displayed in the label for the switch,
    /// whereas the <seealso cref="#setTextOff(CharSequence) off"/> and <seealso cref="#setTextOn(CharSequence) on"/> text
    /// controls the text on the thumb. Similarly, the <seealso cref="#setTextAppearance(android.content.Context, int) textAppearance"/> and the related
    /// setTypeface() methods control the typeface and style of label text, whereas the {@link #setSwitchTextAppearance(android.content.Context, int)
    /// switchTextAppearance} and
    /// the related seSwitchTypeface() methods control that of the thumb.
    /// </summary>
    public class Switch : CompoundButton
    {
        private const int TOUCH_MODE_IDLE = 0;
        private const int TOUCH_MODE_DOWN = 1;
        private const int TOUCH_MODE_DRAGGING = 2;

        // Enum for the "typeface" XML parameter.
        private const int SANS = 1;
        private const int SERIF = 2;
        private const int MONOSPACE = 3;

        private readonly Drawable MThumbDrawable;
        private readonly Drawable MTrackDrawable;
        private readonly int MThumbTextPadding;
        private readonly int MSwitchMinWidth;
        private readonly int MSwitchPadding;
        private string MTextOn;
        private string MTextOff;

        private int MTouchMode;
        private readonly int MTouchSlop;
        private float MTouchX;
        private float MTouchY;
        private readonly VelocityTracker MVelocityTracker = VelocityTracker.Obtain();
        private readonly int MMinFlingVelocity;

        private float MThumbPosition;
        private int MSwitchWidth;
        private int MSwitchHeight;
        private int MThumbWidth; // Does not include padding

        private int MSwitchLeft;
        private int MSwitchTop;
        private int MSwitchRight;
        private int MSwitchBottom;

        private readonly TextPaint MTextPaint;
        private ColorStateList MTextColors;
        private Layout MOnLayout;
        private Layout MOffLayout;

        private readonly Rect MTempRect = new Rect();

        private static readonly int[] CHECKED_STATE_SET = { Android.Resource.Attribute.StateChecked };

        /// <summary>
        /// Construct a new Switch with default styling.
        /// </summary>
        /// <param name="context"> The Context that will determine this widget's theming. </param>
        public Switch(Context context)
            : this(context, null) { }

        /// <summary>
        /// Construct a new Switch with default styling, overriding specific style
        /// attributes as requested.
        /// </summary>
        /// <param name="context"> The Context that will determine this widget's theming. </param>
        /// <param name="attrs"> Specification of attributes that should deviate from default styling. </param>
        public Switch(Context context, IAttributeSet attrs)
            : this(context, attrs, Resource.Attribute.switchStyle) { }

        /// <summary>
        /// Construct a new Switch with a default style determined by the given theme attribute,
        /// overriding specific style attributes as requested.
        /// </summary>
        /// <param name="context"> The Context that will determine this widget's theming. </param>
        /// <param name="attrs"> Specification of attributes that should deviate from the default styling. </param>
        /// <param name="defStyle"> An attribute ID within the active theme containing a reference to the
        ///            default style for this widget. e.g. android.R.attr.switchStyle. </param>
        public Switch(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            MTextPaint = new TextPaint(PaintFlags.AntiAlias);
            var res = Resources;
            MTextPaint.Density = res.DisplayMetrics.Density;
            // XXX Was on the Android source, but had to comment it out (doesn't exist in 2.1). -- BoD
            // mTextPaint.setCompatibilityScaling(res.getCompatibilityInfo().applicationScale);

            var a = context.ObtainStyledAttributes(attrs,Resource.Styleable.Switch, defStyle, 0);

            MThumbDrawable = a.GetDrawable(Resource.Styleable.Switch_thumb);
            MTrackDrawable = a.GetDrawable(Resource.Styleable.Switch_track);
            MTextOn = a.GetText(Resource.Styleable.Switch_textOn);
            MTextOff = a.GetText(Resource.Styleable.Switch_textOff);
            MThumbTextPadding = a.GetDimensionPixelSize(Resource.Styleable.Switch_thumbTextPadding, 0);
            MSwitchMinWidth = a.GetDimensionPixelSize(Resource.Styleable.Switch_switchMinWidth, 0);
            MSwitchPadding = a.GetDimensionPixelSize(Resource.Styleable.Switch_switchPadding, 0);

            int appearance = a.GetResourceId(Resource.Styleable.Switch_switchTextAppearance, 0);
            if (appearance != 0)
            {
                SetSwitchTextAppearance(context, appearance);
            }
            a.Recycle();

            var config = ViewConfiguration.Get(context);
            MTouchSlop = config.ScaledTouchSlop;
            MMinFlingVelocity = config.ScaledMinimumFlingVelocity;

            // Refresh display with current params
            RefreshDrawableState();
            Checked = Checked;
        }

        /// <summary>
        /// Sets the switch text color, size, style, hint color, and highlight color
        /// from the specified TextAppearance resource.
        /// </summary>
        public virtual void SetSwitchTextAppearance(Context context, int resid)
        {
            var appearance = context.ObtainStyledAttributes(resid, Resource.Styleable.Android);

            ColorStateList colors;
            int ts;

            colors = appearance.GetColorStateList(Resource.Styleable.Android_android_textColor);
            if (colors != null)
            {
                MTextColors = colors;
            }
            else
            {
                // If no color set in TextAppearance, default to the view's textColor
                MTextColors = TextColors;
            }

            ts = appearance.GetDimensionPixelSize(Resource.Styleable.Android_android_textSize, 0);
            if (ts != 0)
            {
                if (ts != MTextPaint.TextSize)
                {
                    MTextPaint.TextSize = ts;
                    RequestLayout();
                }
            }

            var typefaceIndex = appearance.GetInt(Resource.Styleable.Android_android_typeface, -1);
            var styleIndex = appearance.GetInt(Resource.Styleable.Android_android_textStyle, -1);

            SetSwitchTypefaceByIndex(typefaceIndex, (TypefaceStyle)styleIndex);

            appearance.Recycle();
        }

        private void SetSwitchTypefaceByIndex(int typefaceIndex, TypefaceStyle styleIndex)
        {
            Typeface tf = null;
            switch (typefaceIndex)
            {
                case SANS:
                    tf = Typeface.SansSerif;
                    break;

                case SERIF:
                    tf = Typeface.Serif;
                    break;

                case MONOSPACE:
                    tf = Typeface.Monospace;
                    break;
            }

            SetSwitchTypeface(tf, styleIndex);
        }

        /// <summary>
        /// Sets the typeface and style in which the text should be displayed on the
        /// switch, and turns on the fake bold and italic bits in the Paint if the
        /// Typeface that you provided does not have all the bits in the
        /// style that you specified.
        /// </summary>
        public virtual void SetSwitchTypeface(Typeface tf, TypefaceStyle style)
        {
            if (style > 0)
            {
                if (tf == null)
                {
                    tf = Typeface.DefaultFromStyle(style);
                }
                else
                {
                    tf = Typeface.Create(tf, style);
                }

                SwitchTypeface = tf;
                // now compute what (if any) algorithmic styling is needed
                var typefaceStyle = tf != null ? tf.Style : 0;
                var need = style & ~typefaceStyle;
                MTextPaint.FakeBoldText = (need & TypefaceStyle.Bold) != 0;
                MTextPaint.TextSkewX = (need & TypefaceStyle.Italic) != 0 ? -0.25f : 0;
            }
            else
            {
                MTextPaint.FakeBoldText = false;
                MTextPaint.TextSkewX = 0;
                SwitchTypeface = tf;
            }
        }

        /// <summary>
        /// Sets the typeface in which the text should be displayed on the switch.
        /// Note that not all Typeface families actually have bold and italic
        /// variants, so you may need to use <seealso cref="#setSwitchTypeface(android.graphics.Typeface, int)"/> to get the appearance
        /// that you actually want.
        /// 
        /// @attr ref android.R.styleable#TextView_typeface
        /// @attr ref android.R.styleable#TextView_textStyle
        /// </summary>
        public virtual Typeface SwitchTypeface
        {
            set
            {
                if (MTextPaint.Typeface != value)
                {
                    MTextPaint.SetTypeface(value);

                    RequestLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Returns the text displayed when the button is in the checked state.
        /// </summary>
        public virtual string TextOn
        {
            get
            {
                return MTextOn;
            }
            set
            {
                MTextOn = value;
                RequestLayout();
            }
        }


        /// <summary>
        /// Returns the text displayed when the button is not in the checked state.
        /// </summary>
        public virtual string TextOff
        {
            get
            {
                return MTextOff;
            }
            set
            {
                MTextOff = value;
                RequestLayout();
            }
        }


        //@Override @TargetApi(android.os.Build.VERSION_CODES.HONEYCOMB) public void onMeasure(int widthMeasureSpec, int heightMeasureSpec)
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (MOnLayout == null)
            {
                MOnLayout = MakeLayout(MTextOn);
            }
            if (MOffLayout == null)
            {
                MOffLayout = MakeLayout(MTextOff);
            }

            MTrackDrawable.GetPadding(MTempRect);
            int maxTextWidth = Math.Max(MOnLayout.Width, MOffLayout.Width);
            int switchWidth = Math.Max(MSwitchMinWidth, maxTextWidth * 2 + MThumbTextPadding * 4 + MTempRect.Left + MTempRect.Right);
            int switchHeight = MTrackDrawable.IntrinsicHeight;

            MThumbWidth = maxTextWidth + MThumbTextPadding * 2;

            MSwitchWidth = switchWidth;
            MSwitchHeight = switchHeight;

            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            int measuredHeight = MeasuredHeight;
            if (measuredHeight < switchHeight)
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Honeycomb)
                {
                    SetMeasuredDimension(MeasuredWidth, switchHeight);
                }
                else
                {
                    SetMeasuredDimension(MeasuredWidthAndState, switchHeight);
                }
            }
        }

        // XXX Was on the Android source, but had to comment it out (doesn't exist in 2.1). -- BoD
        // @Override
        // public void onPopulateAccessibilityEvent(AccessibilityEvent event) {
        //     super.onPopulateAccessibilityEvent(event);
        //     if (isChecked()) {
        //         CharSequence text = mOnLayout.getText();
        //         if (TextUtils.isEmpty(text)) {
        //             text = getContext().getString(Resource.string.switch_on);
        //         }
        //         event.getText().add(text);
        //     } else {
        //         CharSequence text = mOffLayout.getText();
        //         if (TextUtils.isEmpty(text)) {
        //             text = getContext().getString(Resource.string.switch_off);
        //         }
        //         event.getText().add(text);
        //     }
        // }

        private Layout MakeLayout(string text)
        {
            return new StaticLayout(text, MTextPaint, (int)Math.Ceiling(Layout.GetDesiredWidth(text, MTextPaint)), Layout.Alignment.AlignNormal, 1.0f, 0, true);
        }

        /// <returns> true if (x, y) is within the target area of the switch thumb </returns>
        private bool HitThumb(float x, float y)
        {
            MThumbDrawable.GetPadding(MTempRect);
            int thumbTop = MSwitchTop - MTouchSlop;
            int thumbLeft = MSwitchLeft + (int)(MThumbPosition + 0.5f) - MTouchSlop;
            int thumbRight = thumbLeft + MThumbWidth + MTempRect.Left + MTempRect.Right + MTouchSlop;
            int thumbBottom = MSwitchBottom + MTouchSlop;
            return x > thumbLeft && x < thumbRight && y > thumbTop && y < thumbBottom;
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            MVelocityTracker.AddMovement(ev);

            MotionEventActions action;
            if (Build.VERSION.SdkInt < BuildVersionCodes.Froyo)
            {
                action = ev.Action;
            }
            else
            {
                action = ev.ActionMasked;
            }

            switch (action)
            {
                case MotionEventActions.Down:
                    {
                        float x = ev.GetX();
                        float y = ev.GetY();
                        if (Enabled && HitThumb(x, y))
                        {
                            MTouchMode = TOUCH_MODE_DOWN;
                            MTouchX = x;
                            MTouchY = y;
                        }
                        break;
                    }

                case MotionEventActions.Move:
                    {
                        switch (MTouchMode)
                        {
                            case TOUCH_MODE_IDLE:
                                // Didn't target the thumb, treat normally.
                                break;

                            case TOUCH_MODE_DOWN:
                                {
                                    float x = ev.GetX();
                                    float y = ev.GetY();
                                    if (Math.Abs(x - MTouchX) > MTouchSlop || Math.Abs(y - MTouchY) > MTouchSlop)
                                    {
                                        MTouchMode = TOUCH_MODE_DRAGGING;
                                        Parent.RequestDisallowInterceptTouchEvent(true);
                                        MTouchX = x;
                                        MTouchY = y;
                                        return true;
                                    }
                                    break;
                                }

                            case TOUCH_MODE_DRAGGING:
                                {
                                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                                    //ORIGINAL LINE: final float x = ev.getX();
                                    float x = ev.GetX();
                                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                                    //ORIGINAL LINE: final float dx = x - mTouchX;
                                    float dx = x - MTouchX;
                                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                                    //ORIGINAL LINE: final float newPos = Math.max(0, Math.min(mThumbPosition + dx, getThumbScrollRange()));
                                    float newPos = Math.Max(0, Math.Min(MThumbPosition + dx, ThumbScrollRange));
                                    if (newPos != MThumbPosition)
                                    {
                                        MThumbPosition = newPos;
                                        MTouchX = x;
                                        Invalidate();
                                    }
                                    return true;
                                }
                        }
                        break;
                    }

                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    {
                        if (MTouchMode == TOUCH_MODE_DRAGGING)
                        {
                            StopDrag(ev);
                            return true;
                        }
                        MTouchMode = TOUCH_MODE_IDLE;
                        MVelocityTracker.Clear();
                        break;
                    }
            }

            return base.OnTouchEvent(ev);
        }

        private void CancelSuperTouch(MotionEvent ev)
        {
            MotionEvent cancel = MotionEvent.Obtain(ev);
            cancel.Action = MotionEventActions.Cancel;
            base.OnTouchEvent(cancel);
            cancel.Recycle();
        }

        /// <summary>
        /// Called from onTouchEvent to end a drag operation.
        /// </summary>
        /// <param name="ev"> Event that triggered the end of drag mode - ACTION_UP or ACTION_CANCEL </param>
        private void StopDrag(MotionEvent ev)
        {
            MTouchMode = TOUCH_MODE_IDLE;
            // Up and not canceled, also checks the switch has not been disabled during the drag
            bool commitChange = ev.Action == MotionEventActions.Up && Enabled;

            CancelSuperTouch(ev);

            if (commitChange)
            {
                bool newState;
                MVelocityTracker.ComputeCurrentVelocity(1000);
                float xvel = MVelocityTracker.XVelocity;
                if (Math.Abs(xvel) > MMinFlingVelocity)
                {
                    newState = xvel > 0;
                }
                else
                {
                    newState = TargetCheckedState;
                }
                AnimateThumbToCheckedState(newState);
            }
            else
            {
                AnimateThumbToCheckedState(Checked);
            }
        }

        private void AnimateThumbToCheckedState(bool newCheckedState)
        {
            // TODO animate!
            //float targetPos = newCheckedState ? 0 : getThumbScrollRange();
            //mThumbPosition = targetPos;
            Checked = newCheckedState;
        }

        private bool TargetCheckedState
        {
            get
            {
                return MThumbPosition >= ThumbScrollRange / 2;
            }
        }

        public override bool Checked
        {
            set
            {
                base.Checked = value;
                MThumbPosition = value ? ThumbScrollRange : 0;
                Invalidate();
            }
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);

            MThumbPosition = Checked ? ThumbScrollRange : 0;

            int switchRight = Width - PaddingRight;
            int switchLeft = switchRight - MSwitchWidth;
            int switchTop;
            int switchBottom;
            switch (Gravity & GravityFlags.VerticalGravityMask)
            {
                default:
                    goto case GravityFlags.Top;
                case GravityFlags.Top:
                    switchTop = PaddingTop;
                    switchBottom = switchTop + MSwitchHeight;
                    break;

                case GravityFlags.CenterVertical:
                    switchTop = (PaddingTop + Height - PaddingBottom) / 2 - MSwitchHeight / 2;
                    switchBottom = switchTop + MSwitchHeight;
                    break;

                case GravityFlags.Bottom:
                    switchBottom = Height - PaddingBottom;
                    switchTop = switchBottom - MSwitchHeight;
                    break;
            }

            MSwitchLeft = switchLeft;
            MSwitchTop = switchTop;
            MSwitchBottom = switchBottom;
            MSwitchRight = switchRight;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            // Draw the switch
            int switchLeft = MSwitchLeft;
            int switchTop = MSwitchTop;
            int switchRight = MSwitchRight;
            int switchBottom = MSwitchBottom;

            MTrackDrawable.SetBounds(switchLeft, switchTop, switchRight, switchBottom);
            MTrackDrawable.Draw(canvas);

            canvas.Save();

            MTrackDrawable.GetPadding(MTempRect);
            int switchInnerLeft = switchLeft + MTempRect.Left;
            int switchInnerTop = switchTop + MTempRect.Top;
            int switchInnerRight = switchRight - MTempRect.Right;
            int switchInnerBottom = switchBottom - MTempRect.Bottom;
            canvas.ClipRect(switchInnerLeft, switchTop, switchInnerRight, switchBottom);

            MThumbDrawable.GetPadding(MTempRect);
            int thumbPos = (int)(MThumbPosition + 0.5f);
            int thumbLeft = switchInnerLeft - MTempRect.Left + thumbPos;
            int thumbRight = switchInnerLeft + thumbPos + MThumbWidth + MTempRect.Right;

            MThumbDrawable.SetBounds(thumbLeft, switchTop, thumbRight, switchBottom);
            MThumbDrawable.Draw(canvas);

            // mTextColors should not be null, but just in case
            if (MTextColors != null)
            {
                MTextPaint.Color = new Color(MTextColors.GetColorForState(GetDrawableState(), new Color(MTextColors.DefaultColor)));
            }
            MTextPaint.DrawableState = GetDrawableState();

            var switchText = TargetCheckedState ? MOnLayout : MOffLayout;

            canvas.Translate((thumbLeft + thumbRight) / 2 - switchText.Width / 2, (switchInnerTop + switchInnerBottom) / 2 - switchText.Height / 2);
            switchText.Draw(canvas);

            canvas.Restore();
        }

        public override int CompoundPaddingRight
        {
            get
            {
                int padding = base.CompoundPaddingRight + MSwitchWidth;
                if (!TextUtils.IsEmpty(Text))
                {
                    padding += MSwitchPadding;
                }
                return padding;
            }
        }

        private int ThumbScrollRange
        {
            get
            {
                if (MTrackDrawable == null)
                {
                    return 0;
                }
                MTrackDrawable.GetPadding(MTempRect);
                return MSwitchWidth - MThumbWidth - MTempRect.Left - MTempRect.Right;
            }
        }

        protected override int[] OnCreateDrawableState(int extraSpace)
        {
            int[] drawableState = base.OnCreateDrawableState(extraSpace + 1);
            
            if (Checked)
            {
                MergeDrawableStates(drawableState, CHECKED_STATE_SET);
            }

            return drawableState;
        }

        protected override void DrawableStateChanged()
        {
            base.DrawableStateChanged();

            int[] myDrawableState = GetDrawableState();

            // Set the state of the Drawable
            // Drawable may be null when checked state is set from XML, from super constructor
            if (MThumbDrawable != null)
            {
                MThumbDrawable.SetState(myDrawableState);
            }
            if (MTrackDrawable != null)
            {
                MTrackDrawable.SetState(myDrawableState);
            }

            Invalidate();
        }

        protected override bool VerifyDrawable(Drawable who)
        {
            return base.VerifyDrawable(who) || who == MThumbDrawable || who == MTrackDrawable;
        }

        // XXX Was on the Android source, but had to comment it out (doesn't exist in 2.1). -- BoD
        // @Override
        // public void jumpDrawablesToCurrentState() {
        //     super.jumpDrawablesToCurrentState();
        //     mThumbDrawable.jumpToCurrentState();
        //     mTrackDrawable.jumpToCurrentState();
        // }
    }
}