using System;
using Android.Content;
using Android.Runtime;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

namespace Luma.MvvmCross.ActionBarSherlock
{
    public abstract class MvxSherlockFragmentActivity : MvxEventSourceSherlockFragmentActivity, IMvxAndroidView
    {
        //protected MvxSherlockFragmentActivity(IntPtr javaReference, JniHandleOwnership transfer)
        //    : base(javaReference, transfer)
        //{
        //    BindingContext = new MvxAndroidBindingContext(this, this);
        //    this.AddEventListeners();
        //}

        protected MvxSherlockFragmentActivity()
        {
            BindingContext = new MvxAndroidBindingContext(this, this);
            this.AddEventListeners();
        }

        public object DataContext
        {
            get { return BindingContext.DataContext; }
            set { BindingContext.DataContext = value; }
        }

        public IMvxViewModel ViewModel
        {
            get { return DataContext as IMvxViewModel; }
            set
            {
                DataContext = value;
                OnViewModelSet();
            }
        }

        public void MvxInternalStartActivityForResult(Intent intent, int requestCode)
        {
            base.StartActivityForResult(intent, requestCode);
        }

        public IMvxBindingContext BindingContext { get; set; }

        public override void SetContentView(int layoutResId)
        {
            var view = this.BindingInflate(layoutResId, null);
            SetContentView(view);
        }

        protected virtual void OnViewModelSet()
        {
        }
    }
}