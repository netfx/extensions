using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Custom base domain event handler class, that showcases how 
/// you typically customize the base handler class to have a 
/// fixed aggregate root id type which is the same for all 
/// aggregate roots in a domain.
/// </summary>
public abstract class DomainEventHandler<TEventArgs> : DomainEventHandler<int, TEventArgs>
	where TEventArgs : TimestampedEventArgs
{
}
