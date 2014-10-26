using System;
using Android.Views;

namespace Luma.MvvmCross.ActionBarSherlock.NavigationDrawer
{
    public class ActionBarDrawerEventArgs : EventArgs
    {
        public View DrawerView { get; set; }

        public float SlideOffset { get; set; }

        public int NewState { get; set; }
    }
}