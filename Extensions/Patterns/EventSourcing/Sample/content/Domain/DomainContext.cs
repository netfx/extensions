using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Very simple in-memory context. See netfx-Patterns.DomainContext.EF for a 
/// persistent EF domain context.
/// </summary>
internal class DomainContext : IDomainContext
{
	private List<AggregateRoot> aggregates = new List<AggregateRoot>();
	private IDomainEventBus eventBus;

	public DomainContext(IDomainEventBus eventBus, params AggregateRoot[] sources)
	{
		this.eventBus = eventBus;
		this.aggregates.AddRange(sources);
	}

	public T Find<T>(Guid id)
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
		foreach (var aggregate in this.aggregates)
		{
			this.eventBus.PublishChanges(aggregate);
		}
	}
}
