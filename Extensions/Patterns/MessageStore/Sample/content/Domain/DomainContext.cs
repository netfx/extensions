using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Very simple in-memory context. See netfx-Patterns.DomainContext.EF for a 
/// persistent EF domain context.
/// </summary>
internal class DomainContext
{
	private List<AggregateRoot> aggregates = new List<AggregateRoot>();
	private IDomainEventBus bus;

	public DomainContext(IDomainEventBus bus, params AggregateRoot[] sources)
	{
		this.bus = bus;
		this.aggregates.AddRange(sources);
	}

	public T Find<T>(int id)
		where T : AggregateRoot
	{
		return aggregates.OfType<T>().FirstOrDefault(x => x.Id == id);
	}

	public void Save<T>(T entity)
		where T : AggregateRoot
	{
		this.aggregates.Add(entity);
	}

	public void SaveChanges()
	{
		foreach (var @event in this.aggregates
			.SelectMany(x => x.GetChanges().Select(e => new { Source = x, Event = e })))
		{
			this.bus.Publish(@event.Source, @event.Event);
		}

		foreach (var source in this.aggregates)
		{
			source.AcceptChanges();
		}
	}
}
