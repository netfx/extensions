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
/// Interface implemented by a message stores.
/// </summary>
/// <typeparam name="TBaseMessage">The common base type or interface implemented by message payloads.</typeparam>
/// <nuget id="netfx-Patterns.MessageStore"/>
partial interface IMessageStore<TBaseMessage>
{
	/// <summary>
	/// Notifies the store that the given message 
	/// should be persisted when <see cref="Commit"/> is called.
	/// </summary>
	/// <param name="message">The message to persist.</param>
	/// <param name="headers">The headers associated with the message.</param>
	void Persist(TBaseMessage message, IDictionary<string, object> headers);

	/// <summary>
	/// Queries the store for messages that match the given criteria.
	/// </summary>
	/// <remarks>
	/// Store implementations are advised to provide full support for the 
	/// specified criteria, but aren't required to.
	/// <para>
	/// The <see cref="MessageStoreQueryExtension.Query{TBaseMessage}"/> extension method  
	/// can be used with any message store implementation, and provides a fluent 
	/// API to build the criteria object
	/// </para>
	/// </remarks>
	IEnumerable<TBaseMessage> Query(MessageStoreQueryCriteria criteria);

	/// <summary>
	/// Persists all log entries <see cref="Persist"/>ed so far, effectively commiting 
	/// the changes to the underlying store in a unit-of-work style.
	/// </summary>
	void Commit();
}
