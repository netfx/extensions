using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

internal class EmptyIfNullSpec
{
	[Fact]
	public void WhenEnumerableIsNull_ThenReturnsEmpty()
	{
		var names = default(string[]);

		var name = names.EmptyIfNull().FirstOrDefault();

		Assert.Null(name);
	}

	[Fact]
	public void WhenEnumerableNotNull_ThenReturnsSame()
	{
		var names = new[] { "foo", "bar" };

		var result = names.EmptyIfNull();

		Assert.Same(names, result);

	}
}
