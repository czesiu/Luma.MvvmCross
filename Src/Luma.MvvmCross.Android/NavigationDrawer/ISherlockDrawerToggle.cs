using Xamarin.ActionBarSherlock.Views;

namespace Luma.MvvmCross.ActionBarSherlock.NavigationDrawer
{
    public interface ISherlockDrawerToggle : IDrawerToggle
    {
        bool OnOptionsItemSelected(IMenuItem item);
    }
}