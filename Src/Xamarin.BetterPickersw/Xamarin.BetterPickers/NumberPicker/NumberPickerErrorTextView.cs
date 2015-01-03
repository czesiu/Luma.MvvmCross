using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Java.Lang;

namespace Xamarin.BetterPickers
{

    //using R = android.R;
    //using Context = android.content.Context;
    //using Handler = android.os.Handler;
    //using IAttributeSet = android.util.AttributeSet;
    //using View = android.view.View;
    //using Animation = android.view.animation.Animation;
    //using AnimationUtils = android.view.animation.AnimationUtils;
    //using TextView = android.widget.TextView;

	/// <summary>
	/// User: derek Date: 6/21/13 Time: 10:37 AM
	/// </summary>
	public class NumberPickerErrorTextView : TextView
	{
		private const long LengthShort = 3000;

		public NumberPickerErrorTextView(Context context) 
            : base(context) { }
		public NumberPickerErrorTextView(Context context, IAttributeSet attrs) 
            : base(context, attrs) { }
		public NumberPickerErrorTextView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle) { }

		public virtual void Show()
		{
			fadeInEndHandler.RemoveCallbacks(Hide);
			Animation fadeIn = AnimationUtils.LoadAnimation(Context, Android.Resource.Animation.FadeIn);
			fadeIn.SetAnimationListener(new AnimationListenerAnonymousInnerClassHelper(this));
			StartAnimation(fadeIn);
		}

		private class AnimationListenerAnonymousInnerClassHelper : Object, Animation.IAnimationListener
		{
			private readonly NumberPickerErrorTextView outerInstance;

			public AnimationListenerAnonymousInnerClassHelper(NumberPickerErrorTextView outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void OnAnimationStart(Animation animation)
			{
			}

			public void OnAnimationEnd(Animation animation)
			{
				outerInstance.fadeInEndHandler.PostDelayed(outerInstance.Hide, LengthShort);
                outerInstance.Visibility = ViewStates.Visible;
			}

			public void OnAnimationRepeat(Animation animation)
			{
			}
		}

		private Handler fadeInEndHandler = new Handler();

		public virtual void Hide()
		{
            fadeInEndHandler.RemoveCallbacks(Hide);
			var fadeOut = AnimationUtils.LoadAnimation(Context, Android.Resource.Animation.FadeOut);
			fadeOut.SetAnimationListener(new AnimationListenerAnonymousInnerClassHelper2(this));
			StartAnimation(fadeOut);
		}

		private class AnimationListenerAnonymousInnerClassHelper2 : Java.Lang.Object, Animation.IAnimationListener
		{
			private readonly NumberPickerErrorTextView outerInstance;

			public AnimationListenerAnonymousInnerClassHelper2(NumberPickerErrorTextView outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void OnAnimationStart(Animation animation)
			{
			}

			public void OnAnimationEnd(Animation animation)
			{
                outerInstance.Visibility = ViewStates.Invisible;
			}

			public void OnAnimationRepeat(Animation animation)
			{
			}
		}

		public virtual void HideImmediately()
		{
			fadeInEndHandler.RemoveCallbacks(Hide);
			Visibility = ViewStates.Invisible;
		}
	}

}