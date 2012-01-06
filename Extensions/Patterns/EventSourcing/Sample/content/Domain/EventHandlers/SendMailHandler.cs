using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;

/// <summary>
/// Showscases a handler that adds logic to the product published 
/// event, without requiring the domain to know about mail sending.
/// </summary>
internal class SendMailHandler : IDisposable
{
	private IDisposable subscription;

	public SendMailHandler(IEventStream eventStream)
	{
		this.subscription = eventStream.Of<IEvent<Product, ProductPublishedEvent>>().Subscribe(this.OnProductPublished);
	}

	private void OnProductPublished(IEvent<Product, ProductPublishedEvent> @event)
	{
		// we can access the originating product directly from the event
		var product = @event.Sender;

		// Invoke an email sending service here.

		Console.WriteLine(@"Sent email:
	To: {To}
	From: {From}
	Title: {Title}
	Body: {Body}".FormatWith(new
		{
			To = "joe@netfx.com",
			From = "webmaster@netfx.com",
			Title = "New version {Version} has been published for product '{Title}'"
				.FormatWith(new { Version = @event.EventArgs.Version, Title = product.Title }),
			Body = "Download it now!",
		}));
	}

	public void Dispose()
	{
		if (this.subscription != null)
		{
			this.subscription.Dispose();
			this.subscription = null;
		}
	}
}
