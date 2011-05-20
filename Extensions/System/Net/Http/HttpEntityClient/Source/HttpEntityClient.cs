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

namespace System.Net.Http.Entity
{
	/// <summary>
	/// A client API that communicates with REST services implemented 
	/// using standard REST methods and WCF Web API for querying.
	/// </summary>
	internal class HttpEntityClient
	{
		private HttpClient http;
		private IEntityFormatter formatter;
		private IEntityResourceConvention convention;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpEntityClient"/> class with 
		/// the default formatter <see cref="JsonNetEntityFormatter"/> and convention 
		/// <see cref="PluralizerResourceConvention"/>.
		/// </summary>
		public HttpEntityClient(string baseAddress)
			: this(new Uri(baseAddress))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpEntityClient"/> class with 
		/// the default formatter <see cref="JsonNetEntityFormatter"/> and convention 
		/// <see cref="PluralizerResourceConvention"/>.
		/// </summary>
		public HttpEntityClient(Uri baseAddress)
			: this(baseAddress, new JsonNetEntityFormatter(), new PluralizerResourceConvention())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpEntityClient"/> class.
		/// </summary>
		/// <param name="baseAddress">The base address of the service.</param>
		/// <param name="formatter">The formatter that translates service responses into entitites.</param>
		/// <param name="convention">The convention to discover the resource name (or path) for the entities.</param>
		public HttpEntityClient(Uri baseAddress, IEntityFormatter formatter, IEntityResourceConvention convention)
		{
			this.BaseAddress = baseAddress;
			this.http = new HttpClient(baseAddress);
			this.http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(formatter.ContentType));
			this.formatter = formatter;
			this.convention = convention;
		}

		/// <summary>
		/// Gets the base address for this client.
		/// </summary>
		public Uri BaseAddress { get; private set; }

		/// <summary>
		/// Deletes the specified entity.
		/// </summary>
		public void Delete<T>(string id)
		{
			var response = TryDelete<T>(id);

			if (!response.IsSuccessStatusCode)
				throw new HttpResponseException(response);
		}

		/// <summary>
		/// Gets the entity with the given id.
		/// </summary>
		/// <exception cref="HttpResponseException">The request did not succeed.</exception>
		public T Get<T>(string id)
		{
			var entity = default(T);
			var response = TryGet<T>(id, out entity);

			if (!response.IsSuccessStatusCode)
				throw new HttpResponseException(response);

			return entity;
		}

		/// <summary>
		/// Posts the specified entity and retrieves the new id 
		/// that was assigned by the service from the Location header, if any.
		/// </summary>
		public string Post<T>(T entity)
		{
			var id = default(string);
			var response = TryPost<T>(entity, out id);

			if (response.StatusCode != HttpStatusCode.Created)
				throw new HttpResponseException(response);

			return id;
		}

		/// <summary>
		/// Puts the specified entity to the service.
		/// </summary>
		public void Put<T>(string id, T entity)
		{
			var response = TryPut(id, entity);

			if (!response.IsSuccessStatusCode)
				throw new HttpResponseException(response);
		}

		/// <summary>
		/// Tries to get the entity with the given id.
		/// </summary>
		public HttpResponseMessage TryGet<T>(string id, out T entity)
		{
			var resource = this.convention.GetResourceName(typeof(T));
			var uri = new Uri(this.BaseAddress, resource + "/" + id);
			var response = this.http.Get(uri);
			entity = default(T);

			if (response.IsSuccessStatusCode)
			{
				ThrowIfUnsupportedContentType(response);
				entity = this.formatter.FromContent<T>(response.Content);
			}

			return response;
		}

		/// <summary>
		/// Tries to posts the specified entity and retrieves the new id 
		/// that was assigned by the service from the Location header, if any.
		/// </summary>
		public HttpResponseMessage TryPost<T>(T entity, out string id)
		{
			var resource = this.convention.GetResourceName(typeof(T));
			var uri = new Uri(this.BaseAddress, resource);
			var response = this.http.Post(uri, this.formatter.ToContent(entity));
			id = default(string);

			if (response.StatusCode == HttpStatusCode.Created)
			{
				if (response.Headers.Location.OriginalString.StartsWith(resource) &&
					response.Headers.Location.OriginalString.Length > resource.Length)
					id = response.Headers.Location.OriginalString.Substring(resource.Length + 1);
				else
					// Can't determine new Id.
					id = string.Empty;
			}

			return response;
		}

		/// <summary>
		/// Tries to put the specified entity to the service.
		/// </summary>
		public HttpResponseMessage TryPut<T>(string id, T entity)
		{
			var resource = this.convention.GetResourceName(typeof(T));
			var uri = new Uri(this.BaseAddress, resource + "/" + id);
			return this.http.Put(uri, this.formatter.ToContent(entity));
		}

		/// <summary>
		/// Tries to delete the specified entity.
		/// </summary>
		public HttpResponseMessage TryDelete<T>(string id)
		{
			var resource = this.convention.GetResourceName(typeof(T));
			var uri = new Uri(this.BaseAddress, resource + "/" + id);
			return this.http.Delete(uri);
		}

		/// <summary>
		/// Creates a query for the given entity type, which can be 
		/// filtered using <c>Where</c>.
		/// </summary>
		public HttpEntityQuery<T> Query<T>()
		{
			return new HttpEntityQuery<T>(this.ExecuteQuery);
		}

		private IEnumerable<T> ExecuteQuery<T>(Expression<Func<T, bool>> predicate)
		{
			var resource = this.convention.GetResourceName(typeof(T));
			var response = this.http.Query<T>(resource, predicate);

			if (response.IsSuccessStatusCode)
			{
				ThrowIfUnsupportedContentType(response);
				return this.formatter.FromContent<List<T>>(response.Content);
			}

			throw new HttpResponseException(response);
		}

		private void ThrowIfUnsupportedContentType(HttpResponseMessage response)
		{
			if (response.Content.Headers.ContentType.MediaType != this.formatter.ContentType)
				throw new NotSupportedException(string.Format(
					"Received reponse with content type '{0}' but formatter supports '{1}'.",
					response.Content.Headers.ContentType,
					this.formatter.ContentType));
		}
	}
}