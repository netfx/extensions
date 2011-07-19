using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Newtonsoft.Json;
using System.IO;

/// <summary>
/// Implements an event store on top of EntityFramework 4.1
/// </summary>
/// <typeparam name="TId">The type of identifier used by aggregate roots in the domain.</typeparam>
/// <typeparam name="TStoredEvent">The type of the stored event, which must inherit from <see cref="StoredEvent{TId}"/> for a specific <typeparamref name="TId"/>.</typeparam>
/// <remarks>
/// This class is abstract and must be inherited by a concrete 
/// store class that provides a fixed type for the <typeparamref name="TId"/> and 
/// <typeparamref name="TStoredEvent"/> type parameters, as entity framework cannot 
/// work with generic entities.
/// </remarks>
/// <example>
/// The following is an example of a concrete event store leveraging this class:
/// <code>
/// public class DomainEventStore : DomainEventStore&lt;int, StoredEvent&gt;
/// {
/// 	public DomainEventStore(string nameOrConnectionString)
/// 		: base(nameOrConnectionString)
/// 	{
/// 	}
/// }
/// 
/// public class StoredEvent : StoredEvent&lt;int&gt; { }
/// </code>
/// </example>
/// <nuget id="netfx-Patterns.EventSourcing.EF"/>
abstract partial class DomainEventStore<TId, TStoredEvent> : DbContext, IDomainEventStore<TId>
	where TId : IComparable
	where TStoredEvent : StoredEvent<TId>, new()
{
	private static JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
	{
		TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
		TypeNameHandling = TypeNameHandling.Objects,
	});

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainEventStore{TId, TStoredEvent}"/> class.
	/// </summary>
	/// <param name="nameOrConnectionString">The name or connection string.</param>
	public DomainEventStore(string nameOrConnectionString)
		: base(nameOrConnectionString)
	{
		this.TypeNameConverter = type => type.Name;
	}

	/// <summary>
	/// Gets or sets the function that converts a <see cref="Type"/> to 
	/// its string representation in the store. Used to calculate the 
	/// values of <see cref="IStoredEvent{TId}.AggregateType"/> and 
	/// <see cref="IStoredEvent{TId}.EventType"/>.
	/// </summary>
	public Func<Type, string> TypeNameConverter { get; set; }

	/// <summary>
	/// Gets or sets the events persisted in the store.
	/// </summary>
	public virtual DbSet<TStoredEvent> Events { get; set; }

	/// <summary>
	/// Queries the event store for events that match the given criteria.
	/// </summary>
	public IEnumerable<TimestampedEventArgs> Query(StoredEventCriteria<TId> criteria)
	{
		var predicate = criteria.ToExpression(this.TypeNameConverter);
		var events = this.Events.AsQueryable();

		if (predicate != null)
			events = events.Where(predicate).Cast<TStoredEvent>();

		return events.AsEnumerable()
			.Select(x => (TimestampedEventArgs)serializer.Deserialize(new JsonTextReader(new StringReader(x.Payload))));
	}

	/// <summary>
	/// Saves the given event raised by the given sender aggregate root.
	/// </summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="args">The <see cref="TimestampedEventArgs"/> instance containing the event data.</param>
	public void Save(AggregateRoot<TId> sender, TimestampedEventArgs args)
	{
		var payload = new StringWriter();
		serializer.Serialize(payload, args);
		payload.Flush();

		this.Events.Add(new TStoredEvent
		{
			AggregateId = sender.Id, 
			AggregateType = this.TypeNameConverter.Invoke(sender.GetType()), 
			EventType = this.TypeNameConverter.Invoke(args.GetType()), 
			Timestamp = args.Timestamp, 
			Payload = payload.ToString()
		});
	}

	/// <summary>
	/// Saves the changes to the underlying database.
	/// </summary>
	public new void SaveChanges()
	{
		base.SaveChanges();
	}

	/// <summary>
	/// Saves the changes to the underlying database.
	/// </summary>
	void IDomainEventStore<TId>.SaveChanges()
	{
		base.SaveChanges();
	}
}