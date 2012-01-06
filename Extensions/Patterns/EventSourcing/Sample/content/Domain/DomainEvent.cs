using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Base class for our domain events. Here we can add 
/// default event behavior.
/// </summary>
internal class DomainEvent : EventArgs
{
	public DomainEvent()
	{
		this.Timestamp = DateTimeOffset.Now;
	}

	public DateTimeOffset Timestamp { get; private set; }
}
