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

using System;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Exception = System.Exception;
using String = Java.Lang.String;

namespace Android.Support.v4.Preferences
{
	public abstract class PreferenceFragment : Fragment, PreferenceManagerCompat.IOnPreferenceTreeClickListener
	{
        /// <summary>
        /// The starting request code given out to preference framework.
        /// </summary>
        private const int FIRST_REQUEST_CODE = 100;

        private const int MSG_BIND_PREFERENCES = 1;

		private const string PREFERENCES_TAG = "android:preferences";

		private PreferenceManager _preferenceManager;
		private ListView _listView;
		private bool _havePrefs;
		private bool _initDone;
        private readonly Handler _handler;
        private readonly Runnable _requestFocus;

	    protected PreferenceFragment()
	    {
	        _requestFocus = new Runnable(() => _listView.FocusableViewAvailable(_listView));
            _handler = new Handler(message =>
            {
                switch (message.What)
                {

                    case MSG_BIND_PREFERENCES:
                        BindPreferences();
                        break;
                }
            });
	    }

		/// <summary>
		/// Interface that PreferenceFragment's containing activity should
		/// implement to be able to process preference items that wish to
		/// switch to a new fragment.
		/// </summary>
        public interface IOnPreferenceStartFragmentCallback : IJavaObject, IDisposable
        {
            bool OnPreferenceStartFragment(PreferenceFragment caller, Preference pref);
        }

		public override void OnCreate(Bundle paramBundle)
		{
            base.OnCreate(paramBundle);
			_preferenceManager = PreferenceManagerCompat.NewInstance(Activity, FIRST_REQUEST_CODE);
			PreferenceManagerCompat.SetFragment(_preferenceManager, this);
		}

		public override Views.View OnCreateView(LayoutInflater paramLayoutInflater, ViewGroup paramViewGroup, Bundle paramBundle)
		{
			return paramLayoutInflater.Inflate(Resource.Layout.PreferenceFragment, paramViewGroup, false);
		}

		public override void OnActivityCreated(Bundle savedInstanceState)
		{
            base.OnActivityCreated(savedInstanceState);

			if (_havePrefs)
			{
				BindPreferences();
			}

			_initDone = true;

			if (savedInstanceState != null)
			{
				var container = savedInstanceState.GetBundle(PREFERENCES_TAG);
				if (container != null)
				{

					var preferenceScreen = PreferenceScreen;
					if (preferenceScreen != null)
					{
						preferenceScreen.RestoreHierarchyState(container);
					}
				}
			}
		}

		public override void OnStart()
		{
            base.OnStart();

			PreferenceManagerCompat.SetOnPreferenceTreeClickListener(_preferenceManager, this);
		}

        public override void OnStop()
		{
			base.OnStop();

			PreferenceManagerCompat.DispatchActivityStop(_preferenceManager);
			PreferenceManagerCompat.SetOnPreferenceTreeClickListener(_preferenceManager, null);
		}

		public override void OnDestroyView()
		{
			_listView = null;
			_handler.RemoveCallbacks(_requestFocus);
			_handler.RemoveMessages(MSG_BIND_PREFERENCES);
            base.OnDestroyView();
		}

		public override void OnDestroy()
		{
            base.OnDestroy();

			PreferenceManagerCompat.DispatchActivityDestroy(_preferenceManager);
		}

		public override void OnSaveInstanceState(Bundle outState)
		{
            base.OnSaveInstanceState(outState);

			var preferenceScreen = PreferenceScreen;
			if (preferenceScreen != null)
			{
				var container = new Bundle();
				preferenceScreen.SaveHierarchyState(container);
				outState.PutBundle(PREFERENCES_TAG, container);
			}
		}

		public override void OnActivityResult(int requestCode, int resultCode, Intent data)
		{
            base.OnActivityResult(requestCode, resultCode, data);

			PreferenceManagerCompat.DispatchActivityResult(_preferenceManager, requestCode, resultCode, data);
		}

		/// <summary>
		/// Returns the <seealso cref="PreferenceManager"/> used by this fragment. </summary>
		/// <returns> The <seealso cref="PreferenceManager"/>. </returns>
		public virtual PreferenceManager PreferenceManager
		{
			get
			{
				return _preferenceManager;
			}
		}

