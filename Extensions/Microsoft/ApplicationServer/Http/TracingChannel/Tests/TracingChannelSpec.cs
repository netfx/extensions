using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.ServiceModel.Web;
using System.Net.Http;
using Microsoft.ApplicationServer.Http;
using System.ServiceModel;
using System.Net.Http.Entity;
using Microsoft.ApplicationServer.Http.Description;
using System.Diagnostics.Sdk;
using Moq;
using System.Diagnostics;
using System.IO;

internal class TracingChannelSpec
{
	[Fact]
	public void WhenExecutingRequest_ThenTracesInformationHeaders()
	{
		var listener = new ConsoleTraceListener();
		TracerExtensibility.AddListener(SourceName.For<TracingChannel>(), listener);
		TracerExtensibility.SetTracingLevel(SourceName.For<TracingChannel>(), SourceLevels.All);

		var config = HttpHostConfiguration.Create()
			.UseJsonNet()
			.AddMessageHandlers(typeof(TracingChannel));

		using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", true, config))
		{
			var client = new HttpEntityClient(ws.BaseUri);
			var products = client.Query<Product>().Skip(1).Take(1).ToList();

			Assert.Equal(1, products.Count);
			Assert.Equal(2, products[0].Id);
		}

		listener.Flush();
	}
}

[ServiceContract]
public class TestService
{
	// TODO: this could come from the same pluralizer/formatter as the client.
	private const string resourceName = "products";
	private List<Product> products;

	public TestService()
	{
		this.products = new List<Product>
		{
			new Product
			{
				Title = "A",
				Id = 1,
			}, 
			new Product
			{
				Title = "D",
				Id = 2,
			}, 
			new Product
			{
				Title = "B",
				Id = 3,
			}, 
		};
	}

	[WebGet(UriTemplate = "{id}")]
	public HttpResponseMessage Get(int id)
	{
		var product = this.products.FirstOrDefault(x => x.Id == id);

		if (product == null)
			return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound, "Not found");

		return new HttpResponseMessage<Product>(product);
	}

	[WebInvoke(Method = "DELETE", UriTemplate = "{id}")]
	public HttpResponseMessage Delete(int id)
	{
		var product = this.products.FirstOrDefault(x => x.Id == id);

		if (product == null)
			return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound, "Not found");

		this.products.Remove(product);

		return new HttpResponseMessage(System.Net.HttpStatusCode.OK, "Deleted");
	}

	[WebInvoke(Method = "POST", UriTemplate = "")]
	public HttpResponseMessage Create(Product product)
	{
		var id = this.products.Select(x => x.Id).OrderBy(x => x).Last() + 1;
		product.Id = id;

		this.products.Add(product);

		var response = new HttpResponseMessage<Product>(product, System.Net.HttpStatusCode.Created);
		response.Headers.Location = new Uri(resourceName + "/" + id.ToString(), UriKind.Relative);

		return response;
	}

	[WebInvoke(Method = "PUT", UriTemplate = "{id}")]
	public HttpResponseMessage CreateOrUpdate(int id, Product product)
	{
		var existing = this.products.FirstOrDefault(x => x.Id == id);
		product.Id = id;

		if (product == null)
		{
			this.products.Add(product);
			return new HttpResponseMessage(System.Net.HttpStatusCode.Created, "Created");
		}
		else
		{
			this.products.Remove(existing);
			this.products.Add(product);
			return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted, "Updated");
		}
	}

	[WebGet(UriTemplate = "")]
	public IQueryable<Product> Query()
	{
		return this.products.AsQueryable();
	}
}

public class Product
{
	public int Id { get; set; }
	public string Title { get; set; }
}