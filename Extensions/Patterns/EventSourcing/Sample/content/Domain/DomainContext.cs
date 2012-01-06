using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;

/// <summary>
/// Very simple in-memory context. See netfx-Patterns.DomainContext.EF for a 
/// persistent EF domain context.
/// </summary>
internal class DomainContext : IDomainContext
{
	private List<DomainObject> objects = new List<DomainObject>();
	private IEventStream eventStream;
	private IDomainEventStore eventStore;

	public DomainContext(IEventStream eventStream, IDomainEventStore eventStore, params DomainObject[] sources)
	{
		this.eventStream = eventStream;
		this.eventStore = eventStore;
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
			var events = @object.GetEvents();
			foreach (var @event in @object.GetEvents())
			{
				this.eventStream.Push(Event.Create(@object, @event));
			}

			this.eventStore.SaveChanges(@object);
		}
	}
}
