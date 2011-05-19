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
}

[ServiceContract]
public class TestService
{
	[WebGet(UriTemplate = "{id}")]
	public string Echo(string id)
	{
		return id;
	}
}
