using System.Collections.Generic;
using Android.Support.V4.App;
using Android.Util;

namespace Xamarin.BetterPickers
{
    //using Fragment = android.support.v4.app.Fragment;
    //using FragmentManager = android.support.v4.app.FragmentManager;
    //using FragmentTransaction = android.support.v4.app.FragmentTransaction;
    //using Log = android.util.Log;

	/// <summary>
	/// User: derek Date: 5/2/13 Time: 7:55 PM
	/// </summary>
	public class NumberPickerBuilder
	{

		private FragmentManager manager; // Required
		private int? styleResId; // Required
		private Fragment targetFragment;
		private int? minNumber;
		private int? maxNumber;
		private int? plusMinusVisibility;
		private int? decimalVisibility;
		private string labelText;
		private int mReference;
		private List<NumberPickerDialogFragment.NumberPickerDialogHandler> mNumberPickerDialogHandlers = new List<NumberPickerDialogFragment.NumberPickerDialogHandler>();

		/// <summary>
		/// Attach a FragmentManager. This is required for creation of the Fragment.
		/// </summary>
		/// <param name="manager"> the FragmentManager that handles the transaction </param>
		/// <returns> the current Builder object </returns>
		public virtual NumberPickerBuilder setFragmentManager(FragmentManager manager)
		{
			this.manager = manager;
			return this;
		}

		/// <summary>
		/// Attach a style resource ID for theming. This is required for creation of the Fragment. Two stock styles are
		/// provided using Resource.style.BetterPickersDialogFragment and Resource.style.BetterPickersDialogFragment.Light
		/// </summary>
		/// <param name="styleResId"> the style resource ID to use for theming </param>
		/// <returns> the current Builder object </returns>
		public virtual NumberPickerBuilder setStyleResId(int styleResId)
		{
			this.styleResId = styleResId;
			return this;
		}

		/// <summary>
		/// Attach a target Fragment. This is optional and useful if creating a Picker within a Fragment.
		/// </summary>
		/// <param name="targetFragment"> the Fragment to attach to </param>
		/// <returns> the current Builder object </returns>
		public virtual NumberPickerBuilder setTargetFragment(Fragment targetFragment)
		{
			this.targetFragment = targetFragment;
			return this;
		}

		/// <summary>
		/// Attach a reference to this Picker instance. This is used to track multiple pickers, if the user wishes.
		/// </summary>
		/// <param name="reference"> a user-defined int intended for Picker tracking </param>
		/// <returns> the current Builder object </returns>
		public virtual NumberPickerBuilder setReference(int reference)
		{
			this.mReference = reference;
			return this;
		}

		/// <summary>
		/// Set a minimum number required
		/// </summary>
		/// <param name="minNumber"> the minimum required number </param>
		/// <returns> the current Builder object </returns>
		public virtual NumberPickerBuilder setMinNumber(int minNumber)
		{
			this.minNumber = minNumber;
			return this;
		}

		/// <summary>
		/// Set a maximum number required
		/// </summary>
		/// <param name="maxNumber"> the maximum required number </param>
		/// <returns> the current Builder object </returns>
		public virtual NumberPickerBuilder setMaxNumber(int maxNumber)
		{
			this.maxNumber = maxNumber;
			return this;
		}

		/// <summary>
		/// Set the visibility of the +/- button. This takes an int corresponding to Android's View.VISIBLE, View.INVISIBLE,
		/// or View.GONE.  When using View.INVISIBLE, the +/- button will still be present in the layout but be
		/// non-clickable. When set to View.GONE, the +/- button will disappear entirely, and the "0" button will occupy its
		/// space.
		/// </summary>
		/// <param name="plusMinusVisibility"> an int corresponding to View.VISIBLE, View.INVISIBLE, or View.GONE </param>
		/// <returns> the current Builder object </returns>
		public virtual NumberPickerBuilder setPlusMinusVisibility(int plusMinusVisibility)
		{
			this.plusMinusVisibility = plusMinusVisibility;
			return this;
		}

		/// <summary>
		/// Set the visibility of the decimal button. This takes an int corresponding to Android's View.VISIBLE,
		/// View.INVISIBLE, or View.GONE.  When using View.INVISIBLE, the decimal button will still be present in the layout
		/// but be non-clickable. When set to View.GONE, the decimal button will disappear entirely, and the "0" button will
		/// occupy its space.
		/// </summary>
		/// <param name="decimalVisibility"> an int corresponding to View.VISIBLE, View.INVISIBLE, or View.GONE </param>
		/// <returns> the current Builder object </returns>
		public virtual NumberPickerBuilder setDecimalVisibility(int decimalVisibility)
		{
			this.decimalVisibility = decimalVisibility;
			return this;
		}

		/// <summary>
		/// Set the (optional) text shown as a label. This is useful if wanting to identify data with the number being
		/// selected.
		/// </summary>
		/// <param name="labelText"> the String text to be shown </param>
		/// <returns> the current Builder object </returns>
		public virtual NumberPickerBuilder setLabelText(string labelText)
		{
			this.labelText = labelText;
			return this;
		}


		/// <summary>
		/// Attach universal objects as additional handlers for notification when the Picker is set. For most use cases, this
		/// method is not necessary as attachment to an Activity or Fragment is done automatically.  If, however, you would
		/// like additional objects to subscribe to this Picker being set, attach Handlers here.
		/// </summary>
		/// <param name="handler"> an Object implementing the appropriate Picker Handler </param>
		/// <returns> the current Builder object </returns>
		public virtual NumberPickerBuilder addNumberPickerDialogHandler(NumberPickerDialogFragment.NumberPickerDialogHandler handler)
		{
			this.mNumberPickerDialogHandlers.Add(handler);
			return this;
		}

		/// <summary>
		/// Remove objects previously added as handlers.
		/// </summary>
		/// <param name="handler"> the Object to remove </param>
		/// <returns> the current Builder object </returns>
		public virtual NumberPickerBuilder removeNumberPickerDialogHandler(NumberPickerDialogFragment.NumberPickerDialogHandler handler)
		{
			this.mNumberPickerDialogHandlers.Remove(handler);
			return this;
		}

		/// <summary>
		/// Instantiate and show the Picker
		/// </summary>
		public virtual void show()
		{
			if (manager == null || styleResId == null)
			{
				Log.Error("NumberPickerBuilder", "setFragmentManager() and setStyleResId() must be called.");
				return;
			}
			FragmentTransaction ft = manager.BeginTransaction();
			Fragment prev = manager.FindFragmentByTag("number_dialog");
			if (prev != null)
			{
				ft.Remove(prev);
			}
			ft.AddToBackStack(null);

            NumberPickerDialogFragment fragment = NumberPickerDialogFragment.newInstance(mReference, styleResId.Value, minNumber, maxNumber, plusMinusVisibility, decimalVisibility, labelText);
			if (targetFragment != null)
			{
				fragment.SetTargetFragment(targetFragment, 0);
			}
			fragment.NumberPickerDialogHandlers = mNumberPickerDialogHandlers;
			fragment.Show(ft, "number_dialog");
		}
	}

}