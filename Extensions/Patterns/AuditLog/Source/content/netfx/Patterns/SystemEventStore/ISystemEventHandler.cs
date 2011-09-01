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
/// handlers should inherit <see cref="SystemEventHandler{TEventArgs}"/> 
/// or implement <see cref="ISystemEventHandler{TEventArgs}"/> instead.
/// </summary>
/// <nuget id="netfx-Patterns.SystemEventStore" />
partial interface ISystemEventHandler
{
	/// <summary>
	/// Invocation style hint that the <see cref="ISystemEventBus{TBaseEvent}"/> implementation
	/// can use to invoke a handler asynchronously with regards to the event publisher.
	/// </summary>
	bool IsAsync { get; }

	/// <summary>
	/// Gets the type of the event argument this handler can process.
	/// </summary>
	Type EventType { get; }
}

/// <summary>
/// Base interface for system event handlers that handle a specific type of event.
/// </summary>
/// <remarks>
/// Unlike domain/aggregate root specific handlers, system event handlers process 
/// events that do not apply to a single aggregate root and are typically not related 
/// to the domain model but some system-level service or functionality (such as an 
/// email sending service).
/// </remarks>
/// <typeparam name="TEventArgs">Type of event argument this handler can process.</typeparam>
/// <nuget id="netfx-Patterns.SystemEventStore" />
partial interface ISystemEventHandler<TEventArgs> : ISystemEventHandler
{
	/// <summary>
	/// Handles the specified event.
	/// </summary>
	void Handle(TEventArgs @event);
}