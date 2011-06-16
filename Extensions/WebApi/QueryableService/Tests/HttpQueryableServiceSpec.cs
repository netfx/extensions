using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.ServiceModel;
using System.ServiceModel.Web;
using Microsoft.ApplicationServer.Http.Description;
using Microsoft.ApplicationServer.Http;
using Microsoft.ApplicationServer.Http.Activation;
using System.Diagnostics;
using System.Diagnostics.Sdk;
using System.Net.Http;
using LinqKit;
using System.Linq.Expressions;
using Microsoft.ApplicationServer.Http.Dispatcher;

public class HttpQueryableServiceSpec : IDisposable
{
	private TraceListener listener;

	public HttpQueryableServiceSpec()
	{
		this.listener = new ConsoleTraceListener();
		this.listener.Name = Guid.NewGuid().ToString();

		TracerExtensibility.AddListener(SourceName.For<TracingChannel>(), listener);
		TracerExtensibility.SetTracingLevel(SourceName.For<TracingChannel>(), SourceLevels.All);
	}

	public void Dispose()
	{
		this.listener.Flush();
		TracerExtensibility.RemoveListener(SourceName.For<TracingChannel>(), listener.Name);
	}

	[Fact]
	public void WhenQuerying_ThenSendsCountHeader()
	{
		var baseUri = new Uri("http://localhost:20000");
		var service = new ProductsService();
		var config = HttpHostConfiguration.Create()
			.SetResourceFactory(new SingletonResourceFactory(service))
			.AddMessageHandlers(typeof(TracingChannel));

		using (new SafeHostDisposer(
			new HttpQueryableServiceHost(typeof(ProductsService), 25, config, new Uri(baseUri, "products"))))
		{
			var client = new HttpEntityClient(baseUri);
			var query = (IHttpEntityQuery<Product>)client.Query<Product>("products").Skip(10).Take(10);

			var result = query.Execute();

			Assert.Equal(100, result.TotalCount);
			Assert.Equal(10, result.Count());
			Assert.Equal(10, result.First().Id);
		}
	}

	[Fact]
	public void WhenQueryingOverLimit_ThenGetsLimitedResults()
	{
		var baseUri = new Uri("http://localhost:20000");
		var service = new ProductsService();
		var config = HttpHostConfiguration.Create()
			.SetResourceFactory(new SingletonResourceFactory(service))
			.AddMessageHandlers(typeof(TracingChannel));

		using (new SafeHostDisposer(
			new HttpQueryableServiceHost(typeof(ProductsService), 25, config, new Uri(baseUri, "products"))))
		{
			var client = new HttpEntityClient(baseUri);
			var query = (IHttpEntityQuery<Product>)client.Query<Product>("products").Skip(10).Take(50);

			var result = query.Execute();

			Assert.Equal(100, result.TotalCount);
			Assert.Equal(25, result.Count());
			Assert.Equal(10, result.First().Id);
		}
	}

	[Fact]
	public void WhenMatchingMultipleArgs_ThenUriTemplateMatches()
	{
		var template = new UriTemplate("?search={search}");

		var match = template.Match(new Uri("http://localhost/products"), new Uri("http://localhost/products?search=foo,bar"));

		Assert.NotNull(match);
	}

	[Fact]
	public void WhenQueryingRepeatedArgument_ThenServiceGetsBoth()
	{
		var baseUri = new Uri("http://localhost:20000");
		var service = new ProductsService();
		var config = HttpHostConfiguration.Create()
			.SetResourceFactory(new SingletonResourceFactory(service))
			.UseJsonNet()
			.AddMessageHandlers(typeof(TracingChannel));

		using (new SafeHostDisposer(
			new HttpQueryableServiceHost(typeof(ProductsService), 25, config, new Uri(baseUri, "products"))))
		{
			var client = new HttpEntityClient(baseUri);
			var query = (IHttpEntityQuery<Product>)client.Query<Product>("products/search2",
				new HttpNameValueCollection { { "search", "10" }, { "search", "20" }, { "count", "25" } });

			var result = query.Execute();

			Assert.Equal(2, result.TotalCount);
			Assert.Equal(2, result.Count());
			Assert.Equal(10, result.First().Id);
		}
	}
}

[ServiceContract]
public class ProductsService
{
	public const string resourceName = "products";
	private List<Product> products;

	public ProductsService()
	{
		this.products = new List<Product>(Enumerable
			.Range(0, 100)
			.Select(x => new Product { Id = x, Title = "Product #" + x }));
	}

	[WebGet(UriTemplate = "?search={search}")]
	public IQueryable<Product> Query(string search = null)
	{
		if (string.IsNullOrEmpty(search))
			return this.products.AsQueryable();
		else
			return this.products.Where(x => x.Title.Contains(search)).AsQueryable();
	}

	[WebGet(UriTemplate = "search2/?search={search}&count={count}")]
	public IQueryable<Product> Query2(string[] search, int count)
	{
		if (search.Length == 0)
		{
			return this.products.AsQueryable();
		}
		else
		{
			var where = search.Aggregate(
				default(Expression<Func<Product, bool>>),
				(w, s) => w != null ? w.Or(x => x.Title.Contains(s)) : (x => x.Title.Contains(s)));

			return this.products.AsQueryable().Where(where);
		}
	}
}

public class Product
{
	public int Id { get; set; }
	public string Title { get; set; }
}