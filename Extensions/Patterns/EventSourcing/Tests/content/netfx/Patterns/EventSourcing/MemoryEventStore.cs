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
using System.Reflection;

namespace NetFx.Patterns.EventSourcing.Tests
{
	/// <summary>
	/// Simple in-memory store for testing the API.
	/// </summary>
	partial class MemoryEventStore<TAggregateId, TBaseEvent> : IEventStore<TAggregateId, TBaseEvent>
		where TAggregateId : IComparable
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

		public void Persist(AggregateRoot<TAggregateId, TBaseEvent> sender, TBaseEvent args)
		{
			this.events.Add(new StoredEvent(sender, args) { Timestamp = this.utcNow() });
		}

		public void Persist(TBaseEvent @event)
		{
			this.events.Add(new StoredEvent(@event) { Timestamp = this.utcNow() });
		}

		public void Commit()
		{
		}

		public IEnumerable<TBaseEvent> Query(EventQueryCriteria<TAggregateId> criteria)
		{
			var source = this.events.AsQueryable();
			var predicate = ToExpression(criteria, this.TypeNameConverter);

			if (predicate != null)
				source = source.Where(predicate).Cast<StoredEvent>();

			return source.Select(x => x.EventArgs);
		}

		private class StoredEvent
		{
			public StoredEvent(TBaseEvent @event)
			{
				this.EventArgs = @event;
			}

			public StoredEvent(AggregateRoot<TAggregateId, TBaseEvent> sender, TBaseEvent @event)
				: this(@event)
			{
				this.Aggregate = new StoredAggregate(sender);
			}

			public StoredAggregate Aggregate { get; private set; }
			public TBaseEvent EventArgs { get; private set; }

			public Guid EventId { get; set; }
			public string EventType { get { return this.EventArgs.GetType().Name; } }
			public DateTime Timestamp { get; set; }

			public override string ToString()
			{
				return string.Format("{0}, {1} on {2} (payload: {3})",
					this.Aggregate == null ? "" : this.Aggregate.ToString(),
					this.EventType,
					this.Timestamp,
					this.EventArgs);
			}
		}

		private class StoredAggregate
		{
			public StoredAggregate(AggregateRoot<TAggregateId, TBaseEvent> sender)
			{
				this.AggregateRoot = sender;
			}

			public AggregateRoot<TAggregateId, TBaseEvent> AggregateRoot { get; private set; }

			public TAggregateId AggregateId { get { return this.AggregateRoot.Id; } }
			public string AggregateType { get { return this.AggregateRoot.GetType().Name; } }

			public override string ToString()
			{
				return string.Format("{0}({1})",
					this.AggregateType,
					this.AggregateId);
			}
		}

		static Expression<Func<StoredEvent, bool>> ToExpression(EventQueryCriteria<TAggregateId> criteria, Func<Type, string> typeNameConverter)
		{
			return new StoredEventCriteriaBuilder(criteria, typeNameConverter).Build();
		}

		private class StoredEventCriteriaBuilder
		{
			private static readonly Lazy<PropertyInfo> AggregateProperty = new Lazy<PropertyInfo>(() => typeof(StoredEvent).GetProperty("Aggregate"));
			private static readonly Lazy<PropertyInfo> AggregateIdProperty = new Lazy<PropertyInfo>(() => typeof(StoredAggregate).GetProperty("AggregateId"));

			private EventQueryCriteria<TAggregateId> aggregateCriteria;
			private Func<Type, string> typeNameConverter;

			public StoredEventCriteriaBuilder(EventQueryCriteria<TAggregateId> criteria, Func<Type, string> typeNameConverter)
			{
				this.aggregateCriteria = criteria;
				this.typeNameConverter = typeNameConverter;
			}

			private Expression<Func<StoredEvent, bool>> AddAggregateIdFilter(Expression<Func<StoredEvent, bool>> result)
			{
				if (this.aggregateCriteria == null)
					return result;

				var criteria = default(Expression<Func<StoredEvent, bool>>);

				foreach (var filter in this.aggregateCriteria.AggregateInstances)
				{
					var sourceType = typeNameConverter.Invoke(filter.AggregateType);
					// Builds: Aggregate != null && Aggregate.AggregateId == id && Aggregate.AggregateType == type

					var predicate = ((Expression<Func<StoredEvent, bool>>)
						(e => e.Aggregate != null && e.Aggregate.AggregateType == sourceType)).And
						(BuildEquals(filter.AggregateId));

					// ORs all aggregregate+id filters.
					criteria = Or(criteria, predicate);
				}

				if (criteria == null)
					return result;

				// AND the criteria built so far.
				return And(result, criteria);
			}

			private Expression<Func<StoredEvent, bool>> AddAggregateFilter(Expression<Func<StoredEvent, bool>> result)
			{
				if (this.aggregateCriteria == null)
					return result;

				var criteria = default(Expression<Func<StoredEvent, bool>>);

				foreach (var filter in this.aggregateCriteria.AggregateTypes)
				{
					var sourceType = typeNameConverter.Invoke(filter);

					// ORs all aggregregate filters.
					criteria = Or(criteria, e => e.Aggregate != null && e.Aggregate.AggregateType == sourceType);
				}

				if (criteria == null)
					return result;

				// AND the criteria built so far.
				return And(result, criteria);
			}

			private Expression<Func<StoredEvent, bool>> AddEventFilter(Expression<Func<StoredEvent, bool>> result)
			{
				var criteria = default(Expression<Func<StoredEvent, bool>>);

				foreach (var filter in this.aggregateCriteria.EventTypes)
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

			/// <devdoc>
			/// This is needed because == doesn't compile for TAggregateId and calling CompareTo 
			/// wouldn't work on most non in-memory stores either.
			/// </devdoc>
			private Expression<Func<StoredEvent, bool>> BuildEquals(TAggregateId id)
			{
				var @event = Expression.Parameter(typeof(StoredEvent), "event");
				var lambda = Expression.Lambda<Func<StoredEvent, bool>>(
					Expression.Equal(
						Expression.MakeMemberAccess(
							Expression.MakeMemberAccess(
								@event,
								AggregateProperty.Value),
							AggregateIdProperty.Value),
						Expression.Constant(id, typeof(TAggregateId))), @event);

				return lambda;
			}

			/// <summary>
			/// Builds the expression for the criteria.
			/// </summary>
			public Expression<Func<StoredEvent, bool>> Build()
			{
				var result = default(Expression<Func<StoredEvent, bool>>);

				result = AddAggregateIdFilter(result);
				result = AddAggregateFilter(result);
				result = AddEventFilter(result);

				if (this.aggregateCriteria.Since != null)
				{
					var since = this.aggregateCriteria.Since.Value.ToUniversalTime();
					if (this.aggregateCriteria.IsExclusiveRange)
						result = And(result, e => e.Timestamp > since);
					else
						result = And(result, e => e.Timestamp >= since);
				}

				if (this.aggregateCriteria.Until != null)
				{
					var until = this.aggregateCriteria.Until.Value.ToUniversalTime();
					if (this.aggregateCriteria.IsExclusiveRange)
						result = And(result, e => e.Timestamp < until);
					else
						result = And(result, e => e.Timestamp <= until);
				}

				return result;
			}
		}
	}
}