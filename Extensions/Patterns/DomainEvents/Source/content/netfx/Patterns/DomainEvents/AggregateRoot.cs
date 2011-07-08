﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

/// <summary>
/// Base class for aggregate roots that use events to apply state 
/// changes and notify consumers on an <see cref="IDomainEventBus"/>.
/// </summary>
/// <typeparam name="TId">The type of identifier used by the aggregate root.</typeparam>
partial class AggregateRoot<TId>
{
	/// <summary>
	/// This cache is the only bit of complexity in the whole thing, and it's 
	/// basically an optimization to avoid dynamic and private reflection to 
	/// happen at runtime. This cache contains a generated lambda to apply 
	/// events from history, and is only built if you ever use the <see cref="LoadFrom"/> 
	/// method. Otherwise, it has no impact whatesoever in your entities performance, 
	/// as the cache is never eagerly built.
	/// </summary>
	private static Dictionary<Type, Dictionary<Type, Delegate>> appliersCache = new Dictionary<Type, Dictionary<Type, Delegate>>();

	private List<DomainEvent> changes = new List<DomainEvent>();

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
	public IEnumerable<DomainEvent> GetChanges()
	{
		return this.changes.AsReadOnly();
	}

	/// <summary>
	/// Loads the aggregate root state from an even stream.
	/// </summary>
	public void LoadFrom(IEnumerable<DomainEvent> history)
	{
		EnsureApplyReflectionCache();

		var appliers = appliersCache[this.GetType()];

		foreach (var e in history)
		{
			Delegate applier = null;
			if (appliers.TryGetValue(e.GetType(), out applier))
			{
				// This is the ony bit of dynamic reflection 
				// invocation performed, and it's very fast.
				applier.DynamicInvoke(e);
			}
		}
	}

	/// <summary>
	/// Applies a change to the entity state via an event. 
	/// The derived class should provide a method called <c>Apply</c> 
	/// receiving the concrete type of event, where state 
	/// changes are performed to the entity.
	/// </summary>
	protected void ApplyChange<TEvent>(TEvent @event, Action<TEvent> apply)
		where TEvent : DomainEvent
	{
		ApplyChangeImpl(@event, apply, true);
	}

	private void ApplyChangeImpl<TEvent>(TEvent @event, Action<TEvent> apply, bool isNew)
		where TEvent : DomainEvent
	{
		apply.Invoke(@event);

		if (isNew) 
			this.changes.Add(@event);
	}

	/// <summary>
	/// Builds the reflection-based cache of Apply-style methods 
	/// for the current instance if needed.
	/// </summary>
	private void EnsureApplyReflectionCache()
	{
		// Cache handlers for speed.
		if (!appliersCache.ContainsKey(this.GetType()))
		{
			var applyChangeImpl = ((Action<DomainEvent, Action<DomainEvent>, bool>)this.ApplyChangeImpl).Method.GetGenericMethodDefinition();

			// Cache the Apply methods for this type.
			var cache = new Dictionary<Type, Delegate>();
			foreach (var method in this.GetType()
				.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
				.Where(m =>
					m.ReturnType == typeof(void) &&
					m.GetParameters().Length == 1 &&
						// All methods that have a void return type and a single parameter 
						// that inherits from DomainEvent are considered event appliers.
					typeof(DomainEvent).IsAssignableFrom(m.GetParameters()[0].ParameterType)))
			{
				var eventType = method.GetParameters()[0].ParameterType;
				var applyActionType = typeof(Action<>).MakeGenericType(eventType);

				var param = Expression.Parameter(eventType, "event");

				// Invokes: this.Apply(event);
				var applier = Expression.Lambda(applyActionType,
					Expression.Call(Expression.Constant(this), method, param), param).Compile();

				// Invokes: this.ApplyChange<TEvent>(event, event => this.Apply(event), false);
				var changeApplier = Expression.Lambda(applyActionType,
						Expression.Call(
							Expression.Constant(this),
							applyChangeImpl.MakeGenericMethod(eventType),
							param, Expression.Constant(applier), Expression.Constant(false)),
						param)
					.Compile();

				cache.Add(param.Type, changeApplier);
			}

			appliersCache.Add(this.GetType(), cache);
		}
	}
}
