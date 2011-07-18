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

		public class ConsoleHandler : DomainEventHandler<TimestampedEventArgs>
		{
			public override void Handle(int aggregateId, TimestampedEventArgs @event)
			{
				Console.WriteLine(@event);
			}
		}

		public void WhenHandlerRegistered_ThenCanProcessEntity()
		{
			var product = new Product { Title = "DevStore" };
			var context = default(DomainContext);
			var bus = new DomainEventBus(new DomainEventHandler[] 
			{ 
				new ConsoleHandler(), 
				new SendMailHandler(new Lazy<DomainContext>(() => context)) 
			});
			
			context = new DomainContext(bus, product);

			product.Publish(1);

			context.SaveChanges();
		}
	}
}
