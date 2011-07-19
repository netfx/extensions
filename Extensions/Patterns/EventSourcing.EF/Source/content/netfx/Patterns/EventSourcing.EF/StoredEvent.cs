using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents a persisted event in the <see cref="DomainEventStore{TId, TStoredEvent}"/> store. 
/// Must be inherited by a concrete non-generic class that specifies the type of <typeparamref name="TId"/>.
/// </summary>
/// <typeparam name="TId">The type of identifiers used by the aggregate roots.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing.EF"/>
abstract partial class StoredEvent<TId> : IStoredEvent<TId>
	where TId : IComparable
{
	/// <summary>
	/// Event identifier in the database.
	/// </summary>
	public virtual long Id { get; set; }

	/// <summary>
	/// Gets the aggregate id that the event applies to.
	/// </summary>
	public virtual TId AggregateId { get; set; }

	/// <summary>
	/// Gets the type of the aggregate root that this event applies to.
	/// </summary>
	public virtual string AggregateType { get; set; }

	/// <summary>
	/// Gets the type of the event.
	/// </summary>
	public virtual string EventType { get; set; }

	/// <summary>
	/// Gets the UTC timestamp of the event.
	/// </summary>
	public virtual DateTime Timestamp { get; set; }

	/// <summary>
	/// Gets or sets the payload of the event.
	/// </summary>
	public virtual string Payload { get; set; }
}
