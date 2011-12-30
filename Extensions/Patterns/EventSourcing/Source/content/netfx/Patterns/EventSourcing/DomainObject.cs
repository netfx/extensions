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

/// <summary>
/// Base class for domain classes that raise and optionally consume 
/// event objects in an event sourcing pattern style implementation.
/// </summary>
/// <typeparam name="TObjectId">The type of identifier used by the domain object.</typeparam>
/// <typeparam name="TBaseEvent">The base type or interface implemented by events.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing" />
abstract partial class DomainObject<TObjectId, TBaseEvent>
{
	private Dictionary<Type, Action<TBaseEvent>> handlers = new Dictionary<Type, Action<TBaseEvent>>();
	private List<TBaseEvent> events = new List<TBaseEvent>();

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainObject{TObjectId, TBaseEvent}"/> class.
	/// </summary>
	/// <remarks>
	/// Derived classes decide if they allow direct instantiation or 
	/// if instantiation always has to happen from a stream of events.
	/// </remarks>
	protected DomainObject()
	{
	}

	/// <summary>
	/// Gets or sets the identifier of the domain object.
	/// </summary>
	public virtual TObjectId Id { get; set; }

	/// <summary>
	/// Clears the internal events retrieved from <see cref="GetEvents"/>, 
	/// signaling that all pending events have been commited.
	/// </summary>
	public virtual void AcceptEvents()
	{
		this.events.Clear();
	}

	/// <summary>
	/// Gets the new events that were applied by the domain object.
	/// </summary>
	public virtual IEnumerable<TBaseEvent> GetEvents()
	{
		return this.events.AsReadOnly();
	}

	/// <summary>
	/// Configures a handler for an event. 
	/// </summary>
	protected virtual void Handles<TEvent>(Action<TEvent> handler)
		where TEvent : TBaseEvent
	{
		this.handlers.Add(typeof(TEvent), @event => handler((TEvent)@event));
	}

	/// <summary>
	/// Loads the the domain object from an even stream.
	/// </summary>
	protected void Load(IEnumerable<TBaseEvent> events)
	{
		foreach (var args in events)
		{
			var handler = default(Action<TBaseEvent>);
			if (this.handlers.TryGetValue(args.GetType(), out handler))
				handler.Invoke(args);
		}
	}

	/// <summary>
	/// Applies an event to the entity. 
	/// The derived class should invoke <see cref="Handles{TEvent}"/> 
	/// to configure the handlers for specific types of events. The 
	/// handlers perform the actual state changes to the entity.
	/// </summary>
	protected virtual void Apply<TEvent>(TEvent @event)
		where TEvent : TBaseEvent
	{
		var handler = default(Action<TBaseEvent>);
		if (this.handlers.TryGetValue(@event.GetType(), out handler))
			handler.Invoke(@event);

		this.events.Add(@event);
	}
}
