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
		using (var webservice = new HttpWebService<TestService>("http://localhost:20000", "test", HttpHostConfiguration.Create()))
		{
			var client = new HttpClient(webservice.BaseUri);

			var response = client.Get(webservice.Uri(""));

			Assert.True(response.IsSuccessStatusCode, response.ToString());
		}
	}
}

[ServiceContract]
public class TestService
{
	[WebGet(UriTemplate = "")]
	public DateTime Time()
	{
		return DateTime.Now;
	}
}
