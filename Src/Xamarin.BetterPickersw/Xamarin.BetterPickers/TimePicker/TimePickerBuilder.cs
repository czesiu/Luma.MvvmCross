using System.Collections.Generic;
using Android.Support.V4.App;
using Android.Util;
using com.doomonafireball.betterpickers.timepicker;

namespace Xamarin.BetterPickers.TimePicker
{
    //using TimePickerDialogHandler = com.doomonafireball.betterpickers.timepicker.TimePickerDialogFragment.TimePickerDialogHandler;

    //using Fragment = android.support.v4.app.Fragment;
    //using FragmentManager = android.support.v4.app.FragmentManager;
    //using FragmentTransaction = android.support.v4.app.FragmentTransaction;
    //using Log = android.util.Log;

	/// <summary>
	/// User: derek Date: 5/2/13 Time: 7:55 PM
	/// </summary>
	public class TimePickerBuilder
	{
		private FragmentManager manager; // Required
		private int? styleResId; // Required
		private Fragment targetFragment;
		private int mReference = -1;
		private List<TimePickerDialogFragment.TimePickerDialogHandler> mTimePickerDialogHandlers = new List<TimePickerDialogFragment.TimePickerDialogHandler>();

		/// <summary>
		/// Attach a FragmentManager. This is required for creation of the Fragment.
		/// </summary>
		/// <param name="manager"> the FragmentManager that handles the transaction </param>
		/// <returns> the current Builder object </returns>
		public virtual TimePickerBuilder SetFragmentManager(FragmentManager manager)
		{
			this.manager = manager;
			return this;
		}

		/// <summary>
		/// Attach a style resource ID for theming. This is required for creation of the Fragment. Two stock styles are
		/// provided usingResource.style.BetterPickersDialogFragment andResource.style.BetterPickersDialogFragment.Light
		/// </summary>
		/// <param name="styleResId"> the style resource ID to use for theming </param>
		/// <returns> the current Builder object </returns>
		public virtual TimePickerBuilder setStyleResId(int styleResId)
		{
			this.styleResId = styleResId;
			return this;
		}

		/// <summary>
		/// Attach a target Fragment. This is optional and useful if creating a Picker within a Fragment.
		/// </summary>
		/// <param name="targetFragment"> the Fragment to attach to </param>
		/// <returns> the current Builder object </returns>
		public virtual TimePickerBuilder setTargetFragment(Fragment targetFragment)
		{
			this.targetFragment = targetFragment;
			return this;
		}

		/// <summary>
		/// Attach a reference to this Picker instance. This is used to track multiple pickers, if the user wishes.
		/// </summary>
		/// <param name="reference"> a user-defined int intended for Picker tracking </param>
		/// <returns> the current Builder object </returns>
		public virtual TimePickerBuilder setReference(int reference)
		{
			this.mReference = reference;
			return this;
		}

		/// <summary>
		/// Attach universal objects as additional handlers for notification when the Picker is set. For most use cases, this
		/// method is not necessary as attachment to an Activity or Fragment is done automatically.  If, however, you would
		/// like additional objects to subscribe to this Picker being set, attach Handlers here.
		/// </summary>
		/// <param name="handler"> an Object implementing the appropriate Picker Handler </param>
		/// <returns> the current Builder object </returns>
		public virtual TimePickerBuilder addTimePickerDialogHandler(TimePickerDialogFragment.TimePickerDialogHandler handler)
		{
			this.mTimePickerDialogHandlers.Add(handler);
			return this;
		}

		/// <summary>
		/// Remove objects previously added as handlers.
		/// </summary>
		/// <param name="handler"> the Object to remove </param>
		/// <returns> the current Builder object </returns>
		public virtual TimePickerBuilder removeTimePickerDialogHandler(TimePickerDialogFragment.TimePickerDialogHandler handler)
		{
			this.mTimePickerDialogHandlers.Remove(handler);
			return this;
		}

		/// <summary>
		/// Instantiate and show the Picker
		/// </summary>
		public virtual void show()
		{
			if (manager == null || styleResId == null)
			{
				Log.Error("TimePickerBuilder", "setFragmentManager() and setStyleResId() must be called.");
				return;
			}

			var ft = manager.BeginTransaction();
			var prev = manager.FindFragmentByTag("time_dialog");
			if (prev != null)
			{
				ft.Remove(prev);
			}
			ft.AddToBackStack(null);

			var fragment = TimePickerDialogFragment.newInstance(mReference, styleResId.Value);
			if (targetFragment != null)
			{
				fragment.SetTargetFragment(targetFragment, 0);
			}
			fragment.TimePickerDialogHandlers = mTimePickerDialogHandlers;
			fragment.Show(ft, "time_dialog");
		}
	}

}