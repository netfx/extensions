using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Event raised when an email was sent.
/// </summary>
internal class EmailSentEvent : IDomainEvent
{
	public string From { get; set; }
	public string To { get; set; }
	public string Title { get; set; }
	public string Body { get; set; }
	public DateTimeOffset Timestamp { get; private set; }

	public override string ToString()
	{
		return @"Sent email:
	To: {To}
	From: {From}
	Title: {Title}
	Body: {Body}".FormatWith(this);
	}

}
