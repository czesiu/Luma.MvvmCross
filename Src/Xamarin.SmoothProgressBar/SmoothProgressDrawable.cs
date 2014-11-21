using System;
using System.Globalization;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views.Animations;
using Java.Lang;
using Math = System.Math;

namespace Xamarin
{
    public class SmoothProgressDrawable : Drawable, IAnimatable
    {
        public interface ICallbacks
        {
            void OnStop();

            void OnStart();
        }

        private const long FrameDuration = 1000 / 60;
        private const float OffsetPerFrame = 0.01f;

        private readonly Rect _fBackgroundRect = new Rect();
        private ICallbacks _mCallbacks;
        private IInterpolator _mInterpolator;
        private Rect _mBounds;
        private readonly Paint mPaint;
        private int[] _mColors;
        private int _mColorsIndex;
        private bool _isRunning;
        private float _mCurrentOffset;
        private float _mFinishingOffset;
        private int _mSeparatorLength;
        private int _mSectionsCount;
        private float _mSpeed;
        private float _mProgressiveStartSpeed;
        private float _mProgressiveStopSpeed;
        private bool _mReversed;
        private bool _mNewTurn;
        private bool _mMirrorMode;
        private float _mMaxOffset;
        private bool _mFinishing;
        private bool _mProgressiveStartActivated;
        private int _mStartSection;
        private int _mCurrentSections;
        private readonly float _mStrokeWidth;
        private Drawable _mBackgroundDrawable;
        private bool _mUseGradients;
        private int[] _mLinearGradientColors;
        private float[] _mLinearGradientPositions;

        private SmoothProgressDrawable(IInterpolator interpolator, int sectionsCount, int separatorLength, int[] colors, float strokeWidth, float speed, float progressiveStartSpeed, float progressiveStopSpeed, bool reversed, bool mirrorMode, ICallbacks callbacks, bool progressiveStartActivated, Drawable backgroundDrawable, bool useGradients)
        {
            _isRunning = false;
            _mInterpolator = interpolator;
            _mSectionsCount = sectionsCount;
            _mStartSection = 0;
            _mCurrentSections = _mSectionsCount;
            _mSeparatorLength = separatorLength;
            _mSpeed = speed;
            _mProgressiveStartSpeed = progressiveStartSpeed;
            _mProgressiveStopSpeed = progressiveStopSpeed;
            _mReversed = reversed;
            _mColors = colors;
            _mColorsIndex = 0;
            _mMirrorMode = mirrorMode;
            _mFinishing = false;
            _mBackgroundDrawable = backgroundDrawable;
            _mStrokeWidth = strokeWidth;

            _mMaxOffset = 1f / _mSectionsCount;

            mPaint = new Paint { StrokeWidth = strokeWidth };
            mPaint.SetStyle(Paint.Style.Stroke);
            mPaint.Dither = false;
            mPaint.AntiAlias = false;

            _mProgressiveStartActivated = progressiveStartActivated;
            _mCallbacks = callbacks;

            _mUseGradients = useGradients;
            refreshLinearGradientOptions();

            _mUpdater = () =>
            {
                if (Finishing)
                {
                    _mFinishingOffset += (OffsetPerFrame * _mProgressiveStopSpeed);
                    _mCurrentOffset += (OffsetPerFrame * _mProgressiveStopSpeed);
                    if (_mFinishingOffset >= 1f)
                    {
                        Stop();
                    }
                }
                else if (Starting)
                {
                    _mCurrentOffset += (OffsetPerFrame * _mProgressiveStartSpeed);
                }
                else
                {
                    _mCurrentOffset += (OffsetPerFrame * _mSpeed);
                }

                if (_mCurrentOffset >= _mMaxOffset)
                {
                    _mNewTurn = true;
                    _mCurrentOffset -= _mMaxOffset;
                }

                if (IsRunning)
                {
                    ScheduleSelf(_mUpdater, SystemClock.UptimeMillis() + FrameDuration);
                }

                InvalidateSelf();
            };
        }

