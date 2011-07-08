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
/// <para>
/// Domain events are published after the context SaveChanges has 
/// been called.
/// </para>
/// </remarks>
/// <nuget id="netfx-Patterns.DomainEvents.EF" />
partial class DomainContext<TContextInterface, TId>
{
	// Null pattern for cases where the partial class original DomainContext 
	// constructor without an event bus is used.
	private IDomainEventBus eventBus = DomainEventBus.None;

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainContext&lt;TContextInterface, TId&gt;"/> class.
	/// </summary>
	/// <param name="nameOrConnectionString">The name or connection string.</param>
	/// <param name="eventBus">The event publisher invoked after entities are saved to publish all generated events.</param>
	public DomainContext(string nameOrConnectionString, IDomainEventBus eventBus)
		: this(nameOrConnectionString)
	{
		this.eventBus = eventBus;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainContext&lt;TContextInterface, TId&gt;"/> class.
	/// </summary>
	/// <param name="eventBus">The event publisher invoked after entities are saved to publish all generated events.</param>
	public DomainContext(IDomainEventBus eventBus)
		: this()
	{
		this.eventBus = eventBus;
	}

	partial void OnContextSavedChanges()
	{
		// TODO: publish all events.
		var events = this.ChangeTracker.Entries<AggregateRoot<TId>>()
			.SelectMany(root => root.Entity.GetChanges());

		foreach (var @event in events)
		{
			this.eventBus.Publish(@event);
		}

	}
}