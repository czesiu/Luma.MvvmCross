/*
 * Copyright (C) 2013 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Java.Lang;
using IMenuItem = Xamarin.ActionBarSherlock.Views.IMenuItem;

namespace Luma.MvvmCross.ActionBarSherlock.NavigationDrawer
{
    /// <summary>
	/// This class provides a handy way to tie together the functionality of
	/// <seealso cref="DrawerLayout"/> and the framework <code>ActionBar</code> to implement the recommended
	/// design for navigation drawers.
	/// 
	/// <para>To use <code>ActionBarDrawerToggle</code>, create one in your Activity and call through
	/// to the following methods corresponding to your Activity callbacks:</para>
	/// 
	/// <ul>
	/// <li><seealso cref="Activity#onConfigurationChanged(android.content.res.Configuration) onConfigurationChanged"/></li>
	/// <li><seealso cref="Activity#onOptionsItemSelected(android.view.MenuItem) onOptionsItemSelected"/></li>
	/// </ul>
	/// 
	/// <para>Call <seealso cref="#syncState()"/> from your <code>Activity</code>'s
	/// <seealso cref="Activity#onPostCreate(android.os.Bundle) onPostCreate"/> to synchronize the indicator
	/// with the state of the linked DrawerLayout after <code>onRestoreInstanceState</code>
	/// has occurred.</para>
	/// 
	/// <para><code>ActionBarDrawerToggle</code> can be used directly as a
	/// <seealso cref="DrawerLayout.DrawerListener"/>, or if you are already providing your own listener,
	/// call through to each of the listener methods from your own.</para>
	/// </summary>
	public class SherlockActionBarDrawerToggle : Object, DrawerLayout.IDrawerListener
	{
		private interface ActionBarDrawerToggleImpl
		{
			Drawable getThemeUpIndicator(Activity activity);
			object setActionBarUpIndicator(object info, Activity activity, Drawable themeImage, int contentDescRes);
			object setActionBarDescription(object info, Activity activity, int contentDescRes);
		}

		private class ActionBarDrawerToggleImplCompat : ActionBarDrawerToggleImpl
		{
			public virtual Drawable getThemeUpIndicator(Activity activity)
			{
				return SherlockActionBarDrawerToggleCompat.getThemeUpIndicator(activity);
			}

			public virtual object setActionBarUpIndicator(object info, Activity activity, Drawable themeImage, int contentDescRes)
			{
				return SherlockActionBarDrawerToggleCompat.setActionBarUpIndicator(info, activity, themeImage, contentDescRes);
			}

			public virtual object setActionBarDescription(object info, Activity activity, int contentDescRes)
			{
				return SherlockActionBarDrawerToggleCompat.setActionBarDescription(info, activity, contentDescRes);
			}
		}

		private class ActionBarDrawerToggleImplHC : ActionBarDrawerToggleImpl
		{
			public virtual Drawable getThemeUpIndicator(Activity activity)
			{
				return SherlockActionBarDrawerToggleHoneycomb.getThemeUpIndicator(activity);
			}

			public virtual object setActionBarUpIndicator(object info, Activity activity, Drawable themeImage, int contentDescRes)
			{
				return SherlockActionBarDrawerToggleHoneycomb.setActionBarUpIndicator(info, activity, themeImage, contentDescRes);
			}

			public virtual object setActionBarDescription(object info, Activity activity, int contentDescRes)
			{
				return SherlockActionBarDrawerToggleHoneycomb.setActionBarDescription(info, activity, contentDescRes);
			}
		}

		private static readonly ActionBarDrawerToggleImpl Impl;

		static SherlockActionBarDrawerToggle()
		{
			var version = (int)Build.VERSION.SdkInt;
			if (version >= 11)
			{
				Impl = new ActionBarDrawerToggleImplHC();
			}
			else
			{
				Impl = new ActionBarDrawerToggleImplCompat();
			}
		}

		// android.R.id.home as defined by public API in v11
		private const int ID_HOME = 0x0102002c;

		private readonly Activity mActivity;
		private readonly DrawerLayout mDrawerLayout;
		private bool mDrawerIndicatorEnabled = true;

		private Drawable mThemeImage;
		private Drawable mDrawerImage;
		private SlideDrawable mSlider;
		private readonly int mDrawerImageResource;
		private readonly int mOpenDrawerContentDescRes;
		private readonly int mCloseDrawerContentDescRes;

		private object mSetIndicatorInfo;

		/// <summary>
		/// Construct a new ActionBarDrawerToggle.
		/// 
		/// <para>The given <seealso cref="Activity"/> will be linked to the specified <seealso cref="DrawerLayout"/>.
		/// The provided drawer indicator drawable will animate slightly off-screen as the drawer
		/// is opened, indicating that in the open state the drawer will move off-screen when pressed
		/// and in the closed state the drawer will move on-screen when pressed.</para>
		/// 
		/// <para>String resources must be provided to describe the open/close drawer actions for
		/// accessibility services.</para>
		/// </summary>
		/// <param name="activity"> The Activity hosting the drawer </param>
		/// <param name="drawerLayout"> The DrawerLayout to link to the given Activity's ActionBar </param>
		/// <param name="drawerImageRes"> A Drawable resource to use as the drawer indicator </param>
		/// <param name="openDrawerContentDescRes"> A String resource to describe the "open drawer" action
		///                                 for accessibility </param>
		/// <param name="closeDrawerContentDescRes"> A String resource to describe the "close drawer" action
		///                                  for accessibility </param>
		public SherlockActionBarDrawerToggle(Activity activity, DrawerLayout drawerLayout, int drawerImageRes, int openDrawerContentDescRes, int closeDrawerContentDescRes)
		{
			mActivity = activity;
			mDrawerLayout = drawerLayout;
			mDrawerImageResource = drawerImageRes;
			mOpenDrawerContentDescRes = openDrawerContentDescRes;
			mCloseDrawerContentDescRes = closeDrawerContentDescRes;

			mThemeImage = Impl.getThemeUpIndicator(activity);
			mDrawerImage = activity.Resources.GetDrawable(drawerImageRes);
			mSlider = new SlideDrawable(mDrawerImage);
			mSlider.OffsetBy = 1.0f / 3;
		}

		/// <summary>
		/// Synchronize the state of the drawer indicator/affordance with the linked DrawerLayout.
		/// 
		/// <para>This should be called from your <code>Activity</code>'s
		/// <seealso cref="Activity#onPostCreate(android.os.Bundle) onPostCreate"/> method to synchronize after
		/// the DrawerLayout's instance state has been restored, and any other time when the state
		/// may have diverged in such a way that the ActionBarDrawerToggle was not notified.
		/// (For example, if you stop forwarding appropriate drawer events for a period of time.)</para>
		/// </summary>
		public virtual void SyncState()
		{
			if (mDrawerLayout.IsDrawerOpen(GravityCompat.Start))
			{
				mSlider.Offset = 1.0f;
			}
			else
			{
				mSlider.Offset = 0.0f;
			}

			if (mDrawerIndicatorEnabled)
			{
                mSetIndicatorInfo = Impl.setActionBarUpIndicator(mSetIndicatorInfo, mActivity, mSlider, mDrawerLayout.IsDrawerOpen(GravityCompat.Start) ? mOpenDrawerContentDescRes : mCloseDrawerContentDescRes);
			}
		}

		/// <summary>
		/// Enable or disable the drawer indicator. The indicator defaults to enabled.
		/// 
		/// <para>When the indicator is disabled, the <code>ActionBar</code> will revert to displaying
		/// the home-as-up indicator provided by the <code>Activity</code>'s theme in the
		/// <code>android.R.attr.homeAsUpIndicator</code> attribute instead of the animated
		/// drawer glyph.</para>
		/// </summary>
		/// <param name="enable"> true to enable, false to disable </param>
		public virtual bool DrawerIndicatorEnabled
		{
			set
			{
				if (value != mDrawerIndicatorEnabled)
				{
					if (value)
					{
						mSetIndicatorInfo = Impl.setActionBarUpIndicator(mSetIndicatorInfo, mActivity, mSlider, mDrawerLayout.IsDrawerOpen(GravityCompat.Start) ? mOpenDrawerContentDescRes : mCloseDrawerContentDescRes);
					}
					else
					{
						mSetIndicatorInfo = Impl.setActionBarUpIndicator(mSetIndicatorInfo, mActivity, mThemeImage, 0);
					}
					mDrawerIndicatorEnabled = value;
				}
			}
			get
			{
				return mDrawerIndicatorEnabled;
			}
		}


		/// <summary>
		/// This method should always be called by your <code>Activity</code>'s
		/// <seealso cref="Activity#onConfigurationChanged(android.content.res.Configuration) onConfigurationChanged"/>
		/// method.
		/// </summary>
		/// <param name="newConfig"> The new configuration </param>
		public virtual void OnConfigurationChanged(Configuration newConfig)
		{
			// Reload drawables that can change with configuration
			mThemeImage = Impl.getThemeUpIndicator(mActivity);
			mDrawerImage = mActivity.Resources.GetDrawable(mDrawerImageResource);
			SyncState();
		}

		/// <summary>
		/// This method should be called by your <code>Activity</code>'s
		/// <seealso cref="Activity#onOptionsItemSelected(android.view.MenuItem) onOptionsItemSelected"/> method.
		/// If it returns true, your <code>onOptionsItemSelected</code> method should return true and
		/// skip further processing.
		/// </summary>
		/// <param name="item"> the MenuItem instance representing the selected menu item </param>
		/// <returns> true if the event was handled and further processing should not occur </returns>
		public virtual bool OnOptionsItemSelected(IMenuItem item)
		{
			if (item != null && item.ItemId == ID_HOME && mDrawerIndicatorEnabled)
			{
				if (mDrawerLayout.IsDrawerVisible(GravityCompat.Start))
				{
                    mDrawerLayout.CloseDrawer(GravityCompat.Start);
				}
				else
				{
                    mDrawerLayout.OpenDrawer(GravityCompat.Start);
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// <seealso cref="DrawerLayout.DrawerListener"/> callback method. If you do not use your
		/// ActionBarDrawerToggle instance directly as your DrawerLayout's listener, you should call
		/// through to this method from your own listener object.
		/// </summary>
		/// <param name="drawerView"> The child view that was moved </param>
		/// <param name="slideOffset"> The new offset of this drawer within its range, from 0-1 </param>
        public virtual void OnDrawerSlide(View drawerView, float slideOffset)
		{
			float glyphOffset = mSlider.Offset;
			if (slideOffset > 0.5f)
			{
				glyphOffset = Math.Max(glyphOffset, Math.Max(0.0f, slideOffset - 0.5f) * 2);
			}
			else
			{
				glyphOffset = Math.Min(glyphOffset, slideOffset * 2);
			}
			mSlider.Offset = glyphOffset;
		}

		/// <summary>
		/// <seealso cref="DrawerLayout.DrawerListener"/> callback method. If you do not use your
		/// ActionBarDrawerToggle instance directly as your DrawerLayout's listener, you should call
		/// through to this method from your own listener object.
		/// </summary>
		/// <param name="drawerView"> Drawer view that is now open </param>
        public virtual void OnDrawerOpened(View drawerView)
		{
			mSlider.Offset = 1.0f;
			if (mDrawerIndicatorEnabled)
			{
				mSetIndicatorInfo = Impl.setActionBarDescription(mSetIndicatorInfo, mActivity, mOpenDrawerContentDescRes);
			}
		}

		/// <summary>
		/// <seealso cref="DrawerLayout.DrawerListener"/> callback method. If you do not use your
		/// ActionBarDrawerToggle instance directly as your DrawerLayout's listener, you should call
		/// through to this method from your own listener object.
		/// </summary>
		/// <param name="drawerView"> Drawer view that is now closed </param>
        public virtual void OnDrawerClosed(View drawerView)
		{
			mSlider.Offset = 0.0f;
			if (mDrawerIndicatorEnabled)
			{
				mSetIndicatorInfo = Impl.setActionBarDescription(mSetIndicatorInfo, mActivity, mCloseDrawerContentDescRes);
			}
		}

		/// <summary>
		/// <seealso cref="DrawerLayout.DrawerListener"/> callback method. If you do not use your
		/// ActionBarDrawerToggle instance directly as your DrawerLayout's listener, you should call
		/// through to this method from your own listener object.
		/// </summary>
		/// <param name="newState"> The new drawer motion state </param>
		public virtual void OnDrawerStateChanged(int newState)
		{
		}

        private class SlideDrawable : Drawable, Drawable.ICallback
		{
			internal Drawable mWrapped;
			internal float mOffset;
			internal float mOffsetBy;

			internal readonly Rect mTmpRect = new Rect();

			public SlideDrawable(Drawable wrapped)
			{
				mWrapped = wrapped;
			}

			public virtual float Offset
			{
				set
				{
					mOffset = value;
                    InvalidateSelf();
				}
				get
				{
					return mOffset;
				}
			}


			public virtual float OffsetBy
			{
				set
				{
					mOffsetBy = value;
					InvalidateSelf();
				}
			}

			public override void Draw(Canvas canvas)
			{
				mWrapped.CopyBounds(mTmpRect);
				canvas.Save();
				canvas.Translate(mOffsetBy * mTmpRect.Width() * -mOffset, 0);
				mWrapped.Draw(canvas);
				canvas.Restore();
			}

			public override ConfigChanges ChangingConfigurations
			{
				set
				{
					mWrapped.ChangingConfigurations = value;
				}
				get
				{
					return mWrapped.ChangingConfigurations;
				}
			}

            public override void SetDither(bool dither)
            {
                mWrapped.SetDither(dither);
            }

            public override void SetFilterBitmap(bool filter)
            {
                mWrapped.SetFilterBitmap(filter);
            }

            public override void SetAlpha(int alpha)
            {
                mWrapped.SetAlpha(alpha);
            }

            public override void SetColorFilter(ColorFilter cf)
            {
                mWrapped.SetColorFilter(cf);
            }

            public override void SetColorFilter(Color color, PorterDuff.Mode mode)
            {
                mWrapped.SetColorFilter(color, mode);
            }

			public override void ClearColorFilter()
			{
				mWrapped.ClearColorFilter();
			}

            public override bool IsStateful
            {
                get { return mWrapped.IsStateful; }
            }

            public override bool SetState(int[] stateSet)
            {
                return mWrapped.SetState(stateSet);
            }

            public override int[] GetState()
            {
                return mWrapped.GetState();
            }
           
			public override Drawable Current
			{
				get
				{
					return mWrapped.Current;
				}
			}

			public override int Opacity
			{
				get
				{
					return mWrapped.Opacity;
				}
			}

			public override Region TransparentRegion
			{
				get
				{
					return mWrapped.TransparentRegion;
				}
			}

			protected override bool OnStateChange(int[] state)
			{
				mWrapped.SetState(state);

                return base.OnStateChange(state);
			}

            protected override void OnBoundsChange(Rect bounds)
			{
                base.OnBoundsChange(bounds);

				mWrapped.Bounds = bounds;
			}

			public override int IntrinsicWidth
			{
				get
				{
					return mWrapped.IntrinsicWidth;
				}
			}

			public override int IntrinsicHeight
			{
				get
				{
					return mWrapped.IntrinsicHeight;
				}
			}

			public override int MinimumWidth
			{
				get
				{
					return mWrapped.MinimumWidth;
				}
			}

			public override int MinimumHeight
			{
				get
				{
					return mWrapped.MinimumHeight;
				}
			}

			public override bool GetPadding(Rect padding)
			{
				return mWrapped.GetPadding(padding);
			}

            public void InvalidateDrawable(Drawable who)
			{
				if (who == mWrapped)
				{
					InvalidateSelf();
				}
			}

            public void ScheduleDrawable(Drawable who, IRunnable what, long when)
			{
				if (who == mWrapped)
				{
					ScheduleSelf(what, when);
				}
			}

            public void UnscheduleDrawable(Drawable who, IRunnable what)
			{
				if (who == mWrapped)
				{
                    UnscheduleSelf(what);
				}
			}
		}
	}
}