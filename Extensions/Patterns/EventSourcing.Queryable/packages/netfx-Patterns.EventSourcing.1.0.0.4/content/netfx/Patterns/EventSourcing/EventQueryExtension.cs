#region BSD License
/* 
Copyright (c) 2010, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Provides the entry point <see cref="Query"/> for a fluent API 
/// that makes querying event stores easier.
/// </summary>
/// <nuget id="netfx-Patterns.EventSourcing"/>
static partial class EventQueryExtension
{
	/// <summary>
	/// Queries the event store for events that match specified 
	/// criteria via the returned fluent API methods 
	/// <see cref="IEventQuery{TObjectId, TBaseEvent}.For{TObject}()"/> and 
	/// <see cref="IEventQuery{TObjectId, TBaseEvent}.OfType{TEvent}()"/>. 
	/// </summary>
	/// <typeparam name="TObjectId">The type of identifier used by the domain objects in the domain.</typeparam>
	/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
	/// <param name="store">The domain event store.</param>
	public static IEventQuery<TObjectId, TBaseEvent> Query<TObjectId, TBaseEvent>(this IEventStore<TObjectId, TBaseEvent> store)
		where TBaseEvent : ITimestamped
	{
		return new EventQuery<TObjectId, TBaseEvent>(store);
	}

	/// <summary>
	/// Provides a fluent API to filter events from the event store. 
	/// </summary>
	/// <remarks>
	/// This interface is returned from the <see cref="EventQueryExtension.Query"/> 
	/// extension method for <see cref="IEventStore{TObjectId, TBaseEvent}"/>.
	/// </remarks>
	/// <typeparam name="TObjectId">The type of identifier used by the domain objects in the domain.</typeparam>
	/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
	/// <nuget id="netfx-Patterns.EventSourcing"/>
	public partial interface IEventQuery<TObjectId, TBaseEvent>
		where TBaseEvent : ITimestamped
	{
		/// <summary>
		/// Executes the query built using the fluent API 
		/// against the underlying store.
		/// </summary>
		IEnumerable<TBaseEvent> Execute();

		/// <summary>
		/// Filters events that target the given domain object type. Can be called 
		/// multiple times and will filter for any of the specified types (OR operator).
		/// </summary>
		/// <typeparam name="TObject">The type of the domain object to filter events for.</typeparam>
		IEventQuery<TObjectId, TBaseEvent> For<TObject>();

		/// <summary>
		/// Filters events that target the given domain object type and identifier. Can be called 
		/// multiple times and will filter for any of the specified types and ids (OR operator).
		/// </summary>
		/// <typeparam name="TObject">The type of the domain object to filter events for.</typeparam>
		/// <param name="objectId">The domain object identifier to filter by.</param>
		IEventQuery<TObjectId, TBaseEvent> For<TObject>(TObjectId objectId);

		/// <summary>
		/// Filters events that are assignable to the given type. Can be called 
		/// multiple times and will filter for any of the specified types (OR operator).
		/// </summary>
		/// <typeparam name="TEvent">The type of the events to filter.</typeparam>
		IEventQuery<TObjectId, TBaseEvent> OfType<TEvent>() where TEvent : TBaseEvent;

		/// <summary>
		/// Filters events that happened after the given starting date.
		/// </summary>
		/// <param name="when">The starting date to filter by.</param>
		/// <remarks>
		/// By default, includes events with the given date, unless the 
		/// <see cref="ExclusiveRange"/> is called to make the range exclusive.
		/// </remarks>
		IEventQuery<TObjectId, TBaseEvent> Since(DateTime when);

		/// <summary>
		/// Filters events that happened before the given ending date.
		/// </summary>
		/// <param name="when">The ending date to filter by.</param>
		/// <remarks>
		/// By default, includes events with the given date, unless the 
		/// <see cref="ExclusiveRange"/> is called to make the range exclusive.
		/// </remarks>
		IEventQuery<TObjectId, TBaseEvent> Until(DateTime when);

		/// <summary>
		/// Makes the configured <see cref="Since"/> and/or <see cref="Until"/> dates 
		/// exclusive, changing the default behavior which is to be inclusive.
		/// </summary>
		IEventQuery<TObjectId, TBaseEvent> ExclusiveRange();
	}

	private class EventQuery<TObjectId, TBaseEvent> : IEventQuery<TObjectId, TBaseEvent>
		where TBaseEvent : ITimestamped
	{	
		private IEventStore<TObjectId, TBaseEvent> store;
		private EventQueryCriteria<TObjectId> criteria = new EventQueryCriteria<TObjectId>();

		public EventQuery(IEventStore<TObjectId, TBaseEvent> store)
		{
			this.store = store;
		}

		public IEnumerable<TBaseEvent> Execute()
		{
			return this.store.Query(this.criteria);
		}

		public IEventQuery<TObjectId, TBaseEvent> For<TObject>()
		{
			foreach (var type in GetInheritance<TObject>())
			{
				this.criteria.ObjectTypes.Add(type);
			}

			return this;
		}

		public IEventQuery<TObjectId, TBaseEvent> For<TObject>(TObjectId objectId)
		{
			foreach (var type in GetInheritance<TObject>())
			{
				this.criteria.ObjectInstances.Add(new EventQueryCriteria<TObjectId>.ObjectFilter(type, objectId));
			}

			return this;
		}

		public IEventQuery<TObjectId, TBaseEvent> OfType<TEvent>()
			where TEvent : TBaseEvent
		{
			foreach (var type in GetInheritance<TEvent>())
			{
				this.criteria.EventTypes.Add(type);
			}

			return this;
		}

		public IEventQuery<TObjectId, TBaseEvent> Since(DateTime when)
		{
			this.criteria.Since = when;
			return this;
		}

		public IEventQuery<TObjectId, TBaseEvent> Until(DateTime when)
		{
			this.criteria.Until = when;
			return this;
		}

		public IEventQuery<TObjectId, TBaseEvent> ExclusiveRange()
		{
			this.criteria.IsExclusiveRange = true;
			return this;
		}

		/// <devdoc>
		/// Returns a list of the T and all its base types until an 
		/// abstract base class is found (can't be persisted therefore) 
		/// or System.Object is found (events can't be System.Object either).
		/// </devdoc>
		private IEnumerable<Type> GetInheritance<T>()
		{
			var current = typeof(T);
			while (current != typeof(object) && !current.IsAbstract)
			{
				yield return current;
				current = current.BaseType;
			}
		}
	}
}