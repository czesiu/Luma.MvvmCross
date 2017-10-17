using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using global::Android.Widget;

namespace Xamarin.SuperListView
{
	/// <summary>
	/// Created by kentin on 24/04/14.
	/// </summary>
	public class ListView : ListViewBase
	{
		public ListView(Context context)
            : base(context) { }
		public ListView(Context context, IAttributeSet attrs)
            : base(context, attrs) { }
        public ListView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle) { }

        public event EventHandler<AdapterView.ItemClickEventArgs> ItemClick
	    {
            add { List.ItemClick += value; }
            remove { List.ItemClick -= value; }
	    }

        public event EventHandler<AdapterView.ItemLongClickEventArgs> ItemLongClick
        {
            add { List.ItemLongClick += value; }
            remove { List.ItemLongClick -= value; }
        } 

		public new global::Android.Widget.ListView List { get { return InnerList as global::Android.Widget.ListView; } }

        protected override void InitAttributes(IAttributeSet attrs)
		{
			base.InitAttributes(attrs);
			var a = Context.ObtainStyledAttributes(attrs, Resource.Styleable.superlistview);
			try
			{
				SuperListViewMainLayout = a.GetResourceId(Resource.Styleable.superlistview_superlv_mainLayoutID, Resource.Layout.view_progress_listview);
			}
			finally
			{
				a.Recycle();
			}
		}

		protected override void InitView(View view)
		{
			InnerList = view.FindViewById(global::Android.Resource.Id.List) as global::Android.Widget.ListView;

			if (InnerList == null)
			{
				throw new ArgumentException("ListView works with a List!");
			}

			if (Divider != -1)
			{
                List.Divider = new ColorDrawable(new Color(Divider));
			}

			if (DividerHeight != -1)
			{
                List.DividerHeight = DividerHeight;
			}
		}

		public override IListAdapter Adapter
		{
			set
			{
				List.Adapter = value;
				base.Adapter = value;
			}
		}

		public virtual void setupSwipeToDismiss(SwipeDismissListViewTouchListener.DismissCallbacks listener, bool autoRemove)
		{
			SwipeDismissListViewTouchListener touchListener = new SwipeDismissListViewTouchListener((global::Android.Widget.ListView) InnerList, new DismissCallbacksAnonymousInnerClassHelper(this, listener, autoRemove));
			InnerList.SetOnTouchListener(touchListener);
		}

		private class DismissCallbacksAnonymousInnerClassHelper : SwipeDismissListViewTouchListener.DismissCallbacks
		{
			private readonly ListView outerInstance;

			private SwipeDismissListViewTouchListener.DismissCallbacks listener;
			private bool autoRemove;

			public DismissCallbacksAnonymousInnerClassHelper(ListView outerInstance, SwipeDismissListViewTouchListener.DismissCallbacks listener, bool autoRemove)
			{
				this.outerInstance = outerInstance;
				this.listener = listener;
				this.autoRemove = autoRemove;
			}

			public virtual bool canDismiss(int position)
			{
				return listener.canDismiss(position);
			}

			public virtual void onDismiss(global::Android.Widget.ListView listView, int[] reverseSortedPositions)
			{
				if (autoRemove)
				{
					foreach (int position in reverseSortedPositions)
					{

                        ((ArrayAdapter)outerInstance.List.Adapter).Remove(outerInstance.List.Adapter.GetItem(position));
                    } ((ArrayAdapter)outerInstance.List.Adapter).NotifyDataSetChanged();
				}
				listener.onDismiss(listView, reverseSortedPositions);
			}
		}
	}

}