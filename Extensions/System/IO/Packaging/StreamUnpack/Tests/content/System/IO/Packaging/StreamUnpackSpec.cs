using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;

internal class StreamUnpackSpec
{
	[Fact]
	public void WhenUnpacking_ThenExtractsAllFiles()
	{
		using (var pkg = File.OpenRead("netfx-Guard.1.2.0.0.nupkg"))
		{
			pkg.Unpack("guard");

			var allFiles = new DirectoryInfo("guard")
				.EnumerateFiles("*", SearchOption.AllDirectories)
				.Select(x => x.Name)
				.ToList();

			Assert.True(allFiles.Contains("License.txt"));
			Assert.True(allFiles.Contains("Guard.cs"));
			Assert.True(allFiles.Contains("netfx-Guard.nuspec"));
			Assert.True(allFiles.Contains("notnull.snippet"));
		}
	}

	[Fact]
	public void WhenUnpackingSingleFileToStream_ThenExtractsIt()
	{
		using (var pkg = File.OpenRead("netfx-Guard.1.2.0.0.nupkg"))
		{
			var stream = new MemoryStream();
			var succeed = pkg.Unpack("License.txt", stream);

			Assert.True(succeed);

			stream.Position = 0;
			var content = new StreamReader(stream).ReadLine();

			Assert.Equal("Copyright (c) 2011, NETFx", content);
		}
	}

	[Fact]
	public void WhenUnpackingSingleFile_ThenExtractsIt()
	{
		using (var pkg = File.OpenRead("netfx-Guard.1.2.0.0.nupkg"))
		{
			pkg.Unpack("guard", "Guard.cs");

			var allFiles = new DirectoryInfo("guard")
				.EnumerateFiles("*", SearchOption.AllDirectories)
				.Select(x => x.Name)
				.ToList();

			Assert.Equal(1, allFiles.Count);
			Assert.True(allFiles[0] == "Guard.cs");
		}
	}

	[Fact]
	public void WhenUnpackingMemoryStream_ThenStreamRemainsOpen()
	{
		using (var pkg = File.OpenRead("netfx-Guard.1.2.0.0.nupkg").WriteTo(new MemoryStream()))
		{
			var fileMem = new MemoryStream();
			var succeed = pkg.Unpack("License.txt", fileMem);

			Assert.True(succeed);

			// Manipulate original stream again.
			fileMem = new MemoryStream();
			Assert.True(pkg.Unpack("netfx-Guard.nuspec", fileMem));
		}
	}
}
