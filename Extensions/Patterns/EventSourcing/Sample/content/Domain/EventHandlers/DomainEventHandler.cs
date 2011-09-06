using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Custom base domain event handler class, that showcases how 
/// you typically customize the base handler class to have a 
/// fixed the domain object sourcing the event id type which is the same for all 
/// the domain object sourcing the event in a domain.
/// </summary>
internal abstract class DomainEventHandler<TEvent> : EventHandler<int, TEvent>
	where TEvent : DomainEvent
{
}
