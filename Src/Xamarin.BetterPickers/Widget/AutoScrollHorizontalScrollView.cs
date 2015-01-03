using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Xamarin.BetterPickers
{
	/// <summary>
	/// User: derek Date: 5/2/13 Time: 9:19 PM
	/// </summary>
	public class AutoScrollHorizontalScrollView : HorizontalScrollView
	{
		public AutoScrollHorizontalScrollView(Context context)
            : base(context) { }
		public AutoScrollHorizontalScrollView(Context context, IAttributeSet attrs)
            : base(context, attrs) { }
		public AutoScrollHorizontalScrollView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle) { }

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
            base.OnLayout(changed, l, t, r, b);

			FullScroll(FocusSearchDirection.Right);
		}
	}

}