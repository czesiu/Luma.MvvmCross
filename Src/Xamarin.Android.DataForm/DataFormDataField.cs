using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Xamarin.Android.DataForm
{
    public class DataFormDataField : LinearLayout
    {
        protected DataFormDataField(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }
        public DataFormDataField(Context context)
            : base(context) { }
        public DataFormDataField(Context context, IAttributeSet attrs)
            : base(context, attrs) { }
        public DataFormDataField(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle) { }

        protected override void OnFinishInflate()
        {
            int index = ChildCount;
            // Collect children declared in XML.
            View[] children = new View[index];
            while (--index >= 0)
            {
                children[index] = GetChildAt(index);
            }
            // Pressumably, wipe out existing content (still holding reference to it).
            DetachAllViewsFromParent();
            // Inflate new "template".
            var template = LayoutInflater.From(Context).Inflate(Resource.Layout.DataFormDataField, this, true);
            // Obtain reference to a new container within "template".
            var vg = template.FindViewById<ViewGroup>(Resource.Id.layout);
            index = children.Length;
            // Push declared children into new container.
            while (--index >= 0)
            {
                vg.AddView(children[index]);
            }

            // They suggest to call it no matter what.
            base.OnFinishInflate();
        }
    }
}