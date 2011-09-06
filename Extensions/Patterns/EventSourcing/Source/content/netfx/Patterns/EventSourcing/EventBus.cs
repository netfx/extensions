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
/// Default implementation of an <see cref="IEventBus{TAggregateId, TBaseEvent}"/> that 
/// invokes handlers as events are published, and where handlers are 
/// run in-process (if any handlers are provided).
/// </summary>
/// <remarks>
/// A persistent <see cref="IEventStore{TAggregateId, TBaseEvent}"/> can also be specified 
/// to persist events to a store for later playback or auditing.
/// <para>
/// Handlers with <see cref="IEventHandler.IsAsync"/> set to 
/// <see langword="true"/> are invoked through the optional 
/// async runner delegate passed to the constructor.
/// </para>
/// </remarks>
/// <typeparam name="TAggregateId">The type of identifier used by the aggregate roots in the domain.</typeparam>
/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing" />
partial class EventBus<TAggregateId, TBaseEvent> : IEventBus<TAggregateId, TBaseEvent>
	where TAggregateId : IComparable
{
	private IEventStore<TAggregateId, TBaseEvent> eventStore;
	private Action<Action> asyncActionRunner;
	private List<HandlerDescriptor> handlerDescriptors;
	// Pipelines indexed by event type, containing two lists: async and sync handlers.
	private Dictionary<Type, Tuple<List<dynamic>, List<dynamic>>> handlerPipelines = new Dictionary<Type, Tuple<List<dynamic>, List<dynamic>>>();

	/// <summary>
	/// Initializes the <see cref="None"/> null object 
	/// pattern property.
	/// </summary>
	static EventBus()
	{
		None = new NullBus();
	}

	/// <summary>
	/// Gets a default domain event bus implementation that 
	/// does nothing (a.k.a. Null Object Pattern).
	/// </summary>
	public static IEventBus<TAggregateId, TBaseEvent> None { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="EventBus{TAggregateId, TBaseEvent}"/> class
	/// with a persistent store for events and no in-memory handlers.
	/// </summary>
	/// <param name="eventStore">The event store to persist events to.</param>
	public EventBus(IEventStore<TAggregateId, TBaseEvent> eventStore)
		: this(eventStore, Enumerable.Empty<IEventHandler>())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="EventBus{TAggregateId, TBaseEvent}"/> class with 
	/// the default async runner that enqueues work in the <see cref="ThreadPool"/>.
	/// </summary>
	/// <param name="eventHandlers">The event handlers.</param>
	public EventBus(IEnumerable<IEventHandler> eventHandlers)
		: this(new NullStore(), eventHandlers, action => ThreadPool.QueueUserWorkItem(state => action()))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="EventBus{TAggregateId, TBaseEvent}"/> class with 
	/// a persistent store for events and the default async runner that enqueues work in the <see cref="ThreadPool"/>.
	/// </summary>
	/// <param name="eventStore">The event store to persist events to.</param>
	/// <param name="eventHandlers">The event handlers.</param>
	public EventBus(IEventStore<TAggregateId, TBaseEvent> eventStore, IEnumerable<IEventHandler> eventHandlers)
		: this(eventStore, eventHandlers, action => ThreadPool.QueueUserWorkItem(state => action()))
	{
	}


	/// <summary>
	/// Initializes a new instance of the <see cref="EventBus{TAggregateId, TBaseEvent}"/> class with 
	/// the given async runner.
	/// </summary>
	/// <param name="eventHandlers">The event handlers.</param>
	/// <param name="asyncActionRunner">The async action runner to use to invoke event handlers 
	/// that have <see cref="IEventHandler.IsAsync"/> set to <see langword="true"/>.</param>
	public EventBus(IEnumerable<IEventHandler> eventHandlers, Action<Action> asyncActionRunner)
		: this(new NullStore(), eventHandlers, asyncActionRunner)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="EventBus{TAggregateId, TBaseEvent}"/> class with 
	/// a persistent store for events, a set of event handlers and a specific async runner.
	/// </summary>
	/// <param name="eventStore">The event store to persist events to.</param>
	/// <param name="eventHandlers">The event handlers.</param>
	/// <param name="asyncActionRunner">The async action runner to use to invoke event handlers 
	/// that have <see cref="IEventHandler.IsAsync"/> set to <see langword="true"/>.</param>
	public EventBus(IEventStore<TAggregateId, TBaseEvent> eventStore, IEnumerable<IEventHandler> eventHandlers, Action<Action> asyncActionRunner)
	{
		Guard.NotNull(() => eventStore, eventStore);
		Guard.NotNull(() => eventHandlers, eventHandlers);
		Guard.NotNull(() => asyncActionRunner, asyncActionRunner);

		if (eventHandlers.Any(eh => eh == null))
			throw new ArgumentException("Invalid null handler found.", "eventHandlers");

		var genericHandler = typeof(IEventHandler<,>);

		this.handlerDescriptors = eventHandlers.Select(handler =>
			new HandlerDescriptor
			{
				Handler = handler,
				// Grab the type of body from the generic 
				// type argument, if any.
				EventType = handler.GetType()
					.GetInterfaces()
					.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericHandler)
					.Select(x => x.GetGenericArguments()[1])
					.FirstOrDefault()
			})
			.ToList();

		var invalidHandlers = this.handlerDescriptors.Where(x => x.EventType == null).ToList();
		if (invalidHandlers.Any())
			throw new ArgumentException(string.Format(
				CultureInfo.CurrentCulture,
				"The following event handlers to not implement the generic interface IEventHandler<TAggregateId, TEvent>: {0}.",
				 string.Join(", ", invalidHandlers.Select(handler => handler.GetType().FullName))),
				 "eventHandlers");

		this.eventStore = eventStore;
		this.asyncActionRunner = asyncActionRunner;
	}

	/// <summary>
	/// Publishes the specified event to the bus so that all subscribers are notified.
	/// </summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="event">The event payload.</param>
	public virtual void Publish(AggregateRoot<TAggregateId, TBaseEvent> sender, TBaseEvent @event)
	{
		Guard.NotNull(() => sender, sender);
		Guard.NotNull(() => @event, @event);

		// Events are persisted first of all.
		this.eventStore.Persist(sender, @event);

		var eventType = @event.GetType();

		var pipeline = this.handlerPipelines.GetOrAdd(eventType, type =>
		{
			// We calculate the pipeline only once, as handlers can't 
			// be added after bus construction.
			// This is also done lazily for each event type received 
			// to avoid negatively impacting initialization time.
			var compatibleHandlers = this.handlerDescriptors
				.Where(h => h.EventType.IsAssignableFrom(eventType))
				.ToList();

			// We separate the lists of async and sync handlers as they
			// are invoked separately below.
			return new Tuple<List<dynamic>, List<dynamic>>(
				compatibleHandlers.Where(h => !h.Handler.IsAsync).Select(x => (dynamic)x.Handler).ToList(),
				compatibleHandlers.Where(h => h.Handler.IsAsync).Select(x => (dynamic)x.Handler).ToList());
		});

		// By making this dynamic, we allow event handlers to subscribe to base classes
		foreach (var handler in pipeline.Item1.AsParallel())
		{
			OnHandle(handler, sender.Id, @event);
		}

		// Run background handlers through the async runner.
		foreach (var handler in pipeline.Item2.AsParallel())
		{
			asyncActionRunner(() => OnHandle(handler, sender.Id, @event));
		}
	}

	/// <summary>
	/// Called when invoking the handler for an event with the given headers.
	/// </summary>
	/// <remarks>
	/// Derived classes can change the way handlers are invoked, optimize it, 
	/// or do pre/post processing right before/after the command is handled.
	/// </remarks>
	protected virtual void OnHandle(dynamic handler, TAggregateId aggregateId, dynamic @event)
	{
		handler.Handle(aggregateId, @event);
	}

	private class HandlerDescriptor
	{
		/// <summary>
		/// Gets or sets the type of event payload the handler can process, 
		/// retrieved from the handler TEvent generic parameter.
		/// </summary>
		public Type EventType { get; set; }

		/// <summary>
		/// Gets or sets the handler.
		/// </summary>
		public IEventHandler Handler { get; set; }
	}

	/// <summary>
	/// Provides a null <see cref="IEventBus{TAggregateId, TBaseEvent}"/> implementation 
	/// for use when no events have been configured.
	/// </summary>
	private class NullBus : IEventBus<TAggregateId, TBaseEvent>
	{
		/// <summary>
		/// Does nothing.
		/// </summary>
		public void Publish(AggregateRoot<TAggregateId, TBaseEvent> sender, TBaseEvent args)
		{
		}
	}

	private class NullStore : IEventStore<TAggregateId, TBaseEvent>
	{
		public void Persist(AggregateRoot<TAggregateId, TBaseEvent> sender, TBaseEvent args)
		{
		}

		public IEnumerable<TBaseEvent> Query(EventQueryCriteria<TAggregateId> criteria)
		{
			return Enumerable.Empty<TBaseEvent>();
		}

		public void Persist(TBaseEvent @event)
		{
		}

		public void Commit()
		{
		}
	}
}
