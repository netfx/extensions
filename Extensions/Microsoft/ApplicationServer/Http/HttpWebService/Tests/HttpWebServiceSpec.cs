using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.ServiceModel;
using System.ServiceModel.Web;
using Microsoft.ApplicationServer.Http.Description;
using System.Net.Http;

internal class HttpWebServiceSpec
{
	[Fact]
	public void WhenHostingService_ThenCanInvokeIt()
	{
		using (var webservice = new HttpWebService<TestService>(
			serviceBaseUrl: "http://localhost:20000",
			serviceResourcePath: "test",
			serviceConfiguration: HttpHostConfiguration.Create()))
		{
			var client = new HttpClient(webservice.BaseUri);

			// Builds: http://localhost:2000/test/23
			var uri = webservice.Uri("23");
			var response = client.Get(uri);

			Assert.True(response.IsSuccessStatusCode, response.ToString());
			Assert.True(response.Content.ReadAsString().Contains("23"));
		}
	}

	[Fact]
	public void WhenHostingSpecificServiceInstance_ThenGetsConfiguredResult()
	{
		var id = Guid.NewGuid();
		using (var webservice = HttpWebService.Create(new TestService(id),
			serviceBaseUrl: "http://localhost:20000",
			serviceResourcePath: "test"))
		{
			var client = new HttpClient(webservice.BaseUri);

			// Builds: http://localhost:2000/test
			var uri = webservice.Uri("");
			var response = client.Get(uri);

			Assert.True(response.IsSuccessStatusCode, response.ToString());
			Assert.True(response.Content.ReadAsString().Contains(id.ToString()));
		}
	}

	[Fact]
	public void WhenHostingCachedServiceInstance_ThenGetsSameResultAlways()
	{
		using (var webservice = new HttpWebService<TestService>(
			cacheServiceInstance: true,
			serviceBaseUrl: "http://localhost:20000",
			serviceResourcePath: "test",
			serviceConfiguration: HttpHostConfiguration.Create()))
		{
			var client = new HttpClient(webservice.BaseUri);

			// Builds: http://localhost:2000/test/23
			var uri = webservice.Uri("23");
			var response = client.Get(uri);

			Assert.True(response.IsSuccessStatusCode, response.ToString());

			var content1 = response.Content.ReadAsString();
			var content2 = client.Get(uri).Content.ReadAsString();
			Assert.Equal(content1, content2);
		}
	}
}

[ServiceContract]
public class TestService
{
	private Guid id;

	public TestService()
	{
		this.id = Guid.NewGuid();
	}

	public TestService(Guid id)
	{
		this.id = id;
	}

	[WebGet(UriTemplate = "{id}")]
	public string Echo(string id)
	{
		return id;
	}

	[WebGet(UriTemplate = "")]
	public string Ping()
	{
		return this.id.ToString();
	}
}
