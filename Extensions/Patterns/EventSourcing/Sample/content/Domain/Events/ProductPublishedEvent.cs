using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Event raised when a new version of a product is published.
/// </summary>
internal class ProductPublishedEvent : IDomainEvent
{
	public int Version { get; set; }
	public DateTimeOffset Timestamp { get; private set; }

	public override string ToString()
	{
		return "Published new product version {Version}.".FormatWith(this);
	}
}