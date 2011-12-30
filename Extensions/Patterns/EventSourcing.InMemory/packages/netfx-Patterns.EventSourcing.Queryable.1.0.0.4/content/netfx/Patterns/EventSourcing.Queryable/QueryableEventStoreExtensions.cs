using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

static partial class QueryableEventStoreExtensions
{
	private static readonly Dictionary<Type, PropertyInfo> objectIdProperties = new Dictionary<Type, PropertyInfo>();

	/// <summary>
	/// Builds the equals expression that can be used to find a matching object
	/// in a queryable of <see cref="IStoredObject{TObjectId}"/> entities using the <c>FirstOrDefault</c> 
	/// Linq operator, for example.
	/// </summary>
	/// <remarks>
	/// This is needed because == doesn't compile for TObjectId and calling CompareTo 
	/// wouldn't work on most non in-memory stores either.
	/// </remarks>
	public static Expression<Func<TStoredObject, bool>> ObjectIdEquals<TObjectId, TBaseEvent, TStoredObject, TStoredEvent>(
		this IQueryableEventStore<TObjectId, TBaseEvent, TStoredObject, TStoredEvent> store, TObjectId id)
		where TObjectId : IComparable
		where TBaseEvent : ITimestamped
		where TStoredObject : class, IStoredObject<TObjectId>
		where TStoredEvent : class, IStoredEvent<TStoredObject, TObjectId>
	{
		var idProperty = objectIdProperties.GetOrAdd(typeof(TStoredObject), type => type.GetProperty("ObjectId"));

		var entity = Expression.Parameter(typeof(TStoredObject), "entity");
		var lambda = Expression.Lambda<Func<TStoredObject, bool>>(
			Expression.Equal(
				Expression.MakeMemberAccess(entity, idProperty),
				Expression.Constant(id, typeof(TObjectId))), entity);

		return lambda;
	}

	/// <summary>
	/// Converts a criteria object passed to the queryable event store 
	/// <see cref="IEventStore{TObjectId, TBaseEvent}.Query"/> method 
	/// into a Linq expression that can be used directly as a filter (<c>Where</c>) 
	/// over the stored queryable event stream.
	/// </summary>
	/// <typeparam name="TObjectId">The type of identifier used by domain objects in the domain.</typeparam>
	/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
	/// <typeparam name="TStoredEvent">The type of the stored event.</typeparam>
	/// <typeparam name="TStoredObject">The type of the stored object.</typeparam>
	/// <param name="store">The store to provide the query expression for.</param>
	/// <param name="criteria">The criteria that will be converted to a Linq expression.</param>
	/// <param name="typeNameConverter">The type name converter to use to transform the <see cref="Type"/> 
	/// filters in the criteria object into type name strings that are persisted by the store.</param>
	public static Expression<Func<TStoredEvent, bool>> ToExpression<TObjectId, TBaseEvent, TStoredObject, TStoredEvent>(
		this IQueryableEventStore<TObjectId, TBaseEvent, TStoredObject, TStoredEvent> store, 
		EventQueryCriteria<TObjectId> criteria, 
		Func<Type, string> typeNameConverter)
		where TObjectId : IComparable
		where TBaseEvent : ITimestamped
		where TStoredObject : class, IStoredObject<TObjectId>
		where TStoredEvent : class, IStoredEvent<TStoredObject, TObjectId>
	{
		return new CriteriaBuilder<TObjectId, TStoredObject, TStoredEvent>(criteria, typeNameConverter).Build();
	}

	private class CriteriaBuilder<TObjectId, TStoredObject, TStoredEvent>
		where TObjectId : IComparable
		where TStoredObject : class, IStoredObject<TObjectId>
		where TStoredEvent : class, IStoredEvent<TStoredObject, TObjectId>
	{
		private static readonly Lazy<PropertyInfo> TargetObjectProperty = new Lazy<PropertyInfo>(() => typeof(TStoredEvent).GetProperty("TargetObject"));
		private static readonly Lazy<PropertyInfo> ObjectIdProperty = new Lazy<PropertyInfo>(() => typeof(TStoredObject).GetProperty("ObjectId"));

		private EventQueryCriteria<TObjectId> criteria;
		private Func<Type, string> typeNameConverter;

		public CriteriaBuilder(EventQueryCriteria<TObjectId> criteria, Func<Type, string> typeNameConverter)
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
				result = AddObjectIdFilter(result);
				result = AddObjectFilter(result);
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

		private Expression<Func<TStoredEvent, bool>> AddObjectIdFilter(Expression<Func<TStoredEvent, bool>> result)
		{
			var criteria = default(Expression<Func<TStoredEvent, bool>>);

			foreach (var filter in this.criteria.ObjectInstances)
			{
				var sourceType = typeNameConverter.Invoke(filter.ObjectType);
				// Builds: TargetObject != null && TargetObject.ObjectId == id && TargetObject.ObjectType == type

				var predicate = ((Expression<Func<TStoredEvent, bool>>)
					(e => e.TargetObject.ObjectId != null && e.TargetObject.ObjectType == sourceType)).And
					(EventObjectIdEquals(filter.ObjectId));

				// ORs all object+id filters.
				criteria = Or(criteria, predicate);
			}

			if (criteria == null)
				return result;

			// AND the criteria built so far.
			return And(result, criteria);
		}

		private Expression<Func<TStoredEvent, bool>> AddObjectFilter(Expression<Func<TStoredEvent, bool>> result)
		{
			var criteria = default(Expression<Func<TStoredEvent, bool>>);

			foreach (var filter in this.criteria.ObjectTypes)
			{
				var sourceType = typeNameConverter.Invoke(filter);

				// ORs all aggregregate filters.
				criteria = Or(criteria, e => e.TargetObject != null && e.TargetObject.ObjectType == sourceType);
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
		/// This is needed because == doesn't compile for TObjectId and calling CompareTo 
		/// wouldn't work on most non in-memory stores either.
		/// </devdoc>
		private Expression<Func<TStoredEvent, bool>> EventObjectIdEquals(TObjectId id)
		{
			var @event = Expression.Parameter(typeof(TStoredEvent), "event");
			var lambda = Expression.Lambda<Func<TStoredEvent, bool>>(
				Expression.Equal(
					Expression.MakeMemberAccess(
						Expression.MakeMemberAccess(
							@event,
							TargetObjectProperty.Value),
						ObjectIdProperty.Value),
					Expression.Constant(id, typeof(TObjectId))), @event);

			return lambda;
		}
	}
}