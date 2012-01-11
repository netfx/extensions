using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

partial class FileStoredEvent<TObjectId, TBaseEvent> : IStoredEvent<
	IStoredObject<TObjectId>, TObjectId>, 
	IStoredObject<TObjectId>
{
	public FileStoredEvent(DomainObject<TObjectId, TBaseEvent> domainObject, TBaseEvent @event)
	{
		// Header
		this.ObjectId = domainObject.Id;
		this.ObjectType = domainObject.GetType().FullName;

		// Event
		this.Event = @event;
		this.EventId = Guid.NewGuid();
		this.EventType = @event.GetType().FullName;
		this.Timestamp = DateTimeOffset.Now;
	}

	public string ObjectType { get; set; }
	public TObjectId ObjectId { get; set; }

	public TBaseEvent Event { get; set; }
	public Guid EventId { get; set; }
	public string EventType { get; set; }
	public DateTimeOffset Timestamp { get; set; }

	[JsonIgnore]
	public IStoredObject<TObjectId> TargetObject { get { return this; } set { } }
}