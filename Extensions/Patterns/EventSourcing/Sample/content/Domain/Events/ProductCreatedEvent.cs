using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Event raised when a new product is created.
/// </summary>
internal class ProductCreatedEvent : IDomainEvent
{
	public Guid Id { get; set; }
	public string Title { get; set; }
	public DateTimeOffset Timestamp { get; private set; }

	public override string ToString()
	{
		return "Created new product with Id={Id} and Title='{Title}'.".FormatWith(this);
	}
}