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
/// <typeparam name="TAggregateId">The type of identifier used by the aggregate roots in the domain.</typeparam>
/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing"/>
partial interface IDomainEventStore<TAggregateId, TBaseEvent>
	where TAggregateId : IComparable
{
	/// <summary>
	/// Notifies the store that the given event raised by the given sender 
	/// should be persisted when <see cref="IDomainEventStore{TAggregateId, TBaseEvent}.Commit"/> is called.
	/// </summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="args">The instance containing the event data.</param>
	void Persist(AggregateRoot<TAggregateId, TBaseEvent> sender, TBaseEvent args);

	/// <summary>
	/// Queries the event store for events that match the given criteria.
	/// </summary>
	/// <remarks>
	/// Store implementations are advised to provide full support for the 
	/// specified criteria, but aren't required to.
	/// <para>
	/// The more user-friendly querying API in <see cref="IDomainEventQuery{TAggregateId, TBaseEvent}"/> 
	/// provides a fluent API over any store to build the criteria object, 
	/// and can therefore be used with any event store implementation. 
	/// It's accessible by executing the 
	/// <see cref="DomainEventQueryBuilder.Query{TAggregateId, TBaseEvent}"/> extension method
	/// on a domain event store instance.
	/// </para>
	/// </remarks>
	IEnumerable<TBaseEvent> Query(DomainEventQueryCriteria<TAggregateId> criteria);

	/// <summary>
	/// Persists all events <see cref="Persist"/>ed so far, effectively commiting 
	/// the changes to the underlying store in a unit-of-work style.
	/// </summary>
	void Commit();
}
