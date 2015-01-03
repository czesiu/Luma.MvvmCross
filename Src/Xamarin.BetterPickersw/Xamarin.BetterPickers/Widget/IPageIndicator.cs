/*
 * Copyright (C) 2011 Patrik Akerfeldt
 * Copyright (C) 2011 Jake Wharton
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

using Android.Support.V4.View;

namespace Xamarin.BetterPickers
{
	/// <summary>
	/// A PageIndicator is responsible to show an visual indicator on the total views number and the current visible view.
	/// </summary>
	public interface IPageIndicator : ViewPager.IOnPageChangeListener
	{
		/// <summary>
		/// Bind the indicator to a ViewPager.
		/// </summary>
		ViewPager ViewPager {set;}

		/// <summary>
		/// Bind the indicator to a ViewPager.
		/// </summary>
		void setViewPager(ViewPager view, int initialPosition);

		/// <summary>
		/// <para>Set the current page of both the ViewPager and indicator.</para>
		/// 
		/// <para>This <strong>must</strong> be used if you need to set the page before the views are drawn on screen (e.g.,
		/// default start page).</para>
		/// </summary>
		int CurrentItem {set;}

	    /// <summary>
	    /// Set a page change listener which will receive forwarded events.
	    /// </summary>
	    void SetOnPageChangeListener(ViewPager.IOnPageChangeListener l);

		/// <summary>
		/// Notify the indicator that the fragment list has changed.
		/// </summary>
		void notifyDataSetChanged();
	}

}