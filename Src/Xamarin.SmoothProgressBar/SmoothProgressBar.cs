using System;
using System.Globalization;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views.Animations;
using Android.Widget;

namespace Xamarin
{
    public class SmoothProgressBar : ProgressBar
    {
        private const int InterpolatorAccelerate = 0;
        private const int InterpolatorLinear = 1;
        private const int InterpolatorAccelerateDecelerate = 2;
        private const int InterpolatorDecelerate = 3;

        public SmoothProgressBar(IntPtr intPtr, JniHandleOwnership jniHandleOwnership)
            : base(intPtr, jniHandleOwnership) { }

        public SmoothProgressBar(Context context)
            : this(context, null) { }

        public SmoothProgressBar(Context context, IAttributeSet attrs)
            : this(context, attrs, Resource.Attribute.spbStyle) { }

        public SmoothProgressBar(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            if (IsInEditMode)
            {
                IndeterminateDrawable = (new SmoothProgressDrawable.Builder(context)).Build();
                return;
            }

            var res = context.Resources;
            var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.SmoothProgressBar, defStyle, 0);

            var color = a.GetColor(Resource.Styleable.SmoothProgressBar_spb_color, res.GetColor(Resource.Color.spb_default_color));
            var sectionsCount = a.GetInteger(Resource.Styleable.SmoothProgressBar_spb_sections_count, res.GetInteger(Resource.Integer.spb_default_sections_count));
            var separatorLength = a.GetDimensionPixelSize(Resource.Styleable.SmoothProgressBar_spb_stroke_separator_length, res.GetDimensionPixelSize(Resource.Dimension.spb_default_stroke_separator_length));
            var strokeWidth = a.GetDimension(Resource.Styleable.SmoothProgressBar_spb_stroke_width, res.GetDimension(Resource.Dimension.spb_default_stroke_width));
            var speed = a.GetFloat(Resource.Styleable.SmoothProgressBar_spb_speed, float.Parse(res.GetString(Resource.String.spb_default_speed), CultureInfo.InvariantCulture));
            var speedProgressiveStart = a.GetFloat(Resource.Styleable.SmoothProgressBar_spb_progressiveStart_speed, speed);
            var speedProgressiveStop = a.GetFloat(Resource.Styleable.SmoothProgressBar_spb_progressiveStop_speed, speed);
            var iInterpolator = a.GetInteger(Resource.Styleable.SmoothProgressBar_spb_interpolator, -1);
            var reversed = a.GetBoolean(Resource.Styleable.SmoothProgressBar_spb_reversed, res.GetBoolean(Resource.Boolean.spb_default_reversed));
            var mirrorMode = a.GetBoolean(Resource.Styleable.SmoothProgressBar_spb_mirror_mode, res.GetBoolean(Resource.Boolean.spb_default_mirror_mode));
            var colorsId = a.GetResourceId(Resource.Styleable.SmoothProgressBar_spb_colors, 0);
            var progressiveStartActivated = a.GetBoolean(Resource.Styleable.SmoothProgressBar_spb_progressiveStart_activated, res.GetBoolean(Resource.Boolean.spb_default_progressiveStart_activated));
            var backgroundDrawable = a.GetDrawable(Resource.Styleable.SmoothProgressBar_spb_background);
            var generateBackgroundWithColors = a.GetBoolean(Resource.Styleable.SmoothProgressBar_spb_generate_background_with_colors, false);

            var gradients = a.GetBoolean(Resource.Styleable.SmoothProgressBar_spb_gradients, false);
            a.Recycle();

            IInterpolator interpolator = null;
            if (iInterpolator == -1)
            {
                interpolator = Interpolator;
            }
            if (interpolator == null)
            {
                switch (iInterpolator)
                {
                    case InterpolatorAccelerateDecelerate:
                        interpolator = new AccelerateDecelerateInterpolator();
                        break;
                    case InterpolatorDecelerate:
                        interpolator = new DecelerateInterpolator();
                        break;
                    case InterpolatorLinear:
                        interpolator = new LinearInterpolator();
                        break;
                    default:
                        interpolator = new AccelerateInterpolator();
                        break;
                }
            }

            int[] colors = null;
            if (colorsId != 0)
            {
                colors = res.GetIntArray(colorsId);
            }

