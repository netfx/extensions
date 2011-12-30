using System;

// Very simple generic repository pattern.
internal interface IDomainContext
{
	T Find<T>(Guid id) where T : DomainObject;
	void Save<T>(T entity) where T : DomainObject;
	void SaveChanges();
}
