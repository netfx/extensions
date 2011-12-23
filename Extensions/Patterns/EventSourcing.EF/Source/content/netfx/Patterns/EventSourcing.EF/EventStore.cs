#region BSD License
/* 
Copyright (c) 2010, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;

/// <summary>
/// Implements an event store on top of EntityFramework 4.1
/// </summary>
/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing.EF"/>
partial class EventStore<TBaseEvent> : DbContext, IEventStore<TBaseEvent>
	where TBaseEvent : ITimestamped
{
	/// <summary>
	/// Initializes a new instance of the <see cref="EventStore{TBaseEvent}"/> class.
	/// </summary>
	/// <param name="nameOrConnectionString">The name or connection string.</param>
	/// <param name="serializer">The serializer to use to persist the events.</param>
	public EventStore(string nameOrConnectionString, ISerializer serializer)
		: base(nameOrConnectionString)
	{
		Guard.NotNull(() => serializer, serializer);

		this.Serializer = serializer;
		this.TypeNameConverter = type => type.Name;
	}

	/// <summary>
	/// Gets the serializer that converts the event payloads to/from a byte array.
	/// </summary>
	public ISerializer Serializer { get; private set; }

	/// <summary>
	/// Gets or sets the function that converts a <see cref="Type"/> to 
	/// its string representation in the store. Used to calculate the 
	/// values of <see cref="StoredAggregate.AggregateType"/> and 
	/// <see cref="StoredEvent.EventType"/>.
	/// </summary>
	public Func<Type, string> TypeNameConverter { get; set; }

	/// <summary>
	/// Gets or sets the events persisted in the store.
	/// </summary>
	public virtual DbSet<StoredEvent> Events { get; set; }

	/// <summary>
	/// Gets or sets the aggregate roots persisted in the store.
	/// </summary>
	public virtual DbSet<StoredAggregate> Aggregates { get; set; }

	/// <summary>
	/// Queries the event store for events that match the given criteria.
	/// </summary>
	public virtual IEnumerable<TBaseEvent> Query(EventQueryCriteria<Guid> criteria)
	{
		var predicate = this.ToExpression(criteria, this.TypeNameConverter);
		var events = this.Events.AsQueryable();

		if (predicate != null)
			events = events.Where(predicate).Cast<StoredEvent>();

		return events
			.OrderBy(x => x.RowVersion)
			// Executes the query against the database
			.AsEnumerable()
			// Dehydrates using the configured serializer.
			.Select(x => this.Serializer.Deserialize<TBaseEvent>(x.Payload));
	}

	/// <summary>
	/// Saves the given events raised by the given sender aggregate root.
	/// </summary>
	/// <param name="aggregate">The aggregate raising the events.</param>
	public void SaveChanges(AggregateRoot<Guid, TBaseEvent> aggregate)
	{
		foreach (var @event in aggregate.GetChanges().ToList())
		{
			SaveEvent(aggregate, @event);
		}

		aggregate.AcceptChanges();
	}

	IQueryable<StoredEvent> IQueryableEventStore<Guid, TBaseEvent, StoredAggregate, StoredEvent>.Events
	{
		get { return this.Events; }
	}	

	private void SaveEvent(AggregateRoot<Guid, TBaseEvent> aggregate, TBaseEvent @event)
	{
		var aggregateFilter = this.AggregateIdEquals(aggregate.Id);
		// First look at the in-memory entities.
		var storedAggregate = this.Aggregates.Local.AsQueryable().FirstOrDefault(aggregateFilter);
		// Fallback to the database
		if (storedAggregate == null)
			storedAggregate = this.Aggregates.FirstOrDefault(aggregateFilter);

		if (storedAggregate == null)
		{
			storedAggregate = this.Aggregates.Add(new StoredAggregate
			{
				AggregateId = aggregate.Id,
				AggregateType = this.TypeNameConverter.Invoke(aggregate.GetType())
			});

			OnSavingAggregate(aggregate, storedAggregate);
		}

		var stored = new StoredEvent
		{
			ActivityId = Trace.CorrelationManager.ActivityId,
			EventId = SequentialGuid.NewGuid(),
			EventType = this.TypeNameConverter.Invoke(@event.GetType()),
			Timestamp = @event.Timestamp.UtcDateTime,
			Payload = this.Serializer.Serialize(@event),
			Aggregate = storedAggregate,
		};

		this.Events.Add(stored);

		OnSavingEvent(@event, stored);

		this.SaveChanges();
	}

	/// <summary>
	/// Extensibility hook called when an event for the given aggregate 
	/// is saved for the first time. At this point, the corresponding 
	/// aggregate header information in <see cref="StoredAggregate"/> entity 
	/// is created and persisted.
	/// </summary>
	/// <param name="aggregate">The aggregate being persisted for the first time in this event store.</param>
	/// <param name="entity">The entity that will be saved to the underlying database.</param>
	protected virtual void OnSavingAggregate(AggregateRoot<Guid, TBaseEvent> aggregate, StoredAggregate entity) { }

	/// <summary>
	/// Extensibility hook called when an event is being saved to the 
	/// store.
	/// </summary>
	/// <param name="event">The event that will be persisted.</param>
	/// <param name="entity">The entity that was created to persist to the underlying database.</param>
	protected virtual void OnSavingEvent(TBaseEvent @event, StoredEvent entity) { }
}