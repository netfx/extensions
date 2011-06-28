using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;

internal class StreamGetBytesSpec
{
	[Fact]
	public void WhenGettingBytes_ThenEqualsContent()
	{
		Stream source = new MemoryStream(Encoding.UTF8.GetBytes("Hello World"));
		var bytes = source.GetBytes();

		var target = new MemoryStream(bytes);

		Assert.Equal("Hello World", new StreamReader(target).ReadToEnd());
	}
}
