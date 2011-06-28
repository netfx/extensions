using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Microsoft.ApplicationServer.Http.Description;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Data.Services.Client;

internal class HttpClientQuerySpec
{
	[Fact]
	public void WhenOrdering_ThenSucceeds()
	{
		var config = HttpHostConfiguration.Create();
		config.Configuration.OperationHandlerFactory.Formatters.Insert(0, new JsonNetMediaTypeFormatter());

		using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", config))
		{
			var client = new HttpClient("http://localhost:20000");
			client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/json"));

			var context = new DataServiceContext(new Uri("http://localhost:20000"));
			// We always specify how many to take, to be explicit.
			var query = context.CreateQuery<Product>("products")
				.Where(x => x.Owner.Name == "kzu")
				.OrderBy(x => x.Id)
				.ThenBy(x => x.Owner.Id)
				.Skip(1)
				.Take(1);

			//var uri = ((DataServiceQuery)query).RequestUri;
			var uri = new Uri(((DataServiceQuery)query).RequestUri.ToString().Replace("()?$", "?$"));
			Console.WriteLine(uri);
			var response = client.Get(uri);

			Assert.True(response.IsSuccessStatusCode, "Failed : " + response.StatusCode + " " + response.ReasonPhrase);

			var products = new JsonSerializer().Deserialize<List<Product>>(new JsonTextReader(new StreamReader(response.Content.ContentReadStream)));

			Assert.Equal(1, products.Count);
		}
	}

	[Fact]
	public void WhenQuerying_ThenGetsResponse()
	{
		var config = HttpHostConfiguration.Create();
		config.Configuration.OperationHandlerFactory.Formatters.Insert(0, new JsonNetMediaTypeFormatter());

		using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", config))
		{
			var client = new HttpClient("http://localhost:20000");
			client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/json"));

			var response = client.Query<Product>("products", x => x.Id <= 2);

			Assert.True(response.IsSuccessStatusCode);

			var products = new JsonSerializer().Deserialize<List<Product>>(new JsonTextReader(new StreamReader(response.Content.ContentReadStream)));

			Assert.Equal(2, products.Count);
			Assert.True(products.All(x => x.Id <= 2));
		}
	}

	[Fact]
	public void WhenTakeOnly_ThenGetsResponse()
	{
		var config = HttpHostConfiguration.Create();
		config.Configuration.OperationHandlerFactory.Formatters.Insert(0, new JsonNetMediaTypeFormatter());

		using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", config))
		{
			var client = new HttpClient("http://localhost:20000");
			client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/json"));

			var response = client.Query<Product>("products", take: 1);

			Assert.True(response.IsSuccessStatusCode);

			var products = new JsonSerializer().Deserialize<List<Product>>(new JsonTextReader(new StreamReader(response.Content.ContentReadStream)));

			Assert.Equal(1, products.Count);
			Assert.Equal(1, products[0].Id);
		}
	}

	[Fact]
	public void WhenPaging_ThenGetsResponse()
	{
		var config = HttpHostConfiguration.Create();
		config.Configuration.OperationHandlerFactory.Formatters.Insert(0, new JsonNetMediaTypeFormatter());

		using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", config))
		{
			var client = new HttpClient("http://localhost:20000");
			client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/json"));

			var response = client.Query<Product>("products", x => x.Id <= 2, skip: 1, take: 1);

			Assert.True(response.IsSuccessStatusCode);

			var products = new JsonSerializer().Deserialize<List<Product>>(new JsonTextReader(new StreamReader(response.Content.ContentReadStream)));

			Assert.Equal(1, products.Count);
			Assert.Equal(2, products[0].Id);
		}
	}

	[Fact]
	public void WhenQueryingSubEntity_ThenGetsResponse()
	{
		var config = HttpHostConfiguration.Create();
		config.Configuration.OperationHandlerFactory.Formatters.Insert(0, new JsonNetMediaTypeFormatter());

		using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", config))
		{
			var client = new HttpClient("http://localhost:20000");
			client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/json"));

			var response = client.Query<Product>("products", x => x.Owner.Name == "kzu");

			Assert.True(response.IsSuccessStatusCode);

			var products = new JsonSerializer().Deserialize<List<Product>>(new JsonTextReader(new StreamReader(response.Content.ContentReadStream)));

			Assert.Equal(2, products.Count);
			Assert.True(products.All(x => x.Owner.Name == "kzu"));
		}
	}
}
