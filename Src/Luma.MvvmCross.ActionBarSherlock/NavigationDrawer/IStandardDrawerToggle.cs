using Android.Views;

namespace Luma.MvvmCross.ActionBarSherlock.NavigationDrawer
{
    public interface IStandardDrawerToggle : IDrawerToggle
    {
        bool OnOptionsItemSelected(IMenuItem item);
    }
}