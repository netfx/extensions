using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

partial class FileStoredEvent<TObjectId, TBaseEvent> : IStoredEvent<TObjectId>
	where TBaseEvent : ITimestamped
{
	public FileStoredEvent()
	{
	}

	public FileStoredEvent(DomainObject<TObjectId, TBaseEvent> domainObject, TBaseEvent @event)
	{
		// Header
		this.ObjectId = domainObject.Id;
		this.ObjectType = domainObject.GetType().FullName;

		// Event
		this.Event = @event;
		this.EventId = Guid.NewGuid();
		this.EventType = @event.GetType().FullName;
		this.Timestamp = @event.Timestamp;
	}

	public string ObjectType { get; set; }
	public TObjectId ObjectId { get; set; }

	public TBaseEvent Event { get; set; }
	public Guid EventId { get; set; }
	public string EventType { get; set; }
	public DateTimeOffset Timestamp { get; set; }

	public override string ToString()
	{
		return string.Format("{0}.{1} at {2:dd/MM/yyyy HH:mm:ss} (id={3})",
			this.ObjectType.Substring(this.ObjectType.LastIndexOf('.') + 1),
			this.EventType.Substring(this.EventType.LastIndexOf('.') + 1),
			this.Timestamp,
			this.ObjectId);
	}
}