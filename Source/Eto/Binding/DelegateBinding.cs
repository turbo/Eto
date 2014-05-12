using System;

namespace Eto
{
	public class DelegateBinding<TValue> : DirectBinding
	{
		public Func<TValue> GetValue { get; set; }

		public Action<TValue> SetValue { get; set; }

		public Func<object, TValue> ChangeType { get; set; }

		public Action<EventHandler<EventArgs>> AddChangeEvent { get; set; }

		public Action<EventHandler<EventArgs>> RemoveChangeEvent { get; set; }


		public override object DataValue
		{
			get { return GetValue(); }
			set
			{ 
				if (value is TValue)
					SetValue((TValue)value);
				if (ChangeType != null)
					SetValue(ChangeType(value));
			}
		}

		void HandleChangedEvent(object sender, EventArgs e)
		{
			OnDataValueChanged(e);
		}

		/// <summary>
		/// Hooks up the late bound events for this object
		/// </summary>
		protected override void HandleEvent(string id)
		{
			switch (id)
			{
				case DataValueChangedEvent:
					AddChangeEvent(new EventHandler<EventArgs>(HandleChangedEvent));
					break;
				default:
					base.HandleEvent(id);
					break;
			}
		}

		/// <summary>
		/// Removes the late bound events for this object
		/// </summary>
		protected override void RemoveEvent(string id)
		{
			switch (id)
			{
				case DataValueChangedEvent:
					RemoveChangeEvent(new EventHandler<EventArgs>(HandleChangedEvent));
					break;
				default:
					base.RemoveEvent(id);
					break;
			}
		}
	}

	/// <summary>
	/// Binding using delegate methods
	/// </summary>
	/// <copyright>(c) 2014 by Curtis Wensley</copyright>
	/// <license type="BSD-3">See LICENSE for full terms</license>
	public class DelegateBinding<T, TValue> : IndirectBinding
	{
		static readonly Type underlyingType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

		public new Func<T, TValue> GetValue { get; set; }

		public new Action<T, TValue> SetValue { get; set; }

		public Action<T, EventHandler<EventArgs>> AddChangeEvent { get; set; }

		public Action<T, EventHandler<EventArgs>> RemoveChangeEvent { get; set; }

		public TValue DefaultGetValue { get; set; }

		public TValue DefaultSetValue { get; set; }

		public DelegateBinding(Func<T, TValue> getValue, Action<T, TValue> setValue = null, Action<T, EventHandler<EventArgs>> addChangeEvent = null, Action<T, EventHandler<EventArgs>> removeChangeEvent = null, TValue defaultGetValue = default(TValue), TValue defaultSetValue = default(TValue))
		{
			GetValue = getValue;
			DefaultGetValue = defaultGetValue;
			SetValue = setValue;
			DefaultSetValue = defaultSetValue;
			if (addChangeEvent != null && removeChangeEvent != null)
			{
				AddChangeEvent = addChangeEvent;
				RemoveChangeEvent = removeChangeEvent;
			}
			else if (addChangeEvent != null || removeChangeEvent != null)
				throw new ArgumentException("You must either specify both the add and remove change event delegates, or pass null for both");
		}

		protected override object InternalGetValue(object dataItem)
		{
			if (GetValue != null && dataItem != null)
				return GetValue((T)dataItem);
			return DefaultGetValue;
		}

		protected virtual TValue ChangeType(object value)
		{
#if PCL
			// TODO: what do we need to do about DBNull here? 
			// See http://stackoverflow.com/questions/21357321/handling-missing-types-in-pcl-with-real-types-existing-on-some-of-the-platforms

			return (value == null) // || DBNull.Value.Equals(value))  
				? DefaultSetValue
				: (TValue)Convert.ChangeType(value, underlyingType);
#else
			return (value == null || DBNull.Value.Equals(value)) 
				? DefaultSetValue
				: (TValue)Convert.ChangeType(value, underlyingType);
#endif
		}

		protected override void InternalSetValue(object dataItem, object value)
		{
			if (SetValue != null && dataItem != null)
				SetValue((T)dataItem, ChangeType(value));
		}

		/// <summary>
		/// Wires an event handler to fire when the property of the dataItem is changed
		/// </summary>
		/// <param name="dataItem">object to detect changes on</param>
		/// <param name="handler">handler to fire when the property changes on the specified dataItem</param>
		/// <returns>binding reference used to track the event hookup, to pass to <see cref="RemoveValueChangedHandler"/> when removing the handler</returns>
		public override object AddValueChangedHandler(object dataItem, EventHandler<EventArgs> handler)
		{
			if (AddChangeEvent != null && dataItem != null)
			{
				AddChangeEvent((T)dataItem, handler);
				return dataItem;
			}
			return false;
		}

		/// <summary>
		/// Removes the handler for the specified reference from <see cref="AddValueChangedHandler"/>
		/// </summary>
		/// <param name="bindingReference">Reference from the call to <see cref="AddValueChangedHandler"/></param>
		/// <param name="handler">Same handler that was set up during the <see cref="AddValueChangedHandler"/> call</param>
		public override void RemoveValueChangedHandler(object bindingReference, EventHandler<EventArgs> handler)
		{
			if (RemoveChangeEvent != null && bindingReference is T)
			{
				var dataItem = bindingReference;
				RemoveChangeEvent((T)dataItem, handler);
			}
		}
	}
}
