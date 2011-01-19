using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

internal class IpsumGeneratorSpec
{
	[Fact]
	public void WhenGettingWords_ThenEndsWithDot()
	{
		var phrase = Ipsum.GetPhrase(10);

		Assert.True(phrase.EndsWith("."));
	}

	[Fact]
	public void WhenGetting10Words_ThenStartsWithLoremIpsum()
	{
		var phrase = Ipsum.GetPhrase(10);

		Assert.True(phrase.StartsWith("Lorem ipsum dolor sit amet"));
	}

	[Fact]
	public void WhenGetting3Words_ThenStartsWithLoremIpsumDolor()
	{
		var phrase = Ipsum.GetPhrase(3);

		Assert.Equal("Lorem ipsum dolor.", phrase);
	}
}
