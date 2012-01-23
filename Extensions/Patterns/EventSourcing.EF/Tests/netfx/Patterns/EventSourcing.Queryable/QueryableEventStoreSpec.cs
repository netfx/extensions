#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using NetFx.Patterns.EventSourcing.Tests;
using System.Collections;

namespace NetFx.Patterns.EventSourcing.Queryable.Tests
{
	public class EventSourcingQueryableSpec
	{
		private QueryableStore store;
		private Func<DateTime> utcNow = () => DateTime.UtcNow;
		private Guid id1 = Guid.NewGuid();
		private Guid id2 = Guid.NewGuid();

		public EventSourcingQueryableSpec()
		{
			this.store = new QueryableStore(() => this.utcNow());

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
			store.Persist(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.utcNow = () => when;
			product.Deactivate();
			store.Persist(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			var events = store.Query().OfType<DeactivatedEvent>().Since(when).Execute();

			Assert.Equal(2, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateSinceExclusive_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.utcNow = () => when;
			product.Deactivate();
			store.Persist(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			var events = store.Query().OfType<DeactivatedEvent>().Since(when).ExclusiveRange().Execute();

			Assert.Equal(1, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntil_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.utcNow = () => when;
			product.Deactivate();
			store.Persist(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			var events = store.Query().OfType<DeactivatedEvent>().Until(when).Execute();

			Assert.Equal(3, events.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntilExclusive_ThenSucceeds()
		{
			var product = new Product(store.Query().For<Product>(id2).Execute());

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			this.utcNow = () => when;
			product.Deactivate();
			store.Persist(product);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			product.Deactivate();
			store.Persist(product);

			var events = store.Query().OfType<DeactivatedEvent>().Until(when).ExclusiveRange().Execute();

			Assert.Equal(2, events.Count());
		}

		internal class DeactivatedEvent : DomainEvent
		{
			public override string ToString()
			{
				return this.GetType().Name;
			}
		}

		[Fact]
		public void WhenSavingEvent_ThenCanRetrieveIt()
		{
			var store = new QueryableStore();
			var product = new Product(id1, "DevStore");
			product.Publish(1);

			store.Persist(product);

			var events = store.Query().Execute().ToList();

			Assert.Equal(2, events.Count);
			Assert.True(events.OfType<Product.CreatedEvent>().Any(x => x.Id == id1 && x.Title == "DevStore"));
			Assert.True(events.OfType<Product.PublishedEvent>().Any(x => x.Version == 1));
		}

		[Fact]
		public void WhenSavingMultipleEvents_ThenCanLoadSpecificObject()
		{
			var store = new QueryableStore();
			var product = new Product(id1, "DevStore");
			product.Publish(1);

			store.Persist(product);

			product = new Product(id2, "WoVS");
			product.Publish(1);
			product.Publish(2);
			product.Publish(3);

			store.Persist(product);

			var saved = new Product(store.Query().For<Product>(id2).Execute());

			Assert.Equal(3, saved.Version);
			Assert.Equal("WoVS", saved.Title);
			Assert.Equal(id2, saved.Id);
		}
	}

	internal class QueryableStore : IQueryableEventStore<Guid, DomainEvent, StoredEvent>
	{
		private List<StoredEvent> events = new List<StoredEvent>();
		private Func<DateTime> utcNow;

		public QueryableStore()
			: this(() => DateTime.UtcNow)
		{
		}

		public QueryableStore(Func<DateTime> utcNow)
		{
			this.utcNow = utcNow;
		}

		public IQueryable<StoredEvent> Events { get { return this.events.AsQueryable(); } }

		public void Commit() { }

		public void Persist(DomainObject<Guid, DomainEvent> entity)
		{
			foreach (var @event in entity.GetEvents())
			{
				Save(entity, @event);
			}

			entity.AcceptEvents();
		}

		private void Save(DomainObject<Guid, DomainEvent> entity, DomainEvent @event)
		{
			this.events.Add(new StoredEvent(entity, @event, this.utcNow()));
		}

		public IEnumerable<DomainEvent> Query(EventQueryCriteria<Guid> criteria)
		{
			// Both forms are available.
			var expr2 = criteria.ToExpression(this, type => type.Name);
			var expr = this.ToExpression(criteria, type => type.Name);
			if (expr == null)
				return this.events.Select(x => x.Event);

			return this.events.AsQueryable().Where(expr).Select(x => x.Event);
		}
	}

	internal class StoredEvent : IStoredEvent<Guid>
	{
		public StoredEvent()
		{
		}

		public StoredEvent(DomainObject<Guid, DomainEvent> target, DomainEvent @event, DateTimeOffset timeStamp)
		{
			this.Object = target;
			this.Event = @event;
			this.EventId = Guid.NewGuid();
			this.Timestamp = timeStamp;
		}

		public DomainObject<Guid, DomainEvent> Object { get; private set; }
		public DomainEvent Event { get; set; }

		public Guid ObjectId { get { return this.Object.Id; } set { } }
		public string ObjectType { get { return this.Object.GetType().Name; } set { } }


		public Guid EventId { get; set; }
		public string EventType { get { return this.Event.GetType().Name; } set { } }
		public DateTimeOffset Timestamp { get; set; }
	}
}