            var builder = new SmoothProgressDrawable.Builder(context)
                .speed(speed)
                .progressiveStartSpeed(speedProgressiveStart)
                .progressiveStopSpeed(speedProgressiveStop)
                .interpolator(interpolator)
                .sectionsCount(sectionsCount)
                .separatorLength(separatorLength)
                .strokeWidth(strokeWidth)
                .reversed(reversed)
                .mirrorMode(mirrorMode)
                .progressiveStart(progressiveStartActivated)
                .Gradients(gradients);

            if (backgroundDrawable != null)
            {
                builder.backgroundDrawable(backgroundDrawable);
            }

            if (generateBackgroundWithColors)
            {
                builder.GenerateBackgroundUsingColors();
            }

            if (colors != null && colors.Length > 0)
            {
                builder.colors(colors);
            }
            else
            {
                builder.color(color);
            }

            IndeterminateDrawable = builder.Build();
        }

        public virtual void ApplyStyle(int styleResId)
        {
            var a = Context.ObtainStyledAttributes(null, Resource.Styleable.SmoothProgressBar, 0, styleResId);

            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_color))
            {
                SmoothProgressDrawableColor = a.GetColor(Resource.Styleable.SmoothProgressBar_spb_color, 0);
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_colors))
            {
                int colorsId = a.GetResourceId(Resource.Styleable.SmoothProgressBar_spb_colors, 0);
                if (colorsId != 0)
                {
                    int[] colors = Resources.GetIntArray(colorsId);
                    if (colors != null && colors.Length > 0)
                    {
                        SmoothProgressDrawableColors = colors;
                    }
                }
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_sections_count))
            {
                SmoothProgressDrawableSectionsCount = a.GetInteger(Resource.Styleable.SmoothProgressBar_spb_sections_count, 0);
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_stroke_separator_length))
            {
                SmoothProgressDrawableSeparatorLength = a.GetDimensionPixelSize(Resource.Styleable.SmoothProgressBar_spb_stroke_separator_length, 0);
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_stroke_width))
            {
                SmoothProgressDrawableStrokeWidth = a.GetDimension(Resource.Styleable.SmoothProgressBar_spb_stroke_width, 0);
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_speed))
            {
                SmoothProgressDrawableSpeed = a.GetFloat(Resource.Styleable.SmoothProgressBar_spb_speed, 0);
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_progressiveStart_speed))
            {
                SmoothProgressDrawableProgressiveStartSpeed = a.GetFloat(Resource.Styleable.SmoothProgressBar_spb_progressiveStart_speed, 0);
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_progressiveStop_speed))
            {
                SmoothProgressDrawableProgressiveStopSpeed = a.GetFloat(Resource.Styleable.SmoothProgressBar_spb_progressiveStop_speed, 0);
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_reversed))
            {
                SmoothProgressDrawableReversed = a.GetBoolean(Resource.Styleable.SmoothProgressBar_spb_reversed, false);
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_mirror_mode))
            {
                SmoothProgressDrawableMirrorMode = a.GetBoolean(Resource.Styleable.SmoothProgressBar_spb_mirror_mode, false);
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_progressiveStart_activated))
            {
                ProgressiveStartActivated = a.GetBoolean(Resource.Styleable.SmoothProgressBar_spb_progressiveStart_activated, false);
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_progressiveStart_activated))
            {
                ProgressiveStartActivated = a.GetBoolean(Resource.Styleable.SmoothProgressBar_spb_progressiveStart_activated, false);
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_gradients))
            {
                SmoothProgressDrawableUseGradients = a.GetBoolean(Resource.Styleable.SmoothProgressBar_spb_gradients, false);
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_generate_background_with_colors))
            {
                if (a.GetBoolean(Resource.Styleable.SmoothProgressBar_spb_generate_background_with_colors, false))
                {
                    SmoothProgressDrawableBackgroundDrawable = SmoothProgressBarUtils.GenerateDrawableWithColors(CheckIndeterminateDrawable().Colors, CheckIndeterminateDrawable().StrokeWidth);
                }
            }
            if (a.HasValue(Resource.Styleable.SmoothProgressBar_spb_interpolator))
            {
                int iInterpolator = a.GetInteger(Resource.Styleable.SmoothProgressBar_spb_interpolator, -1);
                IInterpolator interpolator;
                switch (iInterpolator)
                {
                    case InterpolatorAccelerateDecelerate:
                        interpolator = new AccelerateDecelerateInterpolator();
                        break;
                    case InterpolatorDecelerate:
                        interpolator = new DecelerateInterpolator();
                        break;
                    case InterpolatorLinear:
                        interpolator = new LinearInterpolator();
                        break;
                    case InterpolatorAccelerate:
                        interpolator = new AccelerateInterpolator();
                        break;
                    default:
                        interpolator = null;
                        break;
                }
                if (interpolator != null)
                {
                    Interpolator = interpolator;
                }
            }
            a.Recycle();
        }

        protected override void OnDraw(Canvas canvas)
        {
            lock (this)
            {
                base.OnDraw(canvas);

                if (Indeterminate && IndeterminateDrawable is SmoothProgressDrawable && !((SmoothProgressDrawable)IndeterminateDrawable).IsRunning)
                {
                    IndeterminateDrawable.Draw(canvas);
                }
            }
        }

        private SmoothProgressDrawable CheckIndeterminateDrawable()
        {
            var smoothProgressDrawable = IndeterminateDrawable as SmoothProgressDrawable;
            if (smoothProgressDrawable == null)
            {
                throw new Exception("The drawable is not a SmoothProgressDrawable");
            }

            return smoothProgressDrawable;
        }

        public override IInterpolator Interpolator
        {
            get
            {
                return base.Interpolator;
            }
            set
            {
                base.Interpolator = value;

                var drawable = IndeterminateDrawable as SmoothProgressDrawable;
                if (drawable != null)
                {
                    drawable.Interpolator = value;
                }
            }
        }

        public virtual IInterpolator SmoothProgressDrawableInterpolator
        {
            set
            {
                CheckIndeterminateDrawable().Interpolator = value;
            }
        }

        public virtual int[] SmoothProgressDrawableColors
        {
            set
            {
                CheckIndeterminateDrawable().Colors = value;
            }
        }

        public virtual int SmoothProgressDrawableColor
        {
            set
            {
                CheckIndeterminateDrawable().Color = value;
            }
        }

        public virtual float SmoothProgressDrawableSpeed
        {
            set
            {
                CheckIndeterminateDrawable().Speed = value;
            }
        }

        public virtual float SmoothProgressDrawableProgressiveStartSpeed
        {
            set
            {
                CheckIndeterminateDrawable().ProgressiveStartSpeed = value;
            }
        }

        public virtual float SmoothProgressDrawableProgressiveStopSpeed
        {
            set
            {
                CheckIndeterminateDrawable().ProgressiveStopSpeed = value;
            }
        }

        public virtual int SmoothProgressDrawableSectionsCount
        {
            set
            {
                CheckIndeterminateDrawable().SectionsCount = value;
            }
        }

        public virtual int SmoothProgressDrawableSeparatorLength
        {
            set
            {
                CheckIndeterminateDrawable().SeparatorLength = value;
            }
        }

        public virtual float SmoothProgressDrawableStrokeWidth
        {
            set
            {
                CheckIndeterminateDrawable().StrokeWidth = value;
            }
        }

        public virtual bool SmoothProgressDrawableReversed
        {
            set
            {
                CheckIndeterminateDrawable().Reversed = value;
            }
        }

        public virtual bool SmoothProgressDrawableMirrorMode
        {
            set
            {
                CheckIndeterminateDrawable().MirrorMode = value;
            }
        }

        public virtual bool ProgressiveStartActivated
        {
            set
            {
                CheckIndeterminateDrawable().ProgressiveStartActivated = value;
            }
        }

        public virtual SmoothProgressDrawable.ICallbacks SmoothProgressDrawableCallbacks
        {
            set
            {
                CheckIndeterminateDrawable().SetCallbacks(value);
            }
        }

        public virtual Drawable SmoothProgressDrawableBackgroundDrawable
        {
            set
            {
                CheckIndeterminateDrawable().BackgroundDrawable = value;
            }
        }

        public virtual bool SmoothProgressDrawableUseGradients
        {
            set
            {
                CheckIndeterminateDrawable().UseGradients = value;
            }
        }

        public virtual void ProgressiveStart()
        {
            CheckIndeterminateDrawable().ProgressiveStart();
        }

        public virtual void ProgressiveStop()
        {
            CheckIndeterminateDrawable().ProgressiveStop();
        }

        public bool IsBusy
        {
            get { return IsRunning; }
            set { IsRunning = value; }
        }

        public bool IsRunning
        {
            get { return CheckIndeterminateDrawable().IsRunning; }
            set
            {
                if (value)
                {
                    ProgressiveStart();
                }
                else
                {
                    ProgressiveStop();
                }
            }
        }
    }

}