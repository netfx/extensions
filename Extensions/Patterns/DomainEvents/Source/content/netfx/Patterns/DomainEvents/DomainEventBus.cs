using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

/// <summary>
/// Default implementation of an <see cref="IDomainEventBus"/> that 
/// invokes handlers as events are published.
/// <para>
/// Handlers with <see cref="DomainEventHandler.IsAsync"/> set to 
/// <see langword="true"/> are invoked through the optional 
/// async runner delegate passed to the constructor.
/// </para>
/// </summary>
/// <nuget id="netfx-Patterns.DomainEvents" />
partial class DomainEventBus : IDomainEventBus
{
	private Action<Action> asyncActionRunner;
	private IEnumerable<DomainEventHandler> eventHandlers;

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
	public static IDomainEventBus None { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainEventBus"/> class with 
	/// the default async runner that enqueues work in the <see cref="ThreadPool"/>.
	/// </summary>
	/// <param name="eventHandlers">The event handlers.</param>
	public DomainEventBus(IEnumerable<DomainEventHandler> eventHandlers)
		: this(eventHandlers, action => ThreadPool.QueueUserWorkItem(state => action()))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainEventBus"/> class with 
	/// the given async runner.
	/// </summary>
	/// <param name="eventHandlers">The event handlers.</param>
	/// <param name="asyncActionRunner">The async action runner to use to invoke event handlers 
	/// that have <see cref="DomainEventHandler.IsAsync"/> set to <see langword="true"/>.</param>
	public DomainEventBus(IEnumerable<DomainEventHandler> eventHandlers, Action<Action> asyncActionRunner)
	{
		Guard.NotNull(() => eventHandlers, eventHandlers);
		Guard.NotNull(() => asyncActionRunner, asyncActionRunner);

		if (eventHandlers.Any(eh =>
			eh == null ||
			eh.EventType == null ||
			!InheritsFromGenericHandler(eh.GetType())))
			throw new ArgumentException("eventHandlers");

		this.eventHandlers = eventHandlers.Where(eh => eh != null && eh.EventType != null).ToList();
		this.asyncActionRunner = asyncActionRunner;
	}

	/// <summary>
	/// Publishes the specified event to the bus so that all subscribers are notified.
	/// </summary>
	/// <param name="event">The event payload.</param>
	public void Publish(DomainEvent @event)
	{
		Guard.NotNull(() => @event, @event);

		var compatibleHandlers = this.eventHandlers.Where(h => h.EventType.IsAssignableFrom(@event.GetType())).ToList();
		dynamic dynamicEvent = @event;

		// By making this dynamic, we allow event handlers to subscribe to base classes
		foreach (dynamic handler in compatibleHandlers.Where(h => !h.IsAsync).AsParallel())
		{
			handler.Handle(dynamicEvent);
		}

		// Run background handlers through the async runner.
		foreach (dynamic handler in compatibleHandlers.Where(h => h.IsAsync).AsParallel())
		{
			asyncActionRunner(() => handler.Handle(@event));
		}
	}

	private bool InheritsFromGenericHandler(Type type)
	{
		var baseType = type.BaseType;
		while (baseType != typeof(object))
		{
			if (baseType.IsGenericType &&
				baseType.GetGenericTypeDefinition() == typeof(DomainEventHandler<>))
				return true;

			baseType = baseType.BaseType;
		}

		return false;
	}

	/// <summary>
	/// Provides a null <see cref="IDomainEventBus"/> implementation 
	/// for use when no events have been configured.
	/// </summary>
	private class NullBus : IDomainEventBus
	{
		/// <summary>
		/// Does nothing.
		/// </summary>
		public void Publish(DomainEvent @event)
		{
		}
	}
}
