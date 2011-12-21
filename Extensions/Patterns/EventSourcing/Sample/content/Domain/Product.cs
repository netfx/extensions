using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Product is an the domain object sourcing the event with domain logic.
/// </summary>
internal class Product : AggregateRoot
{
	/// <summary>
	/// Initializes the internal event handler map.
	/// </summary>
	public Product()
	{
		// First thing the domain object sourcing the event must do is 
		// setup which methods handle which events.
		// This helps avoid doing any unnecessary 
		// reflection invocation for events.
		this.Handles<ProductCreatedEvent>(this.OnCreated);
		this.Handles<ProductPublishedEvent>(this.OnPublished);
	}

	/// <summary>
	/// Initializes a product and shows how even the 
	/// constructor parameters are processed as an event.
	/// </summary>
	public Product(Guid id, string title)
		// Calling this is essential as it configures the 
		// internal event handler map. Could be a separate
		// Initialize() method called in the body instead.
		: this()
	{
		// Showcases that validation is the only thing that happens in domain 
		// public methods (even the constructor).
		if (id == Guid.Empty)
			throw new ArgumentException("id");
		if (string.IsNullOrEmpty(title))
			throw new ArgumentException("title");

		this.Raise(new ProductCreatedEvent { Id = id, Title = title });
	}

	// Technically, these members wouldn't even need a public setter 
	// at all, but an ORM would need it.
	public string Title { get; private set; }
	public int Version { get; private set; }

	public void Publish(int version)
	{
		// Again, the method only does parameter and possibly state validation.
		if (version <= 0)
			throw new ArgumentException("Can't publish a negative version.");

		// When we're ready to apply state changes, we 
		// apply them through an event that calls back 
		// the OnCreated method as mapped in the ctor.
		this.Raise(new ProductPublishedEvent { Version = version });
	}

	private void OnCreated(ProductCreatedEvent @event)
	{
		this.Id = @event.Id;
		this.Title = @event.Title;
	}

	private void OnPublished(ProductPublishedEvent @event)
	{
		this.Version = @event.Version;
	}
}
