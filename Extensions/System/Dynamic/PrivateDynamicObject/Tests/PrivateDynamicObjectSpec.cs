using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;

internal class PrivateDynamicObjectSpec
{
	[Fact]
	public void WhenAccessingPrivateField_ThenSucceeds()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		target.field = 5;

		Assert.Equal(5, target.field);
	}

	[Fact]
	public void WhenAccessingPrivateProperty_ThenSucceeds()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		target.Property = "hello";

		Assert.Equal("hello", target.Property);
	}

	[Fact]
	public void WhenInvokingMethod_ThenSucceeds()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		var result = target.Echo("hello");

		Assert.Equal("hello", result);
	}

	[Fact]
	public void WhenInvokingMethod2_ThenSucceeds()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		var result = target.Echo("hello {0}", "world");

		Assert.Equal("hello world", result);
	}

	[Fact]
	public void WhenInvokingMethod_ThenResolvesOverload()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		var result = target.Echo("hello", 2);

		Assert.Equal("hellohello", result);
	}

	[Fact]
	public void WhenInvokingMethodWithRef_ThenResolvesOverload()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());
		var value = default(string);

		var result = target.Echo("hello ", ref value);

		Assert.True(result);
	}

	[Fact(Skip = "Ref/Out arguments are not supported by C# 4.0 dynamic. See Connect bug http://connect.microsoft.com/VisualStudio/feedback/details/543101/net-4-0s-dynamicobject-doesn-t-set-ref-out-arguments")]
	public void WhenInvokingMethodWithRef_ThenReturnsRefValue()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());
		var value = default(string);

		var result = target.Echo("hello ", ref value);

		Assert.True(result);
		Assert.Equal("hello world", value);
	}

	[Fact]
	public void WhenInvokingIndexerOverload1_ThenSucceeds()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		var result = target[9];

		Assert.Equal("9", result);
	}

	[Fact]
	public void WhenInvokingIndexerOverload2_ThenSucceeds()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		var result = target["9"];

		Assert.Equal(9, result);
	}

	[Fact]
	public void WhenInvokingIndexerTwoArgs_ThenSucceeds()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		var result = target["hello", 2];

		Assert.Equal("llo", result);
	}

	[Fact]
	public void WhenNullObject_ThenAsPrivateDinamicReturnsNull()
	{
		var target = default(object);

		Assert.Null(target.AsPrivateDynamic());
	}

	[Fact]
	public void WhenInvokingExplicitlyImplementedMethod_ThenSucceeds()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		var clone = target.Clone();

		Assert.Equal(target.Id, clone.Id);
	}

	[Fact]
	public void WhenInvokingExplicitlyImplementedProperty_ThenSucceeds()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		target.Name = "foo";

		Assert.Equal("foo", target.Name);
	}

	[Fact]
	public void WhenInvokingNonExistingMethod_ThenFails()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		Assert.Throws<RuntimeBinderException>(() => target.Do());
	}

	[Fact]
	public void WhenGettingNonExistingProperty_ThenFails()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		Assert.Throws<RuntimeBinderException>(() => target.Blah);
	}

	[Fact]
	public void WhenGettingNonExistingIndex_ThenFails()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		Assert.Throws<RuntimeBinderException>(() => target[true, 24]);
	}

	[Fact]
	public void WhenSettingNonExistingProperty_ThenFails()
	{
		dynamic target = new PrivateDynamicObject(new PrivateObject());

		Assert.Throws<RuntimeBinderException>(() => target.Blah = true);
	}

	private class PrivateObject : ICloneable, IPrivate
	{
		public PrivateObject()
		{
			this.Id = Guid.NewGuid();
		}

		public Guid Id { get; set; }

		private int field;

		private string Property { get; set; }

		private string Echo(string value)
		{
			return value;
		}

		private string Echo(string value, string format)
		{
			return string.Format(value, format);
		}

		private string Echo(string value, int count)
		{
			return Enumerable.Range(0, count)
				.Aggregate("", (s, i) => s += value);
		}

		private bool Echo(string value, ref string result)
		{
			result = value + "world";

			return true;
		}

		private string this[int index]
		{
			get { return index.ToString(); }
		}

		private int this[string index]
		{
			get { return int.Parse(index); }
		}

		private string this[string value, int index]
		{
			get { return value.Substring(index); }
		}

		object ICloneable.Clone()
		{
			return this;
		}

		string IPrivate.Name { get; set; }
	}

	public interface IPrivate
	{
		string Name { get; set; }
	}
}
