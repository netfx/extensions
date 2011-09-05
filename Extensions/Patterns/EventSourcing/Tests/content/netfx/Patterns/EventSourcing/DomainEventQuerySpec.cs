using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Collections;
using Moq;

namespace NetFx.Patterns.EventSourcing.Tests
{
	public class DomainEventQuerySpec
	{
		private MemoryEventStore<int, DomainEvent> store;
		private Func<DateTime> utcNow = () => DateTime.UtcNow;

		public DomainEventQuerySpec()
		{
			this.store = new MemoryEventStore<int, DomainEvent>(() => this.utcNow());

			var product = new Product(5, "DevStore");
			product.Publish(1);
			product.Publish(2);
			product.Publish(3);
			product.GetChanges().ToList()
				.ForEach(e => store.Persist(product, e));

			product = new Product(6, "WoVS");
			product.Publish(1);
			product.Publish(2);
			product.GetChanges().ToList()
				.ForEach(e => store.Persist(product, e));
		}

		[Fact]
		public void WhenGettingEmptyQueryEnumerable_ThenEmptyList()
		{
			var store = new MemoryEventStore<int, DomainEvent>(() => this.utcNow());
			var enumerable = store.Query() as IEnumerable;

			Assert.False(enumerable.GetEnumerator().MoveNext());
		}

		[Fact]
		public void WhenLoadingFromEventsForSourceTypeAndId_ThenStateIsUpdated()
		{
			var product = new Product();
			product.Load(store.Query().For<Product>(6));

			Assert.Equal(6, product.Id);
			Assert.Equal("WoVS", product.Title);
			Assert.Equal(2, product.Version);
		}

		[Fact]
		public void WhenQuerying_ThenCanAccessCriteria()
		{
			var criteria = store.Query().Criteria;

			Assert.NotNull(criteria);
		}

		[Fact]
		public void WhenFilteringBySourceTypeAndIdAndEventType_ThenSucceeds()
		{
			var events = store.Query().For<Product>(5).OfType<Product.PublishedEvent>();

			Assert.Equal(3, events.Count());
			Assert.True(events.All(x => x is Product.PublishedEvent));
		}

		[Fact]
		public void WhenFilteringBySourceTypeAndEventType_ThenSucceeds()
		{
			var events = store.Query().For<Product>().OfType<Product.PublishedEvent>();

			Assert.Equal(5, events.Count());
			Assert.True(events.All(x => x is Product.PublishedEvent));
		}

		[Fact]
		public void WhenFilteringBySourceType_ThenSucceeds()
		{
			var events = store.Query().For<Product>();

			Assert.Equal(7, events.Count());
		}

		[Fact]
		public void WhenFilteringByEventType_ThenSucceeds()
		{
			var events = store.Query().OfType<Product.CreatedEvent>();

			Assert.Equal(2, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateSince_ThenSucceeds()
		{
			var product = new Product();
			product.Load(store.Query().For<Product>(6));

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			store.Persist(product, new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			store.Persist(product, new DeactivatedEvent());

			this.utcNow = () => when;
			store.Persist(product, new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			store.Persist(product, new DeactivatedEvent());

			var events = store.Query().OfType<DeactivatedEvent>().Since(when);

			Assert.Equal(2, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateSinceExclusive_ThenSucceeds()
		{
			var product = new Product();
			product.Load(store.Query().For<Product>(6));

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			store.Persist(product, new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			store.Persist(product, new DeactivatedEvent());

			this.utcNow = () => when;
			store.Persist(product, new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			store.Persist(product, new DeactivatedEvent());

			var events = store.Query().OfType<DeactivatedEvent>().Since(when).ExclusiveRange();

			Assert.Equal(1, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntil_ThenSucceeds()
		{
			var product = new Product();
			product.Load(store.Query().For<Product>(6));

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			store.Persist(product, new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			store.Persist(product, new DeactivatedEvent());

			this.utcNow = () => when;
			store.Persist(product, new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			store.Persist(product, new DeactivatedEvent());

			var events = store.Query().OfType<DeactivatedEvent>().Until(when);

			Assert.Equal(3, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntilExclusive_ThenSucceeds()
		{
			var product = new Product();
			product.Load(store.Query().For<Product>(6));

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			store.Persist(product, new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			store.Persist(product, new DeactivatedEvent());

			this.utcNow = () => when;
			store.Persist(product, new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			store.Persist(product, new DeactivatedEvent());

			var events = store.Query().OfType<DeactivatedEvent>().Until(when).ExclusiveRange();

			Assert.Equal(2, events.Count());
		}

		public class DeactivatedEvent : DomainEvent
		{
			public override string ToString()
			{
				return this.GetType().Name;
			}
		}
	}
}
