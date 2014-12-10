using System;

namespace Xamarin.SuperListView
{
	public class LoadMoreEventArgs : EventArgs
	{
        public int NumberOfItems { get; set; }

        public int MumberBeforeMore { get; set; }

        public int CurrentItemPosition { get; set; }
	}
}