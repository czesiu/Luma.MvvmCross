using Android.Content;
using Android.Graphics;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Views.Animations;

namespace Xamarin
{
    /*
     *    The MIT License (MIT)
     *
     *   Copyright (c) 2014 Danylyk Dmytro
     *
     *   Permission is hereby granted, free of charge, to any person obtaining a copy
     *   of this software and associated documentation files (the "Software"), to deal
     *   in the Software without restriction, including without limitation the rights
     *   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
     *   copies of the Software, and to permit persons to whom the Software is
     *   furnished to do so, subject to the following conditions:
     *
     *   The above copyright notice and this permission notice shall be included in all
     *   copies or substantial portions of the Software.
     *
     *   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
     *   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
     *   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
     *   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
     *   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
     *   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
     *   SOFTWARE.
     */

    public class ActionProcessButton : ProcessButton
    {
        private ProgressBar _mProgressBar;

        private Color _mColor1;
        private Color _mColor2;
        private Color _mColor3;
        private Color _mColor4;

        public ActionProcessButton(Context context)
            : base(context)
        {
            init(context);
        }

        public ActionProcessButton(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            init(context);
        }

        public ActionProcessButton(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            init(context);
        }

        private void init(Context context)
        {
            var res = context.Resources;

            IsIndeterminate = true;

            _mColor1 = res.GetColor(Resource.Color.holo_blue_bright);
            _mColor2 = res.GetColor(Resource.Color.holo_green_light);
            _mColor3 = res.GetColor(Resource.Color.holo_orange_light);
            _mColor4 = res.GetColor(Resource.Color.holo_red_light);
        }

        public bool IsIndeterminate { get; set; }

        public virtual void SetColorScheme(Color color1, Color color2, Color color3, Color color4)
        {
            _mColor1 = color1;
            _mColor2 = color2;
            _mColor3 = color3;
            _mColor4 = color4;
        }

        public override void DrawProgress(Canvas canvas)
        {
            if (Background != NormalDrawable)
            {
                SetBackgroundDrawable(NormalDrawable);
            }

            if (IsIndeterminate)
            {
                DrawEndlessProgress(canvas);
            }
            else
            {
                DrawLineProgress(canvas);
            }
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            if (_mProgressBar != null)
            {
                SetupProgressBarBounds();
            }
        }

        private void DrawLineProgress(Canvas canvas)
        {
            var scale = (float)Progress / MaxProgress;
            var indicatorWidth = MeasuredWidth * scale;

            var indicatorHeightPercent = 0.05; // 5%
            var bottom = (int)(MeasuredHeight - MeasuredHeight * indicatorHeightPercent);
            ProgressDrawable.Bounds = new Rect(0, bottom, (int)indicatorWidth, MeasuredHeight);
            ProgressDrawable.Draw(canvas);
        }

        private void DrawEndlessProgress(Canvas canvas)
        {
            if (_mProgressBar == null)
            {
                _mProgressBar = new ProgressBar(this);
                SetupProgressBarBounds();
                _mProgressBar.SetColorScheme(_mColor1, _mColor2, _mColor3, _mColor4);
                _mProgressBar.Start();
            }

            _mProgressBar.Draw(canvas);
        }

        private void SetupProgressBarBounds()
        {
            var indicatorHeight = GetDimension(Resource.Dimension.layer_padding);
            var bottom = (int)(MeasuredHeight - indicatorHeight);
            _mProgressBar.SetBounds(0, bottom, MeasuredWidth, MeasuredHeight);
        }

        public class ProgressBar
        {
            // Default progress animation colors are grays.
            internal Color COLOR1 = new Color(0, 0, 0, 0xb3);
            internal Color COLOR2 = new Color(0, 0, 0, 0x80);
            internal Color COLOR3 = new Color(0, 0, 0, 0x4d);
            internal Color COLOR4 = new Color(0, 0, 0, 0x1a);

            // The duration of the animation cycle.
            internal const int AnimationDurationMs = 2000;

            // The duration of the animation to clear the bar.
            internal const int FinishAnimationDurationMs = 1000;

