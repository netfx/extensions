using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Fixes the generic bus event source/aggregate ID type, and the 
/// domain events base class/interface.
/// </summary>
internal interface IDomainEventBus : IDomainEventBus<int, DomainEvent>
{
}