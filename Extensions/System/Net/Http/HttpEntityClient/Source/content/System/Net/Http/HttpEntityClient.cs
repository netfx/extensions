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
	internal class HttpEntityClient : IDisposable
	{
		private bool disposed;
		private HttpClient http;

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
		public HttpEntityClient(Uri baseAddress, IEntityFormatter formatter, IEntityResourceNameConvention convention)
		{
			this.BaseAddress = baseAddress;
			this.http = new HttpClient(baseAddress);
			this.http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(formatter.ContentType));
			this.EntityFormatter = formatter;
			this.ResourceNameConvention = convention;
		}

		public IEntityResourceNameConvention ResourceNameConvention { get; private set; }
		public IEntityFormatter EntityFormatter { get; set; }

		/// <summary>
		/// Gets the base address for this client.
		/// </summary>
		public Uri BaseAddress { get; private set; }

		/// <summary>
		/// Gets the underlying HTTP client that is used to perform requests.
		/// </summary>
		public HttpClient Http { get { return this.http; } }

		/// <summary>
		/// Deletes the specified entity.
		/// </summary>
		public void Delete<T>(string id)
		{
			var response = TryDelete<T>(id);

			if (!response.IsSuccessStatusCode)
				throw new HttpEntityException(response);
		}

		/// <summary>
		/// Gets the entity with the given id.
		/// </summary>
		/// <exception cref="HttpEntityException">The request did not succeed.</exception>
		public T Get<T>(string id)
		{
			var entity = default(T);
			var response = TryGet<T>(id, out entity);

			if (!response.IsSuccessStatusCode)
				throw new HttpEntityException(response);

			return entity;
		}

		/// <summary>
		/// Posts the specified entity to the entity resource
		/// and returns the state persisted by the service, which 
		/// should be returned in the response body.
		/// </summary>
		public T Post<T>(T entity)
		{
			var saved = default(T);
			var response = TryPost<T>(entity, out saved);

			if (response.StatusCode != HttpStatusCode.Created)
				throw new HttpEntityException(response);

			return saved;
		}

		/// <summary>
		/// Puts the specified entity to the service.
		/// </summary>
		public void Put<T>(string id, T entity)
		{
			var response = TryPut(id, entity);

			if (!response.IsSuccessStatusCode)
				throw new HttpEntityException(response);
		}

		/// <summary>
		/// Tries to get the entity with the given id.
		/// </summary>
		public HttpResponseMessage TryGet<T>(string id, out T entity)
		{
			var resource = this.ResourceNameConvention.GetResourceName(typeof(T));
			var uri = new Uri(this.BaseAddress, resource + "/" + id);
			var response = this.http.Get(uri);
			entity = default(T);

			if (response.IsSuccessStatusCode)
			{
				ThrowIfUnsupportedContentType(response);
				entity = this.EntityFormatter.FromContent<T>(response.Content);
			}

			return response;
		}

		/// <summary>
		/// Tries to posts the specified entity and retrieves the new id 
		/// that was assigned by the service from the Location header, if any.
		/// </summary>
		public HttpResponseMessage TryPost<T>(T entity, out T saved)
		{
			var resource = this.ResourceNameConvention.GetResourceName(typeof(T));
			var uri = new Uri(this.BaseAddress, resource);
			var response = this.http.Post(uri, this.EntityFormatter.ToContent(entity));
			saved = default(T);

			if (response.StatusCode == HttpStatusCode.Created)
				saved = this.EntityFormatter.FromContent<T>(response.Content);

			return response;
		}

		/// <summary>
		/// Tries to put the specified entity to the service.
		/// </summary>
		public HttpResponseMessage TryPut<T>(string id, T entity)
		{
			var resource = this.ResourceNameConvention.GetResourceName(typeof(T));
			var uri = new Uri(this.BaseAddress, resource + "/" + id);
			return this.http.Put(uri, this.EntityFormatter.ToContent(entity));
		}

		/// <summary>
		/// Tries to delete the specified entity.
		/// </summary>
		public HttpResponseMessage TryDelete<T>(string id)
		{
			var resource = this.ResourceNameConvention.GetResourceName(typeof(T));
			var uri = new Uri(this.BaseAddress, resource + "/" + id);
			return this.http.Delete(uri);
		}

		/// <summary>
		/// Creates a query for the given entity type, that will be
		/// executed when the queryable is enumerated.
		/// </summary>
		/// <typeparam name="T">Type of entity being queried.</typeparam>
		/// <param name="criteria">Optional search criteria to be applied by the service, 
		/// sent as a "q=" query string parameter. Useful to overcome limitations 
		/// in the underlying query support in WCF.</param>
		public IQueryable<T> Query<T>(string criteria = null)
		{
			return new HttpQuery<T>(this, criteria);
		}

		/// <summary>
		/// Releases the underlying <see cref="Http"/> client.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases the underlying <see cref="Http"/> client.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					this.http.Dispose();
				}

				disposed = true;
			}
		}

		~HttpEntityClient()
		{
			Dispose(false);
		}

		private void ThrowIfUnsupportedContentType(HttpResponseMessage response)
		{
			if (response.Content.Headers.ContentType.MediaType != this.EntityFormatter.ContentType)
				throw new NotSupportedException(string.Format(
					"Received reponse with content type '{0}' but formatter supports '{1}'.",
					response.Content.Headers.ContentType,
					this.EntityFormatter.ContentType));
		}

		#region HttpQuery

		private class HttpQueryProvider : IQueryProvider
		{
			private HttpQuery query;
			private MethodInfo execute = typeof(HttpQueryProvider).GetMethods()
				.First(x => x.Name == "Execute" && x.ContainsGenericParameters);

			public HttpQueryProvider(HttpQuery query)
			{
				this.query = query;
			}

			public IQueryable CreateQuery(Expression expression)
			{
				var elementType = TypeSystem.GetElementType(expression.Type);
				try
				{
					return (IQueryable)Activator.CreateInstance(typeof(HttpQuery<>).MakeGenericType(elementType), new object[] { this, expression });
				}
				catch (System.Reflection.TargetInvocationException tie)
				{
					throw tie.InnerException;
				}
			}

			// Queryable's collection-returning standard query operators call this method.
			public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
			{
				// Unsupported expressions see: http://msdn.microsoft.com/en-us/library/ee622463.aspx#unsupportedMethods
				if (IsSelect(expression))
				{
					// Select materializes.
					var methodCall = (MethodCallExpression)expression;
					var enumerableCandidates = typeof(Enumerable).GetMethods().Where(x =>
						x.Name == methodCall.Method.Name &&
						x.GetParameters().Length == methodCall.Method.GetParameters().Length).ToList();

					var genericSelect = FindCompatible(enumerableCandidates, methodCall.Method.GetGenericMethodDefinition()).FirstOrDefault();
					if (genericSelect == null)
						throw new NotSupportedException();

					var concreteSelect = genericSelect.MakeGenericMethod(methodCall.Method.GetGenericArguments());
					var compiledArgs = methodCall.Arguments.Skip(1)
						.Select(x => x.NodeType == ExpressionType.Quote ? ((UnaryExpression)x).Operand : x)
						.OfType<LambdaExpression>()
						.Select(lambda => lambda.Compile());

					// Execute the "source" argument for the Select.
					var source = Execute(methodCall.Arguments[0]);
					var result = concreteSelect.Invoke(null, new[] { source }.Concat(compiledArgs).ToArray());

					return (IQueryable<TResult>)Reflect.GetMethod(() => Queryable.AsQueryable<TResult>(null))
						.Invoke(null, new[] { result });
				}

				return new HttpQuery<TResult>(this.query.EntityClient, this, expression);
			}

			public object Execute(Expression expression)
			{
				var elementType = TypeSystem.GetElementType(expression.Type);
				var sequenceType = typeof(IEnumerable<>).MakeGenericType(elementType);

				return execute.MakeGenericMethod(sequenceType)
					.Invoke(this, new[] { expression });
			}

			// Queryable's "single value" standard query operators call this method.
			public TResult Execute<TResult>(Expression expression)
			{
				var elementType = TypeSystem.GetElementType(typeof(TResult));
				var uri = BuildRequestUri(expression, elementType);

				// http://localhost:20000/products?$skip=1&$top=1
				// Append &q={query}
				if (!string.IsNullOrEmpty(this.query.Criteria))
				{
					var builder = new UriBuilder(uri);
					var querystring = HttpUtility.ParseQueryString(builder.Query);
					querystring.Add("q", this.query.Criteria);
					builder.Query = querystring.ToString();

					uri = builder.Uri;
				}

#if DEBUG
				Debug.WriteLine("Query uri: " + uri);
#endif

				var response = this.query.EntityClient.http.Get(uri);
				if (!response.IsSuccessStatusCode)
					throw new HttpEntityException(response);

				return this.query.EntityClient.EntityFormatter.FromContent<TResult>(response.Content);
			}

			/// <summary>
			/// Builds the actual request URI by setting the expression in a DS 
			/// query context and invoking their translation. Also, remove () from 
			/// the resource name and builds a Uri that Web Api understands.
			/// </summary>
			private Uri BuildRequestUri(Expression expression, Type elementType)
			{
				var context = new DataServiceContext(this.query.EntityClient.BaseAddress);

				// Invoke CreateQuery<elementType>();
				var query = (DataServiceQuery)Reflect<DataServiceContext>
					.GetMethod(x => x.CreateQuery<string>(null))
					.GetGenericMethodDefinition()
					.MakeGenericMethod(elementType)
					.Invoke(context, new object[] { this.query.EntityClient.ResourceNameConvention.GetResourceName(elementType) });

				// Replace entity query expression with the resource set expression from the CreateQuery above
				var replaced = new HttpQueryVisitor(this.query, query.Expression).Visit(expression);
				// \o/ Invoke the constructor again to force it to use the full expression.
				typeof(DataServiceQuery<>)
					.MakeGenericType(elementType)
					.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(Expression), query.Provider.GetType() }, null)
					.Invoke(query, new object[] { replaced, query.Provider });

				return new Uri(query.RequestUri.ToString().Replace("()?$", "?$"));
			}

			private static bool IsSelect(Expression expression)
			{
				return expression.NodeType == ExpressionType.Call &&
					((MethodCallExpression)expression).Method.DeclaringType == typeof(Queryable) &&
					((MethodCallExpression)expression).Method.Name.StartsWith("Select");
			}

			private IEnumerable<MethodInfo> FindCompatible(IEnumerable<MethodInfo> candidateMethods, MethodInfo targetSignature)
			{
				var queryableArgs = targetSignature
					.GetParameters()
					// Always skip the "source" for the extension method, which we know 
					// we'll turn into an IEnumerable<T>
					.Skip(1)
					// From the Queryable method signature, we need the generic argument 
					// to the Expression<T> that makes the selectors, because in Enumerable 
					// they will be Func<T>, not Expression<Func<T>>
					.Select(x => x.ParameterType.GetGenericArguments()[0])
					.ToList();

				foreach (var select in candidateMethods)
				{
					var selArgs = select.GetParameters()
						// Always skip the "source" for the extension method, which we know 						
						.Skip(1)
						.Select(x => x.ParameterType).ToList();
					var equal = true;
					for (int i = 0; i < selArgs.Count; i++)
					{
						// Use ToString as the Func`1[TSource, TResult] generic type 
						// has no other way of comparison that I could find.
						if (selArgs[i].ToString() != queryableArgs[i].ToString())
							equal = false;
					}

					if (equal)
						yield return select;
				}
			}
		}

		/// <summary>
		/// Replaces occurrences of the entity query constant with a data service 
		/// resource set expression which is needed for Uri translation. Also 
		/// translates IQueryable projections to local in-memory Enumerable 
		/// projections, as projections don't seem to be supported by Web Api.
		/// </summary>
		private class HttpQueryVisitor : ExpressionVisitor
		{
			private HttpQuery entityQuery;
			private Expression resourceSetExpression;

			public HttpQueryVisitor(HttpQuery entityQuery, Expression resourceSetExpression)
			{
				this.entityQuery = entityQuery;
				this.resourceSetExpression = resourceSetExpression;
			}

			protected override Expression VisitConstant(ConstantExpression node)
			{
				if (node.Value == this.entityQuery)
					return this.resourceSetExpression;

				return base.VisitConstant(node);
			}
		}

		/// <summary>
		/// Represents the query being built for execution.
		/// </summary>
		private abstract class HttpQuery
		{
			public HttpQuery(HttpEntityClient client, string criteria)
			{
				this.EntityClient = client;
				this.Criteria = criteria;
				this.Provider = new HttpQueryProvider(this);
				this.Expression = Expression.Constant(this);
			}

			public string Criteria { get; private set; }
			public IQueryProvider Provider { get; protected set; }
			public Expression Expression { get; protected set; }
			public HttpEntityClient EntityClient { get; private set; }
			public abstract Type ElementType { get; }
		}

		private class HttpQuery<T> : HttpQuery, IOrderedQueryable<T>
		{
			public HttpQuery(HttpEntityClient client, string criteria = null)
				: base(client, criteria)
			{
			}

			internal HttpQuery(HttpEntityClient client, HttpQueryProvider provider, Expression expression, string query = null)
				: base(client, query)
			{
				if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
					throw new ArgumentOutOfRangeException("expression");

				this.Provider = provider;
				this.Expression = expression;
			}

			public override Type ElementType { get { return typeof(T); } }

			public IEnumerator<T> GetEnumerator()
			{
				var result = this.Provider.Execute<IEnumerable<T>>(Expression);
				return result == null ? Enumerable.Empty<T>().GetEnumerator() : result.GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return (Provider.Execute<System.Collections.IEnumerable>(Expression)).GetEnumerator();
			}
		}

		/// <summary>
		/// This is basically what everyone else (OData, MSDN samples, etc.) 
		/// use for determining the element type given a sequence type (i.e. IEnumerable{T}).
		/// </summary>
		private static class TypeSystem
		{
			internal static Type GetElementType(Type seqType)
			{
				Type ienum = FindIEnumerable(seqType);
				if (ienum == null) return seqType;
				return ienum.GetGenericArguments()[0];
			}

			private static Type FindIEnumerable(Type seqType)
			{
				if (seqType == null || seqType == typeof(string))
					return null;

				if (seqType.IsArray)
					return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

				if (seqType.IsGenericType)
				{
					foreach (Type arg in seqType.GetGenericArguments())
					{
						Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
						if (ienum.IsAssignableFrom(seqType))
						{
							return ienum;
						}
					}
				}

				Type[] ifaces = seqType.GetInterfaces();
				if (ifaces != null && ifaces.Length > 0)
				{
					foreach (Type iface in ifaces)
					{
						Type ienum = FindIEnumerable(iface);
						if (ienum != null) return ienum;
					}
				}

				if (seqType.BaseType != null && seqType.BaseType != typeof(object))
				{
					return FindIEnumerable(seqType.BaseType);
				}

				return null;
			}
		}

		#endregion
	}
}
