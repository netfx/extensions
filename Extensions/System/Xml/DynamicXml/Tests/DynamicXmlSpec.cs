using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Xml.Linq;
using System.Xml;
using System.Collections;

internal class DynamicXmlSpec
{
	[Fact]
	public void WhenAccessingAttribute_ThenUsesIndexerSyntax()
	{
		var doc = XDocument.Load("simple.xml");
		var dyn = doc.Root.ToDynamic();

		var name = dyn["name"];

		Assert.Equal("hi", (string)name);
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
