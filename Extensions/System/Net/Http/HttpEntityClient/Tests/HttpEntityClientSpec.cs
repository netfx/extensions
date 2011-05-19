using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Linq.Expressions;
using System.Net.Http;
using Microsoft.ApplicationServer.Http.Description;

internal class HttpEntityClientSpec
{
	[Fact]
	public void WhenGetting_ThenRetrieves()
	{
		using (var ws = new HttpWebService<TestService>("http://localhost:20000", "products", new ServiceConfiguration()))
		{
			var client = new HttpEntityClient(ws.BaseUri, new JsonNetEntityFormatter());

			var product = client.Get<Product>(ws.Uri(1));

			Assert.NotNull(product);
			Assert.Equal("kzu", product.Owner.Name);
		}

	}

	public class ServiceConfiguration : HttpHostConfiguration
	{
		public ServiceConfiguration()
		{
			this.OperationHandlerFactory.Formatters.Clear();
			this.OperationHandlerFactory.Formatters.Insert(0, new JsonNetMediaTypeFormatter());
		}
	}
}
