using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Xml.Linq;
using System.Xml;
using System.Collections;
using Microsoft.CSharp.RuntimeBinder;
using System.Dynamic;

internal class DynamicXmlSpec
{
	[Fact]
	public void WhenAccessingAttribute_ThenUsesIndexerSyntaxWithString()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		var name = dyn["name"];

		Assert.Equal("hi", (string)name);
	}

	[Fact]
	public void WhenAccessingElementUsingIndexerSyntax_ThenCanUseSimpleString()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		var name = dyn["id"];

		Assert.Equal(22, (int)name);
	}

	[Fact]
	public void WhenAccessingElementUsingIndexerSyntax_ThenCanUseXName()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		var name = dyn[XName.Get("id")];

		Assert.Equal(22, (int)name);
	}

	[Fact]
	public void WhenAccessingElementsUsingIndexerSyntax_ThenCanUseSimpleString()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		var handlers = dyn["system.web"]["handler"];

		Assert.Equal(3, ((IEnumerable<XElement>)handlers).Count());
	}

	[Fact]
	public void WhenAccessingElementsUsingIndexerSyntax_ThenCanUseXName()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		var handlers = dyn["system.web"][XName.Get("handler")];

		Assert.Equal(3, ((IEnumerable<XElement>)handlers).Count());
	}

	[Fact]
	public void WhenAccessingAttribute_ThenUsesIndexerSyntaxWithXName()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		var name = dyn[XName.Get("name")];

		Assert.Equal("hi", (string)name);
	}

	[Fact]
	public void WhenSettingAttributeStringValue_ThenUsesIndexerSyntaxWithString()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		dyn["name"] = "hi";

		Assert.Equal("hi", (string)dyn["name"]);
	}

	[Fact]
	public void WhenSettingAttributeStringValue_ThenUsesIndexerSyntaxWithXName()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		dyn[XName.Get("name")] = "hi";

		Assert.Equal("hi", (string)dyn["name"]);
	}

	[Fact]
	public void WhenSettingElementStringValue_ThenUsesIndexerSyntaxWithString()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		dyn["id"] = 2;

		Assert.Equal("2", (string)dyn["id"]);
	}

	[Fact]
	public void WhenSettingElementStringValue_ThenUsesIndexerSyntaxWithXName()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		dyn[XName.Get("id")] = 2;

		Assert.Equal("2", (string)dyn[XName.Get("id")]);
	}

	[Fact]
	public void WhenAddingAttributeStringValue_ThenUsesIndexerSyntaxWithString()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		dyn["foo"] = "bar";

		Assert.Equal("bar", (string)dyn["foo"]);
	}

	[Fact]
	public void WhenAddingAttributeStringValue_ThenUsesIndexerSyntaxWithXName()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		dyn[XName.Get("foo")] = "bar";

		Assert.Equal("bar", (string)dyn["foo"]);
	}

	[Fact]
	public void WhenTraversingChildNodes_ThenUsesDottedSyntax()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		var baz = dyn.bar.baz;

		Assert.NotNull(baz);
	}

	[Fact]
	public void WhenAttributeToString_ThenReturnsValue()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		var id = dyn.bar.baz["id"].ToString();

		Assert.Equal("22", id);
	}

	[Fact]
	public void WhenCastingElementsToDynamicArray_ThenSucceeds()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		dynamic[] handlers = dyn["system.web"].handler;

		Assert.Equal(3, handlers.Length);
	}

	[Fact]
	public void WhenCastingElementsToEnumerableOfObject_ThenSucceeds()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		IEnumerable<object> handlers = dyn["system.web"].handler;

		Assert.Equal(3, handlers.Count());
	}

	[Fact]
	public void WhenCastingElementsToEnumerableOfDynamic_ThenSucceeds()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		IEnumerable<dynamic> handlers = dyn["system.web"].handler;

		Assert.Equal(3, handlers.Count());
	}

	[Fact]
	public void WhenCastingElementsToEnumerableOfDynamicObject_ThenSucceeds()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		IEnumerable<DynamicObject> handlers = dyn["system.web"].handler;

		Assert.Equal(3, handlers.Count());
	}

	[Fact]
	public void WhenCastingElementToDynamicArray_ThenSucceeds()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		dynamic[] handlers = dyn["system.web"];

		Assert.Equal(1, handlers.Length);
	}

	[Fact]
	public void WhenCastingElementToEnumerableOfObject_ThenSucceeds()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		IEnumerable<object> handlers = dyn["system.web"];

		Assert.Equal(1, handlers.Count());
	}

	[Fact]
	public void WhenCastingElementToEnumerableOfDynamic_ThenSucceeds()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		IEnumerable<dynamic> handlers = dyn["system.web"];

		Assert.Equal(1, handlers.Count());
	}

	[Fact]
	public void WhenCastingElementToEnumerableOfDynamicObject_ThenSucceeds()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		IEnumerable<DynamicObject> handlers = dyn["system.web"];

		Assert.Equal(1, handlers.Count());
	}

	[Fact]
	public void WhenCastingToInt_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		int id = dyn.id;

		Assert.Equal(22, id);

		id = dyn.bar.baz["id"];

		Assert.Equal(22, id);
	}

	[Fact]
	public void WhenCastingToInt16_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		Int16 id = dyn.id;

		Assert.Equal(22, id);

		id = dyn.bar.baz["id"];

		Assert.Equal(22, id);
	}

	[Fact]
	public void WhenCastingToUInt16_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		UInt16 id = dyn.id;

		Assert.Equal(22, id);

		id = dyn.bar.baz["id"];

		Assert.Equal(22, id);
	}

	[Fact]
	public void WhenCastingToUInt64_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		UInt64 id = dyn.id;

		Assert.Equal((UInt64)22, id);

		id = dyn.bar.baz["id"];

		Assert.Equal((UInt64)22, id);
	}

	[Fact]
	public void WhenCastingToSByte_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		sbyte id = dyn.id;

		Assert.Equal(22, id);

		id = dyn.bar.baz["id"];

		Assert.Equal(22, id);
	}

	[Fact]
	public void WhenCastingToSingle_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		Single id = dyn.id;

		Assert.Equal(22, id);

		id = dyn.bar.baz["id"];

		Assert.Equal(22, id);
	}

	[Fact]
	public void WhenCastingToTimespan_ThenPerformsXmlDurationConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		TimeSpan expected = TimeSpan.FromHours(10);
		TimeSpan id = dyn.time;

		Assert.Equal(expected, id);

		id = dyn["time"];

		Assert.Equal(expected, id);
	}

	[Fact]
	public void WhenCastingToLong_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		long id = dyn.id;

		Assert.Equal(22, id);

		id = dyn.bar.baz["id"];

		Assert.Equal(22, id);
	}

	[Fact]
	public void WhenCastingToByte_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		byte id = dyn.id;

		Assert.Equal(22, id);

		id = dyn.bar.baz["id"];

		Assert.Equal(22, id);
	}

	[Fact]
	public void WhenCastingToDecimal_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		decimal id = dyn.id;

		Assert.Equal(22, id);

		id = dyn.bar.baz["id"];

		Assert.Equal(22, id);
	}

	[Fact]
	public void WhenCastingToGuid_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		Guid expected = new Guid("{F0376E1F-D255-4AF8-AB41-585434C93B20}");
		Guid id = dyn.guid;

		Assert.Equal(expected, id);

		id = dyn["guid"];

		Assert.Equal(expected, id);
	}

	[Fact]
	public void WhenCastingToDouble_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		double id = dyn.id;

		Assert.Equal(22, id);

		id = dyn.bar.baz["id"];

		Assert.Equal(22, id);
	}

	[Fact]
	public void WhenCastingChar_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		char id = dyn["letter"];

		Assert.Equal('a', id);

		id = dyn.letter;

		Assert.Equal('a', id);
	}

	[Fact]
	public void WhenCastingToUInt_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		uint id = dyn.id;

		Assert.Equal((ulong)22, id);

		id = dyn.bar.baz["id"];

		Assert.Equal((ulong)22, id);
	}

	[Fact]
	public void WhenCastingToBoolean_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		bool id = dyn.position;

		Assert.True(id);

		id = dyn["system.web"].compilation["debug"];

		Assert.True(id);
	}

	[Fact]
	public void WhenCastingToDateTime_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		DateTime value = dyn.date;

		Assert.Equal(2011, value.Year);
	}

	[Fact]
	public void WhenCastingToDateTimeOffset_ThenPerformsXmlConversion()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		DateTimeOffset value = dyn.date;

		Assert.Equal(TimeSpan.FromHours(-3), value.Offset);
	}

	[Fact]
	public void WhenCastingToXAttribute_ThenGetsUnderlyingAttribute()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		XAttribute id = dyn.bar.baz["id"];

		Assert.Equal("22", id.Value);
	}

	[Fact]
	public void WhenCastingToXElement_ThenGetsUnderlyingElement()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		XElement id = dyn.id;

		Assert.Equal("22", id.Value);
	}

	[Fact]
	public void WhenCastingStringToEnum_ThenParsesEnum()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		Position value = dyn["position"];

		Assert.Equal(Position.First, value);
	}

	[Fact]
	public void WhenCastingIntToEnum_ThenParsesEnum()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		Position value = dyn.position;

		Assert.Equal(Position.Second, value);
	}

	[Fact]
	public void WhenCastingAttributeToInvalidType_ThenThrows()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		Assert.Throws<RuntimeBinderException>(() => (Type)dyn["position"]);
	}

	public enum Position
	{
		First,
		Second,
	}

	[Fact]
	public void WhenElementNameHasDot_ThenCanUseIndexerNotation()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		XElement web = dyn["system.web"];

		Assert.NotNull(web);
	}

	[Fact]
	public void WhenMultipleElements_ThenCanUseElementIndex()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		Assert.Equal("foo", (string)dyn["system.web"].handler[0]["name"]);
		Assert.Equal("bar", (string)dyn["system.web"].handler[1]["name"]);
		Assert.Equal("baz", (string)dyn["system.web"].handler[2]["name"]);
	}

	[Fact]
	public void WhenMultipleElements_ThenNonIntegerIndexFails()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		Assert.Throws<RuntimeBinderException>(() => dyn["system.web"].handler["blag"]);
	}

	[Fact]
	public void WhenMultipleElements_ThenCanCastToEnumerableOfElement()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		IEnumerable<XElement> elements = dyn["system.web"].handler;

		Assert.NotNull(elements);
		Assert.Equal(3, elements.Count());
	}

	[Fact]
	public void WhenMultipleElements_ThenCanCastToEnumerableOfDynamicObject()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		IEnumerable<DynamicObject> elements = dyn["system.web"].handler;

		Assert.NotNull(elements);
		Assert.Equal(3, elements.Count());
	}

	[Fact]
	public void WhenElementConvertedToEnumerableElement_ThenSucceeds()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		IEnumerable<XElement> elements = dyn;

		Assert.NotNull(elements);
		Assert.Equal(8, elements.Count());
	}

	[Fact]
	public void WhenElementConvertedToInvalidType_ThenThrows()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		Assert.Throws<RuntimeBinderException>(() => (string[])dyn);
	}

	[Fact]
	public void WhenMultipleElements_ThenCastingToNonEnumerableOfElementFails()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		Assert.Throws<RuntimeBinderException>(() => (string[])dyn["system.web"].handler);
	}

	[Fact]
	public void WhenMultipleElements_ThenCanIterate()
	{
		var doc = XDocument.Load("simple.xml");
		var elements = doc.Root.ToDynamic()["system.web"].handler;

		foreach (var element in elements)
		{
			Assert.NotNull(element["name"]);
		}
	}

	[Fact]
	public void WhenIteratingElement_ThenIteratesAllChildren()
	{
		var doc = XDocument.Load("simple.xml");
		var elements = doc.Root.ToDynamic()["system.web"];

		foreach (var element in elements)
		{
			Assert.NotNull(element);
		}
	}

	[Fact]
	public void WhenIteratingElement_ThenCanCastToXElement()
	{
		var doc = XDocument.Load("simple.xml");
		var elements = ((IEnumerable)doc.Root.ToDynamic()["system.web"]).Cast<XElement>();

		Assert.True(elements.Any(e => e.Name.LocalName == "compilation"));
		Assert.True(elements.Any(e => e.Name.LocalName == "handler"));
	}

	[Fact]
	public void WhenIteratingElements_ThenCanConvertToElements()
	{
		var doc = XDocument.Load("simple.xml");
		var elements = (IEnumerable<XElement>)doc.Root.ToDynamic()["system.web"];

		Assert.Equal(4, elements.Count());
	}
}