		/// <summary>
		/// Sets the root of the preference hierarchy that this fragment is showing.
		/// </summary>
		/// <param name="preferenceScreen"> The root <seealso cref="PreferenceScreen"/> of the preference hierarchy. </param>
		public virtual PreferenceScreen PreferenceScreen
		{
			set
			{
				if (PreferenceManagerCompat.SetPreferences(_preferenceManager, value) && value != null)
				{
					_havePrefs = true;
					if (_initDone)
					{
						PostBindPreferences();
					}
				}
			}
			get
			{
				return PreferenceManagerCompat.GetPreferenceScreen(_preferenceManager);
			}
		}


		/// <summary>
		/// Adds preferences from activities that match the given <seealso cref="Intent"/>.
		/// </summary>
		/// <param name="intent"> The <seealso cref="Intent"/> to query activities. </param>
		public virtual void AddPreferencesFromIntent(Intent intent)
		{
            RequirePreferenceManager();

			PreferenceScreen = PreferenceManagerCompat.InflateFromIntent(_preferenceManager, intent, PreferenceScreen);
		}

		/// <summary>
		/// Inflates the given XML resource and adds the preference hierarchy to the current
		/// preference hierarchy.
		/// </summary>
		/// <param name="preferencesResId"> The XML resource ID to inflate. </param>
		public virtual void AddPreferencesFromResource(int preferencesResId)
		{
			RequirePreferenceManager();

			PreferenceScreen = PreferenceManagerCompat.InflateFromResource(_preferenceManager, Activity, preferencesResId, PreferenceScreen);
		}

		public virtual bool OnPreferenceTreeClick(PreferenceScreen preferenceScreen, Preference preference)
		{
			if (Activity is IOnPreferenceStartFragmentCallback)
			{
                return ((IOnPreferenceStartFragmentCallback)Activity).OnPreferenceStartFragment(this, preference);
			}
			return false;
		}

		/// <summary>
		/// Finds a <seealso cref="Preference"/> based on its key.
		/// </summary>
		/// <param name="key"> The key of the preference to retrieve. </param>
		/// <returns> The <seealso cref="Preference"/> with the key, or null. </returns>
		/// <seealso cref= PreferenceGroup#findPreference(CharSequence) </seealso>
		public virtual Preference FindPreference(String key)
		{
			if (_preferenceManager == null)
			{
				return null;
			}
			return _preferenceManager.FindPreference(key);
		}

		private void RequirePreferenceManager()
		{
			if (_preferenceManager == null)
			{
				throw new Exception("This should be called after super.onCreate.");
			}
		}

		private void PostBindPreferences()
		{
			if (_handler.HasMessages(MSG_BIND_PREFERENCES))
			{
				return;
			}
			_handler.ObtainMessage(MSG_BIND_PREFERENCES).SendToTarget();
		}

		private void BindPreferences()
		{
			var preferenceScreen = PreferenceScreen;
			if (preferenceScreen != null)
			{
				preferenceScreen.Bind(ListView);
			}
		}

		public virtual ListView ListView
		{
			get
			{
				EnsureList();
				return _listView;
			}
		}

		private void EnsureList()
		{
			if (_listView != null)
			{
				return;
			}
			var root = View;
			if (root == null)
			{
				throw new IllegalStateException("Content view not yet created");
			}
			var rawListView = root.FindViewById(Android.Resource.Id.List);
			if (!(rawListView is ListView))
			{
				throw new Exception("Content has view with id attribute 'android.R.id.list' that is not a ListView class");
			}
			_listView = (ListView)rawListView;
			if (_listView == null)
			{
				throw new Exception("Your content must have a ListView whose id attribute is " + "'android.R.id.list'");
			}
			_listView.KeyPress += (sender, args) =>
			{
			    object selectedItem = _listView.SelectedItem;
				if (selectedItem is Preference)
				{
					var selectedView = _listView.SelectedView;
                    //return ((Preference)selectedItem).onKey(
                    //        selectedView, keyCode, event);
				}
			};
			_handler.Post(_requestFocus);
		}
	}
}