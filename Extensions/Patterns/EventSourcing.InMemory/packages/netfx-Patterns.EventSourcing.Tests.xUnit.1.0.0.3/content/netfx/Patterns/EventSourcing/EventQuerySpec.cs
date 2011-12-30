using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Collections;
using Moq;

namespace NetFx.Patterns.EventSourcing.Tests
{
	public class EventQuerySpec
	{
		private MemoryEventStore<Guid, DomainEvent> store;
		private Func<DateTime> utcNow = () => DateTime.UtcNow;

		private Guid id1 = Guid.NewGuid();
		private Guid id2 = Guid.NewGuid();

		public EventQuerySpec()
		{
			this.store = new MemoryEventStore<Guid, DomainEvent>(() => this.utcNow());

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

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.utcNow = () => when;
			product.Deactivate();
			store.SaveChanges(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			var events = store.Query().OfType<Product.DeactivatedEvent>().Since(when).Execute();

			Assert.Equal(2, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateSinceExclusive_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.utcNow = () => when;
			product.Deactivate();
			store.SaveChanges(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			var events = store.Query().OfType<Product.DeactivatedEvent>().Since(when).ExclusiveRange().Execute();

			Assert.Equal(1, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntil_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.utcNow = () => when;
			product.Deactivate();
			store.SaveChanges(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			var events = store.Query().OfType<Product.DeactivatedEvent>().Until(when).Execute();

			Assert.Equal(3, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntilExclusive_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			this.utcNow = () => when;
			product.Deactivate();
			store.SaveChanges(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.SaveChanges(product);

			var events = store.Query().OfType<Product.DeactivatedEvent>().Until(when).ExclusiveRange().Execute();

			Assert.Equal(2, events.Count());
		}
	}
}
