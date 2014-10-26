using Android.Content.Res;
using Android.Support.V4.Widget;

namespace Luma.MvvmCross.ActionBarSherlock.NavigationDrawer
{
    public interface IDrawerToggle : DrawerLayout.IDrawerListener
    {
        event ActionBarDrawerChangedEventHandler DrawerClosed;
        event ActionBarDrawerChangedEventHandler DrawerOpened;
        event ActionBarDrawerChangedEventHandler DrawerSlide;
        event ActionBarDrawerChangedEventHandler DrawerStateChanged;

        void SyncState();

        void OnConfigurationChanged(Configuration newConfig);
    }
}