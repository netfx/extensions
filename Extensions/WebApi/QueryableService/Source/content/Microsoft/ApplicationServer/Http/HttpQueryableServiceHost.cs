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
using Microsoft.ApplicationServer.Http.Activation;
using Microsoft.ApplicationServer.Http.Description;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.ApplicationServer.Http;
using System.Reflection;
using System.Collections;
using System.Web;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationServer.Http.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Runtime.Remoting.Messaging;
using Microsoft.ApplicationServer.Http.Channels;
using System.Net;
using System.Globalization;
using System.Collections.ObjectModel;

namespace Microsoft.ApplicationServer.Http.Activation
{
	/// <summary>
	/// Provides a custom service host that exposes the total count of a queryable resource 
	/// query as an HTTP header named <c>X-TotalCount</c> and automatically used by the 
	/// HttpEntityClient nuget.
	/// </summary>
	internal class HttpQueryableServiceHost : HttpConfigurableServiceHost
	{
		/// <summary>
		/// Header emitted by the service that allows client-side 
		/// paging by retrieving the server-side total count of entities in a given query.
		/// </summary>
		public const string TotalCountHeader = "X-TotalCount";

		private const string QueryLimitData = "QueryLimit";

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpQueryableServiceHost"/> class.
		/// </summary>
		public HttpQueryableServiceHost(object singletonInstance, int queryLimit, IHttpHostConfigurationBuilder builder, params Uri[] baseAddresses)
			: base(singletonInstance, Configure(builder, queryLimit), baseAddresses)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpQueryableServiceHost"/> class.
		/// </summary>
		public HttpQueryableServiceHost(Type serviceType, int queryLimit, IHttpHostConfigurationBuilder builder, params Uri[] baseAddresses)
			: base(serviceType, Configure(builder, queryLimit), baseAddresses)
		{
		}

		private static IHttpHostConfigurationBuilder Configure(IHttpHostConfigurationBuilder builder, int queryLimit)
		{
			builder.SetMessageHandlerFactory(new HttpQueryMessageHandlerFactory(queryLimit, builder.Configuration.MessageHandlerFactory));
			builder.SetOperationHandlerFactory(new HttpQueryOperationHandlerFactory(builder.Configuration.OperationHandlerFactory));

			return builder;
		}

		/// <summary>
		/// Replaces the built-in HttpBehavior with our derived one.
		/// </summary>
		protected override void OnOpening()
		{
			base.OnOpening();
			if (base.Description != null)
			{
				foreach (var serviceEndpoint in base.Description.Endpoints)
				{
					if (serviceEndpoint.Binding != null)
					{
						var behavior = serviceEndpoint.Behaviors.Find<HttpBehavior>();
						if (behavior != null)
						{
							// Replace with our behavior.
							serviceEndpoint.Behaviors.Remove(behavior);
							serviceEndpoint.Behaviors.Add(new HttpQueryableBehavior
							{
								HelpEnabled = behavior.HelpEnabled,
								OperationHandlerFactory = behavior.OperationHandlerFactory,
								TrailingSlashMode = behavior.TrailingSlashMode
							});
						}
					}
				}
			}
		}

		/// <summary>
		/// Factory that wraps any existing operation factory with ours that 
		/// adds support for multi-value uri query parameters.
		/// </summary>
		private class HttpQueryOperationHandlerFactory : HttpOperationHandlerFactory
		{
			private HttpOperationHandlerFactory innerFactory;

			public HttpQueryOperationHandlerFactory(HttpOperationHandlerFactory innerFactory = null)
				: base(innerFactory != null ? innerFactory.Formatters : new MediaTypeFormatterCollection())
			{
				this.innerFactory = innerFactory;
			}

			protected override Collection<HttpOperationHandler> OnCreateRequestHandlers(ServiceEndpoint endpoint, HttpOperationDescription operation)
			{
				var handlers = this.innerFactory != null ?
					this.innerFactory.CreateRequestHandlers(endpoint, operation) :
					base.OnCreateRequestHandlers(endpoint, operation);

				handlers.Add(new UriMultiValueTemplateHandler(endpoint.Address.Uri, operation.GetUriTemplate()));

				return handlers;
			}

