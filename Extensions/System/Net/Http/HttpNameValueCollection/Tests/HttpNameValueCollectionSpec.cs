using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Net.Http;

internal class HttpNameValueCollectionSpec
{
	[Fact]
	public void WhenCreatingWithAnonymousAdd_ThenCanRetrieveQueryString()
	{
		var collection = new HttpNameValueCollection
		{
			{ "hello", "world" },
			{ "tag", ".net" },
			{ "tag", "wpf" },
			{ "foo", "bar" },
		};

		var query = collection.ToString();

		Assert.True(query.IndexOf("tag") != query.LastIndexOf("tag"), "Tag should appear twice");
		Assert.True(query.Contains("hello=world"));
	}

	[Fact]
	public void WhenCreatingWithAnonymousAdd_ThenCanAddMultivalue()
	{
		var collection = new HttpNameValueCollection
		{
			{ "tag", ".net", "wpf" },
			{ "foo", "bar" },
		};

		var query = collection.ToString();

		Assert.True(query.IndexOf("tag") != query.LastIndexOf("tag"), "Tag should appear twice");
		Assert.True(query.Contains("foo=bar"));
	}

	[Fact]
	public void WhenChainingAdd_ThenCanContinueAdding()
	{
		var collection = new HttpNameValueCollection()
			.Add("tag", ".net")
			.Add("tag", "wpf")
			.Add("foo", "bar");

		var query = collection.ToString();

		Assert.True(query.IndexOf("tag") != query.LastIndexOf("tag"), "Tag should appear twice");
		Assert.True(query.Contains("foo=bar"));
	}
}
