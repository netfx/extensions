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
/// Base interface part of the infrastructure. Concrete 
/// handlers should inherit <see cref="EventHandler{TAggregateId, TEvent}"/> 
/// or implement <see cref="IEventHandler{TAggregateId, TEvent}"/> instead.
/// </summary>
/// <nuget id="netfx-Patterns.EventSourcing" />
partial interface IEventHandler
{
	/// <summary>
	/// Invocation style hint that the <see cref="IEventBus{TAggregateId, TBaseEvent}"/> implementation
	/// can use to invoke a handler asynchronously with regards to the event publisher.
	/// </summary>
	bool IsAsync { get; }
}

/// <summary>
/// Base interface for domain event handlers that handle a specific type of event.
/// </summary>
/// <typeparam name="TAggregateId">The type of identifier used by the aggregate roots in the domain.</typeparam>
/// <typeparam name="TEvent">Type of event argument this handler can process.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing" />
partial interface IEventHandler<TAggregateId, TEvent> : IEventHandler
{
	/// <summary>
	/// Handles the specified event.
	/// </summary>
	void Handle(TAggregateId aggregateId, TEvent @event);
}