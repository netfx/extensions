using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <nuget id="netfx-Patterns.EventSourcing.EF"/>
partial class DomainEventStore<TId> : IDomainEventStore<TId>
	where TId : IComparable
{
	public DomainEventStore()
	{
		this.TypeNameConverter = type => type.Name;
	}

	public Func<Type, string> TypeNameConverter { get; set; }

	public void Save(AggregateRoot<TId> sender, TimestampedEventArgs args)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<TimestampedEventArgs> Query(StoredEventCriteria<TId> criteria)
	{
		throw new NotImplementedException();
	}
}