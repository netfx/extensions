using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

public class DictionaryGetOrAddSpec
{
	[Fact]
	public void WhenGettingNonExisting_ThenCreatesValue()
	{
		var id = Guid.NewGuid();
		var dictionary = new Dictionary<string, Guid>();

		var value = dictionary.GetOrAdd("foo", key => id);

		Assert.Equal(id, value);
	}

	[Fact]
	public void WhenGettingTwice_ThenGetsExisting()
	{
		var dictionary = new Dictionary<string, Guid>();

		var value = dictionary.GetOrAdd("foo", key => Guid.NewGuid());
		var value2 = dictionary.GetOrAdd("foo", key => Guid.NewGuid());
		
		Assert.Equal(value, value2);
	}
}
