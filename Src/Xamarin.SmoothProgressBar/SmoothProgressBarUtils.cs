using System;
using Android.Graphics.Drawables;

namespace Xamarin
{
    public static class SmoothProgressBarUtils
    {
        public static Drawable GenerateDrawableWithColors(int[] colors, float strokeWidth)
        {
            if (colors == null || colors.Length == 0)
            {
                return null;
            }

            return new ShapeDrawable(new ColorsShape(strokeWidth, colors));
        }

        internal static void CheckSpeed(float speed)
        {
            if (speed <= 0f)
            {
                throw new ArgumentException("Speed must be >= 0");
            }
        }

        internal static void CheckColors(int[] colors)
        {
            if (colors == null || colors.Length == 0)
            {
                throw new ArgumentException("You must provide at least 1 color");
            }
        }

        internal static void CheckAngle(int angle)
        {
            if (angle < 0 || angle > 360)
            {
                throw new ArgumentException(string.Format("Illegal angle {0:D}: must be >=0 and <= 360", angle));
            }
        }

        internal static void CheckPositiveOrZero(float number, string name)
        {
            if (number < 0)
            {
                throw new ArgumentException(string.Format("{0} {1:D} must be positive", name, number));
            }
        }

        internal static void CheckPositive(int number, string name)
        {
            if (number <= 0)
            {
                throw new ArgumentException(string.Format("{0} must not be null", name));
            }
        }

        internal static void CheckNotNull(object o, string name)
        {
            if (o == null)
            {
                throw new ArgumentException(string.Format("{0} must be not null", name));
            }
        }
    }
}