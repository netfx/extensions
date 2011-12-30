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
	private List<TBaseEvent> changes = new List<TBaseEvent>();

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
	/// Gets or sets the identifier of the domain object sourcing the event.
	/// </summary>
	public virtual TObjectId Id { get; set; }

	/// <summary>
	/// Clears the internal events retrieved from <see cref="GetChanges"/>, 
	/// signaling that all pending events have been commited.
	/// </summary>
	public virtual void AcceptChanges()
	{
		this.changes.Clear();
	}

	/// <summary>
	/// Gets the pending changes.
	/// </summary>
	public virtual IEnumerable<TBaseEvent> GetChanges()
	{
		return this.changes.AsReadOnly();
	}

	/// <summary>
	/// Gets a value indicating whether this object has pending changes.
	/// </summary>
	public virtual bool HasChanges
	{
		get { return this.changes.Any(); }
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
	protected void Load(IEnumerable<TBaseEvent> history)
	{
		foreach (var e in history)
		{
			ApplyChange(e, false);
		}
	}

	/// <summary>
	/// Applies a change to the entity state via an event. 
	/// The derived class should invoke <see cref="Handles{TEvent}"/> 
	/// to configure the handlers for specific types of events. The 
	/// handlers perform the actual state changes to the entity.
	/// </summary>
	protected virtual void Raise<TEvent>(TEvent @event)
		where TEvent : TBaseEvent
	{
		ApplyChange(@event, true);
	}

	/// <summary>
	/// Called whenever an event is handled by the domain 
	/// object. Useful to mark the object as dirty if needed 
	/// for persistence, beyond the <see cref="HasChanges"/> 
	/// property which reports if there are new events 
	/// in the object, but not if it actually handled an 
	/// event itself at all (changing its internal state).
	/// </summary>
	protected virtual void OnEventHandled(TBaseEvent e)
	{
	}

	private void ApplyChange(TBaseEvent e, bool isNew)
	{
		var handler = default(Action<TBaseEvent>);
		if (this.handlers.TryGetValue(e.GetType(), out handler))
		{
			handler.Invoke(e);
			OnEventHandled(e);
		}

		if (isNew)
			this.changes.Add(e);
	}
}
