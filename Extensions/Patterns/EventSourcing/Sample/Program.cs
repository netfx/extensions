using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sample
{
	public class Program
	{
		static void Main(string[] args)
		{
			new Program().WhenHandlerRegistered_ThenCanProcessEntity();
		}

		internal class ConsoleHandler : DomainEventHandler<IDomainEvent>
		{
			public override void Handle(Guid aggregateId, IDomainEvent @event)
			{
				Console.WriteLine(@event);
			}
		}

		internal class ConsoleEventStore : IDomainEventStore
		{
			private List<IDomainEvent> events = new List<IDomainEvent>();

			public void Save(AggregateRoot<Guid, IDomainEvent> sender, IDomainEvent @event)
			{
				this.events.Add(@event);
				Console.WriteLine("Saved event {0} to the store.", @event);
			}

			public IEnumerable<IDomainEvent> Query(EventQueryCriteria<Guid> criteria)
			{
				throw new NotImplementedException();
			}
		}

		public void WhenHandlerRegistered_ThenCanProcessEntity()
		{
			var id = Guid.NewGuid();
			var product = new Product(id, "DevStore");

			var context = default(IDomainContext);
			var bus = default(IDomainEventBus);
			IDomainEventStore store = new ConsoleEventStore();

			bus = new DomainEventBus(store, new IEventHandler[] 
			{ 
				new ConsoleHandler(), 
				new SendMailHandler(new Lazy<IDomainContext>(() => context), new Lazy<IDomainEventBus>(() => bus)),
			});

			context = new DomainContext(bus);
			context.Save(product);

			// Save changes and cause publication of pending events 
			// in the newly created aggregate root.
			context.SaveChanges();

			Console.WriteLine();

			// Here some command might pull the product from the 
			// context, and invoke a domain method.
			var savedProduct = context.Find<Product>(id);
			product.Publish(1);

			// Saving again causes persistence of the entity state 
			// as well as publishing the events.
			context.SaveChanges();
		}
	}
}
