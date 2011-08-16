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
using System.Linq.Expressions;

namespace NetFx.Patterns.EventStore.Tests
{
	/// <summary>
	/// Simple in-memory store for testing the API.
	/// </summary>
	/// <nuget id="netfx-Patterns.EventStore.Tests"/>
	partial class MemoryEventStore<TBaseEvent> : IEventStore<TBaseEvent>
	{
		private List<StoredEvent> events = new List<StoredEvent>();
		private Func<DateTime> utcNow;

		public MemoryEventStore()
			: this(() => DateTime.UtcNow)
		{
		}

		public MemoryEventStore(Func<DateTime> utcNow)
		{
			this.TypeNameConverter = type => type.Name;
			this.utcNow = utcNow;
		}

		public Func<Type, string> TypeNameConverter { get; set; }
		public IQueryable<IStoredEvent> AllEvents { get { return this.events.OfType<IStoredEvent>().AsQueryable(); } }

		public void Persist(TBaseEvent @event)
		{
			this.events.Add(new StoredEvent(@event) { Timestamp = this.utcNow() });
		}

		public void Commit()
		{
		}

		public IEnumerable<TBaseEvent> Query(StoredEventCriteria criteria)
		{
			var source = this.events.AsQueryable();
			var predicate = criteria.ToExpression(this.TypeNameConverter);

			if (predicate != null)
				source = source.Where(predicate).Cast<StoredEvent>();

			return source.Select(x => x.Event);
		}

		private class StoredEvent : IStoredEvent
		{
			public StoredEvent(TBaseEvent @event)
			{
				this.Event = @event;
				this.EventId = Guid.NewGuid();
			}

			public TBaseEvent Event { get; private set; }
			public Guid EventId { get; private set; }
			public string EventType { get { return this.Event.GetType().Name; } }
			public DateTime Timestamp { get; set; }

			public override string ToString()
			{
				return string.Format("{0}({1}), at {2} (payload: {3})", 
					this.EventType, 
					this.EventId,
					this.Timestamp, 
					this.Event);
			}

		}
	}
}