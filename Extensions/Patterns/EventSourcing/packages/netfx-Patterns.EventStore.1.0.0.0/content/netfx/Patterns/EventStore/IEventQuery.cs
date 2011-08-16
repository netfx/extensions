using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Provides a fluent API to filter events from the event store. 
/// </summary>
/// <remarks>
/// This interface is returned from the <see cref="EventQueryExtensions.Query"/> 
/// extension method for <see cref="IEventStore{TBaseEvent}"/>.
/// </remarks>
/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the system.</typeparam>
/// <nuget id="netfx-Patterns.EventStore"/>
partial interface IEventQuery<TBaseEvent> : IEnumerable<TBaseEvent>
{
	/// <summary>
	/// Gets the criteria that was built using the fluent API so far.
	/// </summary>
	StoredEventCriteria Criteria { get; }

	/// <summary>
	/// Filters events that are assignable to the given type. Can be called 
	/// multiple times and will filter for any of the specified types (OR operator).
	/// </summary>
	/// <typeparam name="TEvent">The type of the events to filter.</typeparam>
	IEventQuery<TBaseEvent> OfType<TEvent>() where TEvent : TBaseEvent;

	/// <summary>
	/// Filters events that happened after the given starting date.
	/// </summary>
	/// <param name="when">The starting date to filter by.</param>
	/// <remarks>
	/// By default, includes events with the given date, unless the 
	/// <see cref="ExclusiveRange"/> is called to make the range exclusive.
	/// </remarks>
	IEventQuery<TBaseEvent> Since(DateTime when);

	/// <summary>
	/// Filters events that happened before the given ending date.
	/// </summary>
	/// <param name="when">The ending date to filter by.</param>
	/// <remarks>
	/// By default, includes events with the given date, unless the 
	/// <see cref="ExclusiveRange"/> is called to make the range exclusive.
	/// </remarks>
	IEventQuery<TBaseEvent> Until(DateTime when);

	/// <summary>
	/// Makes the configured <see cref="Since"/> and/or <see cref="Until"/> dates 
	/// exclusive, changing the default behavior which is to be inclusive.
	/// </summary>
	IEventQuery<TBaseEvent> ExclusiveRange();
}