using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;

namespace Sample
{
	public class Program
	{
		static void Main(string[] args)
		{
			new Program().WhenHandlerRegistered_ThenCanProcessEntity();
		}

		internal class ConsoleHandler
		{
			private IDisposable subscription;

			public ConsoleHandler(IEventStream eventStream)
			{
				this.subscription = eventStream.Of<IEvent<EventArgs>>().Subscribe(this.OnEvent);
			}

			private void OnEvent(IEvent<EventArgs> @event)
			{
				Console.WriteLine("{0}: {1}", @event.Sender, @event.EventArgs);
			}
		}

		internal class ConsoleEventStore : IDomainEventStore
		{
			private List<DomainEvent> events = new List<DomainEvent>();

			public void SaveChanges(DomainObject<Guid, DomainEvent> entity)
			{
				foreach (var @event in entity.GetEvents())
				{
					this.events.Add(@event);
					Console.WriteLine("Saved event {0} to the store.", @event);
				}

				entity.AcceptEvents();
			}

			public IEnumerable<DomainEvent> Query(EventQueryCriteria<Guid> criteria)
			{
				throw new NotImplementedException();
			}
		}

		public void WhenHandlerRegistered_ThenCanProcessEntity()
		{
			var id = Guid.NewGuid();
			var product = new Product(id, "DevStore");

			var context = default(IDomainContext);
			var eventStream = new EventStream();
			IDomainEventStore store = new ConsoleEventStore();

			// Keep the handlers so they are not GC'ed.
			var handlers = new object[]
			{ 
				new ConsoleHandler(eventStream), 
				new SendMailHandler(eventStream),
			};

			context = new DomainContext(eventStream, store);
			context.Save(product);

			// Save changes and cause publication of pending events 
			// in the newly created domain object.
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