            // Interpolator for varying the speed of the animation.
            internal static readonly IInterpolator Interpolator = new AccelerateDecelerateInterpolator();

            internal readonly Paint mPaint = new Paint();
            internal readonly RectF mClipRect = new RectF();
            internal float mTriggerPercentage;
            internal long mStartTime;
            internal long mFinishTime;
            internal bool mRunning;

            // Colors used when rendering the animation,
            internal Color mColor1;
            internal Color mColor2;
            internal Color mColor3;
            internal Color mColor4;
            internal View mParent;

            internal Rect mBounds = new Rect();

            public ProgressBar(View parent)
            {
                mParent = parent;
                mColor1 = COLOR1;
                mColor2 = COLOR2;
                mColor3 = COLOR3;
                mColor4 = COLOR4;
            }

            /// <summary>
            /// Set the four colors used in the progress animation. The first color will
            /// also be the color of the bar that grows in response to a user swipe
            /// gesture.
            /// </summary>
            /// <param name="color1"> Integer representation of a color. </param>
            /// <param name="color2"> Integer representation of a color. </param>
            /// <param name="color3"> Integer representation of a color. </param>
            /// <param name="color4"> Integer representation of a color. </param>
            internal virtual void SetColorScheme(Color color1, Color color2, Color color3, Color color4)
            {
                mColor1 = color1;
                mColor2 = color2;
                mColor3 = color3;
                mColor4 = color4;
            }

            /// <summary>
            /// Start showing the progress animation.
            /// </summary>
            internal virtual void Start()
            {
                if (!mRunning)
                {
                    mTriggerPercentage = 0;
                    mStartTime = AnimationUtils.CurrentAnimationTimeMillis();
                    mRunning = true;
                    mParent.PostInvalidate();
                }
            }

