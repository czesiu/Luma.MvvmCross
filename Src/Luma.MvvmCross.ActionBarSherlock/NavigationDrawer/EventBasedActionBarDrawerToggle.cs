using Android.App;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Views;

namespace Luma.MvvmCross.ActionBarSherlock.NavigationDrawer
{
    public class EventBasedActionBarDrawerToggle : ActionBarDrawerToggle, IStandardDrawerToggle
    {
        public EventBasedActionBarDrawerToggle(Activity activity, DrawerLayout drawerLayout,  int drawerImageRes, int openDrawerContentDescRes, int closeDrawerContentDescRes)
            : base(activity, drawerLayout, drawerImageRes, openDrawerContentDescRes, closeDrawerContentDescRes)
        { }

        public event ActionBarDrawerChangedEventHandler DrawerClosed;
        public event ActionBarDrawerChangedEventHandler DrawerOpened;
        public event ActionBarDrawerChangedEventHandler DrawerSlide;
        public event ActionBarDrawerChangedEventHandler DrawerStateChanged;

        public override void OnDrawerClosed(View drawerView)
        {
            if (DrawerClosed != null)
            {
                DrawerClosed(this, new ActionBarDrawerEventArgs { DrawerView = drawerView });
            }

            base.OnDrawerClosed(drawerView);
        }

        public override void OnDrawerOpened(View drawerView)
        {
            if (DrawerOpened != null)
            {
                DrawerOpened(this, new ActionBarDrawerEventArgs { DrawerView = drawerView });
            }

            base.OnDrawerOpened(drawerView);
        }

        public override void OnDrawerSlide(View drawerView, float slideOffset)
        {
            if (DrawerSlide != null)
            {
                DrawerSlide(this, new ActionBarDrawerEventArgs
                {
                    DrawerView = drawerView,
                    SlideOffset = slideOffset
                });
            }

            base.OnDrawerSlide(drawerView, slideOffset);
        }

        public override void OnDrawerStateChanged(int newState)
        {
            if (DrawerStateChanged != null)
            {
                DrawerStateChanged(this, new ActionBarDrawerEventArgs
                {
                    NewState = newState
                });
            }

            base.OnDrawerStateChanged(newState);
        }
    }
}