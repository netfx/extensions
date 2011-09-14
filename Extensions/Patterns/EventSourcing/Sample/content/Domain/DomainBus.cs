using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// The first you typically do in a project is inherit from the generic EventSourcing 
// APIs and provide derived interfaces that specify the generic types for this domain.
// This simplifies the other components that will consume our APIs so that they 
// don't have to specify multiple generic parameters everywhere.

internal interface IDomainEventBus : IEventBus<Guid, IDomainEvent> { }
internal interface IDomainEventStore : IEventStore<Guid, IDomainEvent> { }
internal abstract class DomainEventHandler<TEvent> : EventHandler<Guid, TEvent> where TEvent : IDomainEvent { }

internal class DomainEventBus : EventBus<Guid, IDomainEvent>, IDomainEventBus
{
	public DomainEventBus(IEventStore<Guid, IDomainEvent> store, IEnumerable<IEventHandler> handlers)
		: base(store, handlers)
	{
	}
}

