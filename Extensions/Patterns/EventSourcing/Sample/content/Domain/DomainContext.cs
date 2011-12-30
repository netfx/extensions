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
	private List<DomainObject> objects = new List<DomainObject>();
	private IDomainEventBus eventBus;

	public DomainContext(IDomainEventBus eventBus, params DomainObject[] sources)
	{
		this.eventBus = eventBus;
		this.objects.AddRange(sources);
	}

	public T Find<T>(Guid id)
		where T : DomainObject
	{
		return objects.OfType<T>().FirstOrDefault(x => x.Id == id);
	}

	public void Save<T>(T entity)
		where T : DomainObject
	{
		this.objects.Add(entity);
	}

	public void SaveChanges()
	{
		foreach (var @object in this.objects)
		{
			this.eventBus.PublishChanges(@object);
		}
	}
}