            internal virtual void Draw(Canvas canvas)
            {
                var width = mBounds.Width();
                var height = mBounds.Height();
                var cx = width / 2;
                var cy = height / 2;
                var drawTriggerWhileFinishing = false;
                var restoreCount = canvas.Save();
                canvas.ClipRect(mBounds);

                if (mRunning || (mFinishTime > 0))
                {
                    long now = AnimationUtils.CurrentAnimationTimeMillis();
                    long elapsed = (now - mStartTime) % AnimationDurationMs;
                    long iterations = (now - mStartTime) / AnimationDurationMs;
                    float rawProgress = (elapsed / (AnimationDurationMs / 100f));

                    // If we're not running anymore, that means we're running through
                    // the finish animation.
                    if (!mRunning)
                    {
                        // If the finish animation is done, don't draw anything, and
                        // don't repost.
                        if ((now - mFinishTime) >= FinishAnimationDurationMs)
                        {
                            mFinishTime = 0;
                            return;
                        }

                        // Otherwise, use a 0 opacity alpha layer to clear the animation
                        // from the inside out. This layer will prevent the circles from
                        // drawing within its bounds.
                        long finishElapsed = (now - mFinishTime) % FinishAnimationDurationMs;
                        float finishProgress = (finishElapsed / (FinishAnimationDurationMs / 100f));
                        float pct = (finishProgress / 100f);
                        // Radius of the circle is half of the screen.
                        float clearRadius = width / 2 * Interpolator.GetInterpolation(pct);
                        mClipRect.Set(cx - clearRadius, 0, cx + clearRadius, height);
                        canvas.SaveLayerAlpha(mClipRect, 0, 0);
                        // Only draw the trigger if there is a space in the center of
                        // this refreshing view that needs to be filled in by the
                        // trigger. If the progress view is just still animating, let it
                        // continue animating.
                        drawTriggerWhileFinishing = true;
                    }

                    // First fill in with the last color that would have finished drawing.
                    if (iterations == 0)
                    {
                        canvas.DrawColor(mColor1);
                    }
                    else
                    {
                        if (rawProgress >= 0 && rawProgress < 25)
                        {
                            canvas.DrawColor(mColor4);
                        }
                        else if (rawProgress >= 25 && rawProgress < 50)
                        {
                            canvas.DrawColor(mColor1);
                        }
                        else if (rawProgress >= 50 && rawProgress < 75)
                        {
                            canvas.DrawColor(mColor2);
                        }
                        else
                        {
                            canvas.DrawColor(mColor3);
                        }
                    }

                    // Then draw up to 4 overlapping concentric circles of varying radii, based on how far
                    // along we are in the cycle.
                    // progress 0-50 draw mColor2
                    // progress 25-75 draw mColor3
                    // progress 50-100 draw mColor4
                    // progress 75 (wrap to 25) draw mColor1
                    if ((rawProgress >= 0 && rawProgress <= 25))
                    {
                        float pct = (((rawProgress + 25) * 2) / 100f);
                        DrawCircle(canvas, cx, cy, mColor1, pct);
                    }
                    if (rawProgress >= 0 && rawProgress <= 50)
                    {
                        float pct = ((rawProgress * 2) / 100f);
                        DrawCircle(canvas, cx, cy, mColor2, pct);
                    }
                    if (rawProgress >= 25 && rawProgress <= 75)
                    {
                        float pct = (((rawProgress - 25) * 2) / 100f);
                        DrawCircle(canvas, cx, cy, mColor3, pct);
                    }
                    if (rawProgress >= 50 && rawProgress <= 100)
                    {
                        float pct = (((rawProgress - 50) * 2) / 100f);
                        DrawCircle(canvas, cx, cy, mColor4, pct);
                    }
                    if ((rawProgress >= 75 && rawProgress <= 100))
                    {
                        float pct = (((rawProgress - 75) * 2) / 100f);
                        DrawCircle(canvas, cx, cy, mColor1, pct);
                    }
                    if (mTriggerPercentage > 0 && drawTriggerWhileFinishing)
                    {
                        // There is some portion of trigger to draw. Restore the canvas,
                        // then draw the trigger. Otherwise, the trigger does not appear
                        // until after the bar has finished animating and appears to
                        // just jump in at a larger width than expected.
                        canvas.RestoreToCount(restoreCount);
                        restoreCount = canvas.Save();
                        canvas.ClipRect(mBounds);
                        DrawTrigger(canvas, cx, cy);
                    }
                    // Keep running until we finish out the last cycle.
                    ViewCompat.PostInvalidateOnAnimation(mParent);
                }
                else
                {
                    // Otherwise if we're in the middle of a trigger, draw that.
                    if (mTriggerPercentage > 0 && mTriggerPercentage <= 1.0)
                    {
                        DrawTrigger(canvas, cx, cy);
                    }
                }
                canvas.RestoreToCount(restoreCount);
            }

            internal virtual void DrawTrigger(Canvas canvas, int cx, int cy)
            {
                mPaint.Color = mColor1;
                canvas.DrawCircle(cx, cy, cx * mTriggerPercentage, mPaint);
            }

            /// <summary>
            /// Draws a circle centered in the view.
            /// </summary>
            /// <param name="canvas"> the canvas to draw on </param>
            /// <param name="cx"> the center x coordinate </param>
            /// <param name="cy"> the center y coordinate </param>
            /// <param name="color"> the color to draw </param>
            /// <param name="pct"> the percentage of the view that the circle should cover </param>
            internal virtual void DrawCircle(Canvas canvas, float cx, float cy, Color color, float pct)
            {
                mPaint.Color = color;
                canvas.Save();
                canvas.Translate(cx, cy);
                var radiusScale = Interpolator.GetInterpolation(pct);
                canvas.Scale(radiusScale, radiusScale);
                canvas.DrawCircle(0, 0, cx, mPaint);
                canvas.Restore();
            }

            /// <summary>
            /// Set the drawing bounds of this SwipeProgressBar.
            /// </summary>
            internal virtual void SetBounds(int left, int top, int right, int bottom)
            {
                mBounds.Left = left;
                mBounds.Top = top;
                mBounds.Right = right;
                mBounds.Bottom = bottom;
            }
        }

    }

}