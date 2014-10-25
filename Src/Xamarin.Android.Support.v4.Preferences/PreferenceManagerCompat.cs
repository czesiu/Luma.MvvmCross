using System;
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
using System.Reflection;
using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Java.Lang;
using Java.Lang.Reflect;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace Android.Support.v4.Preferences
{
	public class PreferenceManagerCompat
	{

		private static readonly string TAG = typeof(PreferenceManagerCompat).Name;

		/// <summary>
		/// Interface definition for a callback to be invoked when a
		/// <seealso cref="Preference"/> in the hierarchy rooted at this <seealso cref="PreferenceScreen"/> is
		/// clicked.
		/// </summary>
		internal interface IOnPreferenceTreeClickListener
		{
			/// <summary>
			/// Called when a preference in the tree rooted at this
			/// <seealso cref="PreferenceScreen"/> has been clicked.
			/// </summary>
			/// <param name="preferenceScreen"> The <seealso cref="PreferenceScreen"/> that the
			///        preference is located in. </param>
			/// <param name="preference"> The preference that was clicked. </param>
			/// <returns> Whether the click was handled. </returns>
			bool OnPreferenceTreeClick(PreferenceScreen preferenceScreen, Preference preference);
		}

		internal static PreferenceManager NewInstance(Activity activity, int firstRequestCode)
		{
			try
			{
                var preferenceManagerClass = Object.GetObject<Class>(JNIEnv.FindClass(typeof (PreferenceManager)), JniHandleOwnership.DoNotTransfer);
                var activityClass = Object.GetObject<Class>(JNIEnv.FindClass(typeof (Activity)), JniHandleOwnership.DoNotTransfer);

                var constructor = preferenceManagerClass.GetDeclaredConstructor(activityClass, Integer.Type);
                constructor.Accessible = true;
                return constructor.NewInstance(activity, firstRequestCode) as PreferenceManager;
			}
			catch (Exception e)
			{
				Log.Warn(TAG, "Couldn't call constructor PreferenceManager by reflection", e);
			}

			return null;
		}

		/// <summary>
		/// Sets the owning preference fragment
		/// </summary>
		internal static void SetFragment(PreferenceManager manager, PreferenceFragment fragment)
		{
			// stub
		}

		/// <summary>
		/// Sets the callback to be invoked when a <seealso cref="Preference"/> in the
		/// hierarchy rooted at this <seealso cref="PreferenceManager"/> is clicked.
		/// </summary>
		/// <param name="listener"> The callback to be invoked. </param>
		internal static void SetOnPreferenceTreeClickListener(PreferenceManager manager, IOnPreferenceTreeClickListener listener)
		{
			try
			{
                var onPreferenceTreeClickListener = manager.Class.GetDeclaredField("mOnPreferenceTreeClickListener");
                onPreferenceTreeClickListener.Accessible = true;
                if (listener != null)
                {
                    Object proxy = Proxy.NewProxyInstance(manager.Class.ClassLoader,
                        new[] { onPreferenceTreeClickListener.Type },
                        new InvocationHandlerAnonymousInnerClassHelper(listener));

                    onPreferenceTreeClickListener.Set(manager, proxy);
                }
                else
                {
                    onPreferenceTreeClickListener.Set(manager, null);
                }
			}
			catch (Exception e)
			{
				Log.Warn(TAG, "Couldn't set PreferenceManager.mOnPreferenceTreeClickListener by reflection", e);
			}
		}

		private class InvocationHandlerAnonymousInnerClassHelper : Object, IInvocationHandler
		{
		    private readonly IOnPreferenceTreeClickListener _listener;

		    public InvocationHandlerAnonymousInnerClassHelper(IOnPreferenceTreeClickListener listener)
			{
			    _listener = listener;
			}

		    public Object Invoke(Object proxy, Method method, Object[] args)
		    {
                if (method.Name.Equals("onPreferenceTreeClick"))
                {
                    return Convert.ToBoolean(_listener.OnPreferenceTreeClick((PreferenceScreen)args[0], (Preference)args[1]));
                }
                
                return null;
		    }
		}

		/// <summary>
		/// Inflates a preference hierarchy from the preference hierarchies of
		/// <seealso cref="Activity Activities"/> that match the given <seealso cref="Intent"/>. An
		/// <seealso cref="Activity"/> defines its preference hierarchy with meta-data using
		/// the <seealso cref="#METADATA_KEY_PREFERENCES"/> key.
		/// <para>
		/// If a preference hierarchy is given, the new preference hierarchies will
		/// be merged in.
		/// 
		/// </para>
		/// </summary>
		/// <param name="queryIntent"> The intent to match activities. </param>
		/// <param name="rootPreferences"> Optional existing hierarchy to merge the new
		///            hierarchies into. </param>
		/// <returns> The root hierarchy (if one was not provided, the new hierarchy's
		///         root). </returns>
		internal static PreferenceScreen InflateFromIntent(PreferenceManager manager, Intent intent, PreferenceScreen screen)
		{
			try
			{
                var m = manager.Class.GetDeclaredMethod("inflateFromIntent", intent.Class, screen.Class);
                m.Accessible = true;
                var prefScreen = (PreferenceScreen)m.Invoke(manager, intent, screen);
                return prefScreen;
            }
			catch (Exception e)
			{
				Log.Warn(TAG, "Couldn't call PreferenceManager.inflateFromIntent by reflection", e);
			}
			return null;
		}

		/// <summary>
		/// Inflates a preference hierarchy from XML. If a preference hierarchy is
		/// given, the new preference hierarchies will be merged in.
		/// </summary>
		/// <param name="context"> The context of the resource. </param>
		/// <param name="resId"> The resource ID of the XML to inflate. </param>
		/// <param name="rootPreferences"> Optional existing hierarchy to merge the new
		///            hierarchies into. </param>
		/// <returns> The root hierarchy (if one was not provided, the new hierarchy's
		///         root).
		/// @hide </returns>
		internal static PreferenceScreen InflateFromResource(PreferenceManager manager, Activity activity, int resId, PreferenceScreen screen)
		{
			try
			{
                var preferenceScreenClass = Object.GetObject<Class>(JNIEnv.FindClass(typeof(PreferenceScreen)), JniHandleOwnership.DoNotTransfer);
                var contextClass = Object.GetObject<Class>(JNIEnv.FindClass(typeof(Context)), JniHandleOwnership.DoNotTransfer);

                var m = manager.Class.GetDeclaredMethod("inflateFromResource", contextClass, Integer.Type, preferenceScreenClass);
                m.Accessible = true;
                var prefScreen = (PreferenceScreen)m.Invoke(manager, activity, resId, screen);
                return prefScreen;
			}
			catch (Exception e)
			{
				Log.Warn(TAG, "Couldn't call PreferenceManager.inflateFromResource by reflection", e);
			}
			return null;
		}

		/// <summary>
		/// Returns the root of the preference hierarchy managed by this class.
		/// </summary>
		/// <returns> The <seealso cref="PreferenceScreen"/> object that is at the root of the hierarchy. </returns>
		internal static PreferenceScreen GetPreferenceScreen(PreferenceManager manager)
		{
			try
			{
                var m = manager.Class.GetDeclaredMethod("getPreferenceScreen");
                m.Accessible = true;
                return (PreferenceScreen)m.Invoke(manager);
			}
			catch (Exception e)
			{
				Log.Warn(TAG, "Couldn't call PreferenceManager.getPreferenceScreen by reflection", e);
			}
			return null;
		}

		/// <summary>
		/// Called by the <seealso cref="PreferenceManager"/> to dispatch a subactivity result.
		/// </summary>
		internal static void DispatchActivityResult(PreferenceManager manager, int requestCode, int resultCode, Intent data)
		{
			try
			{
                var m = manager.Class.GetDeclaredMethod("dispatchActivityResult", Integer.Type, Integer.Type, data.Class);
                m.Accessible = true;
                m.Invoke(manager, requestCode, resultCode, data);
			}
			catch (Exception e)
			{
				Log.Warn(TAG, "Couldn't call PreferenceManager.dispatchActivityResult by reflection", e);
			}
		}

		/// <summary>
		/// Called by the <seealso cref="PreferenceManager"/> to dispatch the activity stop
		/// event.
		/// </summary>
		internal static void DispatchActivityStop(PreferenceManager manager)
		{
			try
			{
                var m = manager.Class.GetDeclaredMethod("dispatchActivityStop");
                m.Accessible = true;
                m.Invoke(manager);
			}
			catch (Exception e)
			{
				Log.Warn(TAG, "Couldn't call PreferenceManager.dispatchActivityStop by reflection", e);
			}
		}

		/// <summary>
		/// Called by the <seealso cref="PreferenceManager"/> to dispatch the activity destroy
		/// event.
		/// </summary>
		internal static void DispatchActivityDestroy(PreferenceManager manager)
		{
			try
			{
                var m = manager.Class.GetDeclaredMethod("dispatchActivityDestroy");
                m.Accessible = true;
                m.Invoke(manager);
			}
			catch (Exception e)
			{
				Log.Warn(TAG, "Couldn't call PreferenceManager.dispatchActivityDestroy by reflection", e);
			}
		}

		/// <summary>
		/// Sets the root of the preference hierarchy.
		/// </summary>
		/// <param name="preferenceScreen"> The root <seealso cref="PreferenceScreen"/> of the preference hierarchy. </param>
		/// <returns> Whether the <seealso cref="PreferenceScreen"/> given is different than the previous.  </returns>
		internal static bool SetPreferences(PreferenceManager manager, PreferenceScreen screen)
		{
			try
			{
                var preferenceScreenClass = Object.GetObject<Class>(JNIEnv.FindClass(typeof(PreferenceScreen)), JniHandleOwnership.DoNotTransfer);
                var m = manager.Class.GetDeclaredMethod("setPreferences", preferenceScreenClass);
                m.Accessible = true;
                return (bool) m.Invoke(manager, screen);
			}
			catch (Exception e)
			{
				Log.Warn(TAG, "Couldn't call PreferenceManager.setPreferences by reflection", e);
			}

			return false;
		}
	}
}