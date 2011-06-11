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
using Microsoft.ApplicationServer.Http.Dispatcher;
using System.Web;

namespace Tests
{
	public class HttpEntityConventionClientSpec
	{
		[Fact]
		public void WhenGetting_ThenRetrieves()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityConventionClient(ws.BaseUri);

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
				var client = new HttpEntityConventionClient(ws.BaseUri);
				var product = new Product { Owner = new User { Id = 1, Name = "kzu" } };

				var saved = client.Post(product);

				Assert.Equal(4, saved.Id);

				Assert.Equal(saved.Owner.Id, product.Owner.Id);
				Assert.Equal(saved.Owner.Name, product.Owner.Name);
			}
		}

		[Fact]
		public void WhenDeletingEntity_ThenGetFails()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityConventionClient(ws.BaseUri);

				client.Delete<Product>("1");
				var exception = Assert.Throws<HttpEntityException>(() => client.Get<Product>("25"));

				Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
			}
		}

		[Fact]
		public void WhenDeleteFails_ThenThrows()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityConventionClient(ws.BaseUri);

				var exception = Assert.Throws<HttpEntityException>(() => client.Delete<Product>("25"));

				Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
			}
		}

		[Fact]
		public void WhenPostFails_ThenThrows()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityConventionClient(ws.BaseUri);

				var exception = Assert.Throws<HttpEntityException>(() => client.Post<Product>(null));

				Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
			}
		}

		[Fact]
		public void WhenPutNew_ThenSaves()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", true, new ServiceConfiguration()))
			{
				var client = new HttpEntityConventionClient(ws.BaseUri);
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
				var client = new HttpEntityConventionClient(ws.BaseUri);
				// We're putting a null which is invalid.
				var exception = Assert.Throws<HttpEntityException>(() => client.Put<Product>("25", null));

				Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
			}
		}

		[Fact]
		public void WhenPutUpdate_ThenSaves()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", true, new ServiceConfiguration()))
			{
				var client = new HttpEntityConventionClient(ws.BaseUri);
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
				var client = new HttpEntityConventionClient(ws.BaseUri);

				var exception = Assert.Throws<HttpEntityException>(() => client.Get<Product>("25"));

				Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
			}
		}

		[Fact]
		public void WhenTryGetFails_ThenReturnsResponse()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityConventionClient(ws.BaseUri);
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
				var client = new HttpEntityConventionClient(ws.BaseUri);
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
				var client = new HttpEntityConventionClient(ws.BaseUri);
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
				var client = new HttpEntityConventionClient(ws.BaseUri);
				var products = client.Query<Product>().Where(x => x.Owner.Name == "foo").ToList();

				Assert.Equal(0, products.Count);
			}
		}

		[Fact]
		public void WhenSkipTakeOnly_ThenReturnsSingleElement()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityConventionClient(ws.BaseUri);
				var products = client.Query<Product>().Skip(1).Take(1).ToList();

				Assert.Equal(1, products.Count);
				Assert.Equal(2, products[0].Id);
			}
		}

		[Fact]
		public void WhenOrderByTake_ThenReturnsOrdered()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityConventionClient(ws.BaseUri);
				var products = client.Query<Product>().OrderBy(x => x.Title).Take(2).ToList();

				Assert.Equal(2, products.Count);
				Assert.Equal("A", products[0].Title);
				Assert.Equal("B", products[1].Title);
			}
		}

		[Fact]
		public void WhenQueryingWithExtraCriteria_ThenPopulatesMatchingEntities()
		{
			using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
			{
				var client = new HttpEntityConventionClient(ws.BaseUri);
				var products = client.Query<Product>("kzu").ToList();

				Assert.True(products.All(x => x.Owner.Name == "kzu"));
			}
		}

		public class ServiceConfiguration : HttpHostConfiguration
		{
			public ServiceConfiguration()
			{
				this.OperationHandlerFactory.Formatters.Insert(0, new JsonNetMediaTypeFormatter());
				this.AddMessageHandlers(typeof(LoggingChannel));
				this.SetErrorHandler<ErrorHandler>();
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