using System;
using Android.Content;
using Android.Runtime;
using Android.Util;

namespace Luma.MvvmCross
{
    public class SmoothProgressBar : Xamarin.SmoothProgressBar
    {
        public SmoothProgressBar(IntPtr intPtr, JniHandleOwnership jniHandleOwnership)
            : base(intPtr, jniHandleOwnership) { }

        public SmoothProgressBar(Context context)
            : base(context) { }

        public SmoothProgressBar(Context context, IAttributeSet attrs)
            : base(context, attrs) { }

        public SmoothProgressBar(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle) { }

    }
}
