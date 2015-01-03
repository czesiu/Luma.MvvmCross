using System.Collections.Generic;
using Android.Support.V4.App;
using Android.Util;

namespace com.doomonafireball.betterpickers.datepicker
{
    //using Fragment = android.support.v4.app.Fragment;
    //using FragmentManager = android.support.v4.app.FragmentManager;
    //using FragmentTransaction = android.support.v4.app.FragmentTransaction;
    //using Log = android.util.Log;

	/// <summary>
	/// User: derek Date: 5/2/13 Time: 7:55 PM
	/// </summary>
	public class DatePickerBuilder
	{

		private FragmentManager manager; // Required
		private int? styleResId; // Required
		private Fragment targetFragment;
		private int? monthOfYear;
		private int? dayOfMonth;
		private int? year;
		private int mReference = -1;
		private List<DatePickerDialogFragment.DatePickerDialogHandler> mDatePickerDialogHandlers = new List<DatePickerDialogFragment.DatePickerDialogHandler>();

		/// <summary>
		/// Attach a FragmentManager. This is required for creation of the Fragment.
		/// </summary>
		/// <param name="manager"> the FragmentManager that handles the transaction </param>
		/// <returns> the current Builder object </returns>
		public virtual DatePickerBuilder setFragmentManager(FragmentManager manager)
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
		public virtual DatePickerBuilder setStyleResId(int styleResId)
		{
			this.styleResId = styleResId;
			return this;
		}

		/// <summary>
		/// Attach a target Fragment. This is optional and useful if creating a Picker within a Fragment.
		/// </summary>
		/// <param name="targetFragment"> the Fragment to attach to </param>
		/// <returns> the current Builder object </returns>
		public virtual DatePickerBuilder setTargetFragment(Fragment targetFragment)
		{
			this.targetFragment = targetFragment;
			return this;
		}

		/// <summary>
		/// Attach a reference to this Picker instance. This is used to track multiple pickers, if the user wishes.
		/// </summary>
		/// <param name="reference"> a user-defined int intended for Picker tracking </param>
		/// <returns> the current Builder object </returns>
		public virtual DatePickerBuilder setReference(int reference)
		{
			this.mReference = reference;
			return this;
		}

		/// <summary>
		/// Pre-set a zero-indexed month of year. This is highly frowned upon as it contributes to user confusion.  The
		/// Pickers do a great job of making input quick and easy, and thus it is preferred to always start with a blank
		/// slate.
		/// </summary>
		/// <param name="monthOfYear"> the zero-indexed month of year to pre-set </param>
		/// <returns> the current Builder object </returns>
		public virtual DatePickerBuilder setMonthOfYear(int monthOfYear)
		{
			this.monthOfYear = monthOfYear;
			return this;
		}

		/// <summary>
		/// Pre-set a day of month. This is highly frowned upon as it contributes to user confusion.  The Pickers do a great
		/// job of making input quick and easy, and thus it is preferred to always start with a blank slate.
		/// </summary>
		/// <param name="dayOfMonth"> the day of month to pre-set </param>
		/// <returns> the current Builder object </returns>
		public virtual DatePickerBuilder setDayOfMonth(int dayOfMonth)
		{
			this.dayOfMonth = dayOfMonth;
			return this;
		}

		/// <summary>
		/// Pre-set a year. This is highly frowned upon as it contributes to user confusion.  The Pickers do a great job of
		/// making input quick and easy, and thus it is preferred to always start with a blank slate.
		/// </summary>
		/// <param name="year"> the year to pre-set </param>
		/// <returns> the current Builder object </returns>
		public virtual DatePickerBuilder setYear(int year)
		{
			this.year = year;
			return this;
		}

		/// <summary>
		/// Attach universal objects as additional handlers for notification when the Picker is set. For most use cases, this
		/// method is not necessary as attachment to an Activity or Fragment is done automatically.  If, however, you would
		/// like additional objects to subscribe to this Picker being set, attach Handlers here.
		/// </summary>
		/// <param name="handler"> an Object implementing the appropriate Picker Handler </param>
		/// <returns> the current Builder object </returns>
		public virtual DatePickerBuilder addDatePickerDialogHandler(DatePickerDialogFragment.DatePickerDialogHandler handler)
		{
			this.mDatePickerDialogHandlers.Add(handler);
			return this;
		}

		/// <summary>
		/// Remove objects previously added as handlers.
		/// </summary>
		/// <param name="handler"> the Object to remove </param>
		/// <returns> the current Builder object </returns>
		public virtual DatePickerBuilder removeDatePickerDialogHandler(DatePickerDialogFragment.DatePickerDialogHandler handler)
		{
			this.mDatePickerDialogHandlers.Remove(handler);
			return this;
		}

		/// <summary>
		/// Instantiate and show the Picker
		/// </summary>
		public virtual void show()
		{
			if (manager == null || styleResId == null)
			{
				Log.Error("DatePickerBuilder", "setFragmentManager() and setStyleResId() must be called.");
				return;
			}

			FragmentTransaction ft = manager.BeginTransaction();
			Fragment prev = manager.FindFragmentByTag("date_dialog");
			if (prev != null)
			{
				ft.Remove(prev);
			}
			ft.AddToBackStack(null);

			var fragment = DatePickerDialogFragment.newInstance(mReference, styleResId.Value, monthOfYear, dayOfMonth, year);
			if (targetFragment != null)
			{
				fragment.SetTargetFragment(targetFragment, 0);
			}
			fragment.DatePickerDialogHandlers = mDatePickerDialogHandlers;
			fragment.Show(ft, "date_dialog");
		}
	}

}