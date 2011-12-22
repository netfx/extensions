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
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

/// <summary>
/// Represents the filter criteria for a domain event store query.
/// </summary>
/// <typeparam name="TAggregateId">The type of identifier used by the aggregate roots in the domain.</typeparam>
/// <nuget id="netfx-Patterns.EventSourcing"/>
partial class EventQueryCriteria<TAggregateId>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="EventQueryCriteria&lt;TAggregateId&gt;"/> class.
	/// </summary>
	public EventQueryCriteria()
	{
		this.AggregateInstances = new List<AggregateFilter>();
		this.AggregateTypes = new List<Type>();
		this.EventTypes = new List<Type>();
	}

	/// <summary>
	/// List of source object type + instance identifier filters. The two filters 
	/// should be considered by event stores as AND'ed (i.e. 
	/// events for <c>Product AND Id = 5</c>) and each entry is OR'ed with the 
	/// others (i.e. <c>(Product AND Id = 5) OR (Order AND Id = 1)</c>.
	/// </summary>
	public List<AggregateFilter> AggregateInstances { get; private set; }

	/// <summary>
	/// List of aggregate root type filters. All types added are OR'ed with the 
	/// others (i.e. <c>AggregateType == Product OR AggregateType == Order</c>).
	/// </summary>
	/// <remarks>
	/// To filter by aggregate type and identifier, 
	/// use <see cref="AggregateInstances"/> instead.
	/// </remarks>
	public List<Type> AggregateTypes { get; private set; }

	/// <summary>
	/// List of event type filters. All types added are OR'ed with the 
	/// others (i.e. <c>EventType == ProductCreated OR EventType == ProductPublished</c>).
	/// </summary>
	public List<Type> EventTypes { get; private set; }

	/// <summary>
	/// Filters events that happened after the given starting date.
	/// </summary>
	public DateTime? Since { get; set; }

	/// <summary>
	/// Filters events that happened before the given ending date.
	/// </summary>
	public DateTime? Until { get; set; }

	/// <summary>
	/// If set to <see langword="true"/>, <see cref="Since"/> and <see cref="Until"/> should 
	/// be considered as exclusive ranges (excludes values with a matching date). 
	/// Defaults to <see langword="false"/>, meaning that ranges are inclusive by default.
	/// </summary>
	public bool IsExclusiveRange { get; set; }

	/// <summary>
	/// Filter criteria containing the aggregate type and identifier. 
	/// </summary>
	public class AggregateFilter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateFilter"/> class.
		/// </summary>
		/// <param name="aggregateType">Type of the event source to filter by.</param>
		/// <param name="aggregateId">The aggregate root identifier to filter by.</param>
		public AggregateFilter(Type aggregateType, TAggregateId aggregateId)
		{
			this.AggregateType = aggregateType;
			this.AggregateId = aggregateId;
		}

		/// <summary>
		/// Gets or sets the type of the aggregate to filter by.
		/// </summary>
		public Type AggregateType { get; private set; }

		/// <summary>
		/// Gets or sets the aggregate root identifier to filter by.
		/// </summary>
		public TAggregateId AggregateId { get; private set; }
	}
}
