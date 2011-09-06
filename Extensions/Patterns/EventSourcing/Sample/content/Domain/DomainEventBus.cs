using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Fixes the generic bus event source/aggregate ID type, and the 
/// domain events base class/interface.
/// </summary>
internal class DomainEventBus : EventBus<int, DomainEvent>, IDomainEventBus
{
	public DomainEventBus(IEnumerable<IEventHandler> handlers)
		: base(handlers)
	{
	}
}