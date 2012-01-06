using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Reactive
{
	/// <summary>
	/// Represents the Sender and EventArg values of a .NET event.
	/// </summary>
	/// <typeparam name="TEventArgs">The type of the event args.</typeparam>
	/// <remarks>
	/// Provides an interface that is missing in Rx since the existing interface 
	/// was changed to a concrete class <see cref="EventPattern{TEventArgs}"/>.
	/// <para>
	/// This interface is useful to make the events in the stream more flexible 
	/// for subscribers, as it makes them covariant on the event argument type.
	/// See this thread for more information: http://kzu.to/ryXd9o
	/// </para>
	/// </remarks>
	/// <nuget id="netfx-System.Reactive.Event"/>
	public interface IEvent<out TEventArgs>
		where TEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the sender value of the event.
		/// </summary>
		object Sender { get; }

		/// <summary>
		/// Gets the event arguments value of the event.
		/// </summary>
		TEventArgs EventArgs { get; }
	}

	/// <summary>
	/// Represents the Sender and EventArg values of a .NET event.
	/// </summary>
	/// <typeparam name="TSender">The type of the sender of the event.</typeparam>
	/// <typeparam name="TEventArgs">The type of the event args.</typeparam>
	/// <remarks>
	/// Provides an interface that is missing in Rx since the existing interface 
	/// was changed to a concrete class <see cref="EventPattern{TEventArgs}"/>.
	/// <para>
	/// This interface is useful to make the events in the stream more flexible 
	/// for subscribers, as it makes them covariant on the event argument type.
	/// See this thread for more information: http://kzu.to/ryXd9o
	/// </para>
	/// </remarks>
	/// <nuget id="netfx-System.Reactive.Event"/>
	public interface IEvent<out TSender, out TEventArgs> : IEvent<TEventArgs>
		where TEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the sender value of the event.
		/// </summary>
		/// <remarks>
		/// Intentionally hides the base Sender property to provide a strong-typed version.
		/// </remarks>
		new TSender Sender { get; }
	}
}
