using System;
using Android.Content;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Xamarin.SuperListView
{
    /// <summary>
    /// Created by kentin on 24/04/14.
    /// </summary>
    public abstract class ListViewBase : FrameLayout, AbsListView.IOnScrollListener, SwipeRefreshLayout.IOnRefreshListener
    {
        protected int ItemLeftToLoadMore = 10;

        protected ViewStub EmptyView;
        protected ViewStub ProgressView;
        protected ViewStub MoreProgressView;

        private int _emptyId;
        private int _progressId;
        private int _moreProgressId;

        protected AbsListView InnerList;

        protected int DividerHeight;
        protected int Divider;
        protected ScrollbarStyles ScrollbarStyle;

        protected int Selector;
        protected SwipeRefreshLayout RefreshLayout;

        protected int SuperListViewMainLayout;

        private bool _clipToPadding;
        private int _paddingLeft;
        private int _paddingTop;
        private int _paddingRight;
        private int _paddingBottom;
        private bool _isLoading;

        public override int PaddingLeft { get { return _paddingLeft; } }

        public override int PaddingTop { get { return _paddingTop; } }

        public override int PaddingRight { get { return _paddingRight; } }

        public override int PaddingBottom { get { return _paddingBottom; } }

        public SwipeRefreshLayout SwipeToRefresh { get { return RefreshLayout; } }

        public AbsListView List { get { return InnerList; } }

        public ListViewBase(Context context)
            : base(context)
        {
            InitView();
        }

        public ListViewBase(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            InitAttributes(attrs);
            InitView();
        }

        public ListViewBase(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            InitAttributes(attrs);
            InitView();
        }

        public override void SetPadding(int left, int top, int right, int bottom)
        {
            _paddingLeft = left;
            _paddingTop = top;
            _paddingRight = right;
            _paddingBottom = bottom;

            if (InnerList == null)
                return;

            InnerList.SetPadding(left, top, right, bottom);
        }

        public override void SetClipToPadding(bool clipToPadding)
        {
            _clipToPadding = clipToPadding;

            if (InnerList == null)
                return;

            InnerList.SetClipToPadding(clipToPadding);
        }

        protected virtual void InitAttributes(IAttributeSet attrs)
        {
            var a = Context.ObtainStyledAttributes(attrs, Resource.Styleable.superlistview);

            try
            {
                _emptyId = a.GetResourceId(Resource.Styleable.superlistview_superlv__empty, 0);
                _moreProgressId = a.GetResourceId(Resource.Styleable.superlistview_superlv__moreProgress, Resource.Layout.view_more_progress);
                _progressId = a.GetResourceId(Resource.Styleable.superlistview_superlv__progress, Resource.Layout.view_progress);
            }
            finally
            {
                a.Recycle();
            }

            var b = Context.ObtainStyledAttributes(attrs, new[]
            {
                Android.Resource.Attribute.Divider,
                Android.Resource.Attribute.DividerHeight,
                Android.Resource.Attribute.ScrollbarStyle,
                Android.Resource.Attribute.ListSelector,
            });

            try
            {
                Divider = a.GetColor(0, -1);
                DividerHeight = (int)b.GetDimension(1, -1);
                ScrollbarStyle = (ScrollbarStyles)a.GetInt(2, -1);
                Selector = a.GetResourceId(3, -1);
            }
            finally
            {
                b.Recycle();
            }
        }

        private void InitView()
        {
            if (IsInEditMode)
            {
                return;
            }

            var view = LayoutInflater.From(Context).Inflate(SuperListViewMainLayout, this);
            RefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.ptr_layout);

            ProgressView = view.FindViewById<ViewStub>(Android.Resource.Id.Progress);

            ProgressView.LayoutResource = _progressId;
            ProgressView.Inflate();

            MoreProgressView = view.FindViewById<ViewStub>(Resource.Id.more_progress);
            MoreProgressView.LayoutResource = _moreProgressId;
            if (_moreProgressId != 0)
            {
                MoreProgressView.Inflate();
            }
            MoreProgressView.Visibility = ViewStates.Gone;

            EmptyView = view.FindViewById<ViewStub>(Resource.Id.empty);
            EmptyView.LayoutResource = _emptyId;
            if (_emptyId != 0)
            {
                EmptyView.Inflate();
            }
            EmptyView.Visibility = ViewStates.Gone;

            InitView(view);

            if (Selector != -1)
            {
                InnerList.SetSelector(Selector);
            }

            if (ScrollbarStyle != (ScrollbarStyles)(-1))
            {
                InnerList.ScrollBarStyle = ScrollbarStyle;
            }

            SetPadding(_paddingLeft, _paddingTop, _paddingRight, _paddingBottom);
            SetClipToPadding(_clipToPadding);
            SetOnRefreshListener(this);
            SetOnScrollListener(this);
            SetRefreshingColorScheme(Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloRedLight);
        }

        public virtual void SetOnScrollListener(AbsListView.IOnScrollListener l)
        {
            InnerList.SetOnScrollListener(l);
        }

        public bool IsRefreshEnabled
        {
            get { return RefreshLayout.Enabled; }
            set { RefreshLayout.Enabled = value; }
        }

        /// <summary>
        /// Implement this method to customize the AbsListView
        /// </summary>
        protected abstract void InitView(View view);

        /// <summary>
        /// Set the adapter to the listview
        /// Automativally hide the progressbar
        /// Set the refresh to false
        /// If adapter is empty, then the emptyview is shown </summary>
        /// <returns> the listview adapter </returns>
        public virtual IListAdapter Adapter
        {
            get { return InnerList.Adapter; }
            set
            {
                ProgressView.Visibility = ViewStates.Gone;
                if (EmptyView != null && _emptyId != 0)
                {
                    InnerList.EmptyView = EmptyView;
                }
                InnerList.Visibility = ViewStates.Visible;
                RefreshLayout.Refreshing = false;
                if ((value == null || value.Count == 0) && _emptyId != 0)
                {
                    EmptyView.Visibility = ViewStates.Visible;
                }
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading == value)
                    return;

                _isLoading = value;

                RefreshLayout.Refreshing = value;

                if (!_isLoading)
                {
                    ProgressView.Visibility = ViewStates.Gone;
                    if (InnerList.Adapter.Count == 0 && _emptyId != 0)
                    {
                        EmptyView.Visibility = ViewStates.Visible;
                    }
                    else if (_emptyId != 0)
                    {
                        EmptyView.Visibility = ViewStates.Gone;
                    }
                }

            }
        }

        /// <summary>
        /// Show the progressbar
        /// </summary>
        public virtual void ShowProgress()
        {
            HideList();
            if (_emptyId != 0)
            {
                EmptyView.Visibility = ViewStates.Invisible;
            }
            ProgressView.Visibility = ViewStates.Visible;
        }

        /// <summary>
        /// Hide the progressbar and show the listview
        /// </summary>
        public virtual void ShowList()
        {
            HideProgress();
            InnerList.Visibility = ViewStates.Visible;
        }

        public virtual void ShowMoreProgress()
        {
            MoreProgressView.Visibility = ViewStates.Visible;

        }

        public virtual void HideMoreProgress()
        {
            MoreProgressView.Visibility = ViewStates.Gone;

        }

        /// <summary>
        /// Set the listener when refresh is triggered and enable the SwipeRefreshLayout </summary>
        /// <param name="refreshListener"></param>
        public virtual void SetOnRefreshListener(SwipeRefreshLayout.IOnRefreshListener refreshListener)
        {
            RefreshLayout.SetOnRefreshListener(refreshListener);

        }

        public event EventHandler Refresh
        {
            add { RefreshLayout.Refresh += value; }
            remove { RefreshLayout.Refresh -= value; }
        }

        /// <summary>
        /// Set the colors for the SwipeRefreshLayout states </summary>
        /// <param name="col1"> </param>
        /// <param name="col2"> </param>
        /// <param name="col3"> </param>
        /// <param name="col4"> </param>
        public virtual void SetRefreshingColorScheme(int col1, int col2, int col3, int col4)
        {
            RefreshLayout.SetColorScheme(col1, col2, col3, col4);
        }

        /// <summary>
        /// Hide the progressbar
        /// </summary>
        public virtual void HideProgress()
        {
            ProgressView.Visibility = ViewStates.Gone;
        }

        /// <summary>
        /// Hide the listview
        /// </summary>
        public virtual void HideList()
        {
            InnerList.Visibility = ViewStates.Gone;
        }

        /// <summary>
        /// Set the scroll listener for the listview </summary>
        /// <param name="listener"> </param>
        public virtual AbsListView.IOnScrollListener OnScrollListener { get; set; }

        /// <summary>
        /// Set the onItemClickListener for the listview </summary>
        public virtual AdapterView.IOnItemClickListener OnItemClickListener
        {
            get { return InnerList.OnItemClickListener; }
            set { InnerList.OnItemClickListener = value; }
        }

        public virtual int FirstVisiblePosition { get { return InnerList.FirstVisiblePosition; } }

        public void OnScrollStateChanged(AbsListView view, ScrollState scrollState)
        {
            if (OnScrollListener != null)
            {
                OnScrollListener.OnScrollStateChanged(view, scrollState);
            }
        }

        public virtual int NumberBeforeMoreIsCalled
        {
            set
            {
                ItemLeftToLoadMore = value;
            }
        }

        public virtual bool IsLoadingMore { get; set; }

        public event EventHandler<LoadMoreEventArgs> LoadMore;

        public void OnScroll(AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount)
        {
            if (((totalItemCount - firstVisibleItem - visibleItemCount) == ItemLeftToLoadMore || (totalItemCount - firstVisibleItem - visibleItemCount) == 0 && totalItemCount > visibleItemCount) && !IsLoadingMore)
            {
                IsLoadingMore = true;
                MoreProgressView.Visibility = ViewStates.Visible;

                if (LoadMore != null)
                {
                    LoadMore(this, new LoadMoreEventArgs { NumberOfItems = InnerList.Adapter.Count, MumberBeforeMore = ItemLeftToLoadMore, CurrentItemPosition = firstVisibleItem });
                }
            }
            if (OnScrollListener != null)
            {
                OnScrollListener.OnScroll(view, firstVisibleItem, visibleItemCount, totalItemCount);
            }
        }

        public override void SetOnTouchListener(IOnTouchListener l)
        {
            InnerList.SetOnTouchListener(l);
        }

        public virtual void OnRefresh()
        {
        }
    }
}