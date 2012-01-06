using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Reactive
{
	/// <summary>
	/// Provides a factory class for <see cref="Event{TSender, TEventArgs}"/>.
	/// </summary>
	/// <nuget id="netfx-System.Reactive.Event"/>
	public static class Event
	{
		/// <summary>
		/// Creates the specified event.
		/// </summary>
		/// <typeparam name="TSender">The type of the sender of the event.</typeparam>
		/// <typeparam name="TEventArgs">The type of the event args.</typeparam>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="args">The instance containing the event data.</param>
		public static Event<TSender, TEventArgs> Create<TSender, TEventArgs>(TSender sender, TEventArgs args)
			where TEventArgs : EventArgs
		{
			return new Event<TSender, TEventArgs>(sender, args);
		}
	}

	/// <summary>
	/// Represents the Sender and EventArg values of a .NET event.
	/// </summary>
	/// <remarks>
	/// This class inherits from the built-in <see cref="EventPattern{TEventArgs}"/> 
	/// but makes it covariant for the arguments class type.
	/// </remarks>
	/// <nuget id="netfx-System.Reactive.Event"/>
	public class Event<TSender, TEventArgs> : EventPattern<TEventArgs>, IEvent<TSender, TEventArgs>
		where TEventArgs : EventArgs
	{
		/// <summary>
		/// Represents the Sender and EventArg values of a .NET event.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A <typeparamref name="TEventArgs"/> that contains the event data.</param>
		public Event(TSender sender, TEventArgs e)
			: base(sender, e)
		{
		}

		/// <summary>
		/// Gets the sender value of the event.
		/// </summary>
		/// <remarks>
		/// Intentionally hides the base Sender property to provide a strong-typed version.
		/// </remarks>
		public new TSender Sender { get { return (TSender)base.Sender; } }
	}
}