			protected override Collection<HttpOperationHandler> OnCreateResponseHandlers(ServiceEndpoint endpoint, HttpOperationDescription operation)
			{
				if (this.innerFactory != null)
					return this.innerFactory.CreateResponseHandlers(endpoint, operation);

				return base.OnCreateResponseHandlers(endpoint, operation);
			}
		}

		/// <summary>
		/// Adds built-in support for query string arrays if needed. No conversion 
		/// to other types of arrays are implemented.
		/// </summary>
		private class UriMultiValueTemplateHandler : HttpOperationHandler
		{
			public UriMultiValueTemplateHandler(Uri baseAddress, UriTemplate uriTemplate)
			{
				this.BaseAddress = baseAddress;
				this.UriTemplate = uriTemplate;
			}

			public Uri BaseAddress { get; private set; }
			public UriTemplate UriTemplate { get; private set; }

			protected override sealed IEnumerable<HttpParameter> OnGetInputParameters()
			{
				return new HttpParameter[] { HttpParameter.RequestMessage };
			}

			protected override sealed IEnumerable<HttpParameter> OnGetOutputParameters()
			{
				var numberOfVariables = this.UriTemplate.PathSegmentVariableNames.Count + this.UriTemplate.QueryValueVariableNames.Count;

				var parameters = new List<HttpParameter>(numberOfVariables);
				parameters.AddRange(this.UriTemplate
					.PathSegmentVariableNames
					.Select(name => new HttpParameter(name, typeof(string))));

				parameters.AddRange(this.UriTemplate
					.QueryValueVariableNames
					.Select(name => new HttpParameter(name, typeof(string[]))));

				return parameters;
			}

			protected override sealed object[] OnHandle(object[] input)
			{
				var requestMessage = input[0] as HttpRequestMessage;
				var uri = requestMessage.RequestUri;
				var numberOfParameters = this.OutputParameters.Count;
				var output = new object[numberOfParameters];
				if (uri != null)
				{
					var match = this.UriTemplate.Match(this.BaseAddress, uri);
					if (match == null)
					{
						throw new InvalidOperationException(string.Format(
							CultureInfo.CurrentCulture,
							"Template '{0}' does not match '{1}.",
							this.UriTemplate.ToString(),
							uri));
					}

					// Wrong handling here by the UriTemplateMatch, as it always concatenates 
					// the multiple query values in a uri.
					for (int i = 0; i < numberOfParameters; i++)
					{
						var key = match.BoundVariables.Keys[i];
						// We access by key as the index will be different if there are 
						// extra query string arguments that haven't been matched.
						output[i] = match.QueryParameters.GetValues(key);
					}
				}

				return output;
			}
		}

		/// <summary>
		/// Factory that wraps any existing handler factory with ours that 
		/// places the total count in the response headers.
		/// </summary>
		private class HttpQueryMessageHandlerFactory : HttpMessageHandlerFactory
		{
			private int queryLimit;
			private HttpMessageHandlerFactory innerFactory;

			public HttpQueryMessageHandlerFactory(int queryLimit, HttpMessageHandlerFactory innerFactory = null)
			{
				this.queryLimit = queryLimit;
				this.innerFactory = innerFactory;
			}

			protected override HttpMessageChannel OnCreate(HttpMessageChannel innerChannel)
			{
				HttpMessageChannel channel = new HttpQueryMessageHandler(innerChannel, this.queryLimit);

				if (this.innerFactory != null)
					channel = this.innerFactory.Create(channel);

				return channel;
			}

			/// <summary>
			/// Handler that takes the total count value set in the call context by the query composer 
			/// and adds it to the response headers.
			/// </summary>
			private class HttpQueryMessageHandler : DelegatingChannel
			{
				private int queryLimit;

