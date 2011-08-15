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
		}

		internal class ConsoleHandler : DomainEventHandler<DomainEvent>
		{
			public override void Handle(int aggregateId, DomainEvent @event)
			{
				Console.WriteLine(@event);
			}
		}

		public void WhenHandlerRegistered_ThenCanProcessEntity()
		{
			var product = new Product { Title = "DevStore" };
			var context = default(DomainContext);
			var bus = default(IDomainEventBus);
			bus = new DomainEventBus(new IDomainEventHandler[] 
			{ 
				new ConsoleHandler(), 
				new SendMailHandler(new Lazy<DomainContext>(() => context), new Lazy<IDomainEventBus>(() => bus)),
			});
			
			context = new DomainContext(bus, product);

			product.Publish(1);

			context.SaveChanges();
		}
	}
}
