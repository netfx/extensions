#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Provides an in-memory event store, useful for testing domains that leverage event sourcing
/// </summary>
///	<nuget id="netfx-Patterns.EventSourcing.InMemory" />
partial class MemoryStore<TAggregateId, TBaseEvent> : IQueryableEventStore<TAggregateId, TBaseEvent, 
	MemoryStore<TAggregateId, TBaseEvent>.StoredAggregate, 
	MemoryStore<TAggregateId, TBaseEvent>.StoredEvent>
	where TAggregateId : IComparable
	where TBaseEvent : ITimestamped
{
	private List<StoredAggregate> aggregates = new List<StoredAggregate>();
	private List<StoredEvent> events = new List<StoredEvent>();

	/// <summary>
	/// Gets the aggregates that contain events persisted by the store.
	/// </summary>
	public IQueryable<StoredAggregate> Aggregates { get { return this.aggregates.AsQueryable(); } }

	/// <summary>
	/// Gets the stream of events persisted by the store.
	/// </summary>
	public IQueryable<StoredEvent> Events { get { return this.events.AsQueryable(); } }

	public void SaveChanges(AggregateRoot<TAggregateId, TBaseEvent> aggregate)
	{
		var stored = this.Aggregates.FirstOrDefault(this.AggregateIdEquals(aggregate.Id));
		if (stored == null)
		{
			stored = new StoredAggregate(aggregate);
			this.aggregates.Add(stored);
		}

		foreach (var change in aggregate.GetChanges())
		{
			this.events.Add(new StoredEvent(stored, change));
		}

		aggregate.AcceptChanges();
	}

	public IEnumerable<TBaseEvent> Query(EventQueryCriteria<TAggregateId> criteria)
	{
		return this.Events.Where(this.ToExpression(criteria, type => type.FullName)).Select(x => x.Event);
	}

	/// <summary>
	/// Internal storage representation of the aggregate header information.
	/// </summary>
	public class StoredAggregate : IStoredAggregate<TAggregateId>
	{
		public StoredAggregate(AggregateRoot<TAggregateId, TBaseEvent> aggregate)
		{
			this.Aggregate = aggregate;
			this.AggregateId = aggregate.Id;
			this.AggregateType = aggregate.GetType().FullName;
		}

		public AggregateRoot<TAggregateId, TBaseEvent> Aggregate { get; set; }

		public TAggregateId AggregateId { get; set; }
		public string AggregateType { get; set; }
	}

	/// <summary>
	/// Internal storage representation of the event payload.
	/// </summary>
	public class StoredEvent : IStoredEvent<StoredAggregate, TAggregateId>
	{
		public StoredEvent(StoredAggregate aggregate, TBaseEvent change)
		{
			this.Event = change;
			this.Aggregate = aggregate;

			this.EventId = Guid.NewGuid();
			this.EventType = change.GetType().FullName;
			this.Timestamp = change.Timestamp.UtcDateTime;
		}

		public TBaseEvent Event { get; set; }

		public Guid EventId { get; set; }
		public string EventType { get; set; }
		public StoredAggregate Aggregate { get; set; }
		public DateTime Timestamp { get; set; }
	}
}