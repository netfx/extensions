using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Showscases a handler that adds logic to the product published 
/// event, without requiring the domain to know about mail sending.
/// </summary>
public class SendMailHandler : DomainEventHandler<Product.PublishedEvent>
{
	private Lazy<DomainContext> context;

	/// <summary>
	/// The domain context is used to retrieve the related aggregate 
	/// root if needed for the event processing.
	/// </summary>
	public SendMailHandler(Lazy<DomainContext> context)
	{
		// The handlers are typically composed from an IoC container, 
		// and they would receive any external dependencies they need, 
		// such as an IMailService in this case...
		this.context = context;
	}

	public override void Handle(int aggregateId, Product.PublishedEvent @event)
	{
		// If the same context as the one the entity lives in 
		// is passed to the constructor (typical for in-proc sync 
		// handlers), most ORMs would make a quick in-memory lookup for this.
		var product = this.context.Value.Find<Product>(aggregateId);

		Console.WriteLine("Sending email: 'New version {0} has been published for product {1}'.", 
			@event.Version, product.Title);
	}
}
