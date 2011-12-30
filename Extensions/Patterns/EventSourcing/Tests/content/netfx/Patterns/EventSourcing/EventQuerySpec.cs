using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Collections;
using Moq;
using System.Reflection;

namespace NetFx.Patterns.EventSourcing.Tests
{
	public class EventQuerySpec
	{
		private Guid id1 = Guid.NewGuid();
		private Guid id2 = Guid.NewGuid();
		private Func<EventQueryExtension.IEventQuery<Guid, DomainEvent>, EventQueryCriteria<Guid>> getCriteria =
			query => (EventQueryCriteria<Guid>)query.GetType().GetField("criteria", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(query);

		[Fact]
		public void WhenQuerying_ThenReturnsQueryObject()
		{
			var store = Mock.Of<IEventStore<Guid, DomainEvent>>();

			var query = store.Query();

			Assert.NotNull(query);
		}

		[Fact]
		public void WhenQueryingForObjectType_ThenAddsToCriteria()
		{
			var store = Mock.Of<IEventStore<Guid, DomainEvent>>();

			var criteria = getCriteria(store.Query().For<Product>());

			Assert.Equal(1, criteria.ObjectTypes.Count);
			Assert.Equal(typeof(Product), criteria.ObjectTypes[0]);
		}

		[Fact]
		public void WhenQueryingForObjectTypeAndId_ThenAddsToCriteria()
		{
			var store = Mock.Of<IEventStore<Guid, DomainEvent>>();

			var criteria = getCriteria(store.Query().For<Product>(id1));

			Assert.Equal(1, criteria.ObjectInstances.Count);
			Assert.Equal(typeof(Product), criteria.ObjectInstances[0].ObjectType);
			Assert.Equal(id1, criteria.ObjectInstances[0].ObjectId);
		}

		[Fact]
		public void WhenQueryingForEventType_ThenAddsToCriteria()
		{
			var store = Mock.Of<IEventStore<Guid, DomainEvent>>();

			var criteria = getCriteria(store.Query().OfType<Product.PublishedEvent>());

			Assert.Equal(1, criteria.EventTypes.Count);
			Assert.Equal(typeof(Product.PublishedEvent), criteria.EventTypes[0]);
		}

		[Fact]
		public void WhenQueryingForObjectTypeIdAndEventType_ThenAggregatesCriteria()
		{
			var store = Mock.Of<IEventStore<Guid, DomainEvent>>();

			var criteria = getCriteria(store.Query().For<Product>(id1).OfType<Product.PublishedEvent>());

			Assert.Equal(1, criteria.ObjectInstances.Count);
			Assert.Equal(typeof(Product), criteria.ObjectInstances[0].ObjectType);
			Assert.Equal(id1, criteria.ObjectInstances[0].ObjectId);
			Assert.Equal(1, criteria.EventTypes.Count);
			Assert.Equal(typeof(Product.PublishedEvent), criteria.EventTypes[0]);
		}

		[Fact]
		public void WhenQueryingByDateSince_ThenAddsCriteria()
		{
			var when = DateTime.Today.ToUniversalTime();
			var store = Mock.Of<IEventStore<Guid, DomainEvent>>();

			var criteria = getCriteria(store.Query().Since(when));

			Assert.Equal(when, criteria.Since);
		}

		[Fact]
		public void WhenQueryingByDateSince_ThenDefaultsToNonExclusiveRange()
		{
			var when = DateTime.Today.ToUniversalTime();
			var store = Mock.Of<IEventStore<Guid, DomainEvent>>();

			var criteria = getCriteria(store.Query().Since(when));

			Assert.False(criteria.IsExclusiveRange);
		}

		[Fact]
		public void WhenQueryingByDateUntil_ThenAddsCriteria()
		{
			var when = DateTime.Today.ToUniversalTime();
			var store = Mock.Of<IEventStore<Guid, DomainEvent>>();

			var criteria = getCriteria(store.Query().Until(when));

			Assert.Equal(when, criteria.Until);
		}

		[Fact]
		public void WhenQueryingByDateUntil_ThenDefaultsToNonExclusiveRange()
		{
			var when = DateTime.Today.ToUniversalTime();
			var store = Mock.Of<IEventStore<Guid, DomainEvent>>();

			var criteria = getCriteria(store.Query().Until(when));

			Assert.False(criteria.IsExclusiveRange);
		}

		[Fact]
		public void WhenSettingExclusiveRange_ThenUpdatesCriteria()
		{
			var when = DateTime.Today.ToUniversalTime();
			var store = Mock.Of<IEventStore<Guid, DomainEvent>>();

			var criteria = getCriteria(store.Query().ExclusiveRange());

			Assert.True(criteria.IsExclusiveRange);
		}

		[Fact]
		public void WhenExecutingQuery_ThenPassesCriteriaToStore()
		{
			var when = DateTime.Today.ToUniversalTime();
			var store = Mock.Of<IEventStore<Guid, DomainEvent>>();

			var query = store.Query();
			var criteria = getCriteria(query);

			query.Execute();

			Mock.Get(store).Verify(x => x.Query(criteria));
		}
	}
}
