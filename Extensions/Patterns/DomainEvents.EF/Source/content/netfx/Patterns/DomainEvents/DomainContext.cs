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
using System.Diagnostics;

/// <remarks>
/// Adds domain event hooks to the domain context partial class, 
/// by implementing the partial methods extension points provided.
/// 
/// Domain events are buffered while the domain entities are 
/// performing their work, and are only dispatched after 
/// the context SaveChanges has been called. Therefore, all 
/// events typically have the past tense.
/// </remarks>
/// <nuget id="netfx-Patterns.DomainEvents.EF" />
partial class DomainContext<TContextInterface, TId> : IDomainEvents
{
	// Null pattern for cases where the partial class original DomainContext 
	// constructor without events is used.
	private IBufferedDomainEvents events = new NullBufferedDomainEvents();

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainContext&lt;TContextInterface, TId&gt;"/> class.
	/// </summary>
	/// <param name="nameOrConnectionString">The name or connection string.</param>
	/// <param name="events">The interface that entities can use to raise domain events.</param>
	public DomainContext(string nameOrConnectionString, IDomainEvents events)
		: this(nameOrConnectionString)
	{
		this.events = new BufferedDomainEvents(events);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainContext&lt;TContextInterface, TId&gt;"/> class.
	/// </summary>
	/// <param name="events">The interface that entities can use to raise domain events.</param>
	public DomainContext(IDomainEvents events)
		: this()
	{
		this.events = new BufferedDomainEvents(events);
	}

	partial void OnEntityCreated(object entity)
	{
		var eventAccessor = entity as IDomainEventsAccessor;
		if (eventAccessor != null)
			eventAccessor.Events = this;
	}

	partial void OnContextSavedChanges()
	{
		this.events.RaiseEvents();
	}

	void IDomainEvents.Raise<T>(T @event)
	{
		this.events.Raise(@event);
	}

	private interface IBufferedDomainEvents : IDomainEvents
	{
		void RaiseEvents();
	}

	private class BufferedDomainEvents : IBufferedDomainEvents
	{
		private IDomainEvents events;
		private Queue<Action<IDomainEvents>> raisers = new Queue<Action<IDomainEvents>>();

		public BufferedDomainEvents(IDomainEvents events)
		{
			this.events = events;
		}

		public void Raise<T>(T @event)
		{
			this.raisers.Enqueue(x => x.Raise(@event));
		}

		public void RaiseEvents()
		{
			while (this.raisers.Any())
			{
				this.raisers.Dequeue().Invoke(this.events);
			}
		}
	}

	private class NullBufferedDomainEvents : IBufferedDomainEvents
	{
		public void Raise<T>(T @event)
		{
		}

		public void RaiseEvents()
		{
		}
	}
}