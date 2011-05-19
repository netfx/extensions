using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Microsoft.ApplicationServer.Http.Description;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Bson;

internal class JsonNetMediaTypeFormatterSpec
{
	[Fact]
	public void WhenGettingEntity_ThenUsesTextJson()
	{
		TestWithContentType("text/json", content => new JsonTextReader(new StreamReader(content.ContentReadStream)));
	}

	[Fact]
	public void WhenGettingEntity_ThenUsesApplicationJson()
	{
		TestWithContentType("application/json", content => new JsonTextReader(new StreamReader(content.ContentReadStream)));
	}

	[Fact]
	public void WhenGettingEntity_ThenUsesApplicationBson()
	{
		TestWithContentType("application/bson", content => new BsonReader(content.ContentReadStream));
	}

	private static void TestWithContentType(string contentType, Func<HttpContent, JsonReader> readerFactory)
	{
		var config = HttpHostConfiguration.Create();
		config.Configuration.OperationHandlerFactory.Formatters.Insert(0, new JsonNetMediaTypeFormatter());

		using (var webservice = new HttpWebService<TestService>("http://localhost:20000", "test", config))
		{
			var client = new HttpClient(webservice.BaseUri);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

			var response = client.Get(webservice.Uri(25));

			Assert.True(response.IsSuccessStatusCode, response.ToString());

			var product = new JsonSerializer().Deserialize<Product>(readerFactory(response.Content));

			Assert.Equal(25, product.Id);
			Assert.Equal("kzu", product.Owner.Name);
		}
	}
}
