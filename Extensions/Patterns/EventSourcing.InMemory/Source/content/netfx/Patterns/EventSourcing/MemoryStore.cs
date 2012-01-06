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
partial class MemoryStore<TObjectId, TBaseEvent> : IQueryableEventStore<TObjectId, TBaseEvent,
	MemoryStore<TObjectId, TBaseEvent>.StoredEvent>
	where TObjectId : IComparable
	where TBaseEvent : ITimestamped
{
	private List<StoredEvent> events = new List<StoredEvent>();
	private Func<TBaseEvent, DateTimeOffset> utcNow;

	public MemoryStore()
		: this(() => DateTimeOffset.Now)
	{
	}

	/// <summary>
	/// Initializes the store with a specific way to calculate the current time, useful 
	/// in tests when there's a need to simulate events happening at specific times.
	/// </summary>
	/// <param name="utcNow">The current date time, in UTC form, to be used if the changes don't provide their own timestamp.</param>
	public MemoryStore(Func<DateTimeOffset> utcNow)
	{
		// If the events have their own time, use that, otherwise, use the provided time.
		this.utcNow = change => change.Timestamp != DateTimeOffset.MinValue ? change.Timestamp : utcNow();
	}

	/// <summary>
	/// Gets the stream of events persisted by the store.
	/// </summary>
	public IQueryable<StoredEvent> Events { get { return this.events.AsQueryable(); } }

	public void SaveChanges(DomainObject<TObjectId, TBaseEvent> entity)
	{
		foreach (var change in entity.GetEvents())
		{
			this.events.Add(new StoredEvent(entity, change) { Timestamp = this.utcNow(change) });
		}

		entity.AcceptEvents();
	}

	public IEnumerable<TBaseEvent> Query(EventQueryCriteria<TObjectId> criteria)
	{
		return this.Events.Where(this.ToExpression(criteria, type => type.FullName)).Select(x => x.Event);
	}

	/// <summary>
	/// Internal storage representation of the event payload.
	/// </summary>
	public class StoredEvent : IStoredEvent<TObjectId>
	{
		public StoredEvent(DomainObject<TObjectId, TBaseEvent> entity, TBaseEvent change)
		{
			this.ObjectId = entity.Id;
			this.ObjectType = entity.GetType().FullName;

			this.Event = change;

			this.EventId = Guid.NewGuid();
			this.EventType = change.GetType().FullName;
			this.Timestamp = change.Timestamp;
		}

		public TObjectId ObjectId { get; set; }
		public string ObjectType { get; set; }

		public TBaseEvent Event { get; set; }

		public Guid EventId { get; set; }
		public string EventType { get; set; }
		public DateTimeOffset Timestamp { get; set; }
	}
}