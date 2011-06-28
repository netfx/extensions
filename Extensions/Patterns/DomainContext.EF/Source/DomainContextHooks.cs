using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

partial class DomainContext<TContextInterface, TId>
{
	/// <summary />
	public bool OnContextCreatedCalled;
	partial void OnContextCreated()
	{
		OnContextCreatedCalled = true;
	}

	/// <summary />
	public List<object> OnEntityCreatedCalls = new List<object>();
	partial void OnEntityCreated(object entity)
	{
		OnEntityCreatedCalls.Add(entity);
	}

	/// <summary />
	public List<object> OnEntitySavingCalls = new List<object>();
	partial void OnEntitySaving<T>(T entity)
	{
		OnEntitySavingCalls.Add(entity);
	}

	/// <summary />
	public List<object> OnEntitySavedCalls = new List<object>();
	partial void OnEntitySaved<T>(T entity)
	{
		OnEntitySavedCalls.Add(entity);
	}

	/// <summary />
	public bool OnContextSavingChangesCalled;
	partial void OnContextSavingChanges()
	{
		OnContextSavingChangesCalled = true;
	}

	/// <summary />
	public bool OnContextSavedChangesCalled;
	partial void OnContextSavedChanges()
	{
		OnContextSavedChangesCalled = true;
	}
}
