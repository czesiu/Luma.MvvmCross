using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Xamarin.SuperListView
{
	public class GridView : ListViewBase
	{
		private int _mColumns;
		private int _mHorizontalSpacing;
		private int _mVerticalSpacing;

		public GridView(Context context)
            : base(context) { }
		public GridView(Context context, IAttributeSet attrs)
            : base(context, attrs) { }
        public GridView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle) { }

		public new Android.Widget.GridView List
		{
			get
			{
				return (Android.Widget.GridView) InnerList;
			}
		}

	    protected override void InitAttributes(IAttributeSet attrs)
		{
			base.InitAttributes(attrs);

			var a = Context.ObtainStyledAttributes(attrs, Resource.Styleable.superlistview);
			try
			{
				SuperListViewMainLayout = a.GetResourceId(Resource.Styleable.superlistview_superlv_mainLayoutID, Resource.Layout.view_progress_gridview);
			}
			finally
			{
				a.Recycle();
			}

			var ag = Context.ObtainStyledAttributes(attrs, Resource.Styleable.supergridview);
			try
			{
				_mColumns = ag.GetInt(Resource.Styleable.supergridview_supergv__columns, 1);
				_mVerticalSpacing = (int) ag.GetDimension(Resource.Styleable.supergridview_supergv__verticalSpacing, 1);
				_mHorizontalSpacing = (int) ag.GetDimension(Resource.Styleable.supergridview_supergv__horizontalSpacing, 1);
			}
			finally
			{
				ag.Recycle();
			}
		}

		protected override void InitView(View view)
		{
            InnerList = view.FindViewById(Android.Resource.Id.List) as Android.Widget.GridView;

			if (InnerList == null)
			{
				throw new ArgumentException();
			}

			List.NumColumns = _mColumns;
			List.SetVerticalSpacing(_mVerticalSpacing);
			List.SetHorizontalSpacing(_mHorizontalSpacing);
			List.SetHorizontalSpacing(DividerHeight);
			List.SetVerticalSpacing(DividerHeight);
		}

		public override IListAdapter Adapter
		{
			set
			{
				List.Adapter = value;
				base.Adapter = value;
			}
		}
	}
}