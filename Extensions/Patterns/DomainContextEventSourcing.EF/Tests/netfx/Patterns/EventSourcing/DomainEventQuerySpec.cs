using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NetFx.Patterns.EventSourcing.Core.Tests
{
	/// <nuget id="netfx-Patterns.EventSourcing.Core.Tests" />
	public class DomainEventQuerySpec
	{
		private IDomainEventStore<int> store = new MemoryEventStore<int>();

		public DomainEventQuerySpec()
		{
			var product = new Product(5, "DevStore");
			product.Publish(1);
			product.Publish(2);
			product.Publish(3);
			product.GetChanges().ToList()
				.ForEach(e => store.Save(product, e));

			product = new Product(6, "WoVS");
			product.Publish(1);
			product.Publish(2);
			product.GetChanges().ToList()
				.ForEach(e => store.Save(product, e));
		}

		[Fact]
		public void WhenLoadingFromEventsForAggregateAndId_ThenStateIsUpdated()
		{
			var product = new Product();
			product.Load(store.Query().For<Product>(6));

			Assert.Equal(6, product.Id);
			Assert.Equal("WoVS", product.Title);
			Assert.Equal(2, product.Version);
		}

		[Fact]
		public void WhenFilteringByAggregateTypeAndIdAndEventType_ThenSucceeds()
		{
			var events = store.Query().For<Product>(5).OfType<Product.PublishedEvent>();

			Assert.Equal(3, events.Count());
			Assert.True(events.All(x => x is Product.PublishedEvent));
		}

		[Fact]
		public void WhenFilteringByAggregateTypeAndEventType_ThenSucceeds()
		{
			var events = store.Query().For<Product>().OfType<Product.PublishedEvent>();

			Assert.Equal(5, events.Count());
			Assert.True(events.All(x => x is Product.PublishedEvent));
		}

		[Fact]
		public void WhenFilteringByAggregateType_ThenSucceeds()
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

			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(7))));
			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(6))));
			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(5))));
			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(4))));

			var events = store.Query().OfType<DeactivatedEvent>().Since(DateTime.Today.Subtract(TimeSpan.FromDays(5)));

			Assert.Equal(2, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateSinceExclusive_ThenSucceeds()
		{
			var product = new Product();
			product.Load(store.Query().For<Product>(6));

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5));

			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(7))));
			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(6))));
			store.Save(product, new DeactivatedEvent(when));
			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(4))));

			var events = store.Query().OfType<DeactivatedEvent>().Since(when).ExclusiveDateRange();

			Assert.Equal(1, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntil_ThenSucceeds()
		{
			var product = new Product();
			product.Load(store.Query().For<Product>(6));

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5));

			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(7))));
			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(6))));
			store.Save(product, new DeactivatedEvent(when));
			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(4))));

			var events = store.Query().OfType<DeactivatedEvent>().Until(when);

			Assert.Equal(3, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntilExclusive_ThenSucceeds()
		{
			var product = new Product();
			product.Load(store.Query().For<Product>(6));

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5));

			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(7))));
			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(6))));
			store.Save(product, new DeactivatedEvent(when));
			store.Save(product, new DeactivatedEvent(DateTime.Today.Subtract(TimeSpan.FromDays(4))));

			var events = store.Query().OfType<DeactivatedEvent>().Until(when).ExclusiveDateRange();

			Assert.Equal(2, events.Count());
		}

		public class DeactivatedEvent : TimestampedEventArgs
		{
			public DeactivatedEvent(DateTime when)
				: base(when)
			{
			}

			public override string ToString()
			{
				return this.GetType().Name;
			}
		}
	}
}
