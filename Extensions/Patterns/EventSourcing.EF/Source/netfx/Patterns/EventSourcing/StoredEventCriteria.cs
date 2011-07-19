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
using System.Text;
using System.Reflection;

/// <summary>
/// Represents the filter criteria for a domain event store query.
/// </summary>
/// <typeparam name="TId">The type of identifiers used by the aggregate roots.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing.Core"/>
partial class StoredEventCriteria<TId>
	where TId : IComparable
{
	private static readonly Lazy<PropertyInfo> AggregateIdProperty = new Lazy<PropertyInfo>(() => typeof(IStoredEvent<TId>).GetProperty("AggregateId"));

	/// <summary>
	/// Initializes a new instance of the <see cref="StoredEventCriteria&lt;TId&gt;"/> class.
	/// </summary>
	public StoredEventCriteria()
	{
		this.AggregateTypeAndIds = new List<Tuple<Type, TId>>();
		this.AggregateTypes = new List<Type>();
		this.EventTypes = new List<Type>();
	}

	/// <summary>
	/// List of aggregate type + identifier filters. The two filters 
	/// should be considered by event stores as AND'ed (i.e. 
	/// events for <c>Product AND Id = 5</c>) and each entry is OR'ed with the 
	/// others (i.e. <c>(Product AND Id = 5) OR (Order AND Id = 1)</c>.
	/// </summary>
	public List<Tuple<Type, TId>> AggregateTypeAndIds { get; private set; }

	/// <summary>
	/// List of aggregate type filters. All types added are OR'ed with the 
	/// others (i.e. <c>AggregateType == Product OR AggregateType == Order</c>).
	/// </summary>
	public List<Type> AggregateTypes { get; private set; }

	/// <summary>
	/// List of event type filters. All types added are OR'ed with the 
	/// others (i.e. <c>EventType == ProductCreated OR EventType == ProductPublished</c>).
	/// </summary>
	public List<Type> EventTypes { get; private set; }

	/// <summary>
	/// Filters events that happened after the given starting date.
	/// </summary>
	public DateTime? Since { get; set; }

	/// <summary>
	/// Filters events that happened before the given ending date.
	/// </summary>
	public DateTime? Until { get; set; }

	/// <summary>
	/// If set to <see langword="true"/>, <see cref="Since"/> and <see cref="Until"/> should 
	/// be considered as exclusive date ranges (excludes values with a matching date). 
	/// Defaults to <see langword="false"/>, meaning that ranges are inclusive by default.
	/// </summary>
	public bool IsExclusiveDateRange { get; set; }

	/// <summary>
	/// Converts the criteria object into an expression tree that can be used to 
	/// run queries against an <see cref="IQueryable{T}"/>.
	/// </summary>
	/// <param name="typeNameConverter">The function that converts a <see cref="Type"/> to 
	/// its string representation in the store. Used to calculate the 
	/// values of <see cref="IStoredEvent{TId}.AggregateType"/> and 
	/// <see cref="IStoredEvent{TId}.EventType"/>.</param>
	/// <returns>The criteria object converted to an expression that can be 
	/// used to query an <see cref="IQueryable{T}"/> if the store 
	/// implementation provides one.</returns>
	public Expression<Func<IStoredEvent<TId>, bool>> ToExpression(Func<Type, string> typeNameConverter)
	{
		var result = default(Expression<Func<IStoredEvent<TId>, bool>>);

		result = AddAggregateIdFilter(typeNameConverter, result);
		result = AddAggregateFilter(typeNameConverter, result);
		result = AddEventFilter(typeNameConverter, result);

		if (this.Since != null)
		{
			var since = this.Since.Value.ToUniversalTime();
			if (this.IsExclusiveDateRange)
				result = And(result, e => e.Timestamp > since);
			else
				result = And(result, e => e.Timestamp >= since);
		}

		if (this.Until != null)
		{
			var until = this.Until.Value.ToUniversalTime();
			if (this.IsExclusiveDateRange)
				result = And(result, e => e.Timestamp < until);
			else
				result = And(result, e => e.Timestamp <= until);
		}

		return result;
	}

	private Expression<Func<IStoredEvent<TId>, bool>> AddAggregateIdFilter(Func<Type, string> typeNameConverter, Expression<Func<IStoredEvent<TId>, bool>> result)
	{
		var criteria = default(Expression<Func<IStoredEvent<TId>, bool>>);

		foreach (var filter in this.AggregateTypeAndIds)
		{
			var aggregateType = typeNameConverter.Invoke(filter.Item1);
			// Builds: AggregateId == id && AggregateType == type
			var predicate = BuildEquals(filter.Item2).And(e => e.AggregateType == aggregateType);

			// ORs all aggregregate+id filters.
			criteria = Or(criteria, predicate);
		}

		if (criteria == null)
			return result;

		// AND the criteria built so far.
		return And(result, criteria);
	}

	private Expression<Func<IStoredEvent<TId>, bool>> AddAggregateFilter(Func<Type, string> typeNameConverter, Expression<Func<IStoredEvent<TId>, bool>> result)
	{
		var criteria = default(Expression<Func<IStoredEvent<TId>, bool>>);

		foreach (var filter in this.AggregateTypes)
		{
			var aggregateType = typeNameConverter.Invoke(filter);

			// ORs all aggregregate filters.
			criteria = Or(criteria, e => e.AggregateType == aggregateType);
		}

		if (criteria == null)
			return result;

		// AND the criteria built so far.
		return And(result, criteria);
	}

	private Expression<Func<IStoredEvent<TId>, bool>> AddEventFilter(Func<Type, string> typeNameConverter, Expression<Func<IStoredEvent<TId>, bool>> result)
	{
		var criteria = default(Expression<Func<IStoredEvent<TId>, bool>>);

		foreach (var filter in this.EventTypes)
		{
			var aggregateType = typeNameConverter.Invoke(filter);

			// ORs all aggregregate filters.
			criteria = Or(criteria, e => e.EventType == aggregateType);
		}

		if (criteria == null)
			return result;

		// AND the criteria built so far.
		return And(result, criteria);
	}

	private Expression<Func<IStoredEvent<TId>, bool>> And(Expression<Func<IStoredEvent<TId>, bool>> left, Expression<Func<IStoredEvent<TId>, bool>> right)
	{
		return left == null ? right : left.And(right);
	}

	private Expression<Func<IStoredEvent<TId>, bool>> Or(Expression<Func<IStoredEvent<TId>, bool>> left, Expression<Func<IStoredEvent<TId>, bool>> right)
	{
		return left == null ? right : left.Or(right);
	}

	/// <devdoc>
	/// This is needed because == doesn't compile for TId and calling CompareTo 
	/// wouldn't work on most non in-memory stores either.
	/// </devdoc>
	private Expression<Func<IStoredEvent<TId>, bool>> BuildEquals(TId id)
	{
		var @event = Expression.Parameter(typeof(IStoredEvent<TId>), "event");
		var lambda = Expression.Lambda<Func<IStoredEvent<TId>, bool>>(
			Expression.Equal(
				Expression.MakeMemberAccess(@event, AggregateIdProperty.Value),
				Expression.Constant(id, typeof(TId))), @event);

		return lambda;
	}
}
