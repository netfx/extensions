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
using System.Linq.Expressions;
using System.Reflection;

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
	where TBaseEvent : ITimestamped
{
	private IEventStore<TAggregateId, TBaseEvent> eventStore;
	private Action<Action> asyncActionRunner;
	private List<HandlerDescriptor> handlerDescriptors;
	// Pipelines indexed by event type, containing two lists: async and sync handlers.
	private Dictionary<Type, Tuple<List<Action<TAggregateId, TBaseEvent>>, List<Action<TAggregateId, TBaseEvent>>>> handlerPipelines = new Dictionary<Type, Tuple<List<Action<TAggregateId, TBaseEvent>>, List<Action<TAggregateId, TBaseEvent>>>>();

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

		this.handlerDescriptors = (from handler in eventHandlers
								   let handlerInterface = handler.GetType()
									   .GetInterfaces()
									   .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericHandler)
									   .FirstOrDefault()
								   where handlerInterface != null
								   select new HandlerDescriptor
								   {
									   Handler = handler,
									   // Grab the type of body from the generic type argument
									   EventType = handlerInterface.GetGenericArguments()[1], 
									   HandleMethod = handler.GetType().GetInterfaceMap(handlerInterface).TargetMethods[0],
								   }).ToList();


		var invalidHandlers = eventHandlers.Select(handler => new
			{
				HandlerType = handler.GetType(),
				GenericType = handler.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericHandler)
			}).Where(x => x.GenericType == null);

		if (invalidHandlers.Any())
			throw new ArgumentException(string.Format(
				CultureInfo.CurrentCulture,
				"The following event handlers to not implement exactly once the generic interface IEventHandler<TAggregateId, TEvent>: {0}.",
				 string.Join(", ", invalidHandlers.Select(handler => handler.HandlerType.FullName))),
				 "eventHandlers");

		this.eventStore = eventStore;
		this.asyncActionRunner = asyncActionRunner;
	}

	/// <summary>
	/// Publishes the pending changes in the given aggregate root, so that all subscribers are notified. 
	/// </summary>
	/// <param name="aggregate">The aggregate root which may contain pending changes.</param>
	/// <remarks>
	/// Also persists the aggregate changes to the event store.
	/// </remarks>
	public virtual void PublishChanges(AggregateRoot<TAggregateId, TBaseEvent> aggregate)
	{
		Guard.NotNull(() => aggregate, aggregate);

		var events = aggregate.GetChanges().ToList();

		// Events are persisted first of all.
		this.eventStore.SaveChanges(aggregate);

		foreach (var @event in events)
		{
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
				return new Tuple<List<Action<TAggregateId, TBaseEvent>>, List<Action<TAggregateId, TBaseEvent>>>(
					compatibleHandlers.Where(h => !h.Handler.IsAsync).Select(x => CreateInvoker(x)).ToList(),
					compatibleHandlers.Where(h => h.Handler.IsAsync).Select(x => CreateInvoker(x)).ToList());
			});

			foreach (var handler in pipeline.Item1.AsParallel())
			{
				handler.Invoke(aggregate.Id, @event);
			}

			// Run background handlers through the async runner.
			foreach (var handler in pipeline.Item2.AsParallel())
			{
				asyncActionRunner(() => handler.Invoke(aggregate.Id, @event));
			}
		}
	}

	/// <summary>
	/// Called when an event was published to the bus.
	/// </summary>
	/// <param name="aggregateId">The identifier of the aggregate root entity that published the event.</param>
	/// <param name="event">The published event.</param>
	protected virtual void OnPublished(TAggregateId aggregateId, TBaseEvent @event) { }

	// Caches a compiled delegate that invokes in a strong-typed fashion the 
	// underlying handler, casting the generic event type to the concrete 
	// type supported by the handler IEventHandler generic implementation.
	private Action<TAggregateId, TBaseEvent> CreateInvoker(HandlerDescriptor descriptor)
	{
		var idParam = Expression.Parameter(typeof(TAggregateId), "aggregateId");
		var eventParam = Expression.Parameter(typeof(TBaseEvent), "event");
		// (id, event) => handler.Handle(id, (TConcreteEvent)event);
		var invoker = Expression.Lambda<Action<TAggregateId, TBaseEvent>>(
			Expression.Call(
				Expression.Constant(descriptor.Handler), 
				descriptor.HandleMethod,
				idParam, 
				Expression.Convert(eventParam, descriptor.EventType)), 
			idParam, 
			eventParam);

		return invoker.Compile();
	}

	private class HandlerDescriptor
	{
		/// <summary>
		/// Gets or sets the type of event payload the handler can process, 
		/// retrieved from the handler TEvent generic parameter.
		/// </summary>
		public Type EventType { get; set; }

		/// <summary>
		/// Gets or sets the handle method that implements 
		/// the <see cref="IEventHandler{TAggregateId, TEvent}"/>.
		/// </summary>
		public MethodInfo HandleMethod { get; set; }

		/// <summary>
		/// Gets or sets the handler.
		/// </summary>
		public IEventHandler Handler { get; set; }
	}
}
