using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Fixes the generic event source/object ID type, and the 
/// domain events base class/interface.
/// </summary>
internal abstract class DomainObject : DomainObject<Guid, IDomainEvent>
{
}
