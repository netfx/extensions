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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq.Expressions;
using System.Data.Services.Client;
using System.Reflection;
using System.Diagnostics;
using System.Web;

namespace System.Net.Http.Entity
{
	/// <summary>
	/// A client API that communicates with REST services implemented 
	/// using standard REST methods and WCF Web API for querying.
	/// </summary>
	/// <remarks>
	/// The API assumes that entities have an ID that can be rendered 
	/// as a string (Uris are strings ;)), and that POST is performed 
	/// to the entity resource parent (not the entity Uri, like "products/23", 
	/// but "products") and it's used for creating the entity. 
	/// <para>
	/// PUT can be used also for entity creation when the client 
	/// determines the server-side URI (i.e. it generates a GUID).
	/// </para>
	/// <para>
	/// Of course the service needs to implement this same style of REST 
	/// service, but this is extremely common.
	/// </para>
	/// <para>
	/// Select expressions in the query cause it to be executed and 
	/// materialized locally, so that further operations can be 
	/// performed even if they are in the 
	/// <see cref="http://msdn.microsoft.com/en-us/library/ee622463.aspx#unsupportedMethods">unsupported methods</see> 
	/// list. If anything fails in the expression, you can always call 
	/// ToList and use Linq to Objects from that point on. This is 
	/// automatic after a Select.
	/// </para>
	/// </remarks>
	internal class HttpEntityConventionClient : HttpEntityClient
	{
		private bool disposed;
		private HttpClient http;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpEntityConventionClient"/> class with 
		/// the default formatter <see cref="JsonNetEntityFormatter"/> and convention 
		/// <see cref="PluralizerResourceConvention"/>.
		/// </summary>
		public HttpEntityConventionClient(string baseAddress)
			: this(new Uri(baseAddress))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpEntityConventionClient"/> class with 
		/// the default formatter <see cref="JsonNetEntityFormatter"/> and convention 
		/// <see cref="PluralizerResourceConvention"/>.
		/// </summary>
		public HttpEntityConventionClient(Uri baseAddress)
			: this(baseAddress, new JsonNetEntityFormatter(), new PluralizerResourceConvention())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpEntityConventionClient"/> class.
		/// </summary>
		/// <param name="baseAddress">The base address of the service.</param>
		/// <param name="formatter">The formatter that translates service responses into entitites.</param>
		/// <param name="convention">The convention to discover the resource name (or path) for the entities.</param>
		public HttpEntityConventionClient(Uri baseAddress, IEntityFormatter formatter, IEntityResourceNameConvention convention)
			: base(baseAddress, formatter)
		{
			this.ResourceNameConvention = convention;
		}

		public IEntityResourceNameConvention ResourceNameConvention { get; private set; }

		/// <summary>
		/// Deletes the specified entity.
		/// </summary>
		public void Delete<T>(string id)
		{
			base.Delete(ResourceFor<T>(), id);
		}

		/// <summary>
		/// Gets the entity with the given id.
		/// </summary>
		/// <exception cref="HttpEntityException">The request did not succeed.</exception>
		public T Get<T>(string id)
		{
			return base.Get<T>(ResourceFor<T>(), id);
		}

		/// <summary>
		/// Posts the specified entity to the entity resource
		/// and returns the state persisted by the service, which 
		/// should be returned in the response body.
		/// </summary>
		public T Post<T>(T entity)
		{
			return base.Post(ResourceFor<T>(), entity);
		}

		/// <summary>
		/// Puts the specified entity to the service.
		/// </summary>
		public void Put<T>(string id, T entity)
		{
			base.Put(ResourceFor<T>(), id, entity);
		}

		/// <summary>
		/// Tries to get the entity with the given id.
		/// </summary>
		public HttpResponseMessage TryGet<T>(string id, out T entity)
		{
			return base.TryGet<T>(ResourceFor<T>(), id, out entity);
		}

		/// <summary>
		/// Tries to posts the specified entity and retrieves the new id 
		/// that was assigned by the service from the Location header, if any.
		/// </summary>
		public HttpResponseMessage TryPost<T>(T entity, out T saved)
		{
			return base.TryPost(ResourceFor<T>(), entity, out saved);
		}

		/// <summary>
		/// Tries to put the specified entity to the service.
		/// </summary>
		public HttpResponseMessage TryPut<T>(string id, T entity)
		{
			return base.TryPut(ResourceFor<T>(), id, entity);
		}

		/// <summary>
		/// Tries to delete the specified entity.
		/// </summary>
		public HttpResponseMessage TryDelete<T>(string id)
		{
			return base.TryDelete(ResourceFor<T>(), id);
		}

		/// <summary>
		/// Creates a query for the given entity type, that will be
		/// executed when the queryable is enumerated.
		/// </summary>
		/// <typeparam name="T">Type of entity being queried.</typeparam>
		/// <param name="search">Optional search criteria to be applied by the service, 
		/// sent as a "q=" query string parameter. Useful to overcome limitations 
		/// in the underlying query support in WCF.</param>
		public IQueryable<T> Query<T>(string search = null)
		{
			return base.Query<T>(ResourceFor<T>(), search);
		}

		/// <summary>
		/// Gets the resource path for the given type, using the configured convention.
		/// </summary>
		private string ResourceFor<T>()
		{
			return this.ResourceNameConvention.GetResourceName(typeof(T));
		}
	}
}
