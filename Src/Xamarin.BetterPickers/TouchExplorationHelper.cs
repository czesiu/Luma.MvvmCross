/*
 * Copyright (C) 2012 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.View.Accessibility;
using Android.Text;
using Android.Views;
using Android.Views.Accessibility;

namespace Xamarin.BetterPickers
{
    //using Context = android.content.Context;
    //using Rect = android.graphics.Rect;
    //using Bundle = android.os.Bundle;
    //using AccessibilityDelegateCompat = android.support.v4.view.AccessibilityDelegateCompat;
    //using ViewCompat = android.support.v4.view.ViewCompat;
    //using AccessibilityNodeInfoCompat = android.support.v4.view.accessibility.AccessibilityNodeInfoCompat;
    //using AccessibilityNodeProviderCompat = android.support.v4.view.accessibility.AccessibilityNodeProviderCompat;
    //using AccessibilityRecordCompat = android.support.v4.view.accessibility.AccessibilityRecordCompat;
    //using TextUtils = android.text.TextUtils;
    //using View = android.view.View;
    //using ViewGroup = android.view.ViewGroup;
    //using AccessibilityEvent = android.view.accessibility.AccessibilityEvent;
    //using AccessibilityManager = android.view.accessibility.AccessibilityManager;

	public abstract class TouchExplorationHelper<T> : AccessibilityNodeProviderCompat
        where T : class
			/* Removed for backwards compatibility to GB implements View.OnHoverListener*/
	{
		/// <summary>
		/// Virtual node identifier value for invalid nodes.
		/// </summary>
		public static readonly int INVALID_ID = int.MinValue;

		private readonly Rect mTempScreenRect = new Rect();
		private readonly Rect mTempParentRect = new Rect();
		private readonly Rect mTempVisibleRect = new Rect();
		private readonly int[] mTempGlobalRect = new int[2];

		private readonly AccessibilityManager mManager;

		private View mParentView;
		private int mFocusedItemId = INVALID_ID;
		private T mCurrentItem;

		/// <summary>
		/// Constructs a new touch exploration helper.
		/// </summary>
		/// <param name="context"> The parent context. </param>
		public TouchExplorationHelper(Context context, View parentView)
		{
			mManager = (AccessibilityManager) context.GetSystemService(Context.AccessibilityService);
			mParentView = parentView;
		}

		/// <returns> The current accessibility focused item, or {@code null} if no item is focused. </returns>
		public virtual T FocusedItem
		{
			get
			{
				return getItemForId(mFocusedItemId);
			}
			set
			{
				int itemId = getIdForItem(value);
				if (itemId == INVALID_ID)
				{
					return;
				}
    
				PerformAction(itemId, AccessibilityNodeInfoCompat.ActionAccessibilityFocus, null);
			}
		}

		/// <summary>
		/// Clears the current accessibility focused item.
		/// </summary>
		public virtual void clearFocusedItem()
		{
			int itemId = mFocusedItemId;
			if (itemId == INVALID_ID)
			{
				return;
			}

			PerformAction(itemId, AccessibilityNodeInfoCompat.ActionClearAccessibilityFocus, null);
		}


		/// <summary>
		/// Invalidates cached information about the parent view. <para> You <b>must</b> call this method after adding or
		/// removing items from the parent view. </para>
		/// </summary>
		public virtual void invalidateParent()
		{
			mParentView.SendAccessibilityEvent((EventTypes)AccessibilityEventCompat.TypeWindowContentChanged);
		}

		/// <summary>
		/// Invalidates cached information for a particular item. <para> You <b>must</b> call this method when any of the
		/// properties set in <seealso cref="#populateNodeForItem(Object, AccessibilityNodeInfoCompat)"/> have changed. </para>
		/// </summary>
		public virtual void invalidateItem(T item)
		{
            sendEventForItem(item, (EventTypes)AccessibilityEventCompat.TypeWindowContentChanged);
		}

		/// <summary>
		/// Populates an event of the specified type with information about an item and attempts to send it up through the
		/// view hierarchy.
		/// </summary>
		/// <param name="item"> The item for which to send an event. </param>
		/// <param name="eventType"> The type of event to send. </param>
		/// <returns> {@code true} if the event was sent successfully. </returns>
		public virtual bool sendEventForItem(T item, EventTypes eventType)
		{
			if (!mManager.IsEnabled)
			{
				return false;
			}

			var @event = getEventForItem(item, eventType);
			var group = (ViewGroup) mParentView.Parent;

			return group.RequestSendAccessibilityEvent(mParentView, @event);
		}

		public override AccessibilityNodeInfoCompat CreateAccessibilityNodeInfo(int virtualViewId)
		{
			if (virtualViewId == View.NoId)
			{
				return NodeForParent;
			}

			T item = getItemForId(virtualViewId);
			if (item == null)
			{
				return null;
			}

            AccessibilityNodeInfoCompat node = AccessibilityNodeInfoCompat.Obtain();
			populateNodeForItemInternal(item, node);
			return node;
		}

		public override bool PerformAction(int virtualViewId, int action, Bundle arguments)
		{
            if (virtualViewId == View.NoId)
			{
				return ViewCompat.PerformAccessibilityAction(mParentView, action, arguments);
			}

			T item = getItemForId(virtualViewId);
			if (item == null)
			{
				return false;
			}

			bool handled = false;

			switch (action)
			{
				case AccessibilityNodeInfoCompat.ActionAccessibilityFocus:
					if (mFocusedItemId != virtualViewId)
					{
						mFocusedItemId = virtualViewId;
						sendEventForItem(item, (EventTypes)AccessibilityEventCompat.TypeViewAccessibilityFocused);
						handled = true;
					}
					break;
				case AccessibilityNodeInfoCompat.ActionClearAccessibilityFocus:
					if (mFocusedItemId == virtualViewId)
					{
						mFocusedItemId = INVALID_ID;
                        sendEventForItem(item, (EventTypes)AccessibilityEventCompat.TypeViewAccessibilityFocusCleared);
						handled = true;
					}
					break;
			}

			handled |= performActionForItem(item, action, arguments);

			return handled;
		}

		/* Removed for backwards compatibility to GB
		@Override
		public boolean onHover(View view, MotionEvent event) {
		    if (!AccessibilityManagerCompat.isTouchExplorationEnabled(mManager)) {
		        return false;
		    }
	
		    switch (event.getAction()) {
		        case MotionEvent.ACTION_HOVER_ENTER:
		        case MotionEvent.ACTION_HOVER_MOVE:
		            final T item = getItemAt(event.getX(), event.getY());
		            setCurrentItem(item);
		            return true;
		        case MotionEvent.ACTION_HOVER_EXIT:
		            setCurrentItem(null);
		            return true;
		    }
	
		    return false;
		}*/

		private T CurrentItem
		{
			set
			{
				if (mCurrentItem == value)
				{
					return;
				}
    
				if (mCurrentItem != null)
				{
					sendEventForItem(mCurrentItem, (EventTypes)AccessibilityEventCompat.TypeViewHoverExit);
				}
    
				mCurrentItem = value;
    
				if (mCurrentItem != null)
				{
                    sendEventForItem(mCurrentItem, (EventTypes)AccessibilityEventCompat.TypeViewHoverExit);
				}
			}
		}

		private AccessibilityEvent getEventForItem(T item, EventTypes eventType)
		{
			var @event = AccessibilityEvent.Obtain(eventType);
            var record = new AccessibilityRecordCompat(@event);
			int virtualDescendantId = getIdForItem(item);

			// Ensure the client has good defaults.
			@event.Enabled = true;

			// Allow the client to populate the event.
			populateEventForItem(item, @event);

			if (TextUtils.IsEmpty(@event.Text.ToString()) && TextUtils.IsEmpty(@event.ContentDescription))
			{
				throw new Exception("You must add text or a content description in populateEventForItem()");
			}

			// Don't allow the client to override these properties.
			@event.ClassName = item.GetType().FullName;
			@event.PackageName = mParentView.Context.PackageName;
			record.SetSource(mParentView, virtualDescendantId);

			return @event;
		}

		private AccessibilityNodeInfoCompat NodeForParent
		{
			get
			{
                var info = AccessibilityNodeInfoCompat.Obtain(mParentView);
				ViewCompat.OnInitializeAccessibilityNodeInfo(mParentView, info);
    
                var items = new LinkedList<T>();

				getVisibleItems(items);
    
				foreach (T item in items)
				{
                    int virtualDescendantId = getIdForItem(item);
					info.AddChild(mParentView, virtualDescendantId);
				}
    
				return info;
			}
		}

		private AccessibilityNodeInfoCompat populateNodeForItemInternal(T item, AccessibilityNodeInfoCompat node)
		{
			var virtualDescendantId = getIdForItem(item);

			// Ensure the client has good defaults.
			node.Enabled = true;

			// Allow the client to populate the node.
			populateNodeForItem(item, node);

            if (TextUtils.IsEmpty(node.Text) && TextUtils.IsEmpty(node.ContentDescription))
			{
				throw new Exception("You must add text or a content description in populateNodeForItem()");
			}

			// Don't allow the client to override these properties.
			node.PackageName = mParentView.Context.PackageName;
			node.ClassName = item.GetType().FullName;
			node.SetParent(mParentView);
			node.SetSource(mParentView, virtualDescendantId);

			if (mFocusedItemId == virtualDescendantId)
			{
				node.AddAction(AccessibilityNodeInfoCompat.ActionClearAccessibilityFocus);
			}
			else
			{
                node.AddAction(AccessibilityNodeInfoCompat.ActionAccessibilityFocus);
			}

			node.GetBoundsInParent(mTempParentRect);
			if (mTempParentRect.IsEmpty)
			{
				throw new Exception("You must set parent bounds in populateNodeForItem()");
			}

			// Set the visibility based on the parent bound.
			if (intersectVisibleToUser(mTempParentRect))
			{
				node.VisibleToUser = true;
				node.SetBoundsInParent(mTempParentRect);
			}

			// Calculate screen-relative bound.
			mParentView.GetLocationOnScreen(mTempGlobalRect);
            var offsetX = mTempGlobalRect[0];
			var offsetY = mTempGlobalRect[1];
			mTempScreenRect.Set(mTempParentRect);
			mTempScreenRect.Offset(offsetX, offsetY);
			node.SetBoundsInScreen(mTempScreenRect);

			return node;
		}

		/// <summary>
		/// Computes whether the specified <seealso cref="android.graphics.Rect"/> intersects with the visible portion of its parent
		/// <seealso cref="android.view.View"/>. Modifies {@code localRect} to contain only the visible portion.
		/// </summary>
		/// <param name="localRect"> A rectangle in local (parent) coordinates. </param>
		/// <returns> Whether the specified <seealso cref="android.graphics.Rect"/> is visible on the screen. </returns>
		private bool intersectVisibleToUser(Rect localRect)
		{
			// Missing or empty bounds mean this view is not visible.
			if ((localRect == null) || localRect.IsEmpty)
			{
				return false;
			}

			// Attached to invisible window means this view is not visible.
			if (mParentView.WindowVisibility != ViewStates.Visible)
			{
				return false;
			}

			// An invisible predecessor or one with alpha zero means
			// that this view is not visible to the user.
			object current = this;
			while (current is View)
			{
				View view = (View) current;
				// We have attach info so this view is attached and there is no
				// need to check whether we reach to ViewRootImpl on the way up.
				if ((view.Alpha <= 0) || (view.Visibility != ViewStates.Visible))
				{
					return false;
				}
				current = view.Parent;
			}

			// If no portion of the parent is visible, this view is not visible.
			if (!mParentView.GetLocalVisibleRect(mTempVisibleRect))
			{
				return false;
			}

			// Check if the view intersects the visible portion of the parent.
			return localRect.Intersect(mTempVisibleRect);
		}

		public virtual AccessibilityDelegateCompat AccessibilityDelegate
		{
			get
			{
				return mDelegate;
			}
		}

		private readonly AccessibilityDelegateCompat mDelegate = new AccessibilityDelegateCompatAnonymousInnerClassHelper();

		private class AccessibilityDelegateCompatAnonymousInnerClassHelper : AccessibilityDelegateCompat
		{
			public AccessibilityDelegateCompatAnonymousInnerClassHelper()
			{
			}

			public override void OnInitializeAccessibilityEvent(View view, AccessibilityEvent e)
			{
                base.OnInitializeAccessibilityEvent(view, e);

				e.ClassName = view.GetType().FullName;
			}

			public override void OnInitializeAccessibilityNodeInfo(View view, AccessibilityNodeInfoCompat a)
			{
                base.OnInitializeAccessibilityNodeInfo(view, a);

				a.ClassName = view.GetType().FullName;
			}
		}

		/// <summary>
		/// Performs an accessibility action on the specified item. See {@link AccessibilityNodeInfoCompat#performAction(int,
		/// android.os.Bundle)}. <para> The helper class automatically handles focus management resulting from {@link
		/// AccessibilityNodeInfoCompat#ACTION_ACCESSIBILITY_FOCUS} and <seealso cref="AccessibilityNodeInfoCompat#ACTION_CLEAR_ACCESSIBILITY_FOCUS"/>,
		/// so typically a developer only needs to handle actions added manually in the {{@link #populateNodeForItem(Object,
		/// AccessibilityNodeInfoCompat)} method. </para>
		/// </summary>
		/// <param name="item"> The item on which to perform the action. </param>
		/// <param name="action"> The accessibility action to perform. </param>
		/// <param name="arguments"> Arguments for the action, or optionally {@code null}. </param>
		/// <returns> {@code true} if the action was performed successfully. </returns>
		protected internal abstract bool performActionForItem(T item, int action, Bundle arguments);

		/// <summary>
		/// Populates an event with information about the specified item. <para> At a minimum, a developer must populate the
		/// event text by doing one of the following: <ul> <li>appending text to <seealso cref="android.view.accessibility.AccessibilityEvent#getText()"/></li>
		/// <li>populating a description with <seealso cref="android.view.accessibility.AccessibilityEvent#setContentDescription(CharSequence)"/></li>
		/// </ul> </para>
		/// </summary>
		/// <param name="item"> The item for which to populate the event. </param>
		/// <param name="event"> The event to populate. </param>
		protected internal abstract void populateEventForItem(T item, AccessibilityEvent @event);

		/// <summary>
		/// Populates a node with information about the specified item. <para> At a minimum, a developer must: <ul> <li>populate
		/// the event text using <seealso cref="AccessibilityNodeInfoCompat#setText(CharSequence)"/> or {@link
		/// AccessibilityNodeInfoCompat#setContentDescription(CharSequence)} </li> <li>set the item's parent-relative bounds
		/// using <seealso cref="AccessibilityNodeInfoCompat#setBoundsInParent(android.graphics.Rect)"/> </ul>
		/// 
		/// </para>
		/// </summary>
		/// <param name="item"> The item for which to populate the node. </param>
		/// <param name="node"> The node to populate. </param>
		protected internal abstract void populateNodeForItem(T item, AccessibilityNodeInfoCompat node);

		/// <summary>
		/// Populates a list with the parent view's visible items. <para> The result of this method is cached until the
		/// developer calls <seealso cref="#invalidateParent()"/>. </para>
		/// </summary>
		/// <param name="items"> The list to populate with visible items. </param>
		protected internal abstract void getVisibleItems(ICollection<T> items);

		/// <summary>
		/// Returns the item under the specified parent-relative coordinates.
		/// </summary>
		/// <param name="x"> The parent-relative x coordinate. </param>
		/// <param name="y"> The parent-relative y coordinate. </param>
		/// <returns> The item under coordinates (x,y). </returns>
		protected internal abstract T getItemAt(float x, float y);

		/// <summary>
		/// Returns the unique identifier for an item. If the specified item does not exist, returns <seealso cref="#INVALID_ID"/>. <para>
		/// This result of this method must be consistent with <seealso cref="#getItemForId(int)"/>. </para>
		/// </summary>
		/// <param name="item"> The item whose identifier to return. </param>
		/// <returns> A unique identifier, or <seealso cref="#INVALID_ID"/>. </returns>
		protected internal abstract int getIdForItem(T item);

		/// <summary>
		/// Returns the item for a unique identifier. If the specified item does not exist, returns {@code null}.
		/// </summary>
		/// <param name="id"> The identifier for the item to return. </param>
		/// <returns> An item, or {@code null}. </returns>
		protected internal abstract T getItemForId(int id);
	}

}