using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Event raised when an email was sent.
/// </summary>
internal class EmailSentEvent : DomainEvent
{
	public string From { get; set; }
	public string To { get; set; }
	public string Title { get; set; }
	public string Body { get; set; }

	public override string ToString()
	{
		return @"Sent email:
	To: {To}
	From: {From}
	Title: {Title}
	Body: {Body}".FormatWith(this);
	}

}
