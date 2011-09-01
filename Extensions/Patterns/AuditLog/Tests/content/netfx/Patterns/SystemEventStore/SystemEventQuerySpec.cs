using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Collections;
using Moq;

namespace NetFx.Patterns.SystemEventStore.Tests
{
	public class SystemEventQuerySpec
	{
		private MemoryEventStore<SystemEvent> store;
		private Func<DateTime> utcNow = () => DateTime.UtcNow;

		public SystemEventQuerySpec()
		{
			this.store = new MemoryEventStore<SystemEvent>(() => this.utcNow());

			this.store.Persist(new ProductCreatedEvent { Id = 5, Title = "DevStore" });
			this.store.Persist(new ProductPublishedEvent { Version = 1 });
			this.store.Persist(new ProductPublishedEvent { Version = 2 });
			this.store.Persist(new ProductPublishedEvent { Version = 3 });

			this.store.Persist(new ProductCreatedEvent { Id = 6, Title = "WoVS" });
			this.store.Persist(new ProductPublishedEvent { Version = 1 });
			this.store.Persist(new ProductPublishedEvent { Version = 2 });
		}

		[Fact]
		public void WhenGettingEmptyQueryEnumerable_ThenEmptyList()
		{
			var store = new MemoryEventStore<SystemEvent>(() => this.utcNow());
			var enumerable = store.Query() as IEnumerable;

			Assert.False(enumerable.GetEnumerator().MoveNext());
		}

		[Fact]
		public void WhenFilteringByEventType_ThenSucceeds()
		{
			var events = store.Query().OfType<ProductCreatedEvent>();

			Assert.Equal(2, events.Count());
		}

		[Fact]
		public void WhenFilteringByEventType_ThenCanAccessCriteria()
		{
			var events = store.Query().OfType<ProductCreatedEvent>();

			Assert.Equal(1, events.Criteria.EventTypes.Count);
		}

		[Fact]
		public void WhenFilteringByDateSince_ThenSucceeds()
		{
			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			store.Persist(new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			store.Persist(new DeactivatedEvent());

			this.utcNow = () => when;
			store.Persist(new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			store.Persist(new DeactivatedEvent());

			var events = store.Query().OfType<DeactivatedEvent>().Since(when);

			Assert.Equal(2, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateSinceExclusive_ThenSucceeds()
		{
			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			store.Persist(new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			store.Persist(new DeactivatedEvent());

			this.utcNow = () => when;
			store.Persist(new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			store.Persist(new DeactivatedEvent());

			var events = store.Query().OfType<DeactivatedEvent>().Since(when).ExclusiveRange();

			Assert.Equal(1, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntil_ThenSucceeds()
		{
			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			store.Persist(new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			store.Persist(new DeactivatedEvent());

			this.utcNow = () => when;
			store.Persist(new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			store.Persist(new DeactivatedEvent());

			var events = store.Query().OfType<DeactivatedEvent>().Until(when);

			Assert.Equal(3, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntilExclusive_ThenSucceeds()
		{
			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			store.Persist(new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			store.Persist(new DeactivatedEvent());

			this.utcNow = () => when;
			store.Persist(new DeactivatedEvent());

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			store.Persist(new DeactivatedEvent());

			var events = store.Query().OfType<DeactivatedEvent>().Until(when).ExclusiveRange();

			Assert.Equal(2, events.Count());
		}

		public class DeactivatedEvent : SystemEvent
		{
			public override string ToString()
			{
				return this.GetType().Name;
			}
		}
	}
}
