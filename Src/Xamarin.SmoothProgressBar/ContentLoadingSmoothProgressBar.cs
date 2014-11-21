using System;
using Android.Content;
using Android.Util;
using Android.Views;

namespace Xamarin
{
    /// <summary>
    /// This is a copy of the ContentLoadingProgressBar from the support library, but extends
    /// SmoothProgressBar.
    /// </summary>
    public class ContentLoadingSmoothProgressBar : SmoothProgressBar
    {
        private const int MinShowTime = 500; // ms
        private const int MinDelay = 500; // ms

        private long mStartTime = -1;

        private bool mPostedHide;

        private bool mPostedShow;

        private bool mDismissed;

        private readonly Action mDelayedHide;
        private readonly Action mDelayedShow;

        public ContentLoadingSmoothProgressBar(Context context)
            : this(context, null) { }

        public ContentLoadingSmoothProgressBar(Context context, IAttributeSet attrs)
            : base(context, attrs, 0)
        {
            mDelayedShow = () =>
            {
                mPostedShow = false;
                if (!mDismissed)
                {
                    mStartTime = DateTimeHelperClass.CurrentUnixTimeMillis();
                    Visibility = ViewStates.Visible;
                }
            };

            mDelayedHide = () =>
            {
                mPostedHide = false;
                mStartTime = -1;
                Visibility = ViewStates.Gone;
            };
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            RemoveCallbacks();
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();

            RemoveCallbacks();
        }

        public void RemoveCallbacks()
        {
            RemoveCallbacks(mDelayedHide);
            RemoveCallbacks(mDelayedShow);
        }

        /// <summary>
        /// Hide the progress view if it is visible. The progress view will not be
        /// hidden until it has been shown for at least a minimum show time. If the
        /// progress view was not yet visible, cancels showing the progress view.
        /// </summary>
        public virtual void Hide()
        {
            mDismissed = true;
            RemoveCallbacks(mDelayedShow);

            var diff = DateTimeHelperClass.CurrentUnixTimeMillis() - mStartTime;
            if (diff >= MinShowTime || mStartTime == -1)
            {
                // The progress spinner has been shown long enough
                // OR was not shown yet. If it wasn't shown yet,
                // it will just never be shown.
                Visibility = ViewStates.Gone;
            }
            else
            {
                // The progress spinner is shown, but not long enough,
                // so put a delayed message in to hide it when its been
                // shown long enough.
                if (!mPostedHide)
                {
                    PostDelayed(mDelayedHide, MinShowTime - diff);
                    mPostedHide = true;
                }
            }
        }

        /// <summary>
        /// Show the progress view after waiting for a minimum delay. If
        /// during that time, hide() is called, the view is never made visible.
        /// </summary>
        public virtual void Show()
        {
            // Reset the start time.
            mStartTime = -1;
            mDismissed = false;
            RemoveCallbacks(mDelayedHide);
            if (!mPostedShow)
            {
                PostDelayed(mDelayedShow, MinDelay);
                mPostedShow = true;
            }
        }
    }
}