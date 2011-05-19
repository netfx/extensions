using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Linq.Expressions;
using System.Net.Http;

internal class HttpEntityClientSpec
{
	[Fact]
	public void WhenAction_ThenAssert()
	{
		var client = new HttpClient("http://wovs.com");

		var response = client.Get("products/1");

		//response.Content
	}

	public static Expression<Func<T, bool>> ToExpression<T>(Expression<Func<T, bool>> expression)
	{
		return expression;
	}

	public class Product
	{
		public string Id { get; set; }
	}
}
