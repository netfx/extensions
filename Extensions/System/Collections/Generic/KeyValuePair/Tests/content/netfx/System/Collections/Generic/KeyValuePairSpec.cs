using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

internal class KeyValuePairSpec
{
	[Fact]
	public void WhenCreating_ThenInfersTypes()
	{
		int value = 25;
		string key = "foo";

		var pair = KeyValuePair.Create(key, value);

		var prms = pair.GetType().GetGenericArguments();

		Assert.Equal(typeof(string), prms[0]);
		Assert.Equal(typeof(int), prms[1]);
	}
}
