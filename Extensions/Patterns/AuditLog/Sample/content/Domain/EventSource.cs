using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Fixes the generic event source/aggregate ID type, and the 
/// domain events base class/interface.
/// </summary>
public abstract class AggregateRoot : AggregateRoot<int, DomainEvent>
{
}
