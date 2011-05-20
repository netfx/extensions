using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Linq.Expressions;
using System.Net.Http;
using Microsoft.ApplicationServer.Http.Description;
using System.Net;
using System.Net.Http.Entity;
using System.Collections.Concurrent;
using System.IO;

namespace Tests
{
	public class HttpEntityClientSpec
	{
		[Fact]
		public void WhenGetting_ThenRetrieves()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);

				var product = client.Get<Product>("1");

				Assert.NotNull(product);
				Assert.Equal("kzu", product.Owner.Name);
			}
		}

		[Fact]
		public void WhenPostNew_ThenSavesAndRetrievesId()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);
				var product = new Product { Owner = new User { Id = 1, Name = "kzu" } };

				var id = client.Post(product);

				Assert.Equal("4", id);

				var saved = client.Get<Product>("4");

				Assert.Equal(saved.Id, int.Parse(id));
				Assert.Equal(saved.Owner.Id, product.Owner.Id);
				Assert.Equal(saved.Owner.Name, product.Owner.Name);
			}
		}

		[Fact]
		public void WhenDeletingEntity_ThenGetFails()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);

				client.Delete<Product>("1");
				var exception = Assert.Throws<HttpResponseException>(() => client.Get<Product>("25"));

				Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
			}
		}

		[Fact]
		public void WhenDeleteFails_ThenThrows()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);

				var exception = Assert.Throws<HttpResponseException>(() => client.Delete<Product>("25"));

				Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
			}
		}

		[Fact]
		public void WhenPostFails_ThenThrows()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);

				var exception = Assert.Throws<HttpResponseException>(() => client.Post<Product>(null));

				Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
			}
		}

		[Fact]
		public void WhenPutNew_ThenSaves()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);
				var product = new Product { Owner = new User { Id = 1, Name = "kzu" } };

				client.Put("4", product);

				var saved = client.Get<Product>("4");

				Assert.Equal(saved.Id, 4);
				Assert.Equal(saved.Owner.Id, product.Owner.Id);
				Assert.Equal(saved.Owner.Name, product.Owner.Name);
			}
		}

		[Fact]
		public void WhenPutFails_ThenThrows()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);
				// We're putting a null which is invalid.
				var exception = Assert.Throws<HttpResponseException>(() => client.Put<Product>("25", null));

				Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
			}
		}

		[Fact]
		public void WhenPutUpdate_ThenSaves()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);
				var product = new Product { Id = 1, Owner = new User { Id = 1, Name = "vga" } };

				client.Put("1", product);

				var saved = client.Get<Product>("1");

				Assert.Equal(saved.Owner.Name, product.Owner.Name);
			}
		}

		[Fact]
		public void WhenGetFails_ThenThrows()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);

				var exception = Assert.Throws<HttpResponseException>(() => client.Get<Product>("25"));

				Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
			}
		}

		[Fact]
		public void WhenTryGetFails_ThenReturnsResponse()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);
				var product = default(Product);
				var response = client.TryGet<Product>("25", out product);

				Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
				Assert.Null(product);
			}
		}

		[Fact]
		public void WhenTryGetSucceeds_ThenPopulatesEntity()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);
				var product = default(Product);
				var response = client.TryGet<Product>("1", out product);

				Assert.True(response.IsSuccessStatusCode);
				Assert.NotNull(product);
				Assert.Equal("kzu", product.Owner.Name);
			}
		}

		[Fact]
		public void WhenQuerying_ThenPopulatesMatchingEntities()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);
				var ids = client.Query<Product>().Where(x => x.Owner.Name == "kzu").Select(x => x.Id).ToList();
				var products = client.Query<Product>().Where(x => x.Owner.Name == "kzu").ToList();

				Assert.Equal(2, ids.Count);
				Assert.True(products.All(x => x.Owner.Name == "kzu"));
			}
		}

		[Fact]
		public void WhenQueryingAndNoMatches_ThenReturnsEmptyEnumerable()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityClient(ws.BaseUri);
				var products = client.Query<Product>().Where(x => x.Owner.Name == "foo").ToList();

				Assert.Equal(0, products.Count);
			}
		}

		public class ServiceConfiguration : HttpHostConfiguration
		{
			public ServiceConfiguration()
			{
				this.OperationHandlerFactory.Formatters.Insert(0, new JsonNetMediaTypeFormatter());
				this.AddMessageHandlers(typeof(LoggingChannel));
				this.SetErrorHandler<ErrorHandler>();
				this.Configure.SetResourceFactory(new CachingResourceFactory());
			}
		}

		public class CachingResourceFactory : IResourceFactory
		{
			private ConcurrentDictionary<Type, object> cachedTypes = new ConcurrentDictionary<Type, object>();

			public object GetInstance(Type serviceType, System.ServiceModel.InstanceContext instanceContext, HttpRequestMessage request)
			{
				return cachedTypes.GetOrAdd(serviceType, type => Activator.CreateInstance(type));
			}

			public void ReleaseInstance(System.ServiceModel.InstanceContext instanceContext, object service)
			{
			}
		}

		public class ErrorHandler : Microsoft.ApplicationServer.Http.Dispatcher.HttpErrorHandler
		{
			protected override bool OnHandleError(Exception error)
			{
				System.Diagnostics.Trace.TraceError(error.ToString());
				return true;
			}

			protected override HttpResponseMessage OnProvideResponse(Exception error)
			{
				return new HttpResponseMessage(HttpStatusCode.InternalServerError, error.Message);
			}
		}

		public class LoggingChannel : DelegatingChannel
		{
			public LoggingChannel(HttpMessageChannel handler)
				: base(handler)
			{
			}

			protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
			{
				var body = "";
				if (request.Content != null)
					body = request.Content.ReadAsString();
				System.Diagnostics.Trace.TraceInformation("Begin Request: {0} {1}\r\n{2}", request.Method, request.RequestUri, body);

				return base.SendAsync(request, cancellationToken)
					.ContinueWith(task =>
					{
						if (task.Result.Content != null)
						{
							var responseBody = task.Result.Content.ReadAsString();
							System.Diagnostics.Trace.TraceInformation("Begin Response: {0} (Reason: {1})\r\n{2}", task.Result.StatusCode, task.Result.ReasonPhrase, responseBody);
						}

						return task.Result;
					}, cancellationToken);
			}
		}
	}
}