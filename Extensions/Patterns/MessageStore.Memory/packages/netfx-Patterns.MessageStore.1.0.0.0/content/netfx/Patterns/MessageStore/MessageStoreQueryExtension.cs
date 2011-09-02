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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Provides the entry point <see cref="Query"/> extension method 
/// for an <see cref="IMessageStore{TBaseMessage}"/> for a fluent API 
/// that makes building the message query criteria easier.
/// </summary>
/// <nuget id="netfx-Patterns.MessageStore"/>
static partial class MessageStoreQueryExtension
{
	/// <summary>
	/// Allows building a query against the message store 
	/// using a fluent API and automatically executing 
	/// it to messages that match built criteria upon 
	/// query enumeration or explicit execution.
	/// </summary>
	/// <typeparam name="TBaseMessage">The base type or interface implemented by log entries in the system.</typeparam>
	/// <param name="store">The audit log store.</param>
	public static IQuery<TBaseMessage> Query<TBaseMessage>(this IMessageStore<TBaseMessage> store)
	{
		Guard.NotNull(() => store, store);

		return new MessageQuery<TBaseMessage>(store);
	}

	/// <summary>
	/// Provides a fluent API to filter messages from a message store. 
	/// </summary>
	/// <remarks>
	/// This interface is returned from the <see cref="MessageStoreQueryExtension.Query"/> 
	/// extension method for <see cref="IMessageStore{TMessage}"/>.
	/// </remarks>
	/// <typeparam name="TBaseMessage">The base type or interface implemented by events in the system.</typeparam>
	/// <nuget id="netfx-Patterns.MessageStore"/>
	public interface IQuery<TBaseMessage> : IEnumerable<TBaseMessage>
	{
		/// <summary>
		/// Executes the <see cref="Criteria"/> built using the fluent API 
		/// against the underlying store.
		/// </summary>
		IEnumerable<TBaseMessage> Execute();

		/// <summary>
		/// Gets the criteria that was built using the fluent API so far.
		/// </summary>
		MessageStoreQueryCriteria Criteria { get; }

		/// <summary>
		/// Includes messages in the result that are assignable to the given type. Can be called 
		/// multiple times and will filter for any of the specified types (OR operator).
		/// </summary>
		/// <typeparam name="TMessage">The type of messages to include.</typeparam>
		IQuery<TBaseMessage> OfType<TMessage>() where TMessage : TBaseMessage;

		/// <summary>
		/// Includes messages that happened after the given starting date.
		/// </summary>
		/// <param name="when">The starting date to filter by.</param>
		/// <remarks>
		/// By default, includes messages with the given date, unless the 
		/// <see cref="ExclusiveRange"/> is called to make the range exclusive.
		/// </remarks>
		IQuery<TBaseMessage> Since(DateTime when);

		/// <summary>
		/// Includes events that happened before the given ending date.
		/// </summary>
		/// <param name="when">The ending date to filter by.</param>
		/// <remarks>
		/// By default, includes messages with the given date, unless the 
		/// <see cref="ExclusiveRange"/> is called to make the range exclusive.
		/// </remarks>
		IQuery<TBaseMessage> Until(DateTime when);

		/// <summary>
		/// Makes the configured <see cref="Since"/> and/or <see cref="Until"/> dates 
		/// exclusive, changing the default behavior which is to be inclusive.
		/// </summary>
		IQuery<TBaseMessage> ExclusiveRange();
	}

	private class MessageQuery<TBaseMessage> : IQuery<TBaseMessage>
	{
		private IMessageStore<TBaseMessage> store;
		private MessageStoreQueryCriteria criteria = new MessageStoreQueryCriteria();

		public MessageQuery(IMessageStore<TBaseMessage> store)
		{
			this.store = store;
		}

		public MessageStoreQueryCriteria Criteria { get { return this.criteria; } }

		public IEnumerable<TBaseMessage> Execute()
		{
			return this.store.Query(this.criteria);
		}		

		public IEnumerator<TBaseMessage> GetEnumerator()
		{
			return Execute().GetEnumerator();
		}

		public IQuery<TBaseMessage> OfType<TEvent>()
			where TEvent : TBaseMessage
		{
			this.criteria.MessageTypes.Add(typeof(TEvent));

			return this;
		}

		public IQuery<TBaseMessage> Since(DateTime when)
		{
			this.criteria.Since = when;
			return this;
		}

		public IQuery<TBaseMessage> Until(DateTime when)
		{
			this.criteria.Until = when;
			return this;
		}

		public IQuery<TBaseMessage> ExclusiveRange()
		{
			this.criteria.IsExclusiveRange = true;
			return this;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}