using System;

// Very simple generic repository pattern.
internal interface IDomainContext
{
	T Find<T>(Guid id) where T : AggregateRoot;
	void Save<T>(T entity) where T : AggregateRoot;
	void SaveChanges();
}