        public virtual IInterpolator Interpolator
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("Interpolator cannot be null");
                }
                _mInterpolator = value;
                InvalidateSelf();
            }
        }

        public virtual int[] Colors
        {
            get
            {
                return _mColors;
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    throw new ArgumentException("Colors cannot be null or empty");
                }
                _mColorsIndex = 0;
                _mColors = value;
                refreshLinearGradientOptions();
                InvalidateSelf();
            }
        }

        public virtual int Color
        {
            set
            {
                Colors = new[] { value };
            }
        }

        public virtual float Speed
        {
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Speed must be >= 0");
                }
                _mSpeed = value;
                InvalidateSelf();
            }
        }

        public virtual float ProgressiveStartSpeed
        {
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("SpeedProgressiveStart must be >= 0");
                }
                _mProgressiveStartSpeed = value;
                InvalidateSelf();
            }
        }

        public virtual float ProgressiveStopSpeed
        {
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("SpeedProgressiveStop must be >= 0");
                }
                _mProgressiveStopSpeed = value;
                InvalidateSelf();
            }
        }

        public virtual int SectionsCount
        {
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("SectionsCount must be > 0");
                }
                _mSectionsCount = value;
                _mMaxOffset = 1f / _mSectionsCount;
                _mCurrentOffset %= _mMaxOffset;
                refreshLinearGradientOptions();
                InvalidateSelf();
            }
        }

        public virtual int SeparatorLength
        {
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("SeparatorLength must be >= 0");
                }
                _mSeparatorLength = value;
                InvalidateSelf();
            }
        }

        public virtual float StrokeWidth
        {
            get
            {
                return _mStrokeWidth;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("The strokeWidth must be >= 0");
                }
                mPaint.StrokeWidth = value;
                InvalidateSelf();
            }
        }

        public virtual bool Reversed
        {
            set
            {
                if (_mReversed == value)
                {
                    return;
                }
                _mReversed = value;
                InvalidateSelf();
            }
        }

        public virtual bool MirrorMode
        {
            set
            {
                if (_mMirrorMode == value)
                {
                    return;
                }
                _mMirrorMode = value;
                InvalidateSelf();
            }
        }

        public virtual Drawable BackgroundDrawable
        {
            get
            {
                return _mBackgroundDrawable;
            }
            set
            {
                if (_mBackgroundDrawable == value)
                {
                    return;
                }
                _mBackgroundDrawable = value;
                InvalidateSelf();
            }
        }

        public virtual bool ProgressiveStartActivated
        {
            set
            {
                _mProgressiveStartActivated = value;
            }
        }

        public virtual bool UseGradients
        {
            set
            {
                if (_mUseGradients == value)
                {
                    return;
                }

                _mUseGradients = value;
                refreshLinearGradientOptions();
                InvalidateSelf();
            }
        }

        protected internal virtual void refreshLinearGradientOptions()
        {
            if (_mUseGradients)
            {
                _mLinearGradientColors = new int[_mSectionsCount + 2];
                _mLinearGradientPositions = new float[_mSectionsCount + 2];
            }
            else
            {
                mPaint.SetShader(null);
                _mLinearGradientColors = null;
                _mLinearGradientPositions = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ///////////////////         DRAW

        public override void Draw(Canvas canvas)
        {
            Bounds = canvas.ClipBounds;
            _mBounds = Bounds;

            //new turn
            if (_mNewTurn)
            {
                _mColorsIndex = DecrementColor(_mColorsIndex);
                _mNewTurn = false;

                if (Finishing)
                {
                    _mStartSection++;

                    if (_mStartSection > _mSectionsCount)
                    {
                        Stop();
                        return;
                    }
                }
                if (_mCurrentSections < _mSectionsCount)
                {
                    _mCurrentSections++;
                }
            }

            if (_mUseGradients)
            {
                DrawGradient(canvas);
            }

            DrawStrokes(canvas);
        }

        private void DrawGradient(Canvas canvas)
        {
            var xSectionWidth = 1f / _mSectionsCount;
            var currentIndexColor = _mColorsIndex;

            _mLinearGradientPositions[0] = 0f;
            _mLinearGradientPositions[_mLinearGradientPositions.Length - 1] = 1f;
            var firstColorIndex = currentIndexColor - 1;
            if (firstColorIndex < 0)
            {
                firstColorIndex += _mColors.Length;
            }

            _mLinearGradientColors[0] = _mColors[firstColorIndex];

            for (var i = 0; i < _mSectionsCount; ++i)
            {
                var position = _mInterpolator.GetInterpolation(i * xSectionWidth + _mCurrentOffset);
                _mLinearGradientPositions[i + 1] = position;
                _mLinearGradientColors[i + 1] = _mColors[currentIndexColor];

                currentIndexColor = (currentIndexColor + 1) % _mColors.Length;
            }
            _mLinearGradientColors[_mLinearGradientColors.Length - 1] = _mColors[currentIndexColor];

            var left = _mReversed ? (_mMirrorMode ? Math.Abs(_mBounds.Left - _mBounds.Right) / 2 : _mBounds.Left) : _mBounds.Left;
            var right = _mMirrorMode ? (_mReversed ? _mBounds.Left : Math.Abs(_mBounds.Left - _mBounds.Right) / 2) : _mBounds.Right;
            var top = _mBounds.CenterY() - _mStrokeWidth / 2;
            var bottom = _mBounds.CenterY() + _mStrokeWidth / 2;
            var linearGradient = new LinearGradient(left, top, right, bottom, _mLinearGradientColors, _mLinearGradientPositions, _mMirrorMode ? Shader.TileMode.Mirror : Shader.TileMode.Clamp);

            mPaint.SetShader(linearGradient);
        }

        private void DrawStrokes(Canvas canvas)
        {
            if (_mReversed)
            {
                canvas.Translate(_mBounds.Width(), 0);
                canvas.Scale(-1, 1);
            }

            var prevValue = 0f;
            var boundsWidth = _mBounds.Width();
            if (_mMirrorMode)
            {
                boundsWidth /= 2;
            }
            var width = boundsWidth + _mSeparatorLength + _mSectionsCount;
            var centerY = _mBounds.CenterY();
            var xSectionWidth = 1f / _mSectionsCount;

            float startX;
            float endX;
            float firstX = 0;
            float lastX = 0;
            float prev;
            float end;
            float spaceLength;
            float xOffset;
            float ratioSectionWidth;
            float sectionWidth;
            float drawLength;
            var currentIndexColor = _mColorsIndex;

            if (_mStartSection == _mCurrentSections && _mCurrentSections == _mSectionsCount)
            {
                firstX = canvas.Width;
            }

            for (var i = 0; i <= _mCurrentSections; ++i)
            {
                xOffset = xSectionWidth * i + _mCurrentOffset;
                prev = Math.Max(0f, xOffset - xSectionWidth);
                ratioSectionWidth = Math.Abs(_mInterpolator.GetInterpolation(prev) - _mInterpolator.GetInterpolation(Math.Min(xOffset, 1f)));
                sectionWidth = (int)(width * ratioSectionWidth);

                if (sectionWidth + prev < width)
                {
                    spaceLength = Math.Min(sectionWidth, _mSeparatorLength);
                }
                else
                {
                    spaceLength = 0f;
                }

                drawLength = sectionWidth > spaceLength ? sectionWidth - spaceLength : 0;
                end = prevValue + drawLength;
                if (end > prevValue && i >= _mStartSection)
                {
                    var xFinishingOffset = _mInterpolator.GetInterpolation(Math.Min(_mFinishingOffset, 1f));
                    startX = Math.Max(xFinishingOffset * width, Math.Min(boundsWidth, prevValue));
                    endX = Math.Min(boundsWidth, end);
                    DrawLine(canvas, boundsWidth, startX, centerY, endX, centerY, currentIndexColor);
                    if (i == _mStartSection)
                    { // first loop
                        firstX = startX - _mSeparatorLength;
                    }
                }
                if (i == _mCurrentSections)
                {
                    lastX = prevValue + sectionWidth; //because we want to keep the separator effect
                }

                prevValue = end + spaceLength;
                currentIndexColor = IncrementColor(currentIndexColor);
            }

            DrawBackgroundIfNeeded(canvas, firstX, lastX);
        }

        private void DrawLine(Canvas canvas, int canvasWidth, float startX, float startY, float stopX, float stopY, int currentIndexColor)
        {
            mPaint.Color = new Color(_mColors[currentIndexColor]);

            if (!_mMirrorMode)
            {
                canvas.DrawLine(startX, startY, stopX, stopY, mPaint);
            }
            else
            {
                if (_mReversed)
                {
                    canvas.DrawLine(canvasWidth + startX, startY, canvasWidth + stopX, stopY, mPaint);
                    canvas.DrawLine(canvasWidth - startX, startY, canvasWidth - stopX, stopY, mPaint);
                }
                else
                {
                    canvas.DrawLine(startX, startY, stopX, stopY, mPaint);
                    canvas.DrawLine(canvasWidth * 2 - startX, startY, canvasWidth * 2 - stopX, stopY, mPaint);
                }
            }
        }

        private void DrawBackgroundIfNeeded(Canvas canvas, float firstX, float lastX)
        {
            if (_mBackgroundDrawable == null)
            {
                return;
            }

            _fBackgroundRect.Top = (int)((canvas.Height - _mStrokeWidth) / 2);
            _fBackgroundRect.Bottom = (int)((canvas.Height + _mStrokeWidth) / 2);

            _fBackgroundRect.Left = 0;
            _fBackgroundRect.Right = _mMirrorMode ? canvas.Width / 2 : canvas.Width;
            _mBackgroundDrawable.Bounds = _fBackgroundRect;

            //draw the background if the animation is over
            if (!IsRunning)
            {
                if (_mMirrorMode)
                {
                    canvas.Save();
                    canvas.Translate(canvas.Width / 2, 0);
                    DrawBackground(canvas, 0, _fBackgroundRect.Width());
                    canvas.Scale(-1, 1);
                    DrawBackground(canvas, 0, _fBackgroundRect.Width());
                    canvas.Restore();
                }
                else
                {
                    DrawBackground(canvas, 0, _fBackgroundRect.Width());
                }
                return;
            }

            if (!Finishing && !Starting)
            {
                return;
            }

            if (firstX > lastX)
            {
                float temp = firstX;
                firstX = lastX;
                lastX = temp;
            }

            if (firstX > 0)
            {
                if (_mMirrorMode)
                {
                    canvas.Save();
                    canvas.Translate(canvas.Width / 2, 0);
                    if (_mReversed)
                    {
                        DrawBackground(canvas, 0, firstX);
                        canvas.Scale(-1, 1);
                        DrawBackground(canvas, 0, firstX);
                    }
                    else
                    {
                        DrawBackground(canvas, canvas.Width / 2 - firstX, canvas.Width / 2);
                        canvas.Scale(-1, 1);
                        DrawBackground(canvas, canvas.Width / 2 - firstX, canvas.Width / 2);
                    }
                    canvas.Restore();
                }
                else
                {
                    DrawBackground(canvas, 0, firstX);
                }
            }
            if (lastX <= canvas.Width)
            {
                if (_mMirrorMode)
                {
                    canvas.Save();
                    canvas.Translate(canvas.Width / 2, 0);
                    if (_mReversed)
                    {
                        DrawBackground(canvas, lastX, canvas.Width / 2);
                        canvas.Scale(-1, 1);
                        DrawBackground(canvas, lastX, canvas.Width / 2);
                    }
                    else
                    {
                        DrawBackground(canvas, 0, canvas.Width / 2 - lastX);
                        canvas.Scale(-1, 1);
                        DrawBackground(canvas, 0, canvas.Width / 2 - lastX);
                    }
                    canvas.Restore();
                }
                else
                {
                    DrawBackground(canvas, lastX, canvas.Width);
                }
            }
        }

        private void DrawBackground(Canvas canvas, float fromX, float toX)
        {
            var count = canvas.Save();
            canvas.ClipRect(fromX, (int)((canvas.Height - _mStrokeWidth) / 2), toX, (int)((canvas.Height + _mStrokeWidth) / 2));
            _mBackgroundDrawable.Draw(canvas);
            canvas.RestoreToCount(count);
        }

        private int IncrementColor(int colorIndex)
        {
            ++colorIndex;
            if (colorIndex >= _mColors.Length)
            {
                colorIndex = 0;
            }
            return colorIndex;
        }

        private int DecrementColor(int colorIndex)
        {
            --colorIndex;
            if (colorIndex < 0)
            {
                colorIndex = _mColors.Length - 1;
            }
            return colorIndex;
        }

        /// <summary>
        /// Start the animation with the first color.
        /// Calls progressiveStart(0)
        /// </summary>
        public virtual void ProgressiveStart()
        {
            ProgressiveStart(0);
        }

        /// <summary>
        /// Start the animation from a given color.
        /// </summary>
        /// <param name="index"> </param>
        public virtual void ProgressiveStart(int index)
        {
            ResetProgressiveStart(index);
            Start();
        }

        private void ResetProgressiveStart(int index)
        {
            CheckColorIndex(index);

            _mCurrentOffset = 0;
            _mFinishing = false;
            _mFinishingOffset = 0f;
            _mStartSection = 0;
            _mCurrentSections = 0;
            _mColorsIndex = index;
        }

        /// <summary>
        /// Finish the animation by animating the remaining sections.
        /// </summary>
        public virtual void ProgressiveStop()
        {
            _mFinishing = true;
            _mStartSection = 0;
        }

        public override void SetAlpha(int alpha)
        {
            mPaint.Alpha = alpha;
        }

        public override void SetColorFilter(ColorFilter cf)
        {
            mPaint.SetColorFilter(cf);
        }

        public override int Opacity
        {
            get
            {
                return (int)Format.Transparent;
            }
        }

        ///////////////////////////////////////////////////////////////////////////
        ///////////////////         Animation: based on http://cyrilmottier.com/2012/11/27/actionbar-on-the-move/
        public void Start()
        {
            if (_mProgressiveStartActivated)
            {
                ResetProgressiveStart(0);
            }
            if (IsRunning)
            {
                return;
            }
            if (_mCallbacks != null)
            {
                _mCallbacks.OnStart();
            }
            ScheduleSelf(_mUpdater, SystemClock.UptimeMillis() + FrameDuration);
            InvalidateSelf();
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }
            if (_mCallbacks != null)
            {
                _mCallbacks.OnStop();
            }
            _isRunning = false;
            UnscheduleSelf(_mUpdater);
        }

        public override void ScheduleSelf(IRunnable what, long when)
        {
            _isRunning = true;
            base.ScheduleSelf(what, when);
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }

        public virtual bool Starting
        {
            get
            {
                return _mCurrentSections < _mSectionsCount;
            }
        }

        public virtual bool Finishing
        {
            get
            {
                return _mFinishing;
            }
        }

        private readonly Action _mUpdater;

        ////////////////////////////////////////////////////////////////////////////
        ///////////////////     Listener

        public virtual void SetCallbacks(ICallbacks callbacks)
        {
            _mCallbacks = callbacks;
        }

        ////////////////////////////////////////////////////////////////////////////
        ///////////////////     Checks

        private void CheckColorIndex(int index)
        {
            if (index < 0 || index >= _mColors.Length)
            {
                throw new ArgumentException(string.Format("Index {0:D} not valid", index));
            }
        }

        public class Builder
        {
            internal IInterpolator MInterpolator;
            internal int MSectionsCount;
            internal int[] MColors;
            internal float mSpeed;
            internal float mProgressiveStartSpeed;
            internal float mProgressiveStopSpeed;
            internal bool mReversed;
            internal bool mMirrorMode;
            internal float mStrokeWidth;
            internal int mStrokeSeparatorLength;
            internal bool mProgressiveStartActivated;
            internal bool mGenerateBackgroundUsingColors;
            internal bool mGradients;
            internal Drawable mBackgroundDrawableWhenHidden;

            internal ICallbacks mOnProgressiveStopEndedListener;

            public Builder(Context context)
            {
                InitValues(context);
            }

            public virtual SmoothProgressDrawable Build()
            {
                if (mGenerateBackgroundUsingColors)
                {
                    mBackgroundDrawableWhenHidden = SmoothProgressBarUtils.GenerateDrawableWithColors(MColors, mStrokeWidth);
                }
                var ret = new SmoothProgressDrawable(MInterpolator, MSectionsCount, mStrokeSeparatorLength, MColors, mStrokeWidth, mSpeed, mProgressiveStartSpeed, mProgressiveStopSpeed, mReversed, mMirrorMode, mOnProgressiveStopEndedListener, mProgressiveStartActivated, mBackgroundDrawableWhenHidden, mGradients);
                return ret;
            }

            internal virtual void InitValues(Context context)
            {
                var res = context.Resources;
                MInterpolator = new AccelerateInterpolator();
                MSectionsCount = res.GetInteger(Resource.Integer.spb_default_sections_count);
                MColors = new int[] { res.GetColor(Resource.Color.spb_default_color) };
                mSpeed = float.Parse(res.GetString(Resource.String.spb_default_speed), CultureInfo.InvariantCulture);
                mProgressiveStartSpeed = mSpeed;
                mProgressiveStopSpeed = mSpeed;
                mReversed = res.GetBoolean(Resource.Boolean.spb_default_reversed);
                mStrokeSeparatorLength = res.GetDimensionPixelSize(Resource.Dimension.spb_default_stroke_separator_length);
                mStrokeWidth = res.GetDimensionPixelSize(Resource.Dimension.spb_default_stroke_width);
                mProgressiveStartActivated = res.GetBoolean(Resource.Boolean.spb_default_progressiveStart_activated);
                mGradients = false;
            }

            public virtual Builder interpolator(IInterpolator interpolator)
            {
                SmoothProgressBarUtils.CheckNotNull(interpolator, "Interpolator");
                MInterpolator = interpolator;
                return this;
            }

            public virtual Builder sectionsCount(int sectionsCount)
            {
                SmoothProgressBarUtils.CheckPositive(sectionsCount, "Sections count");
                MSectionsCount = sectionsCount;
                return this;
            }

            public virtual Builder separatorLength(int separatorLength)
            {
                SmoothProgressBarUtils.CheckPositiveOrZero(separatorLength, "Separator length");
                mStrokeSeparatorLength = separatorLength;
                return this;
            }

            public virtual Builder color(int color)
            {
                MColors = new int[] { color };
                return this;
            }

            public virtual Builder colors(int[] colors)
            {
                SmoothProgressBarUtils.CheckColors(colors);
                MColors = colors;
                return this;
            }

            public virtual Builder strokeWidth(float width)
            {
                SmoothProgressBarUtils.CheckPositiveOrZero(width, "Width");
                mStrokeWidth = width;
                return this;
            }

            public virtual Builder speed(float speed)
            {
                SmoothProgressBarUtils.CheckSpeed(speed);
                mSpeed = speed;
                return this;
            }

            public virtual Builder progressiveStartSpeed(float progressiveStartSpeed)
            {
                SmoothProgressBarUtils.CheckSpeed(progressiveStartSpeed);
                mProgressiveStartSpeed = progressiveStartSpeed;
                return this;
            }

            public virtual Builder progressiveStopSpeed(float progressiveStopSpeed)
            {
                SmoothProgressBarUtils.CheckSpeed(progressiveStopSpeed);
                mProgressiveStopSpeed = progressiveStopSpeed;
                return this;
            }

            public virtual Builder reversed(bool reversed)
            {
                mReversed = reversed;
                return this;
            }

            public virtual Builder mirrorMode(bool mirrorMode)
            {
                mMirrorMode = mirrorMode;
                return this;
            }

            public virtual Builder progressiveStart(bool progressiveStartActivated)
            {
                mProgressiveStartActivated = progressiveStartActivated;
                return this;
            }

            public virtual Builder Callbacks(ICallbacks onProgressiveStopEndedListener)
            {
                mOnProgressiveStopEndedListener = onProgressiveStopEndedListener;
                return this;
            }

            public virtual Builder backgroundDrawable(Drawable backgroundDrawableWhenHidden)
            {
                mBackgroundDrawableWhenHidden = backgroundDrawableWhenHidden;
                return this;
            }

            public virtual Builder GenerateBackgroundUsingColors()
            {
                mGenerateBackgroundUsingColors = true;
                return this;
            }

            public virtual Builder Gradients(bool useGradients = true)
            {
                mGradients = useGradients;
                return this;
            }
        }
    }

}