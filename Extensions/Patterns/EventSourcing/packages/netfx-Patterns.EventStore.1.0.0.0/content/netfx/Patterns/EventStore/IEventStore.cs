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
	/// Gets or sets the function that converts a <see cref="Type"/> to 
	/// its string representation in the store. Used to calculate the 
	/// values of <see cref="IStoredEvent.EventType"/>.
	/// </summary>
	Func<Type, string> TypeNameConverter { get; set; }

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
	/// This is the only low-level querying method that stores need to implement. 
	/// <para>
	/// As a facility for stores that persist events in an <see cref="IQueryable{T}"/> 
	/// queryable object, the <see cref="StoredEventCriteria"/> object 
	/// can be converted to an expression using the <see cref="StoredEventCriteriaExtensions.ToExpression"/> 
	/// extension method, making the query implementation trivial in that case.
	/// </para>
	/// <para>
	/// The more user-friendly querying API in <see cref="IEventQuery{TBaseEvent}"/> 
	/// leverages this method internally and therefore can be used by any 
	/// event store implementation. It's accessible by executing the 
	/// <see cref="EventQueryExtensions.Query{TBaseEvent}"/> extension method
	/// on an event store instance.
	/// </para>
	/// </remarks>
	IEnumerable<TBaseEvent> Query(StoredEventCriteria criteria);

	/// <summary>
	/// Persists all events <see cref="Persist"/>ed so far, effectively commiting 
	/// the changes to the underlying store in a unit-of-work style.
	/// </summary>
	void Commit();
}
