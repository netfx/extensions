using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

static partial class QueryableEventStoreExtensions
{
	private static readonly Dictionary<Type, PropertyInfo> aggregateIdProperties = new Dictionary<Type, PropertyInfo>();

	/// <summary>
	/// Builds the equals expression that can be used to find a matching aggregate 
	/// in a queryable of <see cref="IStoredAggregate{TAggregateId}"/> entities using the <c>FirstOrDefault</c> 
	/// Linq operator, for example.
	/// </summary>
	/// <remarks>
	/// This is needed because == doesn't compile for TAggregateId and calling CompareTo 
	/// wouldn't work on most non in-memory stores either.
	/// </remarks>
	public static Expression<Func<TStoredAggregate, bool>> AggregateIdEquals<TAggregateId, TBaseEvent, TStoredAggregate, TStoredEvent>(
		this IQueryableEventStore<TAggregateId, TBaseEvent, TStoredAggregate, TStoredEvent> store, TAggregateId id)
		where TAggregateId : IComparable
		where TBaseEvent : ITimestamped
		where TStoredAggregate : class, IStoredAggregate<TAggregateId>, new()
		where TStoredEvent : class, IStoredEvent<TStoredAggregate, TAggregateId>, new()
	{
		var idProperty = aggregateIdProperties.GetOrAdd(typeof(TStoredAggregate), type => type.GetProperty("AggregateId"));

		var aggregate = Expression.Parameter(typeof(TStoredAggregate), "aggregate");
		var lambda = Expression.Lambda<Func<TStoredAggregate, bool>>(
			Expression.Equal(
				Expression.MakeMemberAccess(aggregate, idProperty),
				Expression.Constant(id, typeof(TAggregateId))), aggregate);

		return lambda;
	}

	/// <summary>
	/// Converts a criteria object passed to the queryable event store 
	/// <see cref="IEventStore{TAggregateId, TBaseEvent}.Query"/> method 
	/// into a Linq expression that can be used directly as a filter (<c>Where</c>) 
	/// over the stored queryable event stream.
	/// </summary>
	/// <typeparam name="TAggregateId">The type of identifier used by aggregate roots in the domain.</typeparam>
	/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
	/// <typeparam name="TStoredEvent">The type of the stored event.</typeparam>
	/// <typeparam name="TStoredAggregate">The type of the stored aggregate.</typeparam>
	/// <param name="store">The store to provide the query expression for.</param>
	/// <param name="criteria">The criteria that will be converted to a Linq expression.</param>
	/// <param name="typeNameConverter">The type name converter to use to transform the <see cref="Type"/> 
	/// filters in the criteria object into type name strings that are persisted by the store.</param>
	public static Expression<Func<TStoredEvent, bool>> ToExpression<TAggregateId, TBaseEvent, TStoredAggregate, TStoredEvent>(
		this IQueryableEventStore<TAggregateId, TBaseEvent, TStoredAggregate, TStoredEvent> store, 
		EventQueryCriteria<TAggregateId> criteria, 
		Func<Type, string> typeNameConverter)
		where TAggregateId : IComparable
		where TBaseEvent : ITimestamped
		where TStoredAggregate : class, IStoredAggregate<TAggregateId>, new()
		where TStoredEvent : class, IStoredEvent<TStoredAggregate, TAggregateId>, new()
	{
		return new CriteriaBuilder<TAggregateId, TStoredAggregate, TStoredEvent>(criteria, typeNameConverter).Build();
	}

