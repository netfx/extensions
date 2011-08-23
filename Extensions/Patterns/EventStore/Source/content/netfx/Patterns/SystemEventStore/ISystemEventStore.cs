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
/// Interface implemented by domain event stores.
/// </summary>
/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the system.</typeparam>
/// <nuget id="netfx-Patterns.EventStore"/>
partial interface IEventStore<TBaseEvent>
{
	/// <summary>
	/// Notifies the store that the given event
	/// should be persisted when <see cref="Commit"/> is called.
	/// </summary>
	/// <param name="event">The instance containing the event data.</param>
	void Persist(TBaseEvent @event);

	/// <summary>
	/// Queries the event store for events that match the given criteria.
	/// </summary>
	/// <remarks>
	/// Store implementations are advised to provide full support for the 
	/// specified criteria, but aren't required to.
	/// <para>
	/// The more user-friendly querying API in <see cref="IEventQuery{TBaseEvent}"/> 
	/// provides a fluent API over any store to build the criteria object, 
	/// and can therefore be used with any event store implementation. 
	/// It's accessible by executing the 
	/// <see cref="EventQueryBuilder.Query{TBaseEvent}"/> extension method
	/// on an event store instance.
	/// </para>
	/// </remarks>
	IEnumerable<TBaseEvent> Query(EventQueryCriteria criteria);

	/// <summary>
	/// Persists all events <see cref="Persist"/>ed so far, effectively commiting 
	/// the changes to the underlying store in a unit-of-work style.
	/// </summary>
	void Commit();
}
