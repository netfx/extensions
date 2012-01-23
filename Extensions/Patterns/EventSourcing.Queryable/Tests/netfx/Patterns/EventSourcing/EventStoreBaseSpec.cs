using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NetFx.Patterns.EventSourcing.Tests
{
	/// <summary>
	/// Specific event stores can inherit this base class 
	/// to leverage a set of compliance tests. 
	/// </summary>
	public abstract partial class EventStoreBaseSpec
	{
		private IEventStore<Guid, DomainEvent> store;

		private TestClock clock = new TestClock();
		private Guid id1 = Guid.NewGuid();
		private Guid id2 = Guid.NewGuid();

		// If the IClock interface is not defined in the referenced projects,
		// you must install the netfx-System.Clock nuget to get this to compile.
		[Serializable]
		private class TestClock : IClock
		{
			public TestClock()
			{
				this.Now = DateTimeOffset.Now;
				this.UtcNow = DateTimeOffset.UtcNow;
			}

			public DateTimeOffset Now { get; set; }
			public DateTimeOffset UtcNow { get; set; }
		}

		public EventStoreBaseSpec()
		{
			this.store = (IEventStore<Guid, DomainEvent>)CreateStore();

			// Override the ambient singleton value for tests.
			SystemClock.Instance = this.clock;

			var product = new Product(id1, "DevStore");
			product.Publish(1);
			product.Publish(2);
			product.Publish(3);
			store.Persist(product);

			product = new Product(id2, "WoVS");
			product.Publish(1);
			product.Publish(2);

			store.Persist(product);
		}

		/// <summary>
		/// Derived test classes create the store on this method. Made 
		/// object so the class doesn't have to be public. Must return 
		/// an <c>IEventStore{Guid, DomainEvent}</c> implementation.
		/// </summary>
		protected internal abstract object CreateStore();

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

			this.clock.Now = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.clock.Now = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.clock.Now = when;
			product.Deactivate();
			store.Persist(product);

			this.clock.Now = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			var events = store.Query().OfType<Product.DeactivatedEvent>().Since(when).Execute();

			Assert.Equal(2, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateSinceExclusive_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.clock.Now = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.clock.Now = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.clock.Now = when;
			product.Deactivate();
			store.Persist(product);

			this.clock.Now = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			var events = store.Query().OfType<Product.DeactivatedEvent>().Since(when).ExclusiveRange().Execute();

			Assert.Equal(1, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntil_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.clock.Now = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.clock.Now = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.clock.Now = when;
			product.Deactivate();
			store.Persist(product);

			this.clock.Now = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			var events = store.Query().OfType<Product.DeactivatedEvent>().Until(when).Execute();

			Assert.Equal(3, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntilExclusive_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(5));

			this.clock.Now = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7));
			product.Deactivate();
			store.Persist(product);

			this.clock.Now = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(6));
			product.Deactivate();
			store.Persist(product);

			this.clock.Now = when;
			product.Deactivate();
			store.Persist(product);

			this.clock.Now = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(4));
			product.Deactivate();
			store.Persist(product);

			var events = store.Query().OfType<Product.DeactivatedEvent>().Until(when).ExclusiveRange().Execute();

			Assert.Equal(2, events.Count());
		}
	}
}
