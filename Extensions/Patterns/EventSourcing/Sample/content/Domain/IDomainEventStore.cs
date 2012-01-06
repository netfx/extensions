using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;

// The first thing you typically do in a project is inherit from the generic EventSourcing 
// APIs and provide derived interfaces that specify the generic types for this domain.
// This simplifies the other components that will consume our APIs so that they 
// don't have to specify multiple generic parameters everywhere.

internal interface IDomainEventStore : IEventStore<Guid, DomainEvent> { }