using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Showscases a handler that adds logic to the product published 
/// event, without requiring the domain to know about mail sending.
/// </summary>
internal class SendMailHandler : DomainEventHandler<ProductPublishedEvent>
{
	private Lazy<DomainContext> context;
	private Lazy<IDomainEventBus> eventBus;

	/// <summary>
	/// The domain context is used to retrieve the related aggregate 
	/// root if needed for the event processing.
	/// </summary>
	public SendMailHandler(Lazy<DomainContext> context, Lazy<IDomainEventBus> eventBus)
	{
		// The handlers are typically composed from an IoC container, 
		// and they would receive any external dependencies they need, 
		// such as an IMailService in this case...
		this.context = context;

		// Handlers or services may in turn publish more events to the bus.
		this.eventBus = eventBus;
	}

	public override void Handle(int aggregateId, ProductPublishedEvent @event)
	{
		// If the same context as the one the entity lives in 
		// is passed to the constructor (typical for in-proc sync 
		// handlers), most ORMs would make a quick in-memory lookup for this.
		var product = this.context.Value.Find<Product>(aggregateId);

		// Invoke an email sending service here.

		// Optionally notify the bus that we sent an email for the given product.
		this.eventBus.Value.Publish(product, new EmailSentEvent
		{
			To = "joe@netfx.com",
			From = "webmaster@netfx.com",
			Title = "New version {Version} has been published for product {Title}'"
				.FormatWith(new { Version = @event.Version, Title = product.Title }),
			Body = "Download it now!",
		});
	}
}
