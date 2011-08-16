using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Provides a fluent API to filter events from the event store. 
/// </summary>
/// <remarks>
/// This interface is returned from the <see cref="DomainEventQueryExtensions.Query"/> 
/// extension method for <see cref="IDomainEventStore{TAggregateId, TBaseEvent}"/>.
/// </remarks>
/// <typeparam name="TAggregateId">The type of identifier used by the aggregate roots in the domain.</typeparam>
/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing"/>
partial interface IDomainEventQuery<TAggregateId, TBaseEvent> : IEnumerable<TBaseEvent>
	where TAggregateId : IComparable
{
	/// <summary>
	/// Gets the criteria that was built using the fluent API so far.
	/// </summary>
	StoredEventCriteria<TAggregateId> Criteria { get; }

	/// <summary>
	/// Filters events that target the given aggregate root type. Can be called 
	/// multiple times and will filter for any of the specified types (OR operator).
	/// </summary>
	/// <typeparam name="TAggregate">The type of the aggregate root to filter events for.</typeparam>
	IDomainEventQuery<TAggregateId, TBaseEvent> For<TAggregate>();

	/// <summary>
	/// Filters events that target the given aggregate root type and identifier. Can be called 
	/// multiple times and will filter for any of the specified types and ids (OR operator).
	/// </summary>
	/// <typeparam name="TAggregate">The type of the aggregate root to filter events for.</typeparam>
	/// <param name="aggregateId">The aggregate root identifier to filter by.</param>
	IDomainEventQuery<TAggregateId, TBaseEvent> For<TAggregate>(TAggregateId aggregateId);

	/// <summary>
	/// Filters events that are assignable to the given type. Can be called 
	/// multiple times and will filter for any of the specified types (OR operator).
	/// </summary>
	/// <typeparam name="TEvent">The type of the events to filter.</typeparam>
	IDomainEventQuery<TAggregateId, TBaseEvent> OfType<TEvent>() where TEvent : TBaseEvent;

	/// <summary>
	/// Filters events that happened after the given starting date.
	/// </summary>
	/// <param name="when">The starting date to filter by.</param>
	/// <remarks>
	/// By default, includes events with the given date, unless the 
	/// <see cref="ExclusiveRange"/> is called to make the range exclusive.
	/// </remarks>
	IDomainEventQuery<TAggregateId, TBaseEvent> Since(DateTime when);

	/// <summary>
	/// Filters events that happened before the given ending date.
	/// </summary>
	/// <param name="when">The ending date to filter by.</param>
	/// <remarks>
	/// By default, includes events with the given date, unless the 
	/// <see cref="ExclusiveRange"/> is called to make the range exclusive.
	/// </remarks>
	IDomainEventQuery<TAggregateId, TBaseEvent> Until(DateTime when);

	/// <summary>
	/// Makes the configured <see cref="Since"/> and/or <see cref="Until"/> dates 
	/// exclusive, changing the default behavior which is to be inclusive.
	/// </summary>
	IDomainEventQuery<TAggregateId, TBaseEvent> ExclusiveRange();
}