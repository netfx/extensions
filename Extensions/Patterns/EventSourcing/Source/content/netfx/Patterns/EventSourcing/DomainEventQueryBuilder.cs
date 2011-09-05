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
static partial class DomainEventQueryBuilder
{
	/// <summary>
	/// Queries the event store for events that match specified 
	/// criteria via the returned fluent API methods 
	/// <see cref="IDomainEventQuery{TAggregateId, TBaseEvent}.For{TAggregate}()"/> and 
	/// <see cref="IDomainEventQuery{TAggregateId, TBaseEvent}.OfType{TEvent}()"/>. 
	/// </summary>
	/// <typeparam name="TAggregateId">The type of identifier used by the aggregate roots in the domain.</typeparam>
	/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
	/// <param name="store">The domain event store.</param>
	public static IDomainEventQuery<TAggregateId, TBaseEvent> Query<TAggregateId, TBaseEvent>(this IDomainEventStore<TAggregateId, TBaseEvent> store)
		where TAggregateId : IComparable
	{
		return new DomainEventQuery<TAggregateId, TBaseEvent>(store);
	}

	private class DomainEventQuery<TAggregateId, TBaseEvent> : IDomainEventQuery<TAggregateId, TBaseEvent>
		where TAggregateId : IComparable
	{	
		private IDomainEventStore<TAggregateId, TBaseEvent> store;
		private DomainEventQueryCriteria<TAggregateId> criteria = new DomainEventQueryCriteria<TAggregateId>();

		public DomainEventQuery(IDomainEventStore<TAggregateId, TBaseEvent> store)
		{
			this.store = store;
		}

		public DomainEventQueryCriteria<TAggregateId> Criteria { get { return this.criteria; } }

		public IEnumerator<TBaseEvent> GetEnumerator()
		{
			return this.store.Query(this.criteria).GetEnumerator();
		}

		public IDomainEventQuery<TAggregateId, TBaseEvent> For<TAggregate>()
		{
			foreach (var type in GetInheritance<TAggregate>())
			{
				this.criteria.AggregateTypes.Add(type);
			}

			return this;
		}

		public IDomainEventQuery<TAggregateId, TBaseEvent> For<TAggregate>(TAggregateId aggregateId)
		{
			foreach (var type in GetInheritance<TAggregate>())
			{
				this.criteria.AggregateInstances.Add(new DomainEventQueryCriteria<TAggregateId>.AggregateFilter(type, aggregateId));
			}

			return this;
		}

		public IDomainEventQuery<TAggregateId, TBaseEvent> OfType<TEventArgs>()
			where TEventArgs : TBaseEvent
		{
			foreach (var type in GetInheritance<TEventArgs>())
			{
				this.criteria.EventTypes.Add(type);
			}

			return this;
		}

		public IDomainEventQuery<TAggregateId, TBaseEvent> Since(DateTime when)
		{
			this.criteria.Since = when;
			return this;
		}

		public IDomainEventQuery<TAggregateId, TBaseEvent> Until(DateTime when)
		{
			this.criteria.Until = when;
			return this;
		}

		public IDomainEventQuery<TAggregateId, TBaseEvent> ExclusiveRange()
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

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}