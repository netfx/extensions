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
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// Basic in-memory implementation of domain events that supports both synchronous 
/// and asynchronous event handlers (see <see cref="IDomainEventHandler.IsAsync"/>.
/// </summary>
partial class DomainEvents : IDomainEvents
{
	private IEnumerable<IDomainEventHandler> handlers;
	private IDomainEventScheduler scheduler;

	/// <summary>
	/// Initializes the <see cref="None"/> null object 
	/// pattern property.
	/// </summary>
	static DomainEvents()
	{
		None = new NullDomainEvents();
	}

	/// <summary>
	/// Gets a default domain events implementation that 
	/// does nothing (a.k.a. Null Object Pattern).
	/// </summary>
	public static IDomainEvents None { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainEvents"/> class 
	/// with the default <see cref="DomainEventScheduler.Default"/>.
	/// </summary>
	/// <param name="handlers">The available handlers.</param>
	public DomainEvents(IEnumerable<IDomainEventHandler> handlers)
		: this(handlers, DomainEventScheduler.Default)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainEvents"/> class.
	/// </summary>
	/// <param name="handlers">The available handlers.</param>
	/// <param name="asyncRunner">The scheduler to use for running async event handlers.</param>
	public DomainEvents(IEnumerable<IDomainEventHandler> handlers, IDomainEventScheduler asyncRunner)
	{
		// We only process non-null handlers that implement the generic handler interface.
		this.handlers = handlers
			.Where(h => h != null && h.GetType().GetInterfaces().Any(i =>
				i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
			.ToList();

		this.scheduler = asyncRunner;
	}

	/// <summary>
	/// Raises the specified event.
	/// </summary>
	/// <typeparam name="T">Type of data in the event, typically inferred by the compiler based on the received <paramref name="event"/>.</typeparam>
	/// <param name="event">The event payload.</param>
	public void Raise<T>(T @event)
	{
		var compatible = this.handlers.Where(h => h.EventType.IsAssignableFrom(typeof(T)));
		
		foreach (dynamic handler in compatible.Where(h => !h.IsAsync).AsParallel())
		{
			handler.Handle(@event);
		}

		// Run background handlers in another thread.
		foreach (dynamic handler in compatible.Where(h => h.IsAsync).AsParallel())
		{
			this.scheduler.Schedule(() => handler.Handle(@event));
		}
	}

	/// <summary>
	/// Provides a null <see cref="IDomainEvents"/> implementation 
	/// for use when no events have been configured.
	/// </summary>
	private class NullDomainEvents : IDomainEvents
	{
		/// <summary>
		/// Does nothing.
		/// </summary>
		public void Raise<T>(T @event)
		{
		}
	}
}
