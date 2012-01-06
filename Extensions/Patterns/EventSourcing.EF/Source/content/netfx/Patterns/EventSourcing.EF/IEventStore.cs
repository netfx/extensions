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

/// <summary>
/// An Entity Framework-based interface that persists domain 
/// objects with Guid identifiers and <see cref="StoredEvent"/> persisted entities.
/// </summary>
/// <typeparam name="TBaseEvent">The type of the base event.</typeparam>
/// <remarks>
/// This interface simplifies testing against the EF-based <see cref="EventStore{TBaseEvent}"/> 
/// as it removes the need to declare the myriad generic parameters required by the 
/// base queryable event store API.
/// <para>
/// This interface and <see cref="EventStore{TBaseEvent}"/> implementations are just 
/// one way to implement an event store with a database backend. Implementers can 
/// of course just leverage IQueryableEventStore{TObjectId, TBaseEvent, TStoredEvent} 
/// and persist differently altogether, or provide different id types, etc.
/// </para>
/// </remarks>
partial interface IEventStore<TBaseEvent> : IQueryableEventStore<Guid, TBaseEvent, StoredEvent>
	where TBaseEvent : ITimestamped
{
}
