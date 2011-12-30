using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/// <summary>
/// Provides a common API for event stores that leverage Linq.
/// </summary>
/// <typeparam name="TObjectId">The type of identifier used by domain objects in the domain.</typeparam>
/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
/// <typeparam name="TStoredEvent">The type of the stored event.</typeparam>
/// <typeparam name="TStoredObject">The type of the stored object.</typeparam>
partial interface IQueryableEventStore<TObjectId, TBaseEvent, TStoredObject, TStoredEvent> : IEventStore<TObjectId, TBaseEvent>
	where TBaseEvent : ITimestamped
	where TStoredObject : class, IStoredObject<TObjectId>
	where TStoredEvent : class, IStoredEvent<TStoredObject, TObjectId>
{
	/// <summary>
	/// Gets the stream of events persisted by the store.
	/// </summary>
	IQueryable<TStoredEvent> Events { get; }
}