	private class CriteriaBuilder<TAggregateId, TStoredAggregate, TStoredEvent>
		where TAggregateId : IComparable
		where TStoredAggregate : class, IStoredAggregate<TAggregateId>, new()
		where TStoredEvent : class, IStoredEvent<TStoredAggregate, TAggregateId>, new()
	{
		private static readonly Lazy<PropertyInfo> AggregateProperty = new Lazy<PropertyInfo>(() => typeof(TStoredEvent).GetProperty("Aggregate"));
		private static readonly Lazy<PropertyInfo> AggregateIdProperty = new Lazy<PropertyInfo>(() => typeof(TStoredAggregate).GetProperty("AggregateId"));

		private EventQueryCriteria<TAggregateId> criteria;
		private Func<Type, string> typeNameConverter;

		public CriteriaBuilder(EventQueryCriteria<TAggregateId> criteria, Func<Type, string> typeNameConverter)
		{
			this.criteria = criteria;
			this.typeNameConverter = typeNameConverter;
		}

		/// <summary>
		/// Builds the expression for the criteria.
		/// </summary>
		public Expression<Func<TStoredEvent, bool>> Build()
		{
			var result = default(Expression<Func<TStoredEvent, bool>>);

			if (this.criteria != null)
			{
				result = AddAggregateIdFilter(result);
				result = AddAggregateFilter(result);
			}

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

		private Expression<Func<TStoredEvent, bool>> AddAggregateIdFilter(Expression<Func<TStoredEvent, bool>> result)
		{
			var criteria = default(Expression<Func<TStoredEvent, bool>>);

			foreach (var filter in this.criteria.AggregateInstances)
			{
				var sourceType = typeNameConverter.Invoke(filter.AggregateType);
				// Builds: Aggregate != null && Aggregate.AggregateId == id && Aggregate.AggregateType == type

				var predicate = ((Expression<Func<TStoredEvent, bool>>)
					(e => e.Aggregate.AggregateId != null && e.Aggregate.AggregateType == sourceType)).And
					(EventAggregateIdEquals(filter.AggregateId));

				// ORs all aggregregate+id filters.
				criteria = Or(criteria, predicate);
			}

			if (criteria == null)
				return result;

			// AND the criteria built so far.
			return And(result, criteria);
		}

		private Expression<Func<TStoredEvent, bool>> AddAggregateFilter(Expression<Func<TStoredEvent, bool>> result)
		{
			var criteria = default(Expression<Func<TStoredEvent, bool>>);

			foreach (var filter in this.criteria.AggregateTypes)
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

		private Expression<Func<TStoredEvent, bool>> AddEventFilter(Expression<Func<TStoredEvent, bool>> result)
		{
			var criteria = default(Expression<Func<TStoredEvent, bool>>);

			foreach (var filter in this.criteria.EventTypes)
			{
				var sourceType = typeNameConverter.Invoke(filter);

				// ORs all event filters.
				criteria = Or(criteria, e => e.EventType == sourceType);
			}

			if (criteria == null)
				return result;

			// AND the criteria built so far.
			return And(result, criteria);
		}

		private Expression<Func<TStoredEvent, bool>> And(Expression<Func<TStoredEvent, bool>> left, Expression<Func<TStoredEvent, bool>> right)
		{
			return left == null ? right : left.And(right);
		}

		private Expression<Func<TStoredEvent, bool>> Or(Expression<Func<TStoredEvent, bool>> left, Expression<Func<TStoredEvent, bool>> right)
		{
			return left == null ? right : left.Or(right);
		}

		/// <devdoc>
		/// This is needed because == doesn't compile for TAggregateId and calling CompareTo 
		/// wouldn't work on most non in-memory stores either.
		/// </devdoc>
		private Expression<Func<TStoredEvent, bool>> EventAggregateIdEquals(TAggregateId id)
		{
			var @event = Expression.Parameter(typeof(TStoredEvent), "event");
			var lambda = Expression.Lambda<Func<TStoredEvent, bool>>(
				Expression.Equal(
					Expression.MakeMemberAccess(
						Expression.MakeMemberAccess(
							@event,
							AggregateProperty.Value),
						AggregateIdProperty.Value),
					Expression.Constant(id, typeof(TAggregateId))), @event);

			return lambda;
		}
	}
}