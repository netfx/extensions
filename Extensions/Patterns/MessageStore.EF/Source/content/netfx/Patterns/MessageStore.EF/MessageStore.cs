#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Diagnostics;

/// <summary>
/// Provides an implementation of the message store pattern
/// using Entity Framework 4.1+ code first
/// </summary>
/// <typeparam name="TBaseMessage">The common base type or interface implemented by message payloads.</typeparam>
///	<nuget id="netfx-Patterns.MessageStore.EF" />
partial class MessageStore<TBaseMessage> : DbContext, IMessageStore<TBaseMessage>
{
	private ISerializer serializer;
	private string tableName;

	/// <summary>
	/// Initializes a new instance of the <see cref="MessageStore{TBaseMessage}"/> class
	/// with the given serializer.
	/// </summary>
	/// <param name="nameOrConnectionString">The name or connection string for the target entity framework database.</param>
	/// <param name="serializer">The serializer to use to persist the entities.</param>
	/// <param name="tableName">Optional name of the table.</param>
	public MessageStore(string nameOrConnectionString, ISerializer serializer, string tableName = null)
		: base(nameOrConnectionString)
	{
		Guard.NotNull(() => serializer, serializer);

		this.tableName = tableName;
		this.serializer = serializer;
		this.TypeNameConverter = type => type.Name;
		this.SystemClock = global::SystemClock.Instance;
	}

	/// <summary>
	/// Gets or sets the function that converts a <see cref="Type"/> to 
	/// its string representation in the store. Used to calculate the 
	/// value of <see cref="MessageEntity.Type"/> for a given message.
	/// </summary>
	public virtual Func<Type, string> TypeNameConverter { get; set; }

	/// <summary>
	/// Gets or sets the system clock to use to calculate the timestamp
	/// of persisted events.
	/// </summary>
	public virtual IClock SystemClock { get; set; }

	/// <summary>
	/// Gets or sets the messages persisted in the store.
	/// </summary>
	public virtual DbSet<MessageEntity> Messages { get; set; }

	/// <summary>
	/// Saves the specified message to the store.
	/// </summary>
	public virtual void Save(TBaseMessage message, IDictionary<string, object> headers)
	{
		Guard.NotNull(() => message, message);

		var entity = new MessageEntity
		{
			// Uses the sequential guids to improve indexing by ID and 
			// locality.
			Id = SequentialGuid.NewGuid(),
			ActivityId = Trace.CorrelationManager.ActivityId,
			Type = this.TypeNameConverter.Invoke(message.GetType()),
			Timestamp = this.SystemClock.UtcNow,
			Headers = this.serializer.Serialize(headers),
			Payload = this.serializer.Serialize(message),
		};

		OnPersisting(entity, message, headers);

		this.Messages.Add(entity);
		this.SaveChanges();
	}

	public virtual IEnumerable<TBaseMessage> Query(MessageStoreQueryCriteria criteria)
	{
		var predicate = ToExpression(criteria, this.TypeNameConverter);
		var messages = this.Messages.AsQueryable();

		if (predicate != null)
			messages = messages.Where(predicate);

		// NOTE: it's up to the serializer to actually hydrate the 
		// right derived/concrete type from the underlying byte 
		// array. So the <T> passed to Deserialize cannot be used 
		// to determine that.
		return messages
			.OrderBy(x => x.RowVersion)
			.AsEnumerable()
			.Select(x => this.serializer.Deserialize<TBaseMessage>(x.Payload));
	}

	/// <summary>
	/// Optionally modifies the destination table name for messages if provided in the constructor.
	/// </summary>
	protected override void OnModelCreating(DbModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		if (!string.IsNullOrEmpty(this.tableName))
		{
			modelBuilder.Entity<MessageEntity>().ToTable(this.tableName);
		}
	}

	/// <summary>
	/// Called before adding the message entity to the database, provides a chance 
	/// for extending the schema of the entity being saved, 
	/// with properties that can be pulled from the message headers for easier 
	/// querying.
	/// </summary>
	/// <param name="message">The message to persist.</param>
	/// <param name="entity">The entity created to persist the message.</param>
	/// <param name="headers">The headers associated with the message that is being persisted.</param>
	protected virtual void OnPersisting(MessageEntity entity, TBaseMessage message, IDictionary<string, object> headers) { }

	private static Expression<Func<MessageEntity, bool>> ToExpression(MessageStoreQueryCriteria criteria, Func<Type, string> typeNameConverter)
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

		private Expression<Func<MessageEntity, bool>> AddEventFilter(Expression<Func<MessageEntity, bool>> result)
		{
			var criteria = default(Expression<Func<MessageEntity, bool>>);

			foreach (var filter in this.criteria.MessageTypes)
			{
				var sourceType = typeNameConverter.Invoke(filter);

				// ORs all aggregregate filters.
				criteria = Or(criteria, e => e.Type == sourceType);
			}

			if (criteria == null)
				return result;

			// AND the criteria built so far.
			return And(result, criteria);
		}

		private Expression<Func<MessageEntity, bool>> And(Expression<Func<MessageEntity, bool>> left, Expression<Func<MessageEntity, bool>> right)
		{
			return left == null ? right : left.And(right);
		}

		private Expression<Func<MessageEntity, bool>> Or(Expression<Func<MessageEntity, bool>> left, Expression<Func<MessageEntity, bool>> right)
		{
			return left == null ? right : left.Or(right);
		}

		/// <summary>
		/// Builds the expression for the criteria.
		/// </summary>
		public Expression<Func<MessageEntity, bool>> Build()
		{
			var result = default(Expression<Func<MessageEntity, bool>>);
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