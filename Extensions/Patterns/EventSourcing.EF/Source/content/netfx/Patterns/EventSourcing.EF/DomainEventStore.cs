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
using Newtonsoft.Json;
using System.IO;

/// <summary>
/// Implements an event store on top of EntityFramework 4.1
/// </summary>
/// <typeparam name="TAggregateId">The type of identifier used by aggregate roots in the domain.</typeparam>
/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
/// <typeparam name="TStoredEvent">The type of the stored event, which must inherit from <see cref="StoredEvent{TAggregateId}"/> for a specific <typeparamref name="TAggregateId"/>.</typeparam>
/// <remarks>
/// This class is abstract and must be inherited by a concrete 
/// store class that provides a fixed type for the <typeparamref name="TAggregateId"/> and 
/// <typeparamref name="TStoredEvent"/> type parameters, as entity framework cannot 
/// work with generic entities.
/// </remarks>
/// <example>
/// The following is an example of a concrete event store leveraging this class:
/// <code>
/// public class DomainEventStore : DomainEventStore&lt;Guid, DomainEvent, StoredEvent&gt;
/// {
/// 	public DomainEventStore(string nameOrConnectionString)
/// 		: base(nameOrConnectionString)
/// 	{
/// 	}
/// }
/// 
/// public class StoredEvent : StoredEvent&lt;Guid&gt; { }
/// </code>
/// </example>
/// <nuget id="netfx-Patterns.EventSourcing.EF"/>
abstract partial class DomainEventStore<TAggregateId, TBaseEvent, TStoredEvent> : DbContext, IDomainEventStore<TAggregateId, TBaseEvent>
	where TAggregateId : IComparable
	where TStoredEvent : StoredEvent<TAggregateId>, new()
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DomainEventStore{TId, TStoredEvent}"/> class.
	/// </summary>
	/// <param name="nameOrConnectionString">The name or connection string.</param>
	public DomainEventStore(string nameOrConnectionString)
		: base(nameOrConnectionString)
	{
		this.TypeNameConverter = type => type.Name;
		this.SystemClock = global::SystemClock.Instance;
	}

	/// <summary>
	/// Gets or sets the function that converts a <see cref="Type"/> to 
	/// its string representation in the store. Used to calculate the 
	/// values of <see cref="IStoredEvent{TAggregateId}.AggregateType"/> and 
	/// <see cref="IStoredEvent{TId}.EventType"/>.
	/// </summary>
	public Func<Type, string> TypeNameConverter { get; set; }

	/// <summary>
	/// Gets or sets the system clock to use to calculate the timestamp
	/// of persisted events.
	/// </summary>
	public IClock SystemClock { get; set; }

	/// <summary>
	/// Gets or sets the events persisted in the store.
	/// </summary>
	public virtual DbSet<TStoredEvent> Events { get; set; }

	/// <summary>
	/// Queries the event store for events that match the given criteria.
	/// </summary>
	public IEnumerable<TBaseEvent> Query(StoredEventCriteria<TAggregateId> criteria)
	{
		var predicate = criteria.ToExpression(this.TypeNameConverter);
		var events = this.Events.AsQueryable();

		if (predicate != null)
			events = events.Where(predicate).Cast<TStoredEvent>();

		return events.AsEnumerable()
			.Select(x => (TBaseEvent)serializer.Deserialize(new JsonTextReader(new StringReader(x.Payload))));
	}

	/// <summary>
	/// Saves the given event raised by the given sender aggregate root.
	/// </summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="args">The instance containing the event data.</param>
	public void Persist(AggregateRoot<TAggregateId, TBaseEvent> sender, TBaseEvent args)
	{
		var stored = new TStoredEvent
		{
			AggregateId = sender.Id, 
			AggregateType = this.TypeNameConverter.Invoke(sender.GetType()), 
			EventType = this.TypeNameConverter.Invoke(args.GetType()), 
			Timestamp = this.SystemClock.UtcNow, 
		};

		// If this method call does not compile, you need to install 
		// a serializer for the store, such as:
		// netfx-Patterns.EventSourcing.EF.Json (installed by default)
		// netfx-Patterns.EventSourcing.EF.Bson
		Serialize(stored, args);

		this.Events.Add(stored);
	}

	/// <summary>
	/// Saves the changes to the underlying database.
	/// </summary>
	public void Commit()
	{
		base.SaveChanges();
	}
}