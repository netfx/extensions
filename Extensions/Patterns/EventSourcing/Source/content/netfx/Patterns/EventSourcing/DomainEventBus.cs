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
using System.Threading;
using System.Globalization;

/// <summary>
/// Default implementation of an <see cref="IDomainEventBus{TAggregateId, TBaseEvent}"/> that 
/// invokes handlers as events are published, and where handlers are 
/// run in-process (if any handlers are provided).
/// </summary>
/// <remarks>
/// A persistent <see cref="IDomainEventStore{TAggregateId, TBaseEvent}"/> can also be specified 
/// to persist events to a store for later playback or auditing.
/// <para>
/// Handlers with <see cref="IDomainEventHandler.IsAsync"/> set to 
/// <see langword="true"/> are invoked through the optional 
/// async runner delegate passed to the constructor.
/// </para>
/// </remarks>
/// <typeparam name="TAggregateId">The type of identifier used by the aggregate roots in the domain.</typeparam>
/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing" />
partial class DomainEventBus<TAggregateId, TBaseEvent> : IDomainEventBus<TAggregateId, TBaseEvent>
	where TAggregateId : IComparable
{
	private IDomainEventStore<TAggregateId, TBaseEvent> eventStore;
	private Action<Action> asyncActionRunner;
	private IEnumerable<IDomainEventHandler> eventHandlers;

	/// <summary>
	/// Initializes the <see cref="None"/> null object 
	/// pattern property.
	/// </summary>
	static DomainEventBus()
	{
		None = new NullBus();
	}

	/// <summary>
	/// Gets a default domain event bus implementation that 
	/// does nothing (a.k.a. Null Object Pattern).
	/// </summary>
	public static IDomainEventBus<TAggregateId, TBaseEvent> None { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainEventBus{TAggregateId, TBaseEvent}"/> class
	/// with a persistent store for events and no in-memory handlers.
	/// </summary>
	/// <param name="eventStore">The event store to persist events to.</param>
	public DomainEventBus(IDomainEventStore<TAggregateId, TBaseEvent> eventStore)
		: this(eventStore, Enumerable.Empty<IDomainEventHandler>())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainEventBus{TAggregateId, TBaseEvent}"/> class with 
	/// the default async runner that enqueues work in the <see cref="ThreadPool"/>.
	/// </summary>
	/// <param name="eventHandlers">The event handlers.</param>
	public DomainEventBus(IEnumerable<IDomainEventHandler> eventHandlers)
		: this(DomainEventStore<TAggregateId, TBaseEvent>.None, eventHandlers, action => ThreadPool.QueueUserWorkItem(state => action()))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainEventBus{TAggregateId, TBaseEvent}"/> class with 
	/// a persistent store for events and the default async runner that enqueues work in the <see cref="ThreadPool"/>.
	/// </summary>
	/// <param name="eventStore">The event store to persist events to.</param>
	/// <param name="eventHandlers">The event handlers.</param>
	public DomainEventBus(IDomainEventStore<TAggregateId, TBaseEvent> eventStore, IEnumerable<IDomainEventHandler> eventHandlers)
		: this(eventStore, eventHandlers, action => ThreadPool.QueueUserWorkItem(state => action()))
	{
	}


	/// <summary>
	/// Initializes a new instance of the <see cref="DomainEventBus{TAggregateId, TBaseEvent}"/> class with 
	/// the given async runner.
	/// </summary>
	/// <param name="eventHandlers">The event handlers.</param>
	/// <param name="asyncActionRunner">The async action runner to use to invoke event handlers 
	/// that have <see cref="DomainEventHandler.IsAsync"/> set to <see langword="true"/>.</param>
	public DomainEventBus(IEnumerable<IDomainEventHandler> eventHandlers, Action<Action> asyncActionRunner)
		: this(DomainEventStore<TAggregateId, TBaseEvent>.None, eventHandlers, asyncActionRunner)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainEventBus{TAggregateId, TBaseEvent}"/> class with 
	/// a persistent store for events, a set of event handlers and a specific async runner.
	/// </summary>
	/// <param name="eventStore">The event store to persist events to.</param>
	/// <param name="eventHandlers">The event handlers.</param>
	/// <param name="asyncActionRunner">The async action runner to use to invoke event handlers 
	/// that have <see cref="DomainEventHandler.IsAsync"/> set to <see langword="true"/>.</param>
	public DomainEventBus(IDomainEventStore<TAggregateId, TBaseEvent> eventStore, IEnumerable<IDomainEventHandler> eventHandlers, Action<Action> asyncActionRunner)
	{
		Guard.NotNull(() => eventStore, eventStore);
		Guard.NotNull(() => eventHandlers, eventHandlers);
		Guard.NotNull(() => asyncActionRunner, asyncActionRunner);

		if (eventHandlers.Any(eh => eh == null))
			throw new ArgumentException("Invalid null handler found.", "eventHandlers");

		if (eventHandlers.Any(eh => eh.EventType == null))
			throw new ArgumentException(string.Format(
				CultureInfo.CurrentCulture, 
				"Invalid handlers with null EventType found: {0}", 
				string.Join(", ", eventHandlers.Where(eh => eh.EventType == null))), 
				"eventHandlers");

		if (eventHandlers.Any(eh => !ImplementsGenericHandler(eh.GetType())))
			throw new ArgumentException(string.Format(
				CultureInfo.CurrentCulture,
				"The following event handlers to not implement the generic interface IDomainEventHandler<TAggregateId, TEventArgs>: {0}.", 
				 string.Join(", ", eventHandlers.Where(eh => !ImplementsGenericHandler(eh.GetType())).Select(eh => eh.GetType().FullName))), 
				 "eventHandlers");

		this.eventStore = eventStore;
		this.eventHandlers = eventHandlers.ToList();
		this.asyncActionRunner = asyncActionRunner;
	}

	/// <summary>
	/// Publishes the specified event to the bus so that all subscribers are notified.
	/// </summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="args">The event payload.</param>
	public virtual void Publish(AggregateRoot<TAggregateId, TBaseEvent> sender, TBaseEvent args)
	{
		Guard.NotNull(() => sender, sender);
		Guard.NotNull(() => args, args);

		// Events are persisted first of all.
		this.eventStore.Persist(sender, args);

		var compatibleHandlers = this.eventHandlers.Where(h => h.EventType.IsAssignableFrom(args.GetType())).ToList();
		dynamic dynamicEvent = args;

		// By making this dynamic, we allow event handlers to subscribe to base classes
		foreach (dynamic handler in compatibleHandlers.Where(h => !h.IsAsync).AsParallel())
		{
			handler.Handle(sender.Id, dynamicEvent);
		}

		// Run background handlers through the async runner.
		foreach (dynamic handler in compatibleHandlers.Where(h => h.IsAsync).AsParallel())
		{
			asyncActionRunner(() => handler.Handle(sender.Id, args));
		}
	}

	private bool ImplementsGenericHandler(Type type)
	{
		var genericHandler = typeof(IDomainEventHandler<,>);

		return type.GetInterfaces().Any(iface => 
			iface.IsGenericType && 
			iface.GetGenericTypeDefinition() == genericHandler);
	}

	/// <summary>
	/// Provides a null <see cref="IDomainEventBus{TAggregateId, TBaseEvent}"/> implementation 
	/// for use when no events have been configured.
	/// </summary>
	private class NullBus : IDomainEventBus<TAggregateId, TBaseEvent>
	{
		/// <summary>
		/// Does nothing.
		/// </summary>
		public void Publish(AggregateRoot<TAggregateId, TBaseEvent> sender, TBaseEvent args)
		{
		}
	}
}
