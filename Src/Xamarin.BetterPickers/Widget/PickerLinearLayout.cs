using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Xamarin.BetterPickers
{
	public abstract class PickerLinearLayout : LinearLayout
	{
		public PickerLinearLayout(Context context)
            : base(context) { }
		public PickerLinearLayout(Context context, IAttributeSet attrs)
            : base(context, attrs) { }

		public abstract View GetViewAt(int index);
	}
}