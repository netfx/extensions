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
/// Interface implemented by the component that coordinates 
/// event handler invocation when a subscribed event is published.
/// </summary>
/// <typeparam name="TAggregateId">The type of identifier used by the aggregate roots in the domain.</typeparam>
/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing"/>
partial interface IEventBus<TAggregateId, TBaseEvent>
	where TBaseEvent : ITimestamped
{
	/// <summary>
	/// Publishes the pending changes in the given aggregate root, so that all subscribers are notified. 
	/// </summary>
	/// <param name="aggregate">The aggregate root which may contain pending changes.</param>
	/// <remarks>
	/// Default <see cref="EventBus{TAggregateId, TBaseEvent}"/> also calls 
	/// <see cref="AggregateRoot{TAggregateId, TBaseEvent}.AcceptChanges"/> after 
	/// saving the changes to the <see cref="IEventStore{TAggregateId, TBaseEvent}"/> store.
	/// </remarks>
	void PublishChanges(AggregateRoot<TAggregateId, TBaseEvent> aggregate);
}