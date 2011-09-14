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

	/// <summary>
	/// Simple in-memory message store.
	/// </summary>
partial class MemoryMessageStore<TBaseMessage> : IMessageStore<TBaseMessage>
{
	private List<StoredMessage> messages = new List<StoredMessage>();
	private Func<DateTime> utcNow;

	public MemoryMessageStore()
		: this(() => DateTime.UtcNow)
	{
	}

	public MemoryMessageStore(Func<DateTime> utcNow)
	{
		this.TypeNameConverter = type => type.Name;
		this.utcNow = utcNow;
	}

	public Func<Type, string> TypeNameConverter { get; set; }
	public IQueryable<StoredMessage> AllMessages { get { return this.messages.AsQueryable(); } }

	public void Save(TBaseMessage message, IDictionary<string, object> headers)
	{
		this.messages.Add(new StoredMessage(message, headers) { Timestamp = this.utcNow() });
	}

	public IEnumerable<TBaseMessage> Query(MessageStoreQueryCriteria criteria)
	{
		var source = this.messages.AsQueryable();
		var predicate = ToExpression(criteria, this.TypeNameConverter);

		if (predicate != null)
			source = source.Where(predicate).Cast<StoredMessage>();

		return source.Select(x => x.Message);
	}

	public class StoredMessage
	{
		public StoredMessage(TBaseMessage message, IDictionary<string, object> headers)
		{
			this.Message = message;
			this.Headers = headers;
			this.MessageId = Guid.NewGuid();
		}

		public IDictionary<string, object> Headers { get; set; }
		public TBaseMessage Message { get; private set; }
		public Guid MessageId { get; private set; }
		public string MessageType { get { return this.Message.GetType().Name; } }
		public DateTime Timestamp { get; set; }

		public override string ToString()
		{
			return string.Format("{0}({1}), at {2} (payload: {3})",
				this.MessageType,
				this.MessageId,
				this.Timestamp,
				this.Message);
		}
	}

	private static Expression<Func<StoredMessage, bool>> ToExpression(MessageStoreQueryCriteria criteria, Func<Type, string> typeNameConverter)
	{
		return new StoredMessageCriteriaBuilder(criteria, typeNameConverter).Build();
	}

	private class StoredMessageCriteriaBuilder
	{
		private MessageStoreQueryCriteria criteria;
		private Func<Type, string> typeNameConverter;

		public StoredMessageCriteriaBuilder(MessageStoreQueryCriteria criteria, Func<Type, string> typeNameConverter)
		{
			this.criteria = criteria;
			this.typeNameConverter = typeNameConverter;
		}

		private Expression<Func<StoredMessage, bool>> AddMessageFilter(Expression<Func<StoredMessage, bool>> result)
		{
			var criteria = default(Expression<Func<StoredMessage, bool>>);

			foreach (var filter in this.criteria.MessageTypes)
			{
				var sourceType = typeNameConverter.Invoke(filter);

				// ORs all aggregregate filters.
				criteria = Or(criteria, e => e.MessageType == sourceType);
			}

			if (criteria == null)
				return result;

			// AND the criteria built so far.
			return And(result, criteria);
		}

		private Expression<Func<StoredMessage, bool>> And(Expression<Func<StoredMessage, bool>> left, Expression<Func<StoredMessage, bool>> right)
		{
			return left == null ? right : left.And(right);
		}

		private Expression<Func<StoredMessage, bool>> Or(Expression<Func<StoredMessage, bool>> left, Expression<Func<StoredMessage, bool>> right)
		{
			return left == null ? right : left.Or(right);
		}

		/// <summary>
		/// Builds the expression for the criteria.
		/// </summary>
		public Expression<Func<StoredMessage, bool>> Build()
		{
			var result = default(Expression<Func<StoredMessage, bool>>);
			result = AddMessageFilter(result);

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