using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetFx.Patterns.EventSourcing.Tests
{
	public abstract class EventStoreBaseSpec
	{
		private IEventStore<Guid, DomainEvent> store;
		DateTimeOffset nowValue = DateTimeOffset.Now;
		private Func<DateTimeOffset> now;

		private Guid id1 = Guid.NewGuid();
		private Guid id2 = Guid.NewGuid();

		private class TestClock : IClock
		{
			public DateTime Now { get; set; }
			public DateTime UtcNow { get; set; }
		}

		public EventStoreBaseSpec()
		{
			this.now = () => nowValue;
			this.store = new MemoryStore<Guid, DomainEvent>(this.now);

			var product = new Product(id1, "DevStore");
			product.Publish(1);
			product.Publish(2);
			product.Publish(3);
			store.SaveChanges(product);

			product = new Product(id2, "WoVS");
			product.Publish(1);
			product.Publish(2);

			store.SaveChanges(product);
		}

		/// <summary>
		/// Derived test classes create the store on this method.
		/// </summary>
		/// <param name="clockFunc">The clock function, which can optionally be used by the store 
		/// implementation .</param>
		/// <returns></returns>
		protected abstract IEventStore<Guid, DomainEvent> CreateStore(Func<DateTimeOffset> clockFunc);

		[Fact]
		public void WhenLoadingFromEventsForSourceTypeAndId_ThenStateIsUpdated()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			Assert.Equal(id2, product.Id);
			Assert.Equal("WoVS", product.Title);
			Assert.Equal(2, product.Version);
		}

		[Fact]
		public void WhenFilteringBySourceTypeAndIdAndEventType_ThenSucceeds()
		{
			var events = store.Query().For<Product>(id1).OfType<Product.PublishedEvent>().Execute();

			Assert.Equal(3, events.Count());
			Assert.True(events.All(x => x is Product.PublishedEvent));
		}

		[Fact]
		public void WhenFilteringBySourceTypeAndEventType_ThenSucceeds()
		{
			var events = store.Query().For<Product>().OfType<Product.PublishedEvent>().Execute();

			Assert.Equal(5, events.Count());
			Assert.True(events.All(x => x is Product.PublishedEvent));
		}

		[Fact]
		public void WhenFilteringBySourceType_ThenSucceeds()
		{
			var events = store.Query().For<Product>().Execute();

			Assert.Equal(7, events.Count());
		}

		[Fact]
		public void WhenFilteringByEventType_ThenSucceeds()
		{
			var events = store.Query().OfType<Product.CreatedEvent>().Execute();

			Assert.Equal(2, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateSince_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.nowValue = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.nowValue = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.nowValue = when;
			product.Deactivate();
			store.SaveChanges(product);

			this.nowValue = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			var events = store.Query().OfType<Product.DeactivatedEvent>().Since(when).Execute();

			Assert.Equal(2, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateSinceExclusive_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.nowValue = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.nowValue = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.nowValue = when;
			product.Deactivate();
			store.SaveChanges(product);

			this.nowValue = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			var events = store.Query().OfType<Product.DeactivatedEvent>().Since(when).ExclusiveRange().Execute();

			Assert.Equal(1, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntil_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.nowValue = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.nowValue = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.nowValue = when;
			product.Deactivate();
			store.SaveChanges(product);

			this.nowValue = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			var events = store.Query().OfType<Product.DeactivatedEvent>().Until(when).Execute();

			Assert.Equal(3, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntilExclusive_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(5));

			this.nowValue = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7));
			product.Deactivate();
			store.SaveChanges(product);

			this.nowValue = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(6));
			product.Deactivate();
			store.SaveChanges(product);

			this.nowValue = when;
			product.Deactivate();
			store.SaveChanges(product);

			this.nowValue = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(4));
			product.Deactivate();
			store.SaveChanges(product);

			var events = store.Query().OfType<Product.DeactivatedEvent>().Until(when).ExclusiveRange().Execute();

			Assert.Equal(2, events.Count());
		}

		[Serializable]
		internal class DomainEvent : ITimestamped
		{
			protected DomainEvent()
			{
				this.Timestamp = DateTimeOffset.MinValue;
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

				this.Apply(new CreatedEvent { Id = id, Title = title });
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
				this.Apply(new PublishedEvent { Version = version });
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
				base.Apply(new DeactivatedEvent());
			}
		}
	}
}
