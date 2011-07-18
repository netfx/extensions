using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Very simple in-memory context. See netfx-Patterns.DomainContext.EF for a 
/// persistent EF domain context.
/// </summary>
public class DomainContext
{
	private List<AggregateRoot<int>> aggregates = new List<AggregateRoot<int>>();
	private IDomainEventBus bus;

	public DomainContext(IDomainEventBus bus, params AggregateRoot<int>[] aggregates)
	{
		this.bus = bus;
		this.aggregates.AddRange(aggregates);
	}

	public T Find<T>(int id)
		where T : AggregateRoot<int>
	{
		return aggregates.OfType<T>().FirstOrDefault(x => x.Id == id);
	}

	public void Save<T>(T entity)
		where T : AggregateRoot<int>
	{
		this.aggregates.Add(entity);
	}

	public void SaveChanges()
	{
		// Publish in order.
		foreach (var @event in this.aggregates
			.SelectMany(x => x.GetChanges().Select(e => new { Aggregate = x, Event = e }))
			.OrderBy(x => x.Event.Timestamp))
		{
			this.bus.Publish(@event.Aggregate, @event.Event);
		}

		foreach (var aggregate in this.aggregates)
		{
			aggregate.AcceptChanges();
		}
	}
}
