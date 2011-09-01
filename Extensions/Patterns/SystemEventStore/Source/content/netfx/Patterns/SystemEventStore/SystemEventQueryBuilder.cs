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
/// that makes building the event store query criteria easier.
/// </summary>
/// <nuget id="netfx-Patterns.SystemEventStore"/>
static partial class SystemEventQueryBuilder
{
	/// <summary>
	/// Allows building a query against the event store 
	/// using a fluent API and automatically executing 
	/// it to find events that match built criteria upon 
	/// query enumeration or execution.
	/// </summary>
	/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the system.</typeparam>
	/// <param name="store">The domain event store.</param>
	public static ISystemEventQuery<TBaseEvent> Query<TBaseEvent>(this ISystemEventStore<TBaseEvent> store)
	{
		return new EventQuery<TBaseEvent>(store);
	}

	private class EventQuery<TBaseEvent> : ISystemEventQuery<TBaseEvent>
	{
		private ISystemEventStore<TBaseEvent> store;
		private SystemEventQueryCriteria criteria = new SystemEventQueryCriteria();

		public EventQuery(ISystemEventStore<TBaseEvent> store)
		{
			this.store = store;
		}

		public SystemEventQueryCriteria Criteria { get { return this.criteria; } }

		public IEnumerable<TBaseEvent> Execute()
		{
			return this.store.Query(this.criteria);
		}		

		public IEnumerator<TBaseEvent> GetEnumerator()
		{
			return Execute().GetEnumerator();
		}

		public ISystemEventQuery<TBaseEvent> OfType<TEvent>()
			where TEvent : TBaseEvent
		{
			this.criteria.EventTypes.Add(typeof(TEvent));

			return this;
		}

		public ISystemEventQuery<TBaseEvent> Since(DateTime when)
		{
			this.criteria.Since = when;
			return this;
		}

		public ISystemEventQuery<TBaseEvent> Until(DateTime when)
		{
			this.criteria.Until = when;
			return this;
		}

		public ISystemEventQuery<TBaseEvent> ExclusiveRange()
		{
			this.criteria.IsExclusiveRange = true;
			return this;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}