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

/// <summary>
/// Provides the <see cref="None"/> empty store for use 
/// when no store is needed.
/// </summary>
/// <typeparam name="TAggregateId">The type of identifier used by the aggregate roots in the domain.</typeparam>
/// <typeparam name="TBaseEvent">The base type or interface implemented by events in the domain.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing.Core"/>
partial class DomainEventStore<TAggregateId, TBaseEvent>
	where TAggregateId : IComparable
{
	/// <summary>
	/// Initializes the <see cref="None"/> null object 
	/// pattern property.
	/// </summary>
	static DomainEventStore()
	{
		None = new NullStore();
	}

	/// <summary>
	/// Gets a default domain event store implementation that 
	/// does nothing (a.k.a. Null Object Pattern).
	/// </summary>
	public static IDomainEventStore<TAggregateId, TBaseEvent> None { get; private set; }

	private class NullStore : IDomainEventStore<TAggregateId, TBaseEvent>
	{
		public Func<Type, string> TypeNameConverter { get; set; }

		public IEnumerable<TBaseEvent> Query(StoredEventCriteria<TAggregateId> criteria)
		{
			return Enumerable.Empty<TBaseEvent>();
		}

		public void Persist(AggregateRoot<TAggregateId, TBaseEvent> sender, TBaseEvent args)
		{
		}

		public void Commit()
		{
		}
	}
}
