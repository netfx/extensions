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
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Provides helper conversion of <see cref="StoredEventCriteria{TAggregateId}"/> 
/// to a Linq <see cref="Expression"/> that can be applied directly by 
/// stores that have Linq providers to the underlying event stream in 
/// the form of <see cref="IQueryable{T}"/> of <see cref="IStoredEvent{TAggregateId}"/>.
/// </summary>
static partial class StoredEventCriteriaExtensions
{
	/// <summary>
	/// Converts the criteria object into an expression tree that can be used to
	/// run queries against an <see cref="IQueryable{T}"/>.
	/// </summary>
	/// <typeparam name="TAggregateId">The type of identifier used by the aggregate roots in the domain.</typeparam>
	/// <param name="criteria">The criteria object to convert to a Linq expression.</param>
	/// <param name="typeNameConverter">The function that converts a <see cref="Type"/> to
	/// its string representation in the store. Used to calculate the
	/// values of <see cref="IStoredEvent{TAggregateId}.AggregateType"/> and
	/// <see cref="IStoredEvent{TAggregateId}.EventType"/>.</param>
	/// <returns>
	/// The criteria object converted to an expression that can be
	/// used to query an <see cref="IQueryable{T}"/> if the store
	/// implementation provides one.
	/// </returns>
	public static Expression<Func<IStoredEvent<TAggregateId>, bool>> ToExpression<TAggregateId>(this StoredEventCriteria<TAggregateId> criteria, Func<Type, string> typeNameConverter)
		where TAggregateId : IComparable
	{
		return new StoredEventCriteriaBuilder<TAggregateId>(criteria, typeNameConverter).Build();
	}

	private class StoredEventCriteriaBuilder<TAggregateId>
		where TAggregateId : IComparable
	{
		private static readonly Lazy<PropertyInfo> AggregateIdProperty = new Lazy<PropertyInfo>(() => typeof(IStoredEvent<TAggregateId>).GetProperty("AggregateId"));

		private StoredEventCriteria<TAggregateId> criteria;
		private Func<Type, string> typeNameConverter;

		public StoredEventCriteriaBuilder(StoredEventCriteria<TAggregateId> criteria, Func<Type, string> typeNameConverter)
		{
			this.criteria = criteria;
			this.typeNameConverter = typeNameConverter;
		}

		private Expression<Func<IStoredEvent<TAggregateId>, bool>> AddAggregateIdFilter(Expression<Func<IStoredEvent<TAggregateId>, bool>> result)
		{
			var criteria = default(Expression<Func<IStoredEvent<TAggregateId>, bool>>);

			foreach (var filter in this.criteria.AggregateInstances)
			{
				var sourceType = typeNameConverter.Invoke(filter.AggregateType);
				// Builds: AggregateId == id && SourceTypes == type
				var predicate = BuildEquals(filter.AggregateId).And(e => e.AggregateType == sourceType);

				// ORs all aggregregate+id filters.
				criteria = Or(criteria, predicate);
			}

			if (criteria == null)
				return result;

			// AND the criteria built so far.
			return And(result, criteria);
		}

		private Expression<Func<IStoredEvent<TAggregateId>, bool>> AddAggregateFilter(Expression<Func<IStoredEvent<TAggregateId>, bool>> result)
		{
			var criteria = default(Expression<Func<IStoredEvent<TAggregateId>, bool>>);

			foreach (var filter in this.criteria.AggregateTypes)
			{
				var sourceType = typeNameConverter.Invoke(filter);

				// ORs all aggregregate filters.
				criteria = Or(criteria, e => e.AggregateType == sourceType);
			}

			if (criteria == null)
				return result;

			// AND the criteria built so far.
			return And(result, criteria);
		}

		private Expression<Func<IStoredEvent<TAggregateId>, bool>> AddEventFilter(Expression<Func<IStoredEvent<TAggregateId>, bool>> result)
		{
			var criteria = default(Expression<Func<IStoredEvent<TAggregateId>, bool>>);

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

		private Expression<Func<IStoredEvent<TAggregateId>, bool>> And(Expression<Func<IStoredEvent<TAggregateId>, bool>> left, Expression<Func<IStoredEvent<TAggregateId>, bool>> right)
		{
			return left == null ? right : left.And(right);
		}

		private Expression<Func<IStoredEvent<TAggregateId>, bool>> Or(Expression<Func<IStoredEvent<TAggregateId>, bool>> left, Expression<Func<IStoredEvent<TAggregateId>, bool>> right)
		{
			return left == null ? right : left.Or(right);
		}

		/// <devdoc>
		/// This is needed because == doesn't compile for TAggregateId and calling CompareTo 
		/// wouldn't work on most non in-memory stores either.
		/// </devdoc>
		private Expression<Func<IStoredEvent<TAggregateId>, bool>> BuildEquals(TAggregateId id)
		{
			var @event = Expression.Parameter(typeof(IStoredEvent<TAggregateId>), "event");
			var lambda = Expression.Lambda<Func<IStoredEvent<TAggregateId>, bool>>(
				Expression.Equal(
					Expression.MakeMemberAccess(@event, AggregateIdProperty.Value),
					Expression.Constant(id, typeof(TAggregateId))), @event);

			return lambda;
		}

		/// <summary>
		/// Builds the expression for the criteria.
		/// </summary>
		public Expression<Func<IStoredEvent<TAggregateId>, bool>> Build()
		{
			var result = default(Expression<Func<IStoredEvent<TAggregateId>, bool>>);

			result = AddAggregateIdFilter(result);
			result = AddAggregateFilter(result);
			result = AddEventFilter(result);

			if (this.criteria.Since != null)
			{
				var since = this.criteria.Since.Value.ToUniversalTime();
				if (this.criteria.IsExclusiveDateRange)
					result = And(result, e => e.Timestamp > since);
				else
					result = And(result, e => e.Timestamp >= since);
			}

			if (this.criteria.Until != null)
			{
				var until = this.criteria.Until.Value.ToUniversalTime();
				if (this.criteria.IsExclusiveDateRange)
					result = And(result, e => e.Timestamp < until);
				else
					result = And(result, e => e.Timestamp <= until);
			}

			return result;
		}
	}
}
