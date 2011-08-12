using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;

namespace NetFx.System.IO
{
	/// <nuget id="netfx-System.IO.StreamWriteTo.Tests" />
	public class StreamWriteToSpec
	{
		[Fact]
		public void WhenWritingToStream_ThenCopiesAllContent()
		{
			Stream source = new MemoryStream(Encoding.UTF8.GetBytes("Hello World"));
			Stream target = new MemoryStream();

			source.WriteTo(target);

			target.Position = 0;

			Assert.Equal("Hello World", new StreamReader(target).ReadToEnd());
		}

		[Fact]
		public void WhenWritingToFile_ThenCopiesAllContent()
		{
			var source = new MemoryStream(Encoding.UTF8.GetBytes("Hello World"));
			var file = Path.GetTempFileName();

			source.WriteTo(file);

			var content = File.ReadAllText(file);

			Assert.Equal("Hello World", content);
		}

		[Fact]
		public void WhenWritingToFileAppend_ThenAddsContent()
		{
			var source = new MemoryStream(Encoding.UTF8.GetBytes("World"));
			var file = Path.GetTempFileName();

			File.WriteAllText(file, "Hello");

			source.WriteTo(file, append: true);

			var content = File.ReadAllText(file);

			Assert.Equal("HelloWorld", content);
		}
	}
}