				public HttpQueryMessageHandler(HttpMessageChannel inner, int queryLimit)
					: base(inner)
				{
					this.queryLimit = queryLimit;
				}

				protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
				{
					CallContext.LogicalSetData(QueryLimitData, this.queryLimit);

					var response = base.Send(request, cancellationToken);

					AddPagingHeaders(response);

					return response;
				}

				protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
				{
					CallContext.LogicalSetData(QueryLimitData, this.queryLimit);

					return base.SendAsync(request, cancellationToken)
						// Set headers.
						.ContinueWith<HttpResponseMessage>(task =>
						{
							AddPagingHeaders(task.Result);
							return task.Result;
						}, TaskContinuationOptions.OnlyOnRanToCompletion);
				}

				private void AddPagingHeaders(HttpResponseMessage response)
				{
					var count = CallContext.LogicalGetData(TotalCountHeader);

					if (count != null)
						response.Headers.AddWithoutValidation(TotalCountHeader, count.ToString());
				}
			}
		}

		/// <summary>
		/// Extended behavior that sets the query composer to our own that tracks 
		/// the total query count value.
		/// </summary>
		private class HttpQueryableBehavior : HttpBehavior
		{
			protected override void OnApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
			{
				base.OnApplyDispatchBehavior(endpoint, endpointDispatcher);

				var httpOperations = endpoint.Contract.Operations.Select(x => x.ToHttpOperationDescription());

				foreach (var operation in httpOperations)
				{
					if (operation.ReturnValue != null)
					{
						var returnType = operation.ReturnValue.Type;
						if (returnType.IsGenericType && (returnType.GetGenericTypeDefinition() == typeof(IQueryable<>)))
						{
							var composition = operation.Behaviors.Find<QueryCompositionAttribute>();
							if (composition != null)
							{
								operation.Behaviors.Remove(composition);
								operation.Behaviors.Add(new QueryCompositionAttribute(typeof(CountQueryComposer)));
							}
						}
					}
				}
			}

			/// <summary>
			/// Custom composer implementation that delegates all work to the WebApi
			/// built-in UrlComposer, but counts the total results before returning the response, 
			/// and places the value in the call context.
			/// </summary>
			private class CountQueryComposer : IQueryComposer
			{
				private static readonly MethodInfo CountMethod = typeof(Queryable).GetMethods().Single(m => m.Name == "LongCount" && m.GetParameters().Length == 1);

				public IEnumerable ComposeQuery(IEnumerable rootedQuery, string url)
				{
					var urlComposer = new UrlQueryComposer();

					// We already know it's an IQueryable.
					var elementType = rootedQuery.GetType().GetGenericArguments()[0];

					// To retrieve the actual count without Take/Skip, remove those 
					// arguments from the query string and execute the count using 
					// the query translator too.
					var queryString = HttpUtility.ParseQueryString(new Uri(url).Query);
					queryString.Remove("$skip");
					queryString.Remove("$top");
					var filteredUri = new UriBuilder(url);
					filteredUri.Query = queryString.ToString();

					var resultForCount = urlComposer.ComposeQuery(rootedQuery, filteredUri.Uri.AbsoluteUri);
					var count = CountMethod.MakeGenericMethod(elementType).Invoke(null, new object[] { resultForCount });

					// Ensure limit is not exceeded.
					queryString = HttpUtility.ParseQueryString(new Uri(url).Query);
					var topLimit = (int)CallContext.LogicalGetData(QueryLimitData);
					var top = topLimit;

					if (int.TryParse(queryString["$top"], out top))
					{
						if (top > topLimit)
							top = topLimit;
					}
					else
					{
						top = topLimit;
					}

					queryString["$top"] = top.ToString();

					var sanitizedUri = new UriBuilder(url);
					sanitizedUri.Query = queryString.ToString();

					var result = urlComposer.ComposeQuery(rootedQuery, sanitizedUri.Uri.AbsoluteUri);

					CallContext.LogicalSetData(TotalCountHeader, count.ToString());

					return result;
				}
			}
		}
	}
}
