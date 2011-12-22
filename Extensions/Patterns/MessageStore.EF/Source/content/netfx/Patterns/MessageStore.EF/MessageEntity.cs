using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

partial class MessageEntity
{
	/// <summary>
	/// Gets or sets the optional activity id where the message was logged.
	/// </summary>
	public Guid ActivityId { get; set; }

	/// <summary>
	/// Gets or sets the message identifier.
	/// </summary>
	public virtual Guid Id { get; set; }

	/// <summary>
	/// Gets or sets the type of the message.
	/// </summary>
	public virtual string Type { get; set; }

	/// <summary>
	/// Gets or sets the UTC timestamp of the message.
	/// </summary>
	public virtual DateTime Timestamp { get; set; }

	/// <summary>
	/// Gets or sets the headers associated with the message.
	/// </summary>
	public virtual byte[] Headers { get; set; }

	/// <summary>
	/// Gets or sets the payload of the message.
	/// </summary>
	public virtual byte[] Payload { get; set; }

	/// <summary>
	/// Gets or sets the row version.
	/// </summary>
	[Timestamp]
	public byte[] RowVersion { get; set; }
}
