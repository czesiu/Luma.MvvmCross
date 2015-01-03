using System;
/*
 * Copyright (C) 2012 Jake Wharton
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
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;

namespace Xamarin.BetterPickers
{
    //using Context = android.content.Context;
    //using TypedArray = android.content.res.TypedArray;
    //using Canvas = android.graphics.Canvas;
    //using Paint = android.graphics.Paint;
    //using Style = android.graphics.Paint.Style;
    //using Parcel = android.os.Parcel;
    //using Parcelable = android.os.Parcelable;
    //using MotionEventCompat = android.support.v4.view.MotionEventCompat;
    //using ViewConfigurationCompat = android.support.v4.view.ViewConfigurationCompat;
    //using ViewPager = android.support.v4.view.ViewPager;
    //using IAttributeSet = android.util.AttributeSet;
    //using MotionEvent = android.view.MotionEvent;
    //using View = android.view.View;
    //using ViewConfiguration = android.view.ViewConfiguration;

	/// <summary>
	/// Draws a line for each page. The current page line is colored differently than the unselected page lines.
	/// </summary>
	public class UnderlinePageIndicatorPicker : View, IPageIndicator
	{

		private int mColorUnderline;

		private const int INVALID_POINTER = -1;

		private readonly Paint mPaint = new Paint(PaintFlags.AntiAlias);

		private ViewPager mViewPager;
		private ViewPager.IOnPageChangeListener mListener;
		private int mScrollState;
		private int mCurrentPage;
		private float mPositionOffset;

		private int mTouchSlop;
		private float mLastMotionX = -1;
		private int mActivePointerId = INVALID_POINTER;
		private bool mIsDragging;

		private PickerLinearLayout mTitleView = null;
		private Paint rectPaint;

		public UnderlinePageIndicatorPicker(Context context)
            : this(context, null) { }
		public UnderlinePageIndicatorPicker(Context context, IAttributeSet attrs)
            : base(context, attrs) { }
		public UnderlinePageIndicatorPicker(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
		{

			mColorUnderline = Resources.GetColor(Resource.Color.dialog_text_color_holo_dark);

			var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.BetterPickersDialogFragment, defStyle, 0);
            mColorUnderline = a.GetColor(Resource.Styleable.BetterPickersDialogFragment_bpKeyboardIndicatorColor, mColorUnderline);

			rectPaint = new Paint();
			rectPaint.AntiAlias = true;
			rectPaint.SetStyle(Paint.Style.Fill);

			a.Recycle();

			var configuration = ViewConfiguration.Get(context);
			mTouchSlop = ViewConfigurationCompat.GetScaledPagingTouchSlop(configuration);
		}

		public virtual Color SelectedColor
		{
			get
			{
				return mPaint.Color;
			}
			set
			{
				mPaint.Color = value;
				Invalidate();
			}
		}


		protected override void OnDraw(Canvas canvas)
		{
            base.OnDraw(canvas);

			var count = mViewPager.Adapter.Count;

			if (IsInEditMode || count == 0)
			{
				return;
			}

			if (mTitleView != null)
			{
				View currentTab = mTitleView.GetViewAt(mCurrentPage);
				float lineLeft = currentTab.Left;
				float lineRight = currentTab.Right;

				// if there is an offset, start interpolating left and right
				// coordinates
				// between current and next tab
				if (mPositionOffset > 0f && mCurrentPage < count - 1)
				{

					View nextTab = mTitleView.GetViewAt(mCurrentPage + 1);
					float nextTabLeft = nextTab.Left;
					float nextTabRight = nextTab.Right;

					lineLeft = (mPositionOffset * nextTabLeft + (1f - mPositionOffset) * lineLeft);
					lineRight = (mPositionOffset * nextTabRight + (1f - mPositionOffset) * lineRight);
				}

				canvas.DrawRect(lineLeft, PaddingBottom, lineRight, Height - PaddingBottom, mPaint);
			}
		}

		public override bool OnTouchEvent(MotionEvent ev)
		{
            if (base.OnTouchEvent(ev))
			{
				return true;
			}

			if ((mViewPager == null) || (mViewPager.Adapter.Count == 0))
			{
				return false;
			}

			var action = (MotionEventActions)((int)ev.Action & MotionEventCompat.ActionMask);
			switch (action)
			{
				case MotionEventActions.Down:
					mActivePointerId = MotionEventCompat.GetPointerId(ev, 0);
					mLastMotionX = ev.GetX();
					break;

                case MotionEventActions.Move:
				{
					int activePointerIndex = MotionEventCompat.FindPointerIndex(ev, mActivePointerId);
					float x = MotionEventCompat.GetX(ev, activePointerIndex);
					float deltaX = x - mLastMotionX;

					if (!mIsDragging)
					{
						if (Math.Abs(deltaX) > mTouchSlop)
						{
							mIsDragging = true;
						}
					}

					if (mIsDragging)
					{
						mLastMotionX = x;
						if (mViewPager.IsFakeDragging || mViewPager.BeginFakeDrag())
						{
							mViewPager.FakeDragBy(deltaX);
						}
					}

					break;
				}

                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
					if (!mIsDragging)
					{
						int count = mViewPager.Adapter.Count;
						int width = Width;
						float halfWidth = width / 2f;
						float sixthWidth = width / 6f;

						if ((mCurrentPage > 0) && (ev.GetX() < halfWidth - sixthWidth))
						{
							if (action != MotionEventActions.Cancel)
							{
								mViewPager.CurrentItem = mCurrentPage - 1;
							}
							return true;
						}
						else if ((mCurrentPage < count - 1) && (ev.GetX() > halfWidth + sixthWidth))
						{
                            if (action != MotionEventActions.Cancel)
							{
								mViewPager.CurrentItem = mCurrentPage + 1;
							}
							return true;
						}
					}

					mIsDragging = false;
					mActivePointerId = INVALID_POINTER;
					if (mViewPager.IsFakeDragging)
					{
						mViewPager.EndFakeDrag();
					}
					break;

				case (MotionEventActions)MotionEventCompat.ActionPointerDown:
				{
					int index = MotionEventCompat.GetActionIndex(ev);
					mLastMotionX = MotionEventCompat.GetX(ev, index);
					mActivePointerId = MotionEventCompat.GetPointerId(ev, index);
					break;
				}

                case (MotionEventActions)MotionEventCompat.ActionPointerUp:
					int pointerIndex = MotionEventCompat.GetActionIndex(ev);
					int pointerId = MotionEventCompat.GetPointerId(ev, pointerIndex);
					if (pointerId == mActivePointerId)
					{
						int newPointerIndex = pointerIndex == 0 ? 1 : 0;
						mActivePointerId = MotionEventCompat.GetPointerId(ev, newPointerIndex);
					}
					mLastMotionX = MotionEventCompat.GetX(ev, MotionEventCompat.FindPointerIndex(ev, mActivePointerId));
					break;
			}

			return true;
		}

		public virtual ViewPager ViewPager
		{
			set
			{
				if (mViewPager == value)
				{
					return;
				}
				if (mViewPager != null)
				{
					// Clear us from the old pager.
					mViewPager.SetOnPageChangeListener(null);
				}
				if (value.Adapter == null)
				{
					throw new InvalidOperationException("ViewPager does not have adapter instance.");
				}
				mViewPager = value;
                mViewPager.SetOnPageChangeListener(this);
				Invalidate();
			}
		}

		public virtual void setViewPager(ViewPager view, int initialPosition)
		{
			ViewPager = view;
			CurrentItem = initialPosition;
		}

		public virtual int CurrentItem
		{
			set
			{
				if (mViewPager == null)
				{
					throw new System.InvalidOperationException("ViewPager has not been bound.");
				}
				mViewPager.CurrentItem = value;
				mCurrentPage = value;
				Invalidate();
			}
		}

		public virtual void notifyDataSetChanged()
		{
            Invalidate();
		}

		public void OnPageScrollStateChanged(int state)
		{
			mScrollState = state;

			if (mListener != null)
			{
                mListener.OnPageScrollStateChanged(state);
			}
		}

		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
			mCurrentPage = position;
			mPositionOffset = positionOffset;
			Invalidate();

			if (mListener != null)
			{
                mListener.OnPageScrolled(position, positionOffset, positionOffsetPixels);
			}
		}

		public void OnPageSelected(int position)
		{
			if (mScrollState == ViewPager.ScrollStateIdle)
			{
				mCurrentPage = position;
				mPositionOffset = 0;
                Invalidate();
			}
			if (mListener != null)
			{
				mListener.OnPageSelected(position);
			}
		}

		public void SetOnPageChangeListener(ViewPager.IOnPageChangeListener l)
		{
            mListener = l;
		}

        protected override void OnRestoreInstanceState(IParcelable state)
		{
			SavedState savedState = (SavedState) state;
			base.OnRestoreInstanceState(savedState.SuperState);
			mCurrentPage = savedState.currentPage;
			RequestLayout();
		}

        protected override IParcelable OnSaveInstanceState()
		{
            var superState = base.OnSaveInstanceState();
			var savedState = new SavedState(superState);
			savedState.currentPage = mCurrentPage;
			return savedState;
		}

	    private class SavedState : BaseSavedState
		{
			internal int currentPage;

			public SavedState(IParcelable superState)
                : base(superState) { }

			internal SavedState(Parcel @in) : base(@in)
			{
				currentPage = @in.ReadInt();
			}

            public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
			{
				base.WriteToParcel(dest, flags);
				dest.WriteInt(currentPage);
			}
            
            //@SuppressWarnings("UnusedDeclaration") public static final Creator<SavedState> CREATOR = new CreatorAnonymousInnerClassHelper();
            //public static readonly ICreator<SavedState> CREATOR = new CreatorAnonymousInnerClassHelper();

            //private class CreatorAnonymousInnerClassHelper : Creator<SavedState>
            //{
            //    public CreatorAnonymousInnerClassHelper()
            //    {
            //    }

            //    public override SavedState createFromParcel(Parcel @in)
            //    {
            //        return new SavedState(@in);
            //    }

            //    public override SavedState[] newArray(int size)
            //    {
            //        return new SavedState[size];
            //    }
            //}
		}

		public virtual PickerLinearLayout TitleView
		{
			set
			{
				mTitleView = value;
				Invalidate();
			}
		}
	}
}