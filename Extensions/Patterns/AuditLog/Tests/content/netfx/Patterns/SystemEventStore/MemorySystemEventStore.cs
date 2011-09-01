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

namespace NetFx.Patterns.SystemEventStore.Tests
{
	/// <summary>
	/// Simple in-memory store for testing the API.
	/// </summary>
	/// <nuget id="netfx-Patterns.SystemEventStore.Tests.xUnit" />
	partial class MemoryEventStore<TBaseEvent> : ISystemEventStore<TBaseEvent>
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
		public IQueryable<StoredEvent> AllEvents { get { return this.events.AsQueryable(); } }

		public void Persist(TBaseEvent @event)
		{
			this.events.Add(new StoredEvent(@event) { Timestamp = this.utcNow() });
		}

		public void Commit()
		{
		}

		public IEnumerable<TBaseEvent> Query(SystemEventQueryCriteria criteria)
		{
			var source = this.events.AsQueryable();
			var predicate = ToExpression(criteria, this.TypeNameConverter);

			if (predicate != null)
				source = source.Where(predicate).Cast<StoredEvent>();

			return source.Select(x => x.Event);
		}

		public class StoredEvent
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

		/// <summary>
		/// Converts the criteria object into an expression tree that can be used to
		/// run queries against an <see cref="IQueryable{T}"/>.
		/// </summary>
		/// <param name="criteria">The criteria object to convert to a Linq expression.</param>
		/// <param name="typeNameConverter">The function that converts a <see cref="Type"/> to
		/// its string representation in the store. Used to calculate the
		/// <see cref="IStoredEvent.EventType"/>.</param>
		/// <returns>
		/// The criteria object converted to an expression that can be
		/// used to query an <see cref="IQueryable{T}"/> if the store
		/// implementation provides one.
		/// </returns>
		static Expression<Func<StoredEvent, bool>> ToExpression(SystemEventQueryCriteria criteria, Func<Type, string> typeNameConverter)
		{
			return new StoredEventCriteriaBuilder(criteria, typeNameConverter).Build();
		}

		private class StoredEventCriteriaBuilder
		{
			private SystemEventQueryCriteria criteria;
			private Func<Type, string> typeNameConverter;

			public StoredEventCriteriaBuilder(SystemEventQueryCriteria criteria, Func<Type, string> typeNameConverter)
			{
				this.criteria = criteria;
				this.typeNameConverter = typeNameConverter;
			}

			private Expression<Func<StoredEvent, bool>> AddEventFilter(Expression<Func<StoredEvent, bool>> result)
			{
				var criteria = default(Expression<Func<StoredEvent, bool>>);

				foreach (var filter in this.criteria.EventTypes)
				{
					var sourceType = typeNameConverter.Invoke(filter);

					// ORs all aggregregate filters.
					criteria = Or(criteria, e => e.EventType == sourceType);
				}

				if (criteria == null)
					return result;

				// AND the criteria built so far.
				return And(result, criteria);
			}

			private Expression<Func<StoredEvent, bool>> And(Expression<Func<StoredEvent, bool>> left, Expression<Func<StoredEvent, bool>> right)
			{
				return left == null ? right : left.And(right);
			}

			private Expression<Func<StoredEvent, bool>> Or(Expression<Func<StoredEvent, bool>> left, Expression<Func<StoredEvent, bool>> right)
			{
				return left == null ? right : left.Or(right);
			}

			/// <summary>
			/// Builds the expression for the criteria.
			/// </summary>
			public Expression<Func<StoredEvent, bool>> Build()
			{
				var result = default(Expression<Func<StoredEvent, bool>>);
				result = AddEventFilter(result);

				if (this.criteria.Since != null)
				{
					var since = this.criteria.Since.Value.ToUniversalTime();
					if (this.criteria.IsExclusiveRange)
						result = And(result, e => e.Timestamp > since);
					else
						result = And(result, e => e.Timestamp >= since);
				}

				if (this.criteria.Until != null)
				{
					var until = this.criteria.Until.Value.ToUniversalTime();
					if (this.criteria.IsExclusiveRange)
						result = And(result, e => e.Timestamp < until);
					else
						result = And(result, e => e.Timestamp <= until);
				}

				return result;
			}
		}
	}
}