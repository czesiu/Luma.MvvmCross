/*
 * Copyright 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Xamarin.NineOldAndroids.Animations;
using Xamarin.NineOldAndroids.Views.Animations;
using ViewPropertyAnimator = Xamarin.NineOldAndroids.Views.ViewPropertyAnimator;

namespace Xamarin.SuperListView
{
	/// <summary>
    /// A <seealso cref="View.IOnTouchListener"/> that makes the list items in a <seealso cref="ListView"/>
	/// dismissable. <seealso cref="ListView"/> is given special treatment because by default it handles touches
	/// for its list items... i.e. it's in charge of drawing the pressed state (the list selector),
	/// handling list item clicks, etc.
	/// 
	/// <para>After creating the listener, the caller should also call
	/// <seealso cref="ListView#setOnScrollListener(AbsListView.OnScrollListener)"/>, passing
	/// in the scroll listener returned by <seealso cref="#makeScrollListener()"/>. If a scroll listener is
	/// already assigned, the caller should still pass scroll changes through to this listener. This will
	/// ensure that this <seealso cref="SwipeDismissListViewTouchListener"/> is paused during list view
	/// scrolling.</para>
	/// 
	/// <para>Example usage:</para>
	/// 
	/// <pre>
	/// SwipeDismissListViewTouchListener touchListener =
	///         new SwipeDismissListViewTouchListener(
	///                 listView,
	///                 new SwipeDismissListViewTouchListener.OnDismissCallback() {
	///                     public void onDismiss(ListView listView, int[] reverseSortedPositions) {
	///                         for (int position : reverseSortedPositions) {
	///                             adapter.remove(adapter.getItem(position));
	///                         }
	///                         adapter.notifyDataSetChanged();
	///                     }
	///                 });
	/// listView.setOnTouchListener(touchListener);
	/// listView.setOnScrollListener(touchListener.makeScrollListener());
	/// </pre>
	/// 
	/// <para>This class Requires API level 12 or later due to use of {@link
	/// ViewPropertyAnimator}.</para>
	/// 
    /// <para>For a generalized <seealso cref="View.IOnTouchListener"/> that makes any view dismissable,
	/// see <seealso cref="SwipeDismissTouchListener"/>.</para>
	/// </summary>
	/// <seealso cref="SwipeDismissTouchListener" />
	public class SwipeDismissListViewTouchListener : Java.Lang.Object, View.IOnTouchListener
	{
		// Cached ViewConfiguration and system-wide constant values
		private int mSlop;
		private int mMinFlingVelocity;
		private int mMaxFlingVelocity;
		private long mAnimationTime;

		// Fixed properties
		private Android.Widget.ListView mListView;
		private DismissCallbacks mCallbacks;
		private int mViewWidth = 1; // 1 and not 0 to prevent dividing by zero

		// Transient properties
		private List<PendingDismissData> mPendingDismisses = new List<PendingDismissData>();
		private int mDismissAnimationRefCount = 0;
		private float mDownX;
		private float mDownY;
		private bool mSwiping;
		private int mSwipingSlop;
		private VelocityTracker mVelocityTracker;
		private int mDownPosition;
		private View mDownView;
		private AnimatorProxy mDownViewProxy;
		private bool mPaused;

		/// <summary>
		/// The callback interface used by <seealso cref="SwipeDismissListViewTouchListener"/> to inform its client
		/// about a successful dismissal of one or more list item positions.
		/// </summary>
		public interface DismissCallbacks
		{
			/// <summary>
			/// Called to determine whether the given position can be dismissed.
			/// </summary>
			bool canDismiss(int position);

			/// <summary>
			/// Called when the user has indicated they she would like to dismiss one or more list item
			/// positions.
			/// </summary>
			/// <param name="listView">               The originating <seealso cref="Android.Widget.ListView"/>. </param>
			/// <param name="reverseSortedPositions"> An array of positions to dismiss, sorted in descending
			///                               order for convenience. </param>
			void onDismiss(Android.Widget.ListView listView, int[] reverseSortedPositions);
		}

		/// <summary>
		/// Constructs a new swipe-to-dismiss touch listener for the given list view.
		/// </summary>
		/// <param name="listView">  The list view whose items should be dismissable. </param>
		/// <param name="callbacks"> The callback to trigger when the user has indicated that she would like to
		///                  dismiss one or more list items. </param>
		public SwipeDismissListViewTouchListener(Android.Widget.ListView listView, DismissCallbacks callbacks)
		{
			var vc = ViewConfiguration.Get(listView.Context);
			mSlop = vc.ScaledTouchSlop;
			mMinFlingVelocity = vc.ScaledMinimumFlingVelocity * 16;
			mMaxFlingVelocity = vc.ScaledMaximumFlingVelocity;
			mAnimationTime = listView.Context.Resources.GetInteger(Android.Resource.Integer.ConfigShortAnimTime);
			mListView = listView;
			mCallbacks = callbacks;
		}

		/// <summary>
		/// Enables or disables (pauses or resumes) watching for swipe-to-dismiss gestures.
		/// </summary>
		/// <param name="enabled"> Whether or not to watch for gestures. </param>
		public virtual bool Enabled
		{
			set
			{
				mPaused = !value;
			}
		}

		/// <summary>
		/// Returns an <seealso cref="AbsListView.OnScrollListener"/> to be added to the {@link
		/// ListView} using <seealso cref="ListView#setOnScrollListener(AbsListView.OnScrollListener)"/>.
		/// If a scroll listener is already assigned, the caller should still pass scroll changes through
		/// to this listener. This will ensure that this <seealso cref="SwipeDismissListViewTouchListener"/> is
		/// paused during list view scrolling.</p>
		/// </summary>
		/// <seealso cref= SwipeDismissListViewTouchListener </seealso>
		public virtual AbsListView.IOnScrollListener makeScrollListener()
		{
			return new OnScrollListenerAnonymousInnerClassHelper(this);
		}

		private class OnScrollListenerAnonymousInnerClassHelper : Java.Lang.Object, AbsListView.IOnScrollListener
		{
			private readonly SwipeDismissListViewTouchListener _outerInstance;

			public OnScrollListenerAnonymousInnerClassHelper(SwipeDismissListViewTouchListener outerInstance)
			{
				this._outerInstance = outerInstance;
			}

			public void OnScrollStateChanged(AbsListView absListView, ScrollState scrollState)
			{
				_outerInstance.Enabled = scrollState != ScrollState.TouchScroll;
			}

			public void OnScroll(AbsListView absListView, int i, int i1, int i2)
			{
			}
		}

		public bool OnTouch(View view, MotionEvent motionEvent)
		{
			if (mViewWidth < 2)
			{
				mViewWidth = mListView.Width;
			}

			switch ((MotionEventActions)MotionEventCompat.GetActionMasked(motionEvent))
			{
				case MotionEventActions.Down:
				{
					if (mPaused)
					{
						return false;
					}

					// TODO: ensure this is a finger, and set a flag

					// Find the child view that was touched (perform a hit test)
					Rect rect = new Rect();
					int childCount = mListView.ChildCount;
					int[] listViewCoords = new int[2];
					mListView.GetLocationOnScreen(listViewCoords);
					int x = (int) motionEvent.RawX - listViewCoords[0];
					int y = (int) motionEvent.RawY - listViewCoords[1];
					View child;
					for (int i = 0; i < childCount; i++)
					{
						child = mListView.GetChildAt(i);
						child.GetHitRect(rect);
						if (rect.Contains(x, y))
						{
							mDownView = child;
							mDownViewProxy = AnimatorProxy.Wrap(child);
							break;
						}
					}

					if (mDownView != null)
					{
						mDownX = motionEvent.RawX;
						mDownY = motionEvent.RawY;
						mDownPosition = mListView.GetPositionForView(mDownView);
						if (mCallbacks.canDismiss(mDownPosition))
						{
							mVelocityTracker = VelocityTracker.Obtain();
							mVelocityTracker.AddMovement(motionEvent);
						}
						else
						{
							mDownView = null;
							mDownViewProxy = null;
						}
					}
					return false;
				}

                case MotionEventActions.Cancel:
				{
					if (mVelocityTracker == null)
					{
						break;
					}

					if (mDownView != null && mSwiping)
					{
						// cancel
                        ViewPropertyAnimator.Animate(mDownView).TranslationX(0).Alpha(1).SetDuration(mAnimationTime).SetListener(null);
					}
					mVelocityTracker.Recycle();
					mVelocityTracker = null;
					mDownX = 0;
					mDownY = 0;
					mDownView = null;
					mDownViewProxy = null;
					mDownPosition = Android.Widget.ListView.InvalidPosition;
					mSwiping = false;
					break;
				}

                case MotionEventActions.Up:
				{
					if (mVelocityTracker == null)
					{
						break;
					}

					float deltaX = motionEvent.RawX - mDownX;
					mVelocityTracker.AddMovement(motionEvent);
					mVelocityTracker.ComputeCurrentVelocity(1000);
					float velocityX = mVelocityTracker.XVelocity;
					float absVelocityX = Math.Abs(velocityX);
					float absVelocityY = Math.Abs(mVelocityTracker.YVelocity);
					bool dismiss = false;
					bool dismissRight = false;
					if (Math.Abs(deltaX) > mViewWidth / 2 && mSwiping)
					{
						dismiss = true;
						dismissRight = deltaX > 0;
					}
					else if (mMinFlingVelocity <= absVelocityX && absVelocityX <= mMaxFlingVelocity && absVelocityY < absVelocityX && mSwiping)
					{
						// dismiss only if flinging in the same direction as dragging
						dismiss = (velocityX < 0) == (deltaX < 0);
						dismissRight = mVelocityTracker.XVelocity > 0;
					}
					if (dismiss && mDownPosition != AdapterView.InvalidPosition)
					{
						// dismiss
						View downView = mDownView; // mDownView gets null'd before animation ends
						int downPosition = mDownPosition;
						++mDismissAnimationRefCount;
                        ViewPropertyAnimator.Animate(mDownView).TranslationX(dismissRight ? mViewWidth : -mViewWidth).Alpha(0).SetDuration(mAnimationTime).SetListener(new AnimatorListenerAdapterAnonymousInnerClassHelper0(this, downView, downPosition));
					}
					else
					{
                        ViewPropertyAnimator.Animate(mDownView).TranslationX(0).Alpha(1).SetDuration(mAnimationTime).SetListener(null);
					}
					mVelocityTracker.Recycle();
					mVelocityTracker = null;
					mDownX = 0;
					mDownY = 0;
					mDownView = null;
					mDownViewProxy = null;
					mDownPosition = AdapterView.InvalidPosition;
					mSwiping = false;
					break;
				}

                case MotionEventActions.Move:
				{
					if (mVelocityTracker == null || mPaused)
					{
						break;
					}

					mVelocityTracker.AddMovement(motionEvent);
					float deltaX = motionEvent.RawX - mDownX;
					float deltaY = motionEvent.RawY - mDownY;
					if (Math.Abs(deltaX) > mSlop && Math.Abs(deltaY) < Math.Abs(deltaX) / 2)
					{
						mSwiping = true;
						mSwipingSlop = (deltaX > 0 ? mSlop : -mSlop);
						mListView.RequestDisallowInterceptTouchEvent(true);

						// Cancel ListView's touch (un-highlighting the item)
						MotionEvent cancelEvent = MotionEvent.Obtain(motionEvent);
						cancelEvent.Action = MotionEventActions.Cancel | (MotionEventActions)(MotionEventCompat.GetActionIndex(motionEvent) << MotionEventCompat.ActionPointerIndexShift);
						mListView.OnTouchEvent(cancelEvent);
						cancelEvent.Recycle();
					}

					if (mSwiping)
					{
						mDownViewProxy.TranslationX = deltaX - mSwipingSlop;
						mDownViewProxy.Alpha = Math.Max(0f, Math.Min(1f, 1f - 2f * Math.Abs(deltaX) / mViewWidth));
						return true;
					}
					break;
				}
			}
			return false;
		}

		private class AnimatorListenerAdapterAnonymousInnerClassHelper0 : AnimatorListenerAdapter
		{
			private readonly SwipeDismissListViewTouchListener outerInstance;

			private View downView;
			private int downPosition;

			public AnimatorListenerAdapterAnonymousInnerClassHelper0(SwipeDismissListViewTouchListener outerInstance, View downView, int downPosition)
			{
				this.outerInstance = outerInstance;
				this.downView = downView;
				this.downPosition = downPosition;
			}

			public override void OnAnimationEnd(Animator animation)
			{
				outerInstance.performDismiss(downView, downPosition);
			}
		}

		internal class PendingDismissData : IComparable<PendingDismissData>
		{
			private readonly SwipeDismissListViewTouchListener _outerInstance;

			public int position;
			public View view;

			public PendingDismissData(SwipeDismissListViewTouchListener outerInstance, int position, View view)
			{
				this._outerInstance = outerInstance;
				this.position = position;
				this.view = view;
			}

			public virtual int CompareTo(PendingDismissData other)
			{
				// Sort by descending position
				return other.position - position;
			}
		}

		private void performDismiss(View dismissView, int dismissPosition)
		{
			// Animate the dismissed list item to zero-height and fire the dismiss callback when
			// all dismissed list item animations have completed. This triggers layout on each animation
			// frame; in the future we may want to do something smarter and more performant.
			ViewGroup.LayoutParams lp = dismissView.LayoutParameters;

			int originalHeight = dismissView.Height;

			var animator = ValueAnimator.OfInt(originalHeight, 1);

		    animator.SetDuration(mAnimationTime);

			animator.AddListener(new AnimatorListenerAdapterAnonymousInnerClassHelper(this, lp, originalHeight));

			animator.AddUpdateListener(new AnimatorUpdateListenerAnonymousInnerClassHelper2(this, dismissView, lp));

			mPendingDismisses.Add(new PendingDismissData(this, dismissPosition, dismissView));

			animator.Start();
		}

		private class AnimatorListenerAdapterAnonymousInnerClassHelper : AnimatorListenerAdapter
		{
			private readonly SwipeDismissListViewTouchListener outerInstance;

			private ViewGroup.LayoutParams lp;
			private int originalHeight;

			public AnimatorListenerAdapterAnonymousInnerClassHelper(SwipeDismissListViewTouchListener outerInstance, ViewGroup.LayoutParams lp, int originalHeight)
			{
				this.outerInstance = outerInstance;
				this.lp = lp;
				this.originalHeight = originalHeight;
			}

			public override void OnAnimationEnd(Animator animation)
			{
				--outerInstance.mDismissAnimationRefCount;
				if (outerInstance.mDismissAnimationRefCount == 0)
				{
					// No active animations, process all pending dismisses.
					// Sort by descending position
					outerInstance.mPendingDismisses.Sort();

					int[] dismissPositions = new int[outerInstance.mPendingDismisses.Count];
					for (int i = outerInstance.mPendingDismisses.Count - 1; i >= 0; i--)
					{
						dismissPositions[i] = outerInstance.mPendingDismisses[i].position;
					}
					outerInstance.mCallbacks.onDismiss(outerInstance.mListView, dismissPositions);

					// Reset mDownPosition to avoid MotionEvent.ACTION_UP trying to start a dismiss 
					// animation with a stale position
					outerInstance.mDownPosition = Android.Widget.ListView.InvalidPosition;

					ViewGroup.LayoutParams lp;
					foreach (PendingDismissData pendingDismiss in outerInstance.mPendingDismisses)
					{
						// Reset view presentation
						AnimatorProxy.Wrap(pendingDismiss.view).Alpha = 1f;
						AnimatorProxy.Wrap(pendingDismiss.view).TranslationX = 0;
						lp = pendingDismiss.view.LayoutParameters;
						lp.Height = originalHeight;
						pendingDismiss.view.LayoutParameters = lp;
					}

					// Send a cancel event
					var time = SystemClock.UptimeMillis();
					MotionEvent cancelEvent = MotionEvent.Obtain(time, time, MotionEventActions.Cancel, 0, 0, 0);
					outerInstance.mListView.DispatchTouchEvent(cancelEvent);

					outerInstance.mPendingDismisses.Clear();
				}
			}
		}

		private class AnimatorUpdateListenerAnonymousInnerClassHelper2 : Java.Lang.Object, ValueAnimator.IAnimatorUpdateListener
		{
			private readonly SwipeDismissListViewTouchListener _outerInstance;

			private View dismissView;
			private ViewGroup.LayoutParams lp;

			public AnimatorUpdateListenerAnonymousInnerClassHelper2(SwipeDismissListViewTouchListener outerInstance, View dismissView, ViewGroup.LayoutParams lp)
			{
				_outerInstance = outerInstance;
				this.dismissView = dismissView;
				this.lp = lp;
			}

			public void OnAnimationUpdate(ValueAnimator valueAnimator)
			{
				lp.Height = (int)valueAnimator.AnimatedValue;
				dismissView.LayoutParameters = lp;
			}
		}
	}
}