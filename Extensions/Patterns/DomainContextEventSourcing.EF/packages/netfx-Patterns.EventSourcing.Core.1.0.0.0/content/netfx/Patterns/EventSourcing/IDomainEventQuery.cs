using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Provides a fluent API to filter events from the event store. 
/// </summary>
/// <remarks>
/// This interface is returned from the <see cref="DomainEventQueryExtensions.Query"/> 
/// extension method for <see cref="IDomainEventStore{TId}"/>.
/// </remarks>
/// <typeparam name="TId">The type of identifiers used by the aggregate roots.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing.Core"/>
partial interface IDomainEventQuery<TId> : IEnumerable<TimestampedEventArgs>
	where TId : IComparable
{
	/// <summary>
	/// Filters events that target the given aggregate type. Can be called 
	/// multiple times and will filter for any of the specified types (OR operator).
	/// </summary>
	/// <typeparam name="TAggregate">The type of the aggregate root to filter events for.</typeparam>
	IDomainEventQuery<TId> For<TAggregate>();

	/// <summary>
	/// Filters events that target the given aggregate type and identifier. Can be called 
	/// multiple times and will filter for any of the specified types and ids (OR operator).
	/// </summary>
	/// <typeparam name="TAggregate">The type of the aggregate root to filter events for.</typeparam>
	/// <param name="aggregateId">The aggregate root identifier that the events apply to.</param>
	IDomainEventQuery<TId> For<TAggregate>(TId aggregateId);

	/// <summary>
	/// Filters events that are assignable to the given type. Can be called 
	/// multiple times and will filter for any of the specified types (OR operator).
	/// </summary>
	/// <typeparam name="TEventArgs">The type of the events to filter.</typeparam>
	IDomainEventQuery<TId> OfType<TEventArgs>();

	/// <summary>
	/// Filters events that happened after the given starting date.
	/// </summary>
	/// <param name="when">The starting date to filter by.</param>
	/// <remarks>
	/// By default, includes events with the given date, unless the 
	/// <see cref="ExclusiveDateRange"/> is called to make the range exclusive.
	/// </remarks>
	IDomainEventQuery<TId> Since(DateTime when);

	/// <summary>
	/// Filters events that happened before the given ending date.
	/// </summary>
	/// <param name="when">The ending date to filter by.</param>
	/// <remarks>
	/// By default, includes events with the given date, unless the 
	/// <see cref="ExclusiveDateRange"/> is called to make the range exclusive.
	/// </remarks>
	IDomainEventQuery<TId> Until(DateTime when);

	/// <summary>
	/// Makes the configured <see cref="Since"/> and/or <see cref="Until"/> dates 
	/// exclusive, changing the default behavior which is to be inclusive.
	/// </summary>
	IDomainEventQuery<TId> ExclusiveDateRange();
}