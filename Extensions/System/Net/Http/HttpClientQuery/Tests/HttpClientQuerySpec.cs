using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Microsoft.ApplicationServer.Http.Description;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;

internal class HttpClientQuerySpec
{
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
