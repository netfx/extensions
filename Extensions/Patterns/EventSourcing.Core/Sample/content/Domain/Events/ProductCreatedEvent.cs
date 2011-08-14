using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Event raised when a new product is created.
/// </summary>
public class ProductCreatedEvent : DomainEvent
{
	public int Id { get; set; }
	public string Title { get; set; }

	public override string ToString()
	{
		return "Created new product with Id={Id} and Title='{Title}'.".FormatWith(this);
	}
}