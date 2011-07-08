using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Base class for aggregate roots that use events to apply state 
/// changes and notify consumers on an <see cref="IDomainEventBus"/>.
/// </summary>
/// <typeparam name="TId">The type of identifier used by the aggregate root.</typeparam>
partial class AggregateRoot<TId>
{
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
		foreach (var e in history) 
			ApplyChange(e, false);
	}

	/// <summary>
	/// Applies a change to the entity state via an event. 
	/// The derived class should provide a method called <c>Apply</c> 
	/// receiving the concrete type of event, where state 
	/// changes are performed to the entity.
	/// </summary>
	protected void ApplyChange(DomainEvent @event)
	{
		ApplyChange(@event, true);
	}

	private void ApplyChange(DomainEvent @event, bool isNew)
	{
		((dynamic)this).Apply(@event);

		if (isNew) 
			this.changes.Add(@event);
	}

	/// <summary>
	/// Default handler which does nothing and 
	/// satisfies the dynamic invocation that can 
	/// be overriden in the derived aggregate by 
	/// simply providing a method of the same 
	/// name and different concrete event 
	/// signature.
	/// </summary>
	/// <param name="event">The event to apply.</param>
	private void Apply(DomainEvent @event)
	{
		// Default handler which does nothing and 
		// satisfies the dynamic invocation.
	}
}
