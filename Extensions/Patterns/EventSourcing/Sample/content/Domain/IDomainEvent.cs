using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Base class for our domain events.
/// </summary>
internal interface IDomainEvent : ITimestamped
{
}
