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
using System.Reflection;
using System.Linq.Expressions;

/// <summary>
/// Base class for aggregate roots that use events to apply state 
/// changes (Event Sourcing) and notify consumers on an <see cref="IDomainEventBus{TId}"/>.
/// </summary>
/// <typeparam name="TId">The type of identifier used by the aggregate root.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing.Core" />
abstract partial class AggregateRoot<TId>
	where TId : IComparable
{
	private Dictionary<Type, Action<TimestampedEventArgs>> handlers = new Dictionary<Type, Action<TimestampedEventArgs>>();
	private List<TimestampedEventArgs> changes = new List<TimestampedEventArgs>();

	/// <summary>
	/// Gets or sets the aggregate root identifier.
	/// </summary>
	public TId Id { get; set; }

	/// <summary>
	/// Clears the internal events retrieved from <see cref="GetChanges"/>, 
	/// signaling that all pending events have been commited.
	/// </summary>
	public void AcceptChanges()
	{
		this.changes.Clear();
	}

	/// <summary>
	/// Gets the pending changes.
	/// </summary>
	public IEnumerable<TimestampedEventArgs> GetChanges()
	{
		return this.changes.AsReadOnly();
	}

	/// <summary>
	/// Loads the aggregate root state from an even stream.
	/// </summary>
	public void Load(IEnumerable<TimestampedEventArgs> history)
	{
		foreach (var e in history)
		{
			var handler = default(Action<TimestampedEventArgs>);
			if (this.handlers.TryGetValue(e.GetType(), out handler))
			{
				handler.Invoke(e);
			}
		}
	}

	/// <summary>
	/// Configures a handler for an event.
	/// </summary>
	protected void Handles<TEvent>(Action<TEvent> handler)
		where TEvent : TimestampedEventArgs
	{
		this.handlers.Add(typeof(TEvent), @event => handler((TEvent)@event));
	}

	/// <summary>
	/// Applies a change to the entity state via an event. 
	/// The derived class should provide a method called <c>Apply</c> 
	/// receiving the concrete type of event, where state 
	/// changes are performed to the entity.
	/// </summary>
	protected void ApplyChange<TEvent>(TEvent @event)
		where TEvent : TimestampedEventArgs
	{
		ApplyChangeImpl(@event, true);
	}

	private void ApplyChangeImpl<TEvent>(TEvent @event, bool isNew)
		where TEvent : TimestampedEventArgs
	{
		var handler = default(Action<TimestampedEventArgs>);
		if (this.handlers.TryGetValue(typeof(TEvent), out handler))
			handler.Invoke(@event);

		if (isNew) 
			this.changes.Add(@event);
	}
}
