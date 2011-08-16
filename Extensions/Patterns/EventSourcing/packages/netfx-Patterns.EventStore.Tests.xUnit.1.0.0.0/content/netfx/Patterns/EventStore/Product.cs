using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetFx.Patterns.EventStore.Tests
{
	public abstract class DomainEvent { }

	/// <summary>
	/// Event raised when a new product is created.
	/// </summary>
	public class ProductCreatedEvent : DomainEvent
	{
		public int Id { get; set; }
		public string Title { get; set; }

		public override string ToString()
		{
			return string.Format("Created new product with Id={0} and Title='{1}'.",
				this.Id, this.Title);
		}
	}

	/// <summary>
	/// Event raised when a new version of a product is published.
	/// </summary>
	public class ProductPublishedEvent : DomainEvent
	{
		public int Version { get; set; }

		public override string ToString()
		{
			return "Published new product version " + this.Version + ".";
		}
	}
}