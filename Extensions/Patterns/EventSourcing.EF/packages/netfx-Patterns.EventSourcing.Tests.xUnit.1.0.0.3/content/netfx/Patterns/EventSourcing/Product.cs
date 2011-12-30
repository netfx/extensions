using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetFx.Patterns.EventSourcing.Tests
{
	[Serializable]
	internal class DomainEvent : ITimestamped
	{
		protected DomainEvent()
		{
			this.Timestamp = DateTimeOffset.UtcNow;
		}

		public virtual DateTimeOffset Timestamp { get; protected set; }
	}

	internal abstract class DomainObject : DomainObject<Guid, DomainEvent> 
	{
		protected DomainObject() { }
	}

	/// <summary>
	/// Product is an the domain object sourcing the event with domain logic.
	/// </summary>
	internal class Product : DomainObject
	{
		/// <summary>
		/// Event raised when a new product is created.
		/// </summary>
		[Serializable]
		public class CreatedEvent : DomainEvent
		{
			public Guid Id { get; set; }
			public string Title { get; set; }

			public override string ToString()
			{
				return string.Format("Created new product with Id={0} and Title='{1}'.",
					this.Id, this.Title);
			}
		}

		/// <summary>
		/// Event raised when the product is deactivated.
		/// </summary>
		[Serializable]
		public class DeactivatedEvent : DomainEvent
		{
			public override string ToString()
			{
				return this.GetType().Name;
			}
		}

		/// <summary>
		/// Event raised when a new version of a product is published.
		/// </summary>
		[Serializable]
		public class PublishedEvent : DomainEvent
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
			// First thing an the domain object sourcing the event must do is 
			// setup which methods handle which events.
			// This helps avoid doing any unnecessary 
			// reflection invocation for events.
			this.Handles<CreatedEvent>(this.OnCreated);
			this.Handles<PublishedEvent>(this.OnPublished);
		}

		public Product(IEnumerable<DomainEvent> events)
			: this()
		{
			base.Load(events);
		}

		/// <summary>
		/// Initializes a product and shows how even the 
		/// constructor parameters are processed as an event.
		/// </summary>
		public Product(Guid id, string title)
			// Calling this is essential as it configures the 
			// internal event handler map.
			: this()
		{
			// Showcases that validation is the only thing that happens in domain 
			// public methods (even the constructor).
			if (id == Guid.Empty)
				throw new ArgumentException("id");
			if (string.IsNullOrEmpty(title))
				throw new ArgumentException("title");

			this.Raise(new CreatedEvent { Id = id, Title = title });
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
			this.Raise(new PublishedEvent { Version = version });
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

		public void Deactivate()
		{
			base.Raise(new DeactivatedEvent());
		}
	}
}