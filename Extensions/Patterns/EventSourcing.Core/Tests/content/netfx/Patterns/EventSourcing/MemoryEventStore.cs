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

namespace NetFx.Patterns.EventSourcing.Core.Tests
{
	/// <summary>
	/// Simple in-memory store for testing the API.
	/// </summary>
	/// <nuget id="netfx-Patterns.EventSourcing.Core.Tests"/>
	public class MemoryEventStore<TId> : IDomainEventStore<TId>
		where TId : IComparable
	{
		private List<StoredEvent> events = new List<StoredEvent>();

		public MemoryEventStore()
		{
			this.TypeNameConverter = type => type.Name;
		}

		public Func<Type, string> TypeNameConverter { get; set; }

		public void Save(AggregateRoot<TId> sender, TimestampedEventArgs args)
		{
			this.events.Add(new StoredEvent(sender, args));
		}

		public void SaveChanges()
		{
		}

		public IEnumerable<TimestampedEventArgs> Query(StoredEventCriteria<TId> criteria)
		{
			var source = this.events.AsQueryable();
			var predicate = criteria.ToExpression(this.TypeNameConverter);

			if (predicate != null)
				source = source.Where(predicate).Cast<StoredEvent>();

			return source.Select(x => x.EventArgs);
		}

		private class StoredEvent : IStoredEvent<TId>
		{
			public StoredEvent(AggregateRoot<TId> sender, TimestampedEventArgs args)
			{
				this.AggregateRoot = sender;
				this.EventArgs = args;
			}

			public AggregateRoot<TId> AggregateRoot { get; private set; }
			public TimestampedEventArgs EventArgs { get; private set; }

			public TId AggregateId { get { return this.AggregateRoot.Id; } }
			public string AggregateType { get { return this.AggregateRoot.GetType().Name; } }
			public string EventType { get { return this.EventArgs.GetType().Name; } }
			public DateTime Timestamp { get { return this.EventArgs.Timestamp; } }

			public override string ToString()
			{
				return string.Format("{0}({1}), {2} on {3} (payload: {4})", 
					this.AggregateType, 
					this.AggregateId, 
					this.EventType, 
					this.Timestamp, 
					this.EventArgs);
			}
		}
	}
}