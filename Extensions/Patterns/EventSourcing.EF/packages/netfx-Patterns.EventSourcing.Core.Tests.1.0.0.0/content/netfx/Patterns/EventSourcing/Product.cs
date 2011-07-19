using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Product is an aggregate root with domain logic.
/// </summary>
public class Product : AggregateRoot<int>
{
	/// <summary>
	/// Event raised when a new product is created.
	/// </summary>
	public class CreatedEvent : TimestampedEventArgs
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
	public class PublishedEvent : TimestampedEventArgs
	{
		public int Version { get; set; }

		public override string ToString()
		{
			return "Published new product version " + this.Version + ".";
		}
	}

	/// <summary>
	/// Initializes the internal event handler map.
	/// </summary>
	public Product()
	{
		// First thing an aggregate root must do is 
		// setup which methods handle which events.
		// This helps avoid doing any unnecessary 
		// reflection invocation for events.
		this.Handles<CreatedEvent>(this.OnCreated);
		this.Handles<PublishedEvent>(this.OnPublished);
	}

	/// <summary>
	/// Initializes a product and shows how even the 
	/// constructor parameters are processed as an event.
	/// </summary>
	public Product(int id, string title)
		// Calling this is essential as it configures the 
		// internal event handler map.
		: this()
	{
		// Showcases that validation is the only thing that happens in domain 
		// public methods (even the constructor).
		if (id < 0)
			throw new ArgumentException("id");
		if (string.IsNullOrEmpty(title))
			throw new ArgumentException("title");

		this.ApplyChange(new CreatedEvent { Id = id, Title = title });
	}

	// Technically, these members wouldn't even need a public setter 
	// at all, but an ORM would need it.
	public string Title { get; set; }
	public int Version { get; set; }

	public void Publish(int version)
	{
		// Again, the method only does parameter and possibly state validation.
		if (version <= 0)
			throw new ArgumentException();

		// When we're ready to apply state changes, we 
		// apply them through an event that calls back 
		// the OnCreated method as mapped in the ctor.
		this.ApplyChange(new PublishedEvent { Version = version });
	}

	private void OnCreated(CreatedEvent @event)
	{
		this.Id = @event.Id;
		this.Title = @event.Title;
	}

	private void OnPublished(PublishedEvent @event)
	{
		this.Version = @event.Version;
	}
}
