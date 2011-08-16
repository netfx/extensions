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
/// Provides helper conversion of <see cref="StoredEventCriteria"/> 
/// to a Linq <see cref="Expression"/> that can be applied directly by 
/// stores that have Linq providers to the underlying event stream in 
/// the form of <see cref="IQueryable{T}"/> of <see cref="IStoredEvent"/>.
/// </summary>
static partial class StoredEventCriteriaExtensions
{
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
	public static Expression<Func<IStoredEvent, bool>> ToExpression(this StoredEventCriteria criteria, Func<Type, string> typeNameConverter)
	{
		return new StoredEventCriteriaBuilder(criteria, typeNameConverter).Build();
	}

	private class StoredEventCriteriaBuilder
	{
		private StoredEventCriteria criteria;
		private Func<Type, string> typeNameConverter;

		public StoredEventCriteriaBuilder(StoredEventCriteria criteria, Func<Type, string> typeNameConverter)
		{
			this.criteria = criteria;
			this.typeNameConverter = typeNameConverter;
		}

		private Expression<Func<IStoredEvent, bool>> AddEventFilter(Expression<Func<IStoredEvent, bool>> result)
		{
			var criteria = default(Expression<Func<IStoredEvent, bool>>);

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

		private Expression<Func<IStoredEvent, bool>> And(Expression<Func<IStoredEvent, bool>> left, Expression<Func<IStoredEvent, bool>> right)
		{
			return left == null ? right : left.And(right);
		}

		private Expression<Func<IStoredEvent, bool>> Or(Expression<Func<IStoredEvent, bool>> left, Expression<Func<IStoredEvent, bool>> right)
		{
			return left == null ? right : left.Or(right);
		}

		/// <summary>
		/// Builds the expression for the criteria.
		/// </summary>
		public Expression<Func<IStoredEvent, bool>> Build()
		{
			var result = default(Expression<Func<IStoredEvent, bool>>);
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
