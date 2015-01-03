using Android.Content;
using Android.Database;
using Android.Net;
using Android.OS;
using Android.Provider;

namespace Xamarin.BetterPickers
{

    //using Service = android.app.Service;
    //using Context = android.content.Context;
    //using ContentObserver = android.database.ContentObserver;
    //using Uri = android.net.Uri;
    //using SystemClock = android.os.SystemClock;
    //using Vibrator = android.os.Vibrator;
    //using Settings = android.provider.Settings;

	/// <summary>
	/// A simple utility class to handle haptic feedback.
	/// </summary>
	public class HapticFeedbackController
	{
		private const int VIBRATE_DELAY_MS = 125;
		private const int VIBRATE_LENGTH_MS = 5;

		private static bool checkGlobalSetting(Context context)
		{
			return Settings.System.GetInt(context.ContentResolver, Settings.System.HapticFeedbackEnabled, 0) == 1;
		}

		private readonly Context mContext;
		private readonly ContentObserver mContentObserver;

		private Vibrator mVibrator;
		private bool mIsGloballyEnabled;
		private long mLastVibrate;

		public HapticFeedbackController(Context context)
		{
			mContext = context;
			mContentObserver = new ContentObserverAnonymousInnerClassHelper(this);
		}

		private class ContentObserverAnonymousInnerClassHelper : ContentObserver
		{
			private readonly HapticFeedbackController outerInstance;

			public ContentObserverAnonymousInnerClassHelper(HapticFeedbackController outerInstance) : base(null)
			{
				this.outerInstance = outerInstance;
			}

			public override void OnChange(bool selfChange)
			{
				outerInstance.mIsGloballyEnabled = checkGlobalSetting(outerInstance.mContext);
			}
		}

		/// <summary>
		/// Call to setup the controller.
		/// </summary>
		public virtual void Start()
		{
			mVibrator = (Vibrator) mContext.GetSystemService(Context.VibratorService);

			// Setup a listener for changes in haptic feedback settings
			mIsGloballyEnabled = checkGlobalSetting(mContext);
			Uri uri = Settings.System.GetUriFor(Settings.System.HapticFeedbackEnabled);
			mContext.ContentResolver.RegisterContentObserver(uri, false, mContentObserver);
		}

		/// <summary>
		/// Call this when you don't need the controller anymore.
		/// </summary>
		public virtual void Stop()
		{
			mVibrator = null;
			mContext.ContentResolver.UnregisterContentObserver(mContentObserver);
		}

		/// <summary>
		/// Try to vibrate. To prevent this becoming a single continuous vibration, nothing will
		/// happen if we have vibrated very recently.
		/// </summary>
		public virtual void TryVibrate()
		{
			if (mVibrator != null && mIsGloballyEnabled)
			{
				long now = SystemClock.UptimeMillis();
				// We want to try to vibrate each individual tick discretely.
				if (now - mLastVibrate >= VIBRATE_DELAY_MS)
				{
					mVibrator.Vibrate(VIBRATE_LENGTH_MS);
					mLastVibrate = now;
				}
			}
		}
	}

}