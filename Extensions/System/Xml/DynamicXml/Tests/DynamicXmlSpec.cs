using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Xml.Linq;
using System.Xml;

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
}
