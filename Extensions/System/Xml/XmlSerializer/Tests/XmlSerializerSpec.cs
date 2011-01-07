using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;

internal class XmlSerializerSpec
{
	private const string xml = @"<Foo>
	<Id>5</Id>
	<Bar>
		<Name>bar</Name>
	</Bar>
</Foo>";

	[Fact]
	public void WhenSerializingObject_ThenSucceeds()
	{
		var serializer = new XmlSerializer<Foo>();
		var writer = new StringWriter();
		var data = new Foo
		{
			Id = 5,
			Bar = new Bar
			{
				Name = "bar"
			}
		};

		serializer.Serialize(writer, data);
	}

	[Fact]
	public void WhenDeserializingObject_ThenLoadsProperties()
	{
		var serializer = new XmlSerializer<Foo>();
		var tmpFile = Path.GetTempFileName();
		File.WriteAllText(tmpFile, xml);

		var data = serializer.Deserialize(tmpFile);

		Assert.Equal(5, data.Id);
		Assert.NotNull(data.Bar);
		Assert.Equal("bar", data.Bar.Name);
	}
}

public class Foo
{
	public int Id { get; set; }
	public Bar Bar { get; set; }
}

public class Bar
{
	public string Name { get; set; }
}
