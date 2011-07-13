#region BSD License
/* 
Copyright (c) 2010, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Tests.content.netfx.Patterns.EventSourcing
{
	/// <nuget id="netfx-Patterns.EventSourcing.Tests" />
	public class Sample
	{
		[Fact]
		public void WhenHandlerRegistered_ThenCanProcessEntity()
		{
			var product = new Product { Title = "DevStore" };
			var repository = new Repository<Guid>(product);
			var bus = new DomainEventBus(new [] { new SendMailHandler(repository) });

			product.Publish(1);
			product.GetChanges().ToList().ForEach(e => bus.Publish(product, e));
		}

		[Fact]
		public void WhenEventPersisted_ThenCanObserveIt()
		{
			var store = new DomainEventStore<Guid>();
			//store.Events.Where(
			
		}

		public class DomainEventStore<TAggregateId>
			 where TAggregateId : IComparable
		{
			private List<InMemoryPersistedEvent<TAggregateId>> events = new List<InMemoryPersistedEvent<TAggregateId>>();

			public void Save(DomainEvent<TAggregateId> @event)
			{
				this.events.Add(new InMemoryPersistedEvent<TAggregateId>(@event));
			}

			public IQueryable<TDomainEvent> Find<TDomainEvent>()
				where TDomainEvent : DomainEvent<TAggregateId>
			{
				return this.events
					.Select(x => x.Payload as TDomainEvent)
					.Where(x => x != null)
					.AsQueryable();
			}

			public IQueryable<TDomainEvent> Find<TDomainEvent>(TAggregateId aggregateId)
				where TDomainEvent : DomainEvent<TAggregateId>
			{
				return this.events
					.Select(x => x.Payload as TDomainEvent)
					.Where(x => x != null && x.AggregateId.CompareTo(aggregateId) == 0)
					.AsQueryable();
			}

			public IQueryable<TDomainEvent> Find<TDomainEvent>(TAggregateId aggregateId)
				where TDomainEvent : DomainEvent<TAggregateId>
			{
				return this.events
					.Select(x => x.Payload as TDomainEvent)
					.Where(x => x != null && x.AggregateId.CompareTo(aggregateId) == 0)
					.AsQueryable();
			}

			public IQueryable<TDomainEvent> Find<TDomainEvent>(TAggregateId aggregateId, DateTime? since, DateTime? until, RangeType? range)
				where TDomainEvent : DomainEvent<TAggregateId>
			{
				var result = this.events
					.Select(x => x.Payload as TDomainEvent);

				//if (since != null)

				return result
					.Where(x => x != null && x.AggregateId.CompareTo(aggregateId) == 0)
					.AsQueryable();
			}
		}

		public enum RangeType
		{
			Exclusive,
			Inclusive,
		}

		public abstract partial class PersistedEvent<TAggregateId>
		{
			public PersistedEvent()
			{
				this.When = DateTime.UtcNow;
			}

			/// <summary>
			/// Gets or sets the identifier of the aggregate root associated with this event.
			/// </summary>
			public TAggregateId AggregateId { get; set; }

			/// <summary>
			/// Gets or sets the time the event was persisted, in UTC.
			/// </summary>
			public DateTime When { get; set; }
		}

		public class InMemoryPersistedEvent<TAggregateId> : PersistedEvent<TAggregateId>
		{
			public InMemoryPersistedEvent(DomainEvent<TAggregateId> @event)
			{
				this.Payload = @event;
			}

			/// <summary>
			/// Gets or sets the event payload data.
			/// </summary>
			public DomainEvent<TAggregateId> Payload { get; set; }
		}

		public class JsonPersistedEvent<TAggregateId> : PersistedEvent<TAggregateId>
		{
			/// <summary>
			/// Gets or sets the event payload data.
			/// </summary>
			public string Payload { get; set; }
		}
	}

	/// <nuget id="netfx-Patterns.EventSourcing.Tests" />
	internal class Repository<TId>
		where TId : IComparable
	{
		private List<object> aggregates = new List<object>();

		public Repository(params object[] aggregates)
		{
			this.aggregates.AddRange(aggregates);
		}

		public T Find<T>(TId id)
			where T : AggregateRoot<TId>
		{
			return aggregates.OfType<T>().FirstOrDefault(x => x.Id.CompareTo(id) == 0);
		}

		public void SaveChanges()
		{
		}
	}

	/// <nuget id="netfx-Patterns.EventSourcing.Tests" />
	internal class Product : AggregateRoot<Guid>
	{
		public Product()
		{
			this.Id = Guid.NewGuid();
		}

		public string Title { get; set; }
		public int Version { get; set; }

		public void Publish(int version)
		{
			if (version <= 0)
				throw new ArgumentException();

			this.ApplyEvent(new ProductPublishedEvent(this.Id) { Version = version }, this.Apply);
		}

		private void Apply(ProductPublishedEvent @event)
		{
			this.Version = @event.Version;
		}
	}

	/// <nuget id="netfx-Patterns.EventSourcing.Tests" />
	internal class ProductPublishedEvent : DomainEvent<Guid>
	{
		public ProductPublishedEvent(Guid guid)
		{
			this.AggregateId = guid;
		}

		public int Version { get; set; }
	}

	/// <nuget id="netfx-Patterns.EventSourcing.Tests" />
	internal class SendMailHandler : DomainEventHandler<ProductPublishedEvent>
	{
		private Repository<Guid> repository;

		public SendMailHandler(Repository<Guid> repository)
		{
			this.repository = repository;
		}

		public override void Handle(ProductPublishedEvent @event)
		{
			var product = this.repository.Find<Product>(@event.AggregateId);
			Console.WriteLine("Product {0} has current version {1}", product.Title, @event.Version);
		}

		public override bool IsAsync
		{
			get { return false; }
		}
	}
}
