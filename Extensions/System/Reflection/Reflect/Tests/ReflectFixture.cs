using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Linq.Expressions;
using System.Reflection;

public class ReflectFixture
{
	[Fact]
	public void ShouldThrowIfNullMethodLambda()
	{
		Assert.Throws<ArgumentNullException>(() =>
			Reflect<Mock>.GetMethod((Expression<Action<Mock>>)null));
	}

	[Fact]
	public void ShouldThrowIfNullPropertyLambda()
	{
		Assert.Throws<ArgumentNullException>(() =>
			Reflect<Mock>.GetProperty((Expression<Func<Mock, object>>)null));
	}

	[Fact]
	public void ShouldThrowIfNullFieldLambda()
	{
		Assert.Throws<ArgumentNullException>(() =>
			Reflect<Mock>.GetField((Expression<Func<Mock, object>>)null));
	}

	[Fact]
	public void ShouldThrowIfNotMethodLambda()
	{
		Assert.Throws<ArgumentException>(() =>
			Reflect<Mock>.GetMethod(x => new object()));
	}

	[Fact]
	public void ShouldThrowIfNotPropertyLambda()
	{
		Assert.Throws<ArgumentException>(() =>
			Reflect<Mock>.GetProperty(x => x.PublicField));
	}

	[Fact]
	public void ShouldThrowIfNotFieldLambda()
	{
		Assert.Throws<ArgumentException>(() =>
			Reflect<Mock>.GetField(x => x.PublicProperty));
	}

	[Fact]
	public void ShouldGetPublicProperty()
	{
		var info = Reflect<Mock>.GetProperty(x => x.PublicProperty);
		Assert.True(info == typeof(Mock).GetProperty("PublicProperty"));
	}

	[Fact]
	public void ShouldGetPublicField()
	{
		var info = Reflect<Mock>.GetField(x => x.PublicField);
		Assert.True(info == typeof(Mock).GetField("PublicField"));
	}

	[Fact]
	public void ShouldGetPublicVoidMethodWithParenthesis()
	{
		var info = Reflect<Mock>.GetMethod(x => x.PublicVoidMethod());
		Assert.True(info == typeof(Mock).GetMethod("PublicVoidMethod"));
	}

	[Fact]
	public void ShouldGetPublicMethodParameterless()
	{
		var info = Reflect<Mock>.GetMethod(x => x.PublicMethodNoParameters());
		Assert.True(info == typeof(Mock).GetMethod("PublicMethodNoParameters"));
	}

	[Fact]
	public void ShouldGetPublicMethodParameters()
	{
		var info = Reflect<Mock>.GetMethod<string, int>(
			(x, y, z) => x.PublicMethodParameters(y, z));
		Assert.True(info == typeof(Mock).GetMethod("PublicMethodParameters", new Type[] { typeof(string), typeof(int) }));
	}

	[Fact]
	public void ShouldGetPublicMethodParametersUsingReference()
	{
		var info = Reflect<Mock>.GetMethod<string, int, bool>(x => x.PublicMethodParameters);
		Assert.True(info == typeof(Mock).GetMethod("PublicMethodParameters", new Type[] { typeof(string), typeof(int) }));
	}

	[Fact]
	public void ShouldGetPublicVoidInstanceMethodUsingReference()
	{
		var info = Reflect<Mock>.GetMethod(x => x.PublicVoidMethod);
		Assert.True(info == typeof(Mock).GetMethod("PublicVoidMethod"));
	}

	[Fact]
	public void ShouldGetPublicVoidInstanceWithParamsMethodUsingReference()
	{
		var info = Reflect<Mock>.GetMethod<string>(x => x.PublicVoidMethodParameters);
		Assert.True(info == typeof(Mock).GetMethod("PublicVoidMethodParameters"));
	}

	[Fact]
	public void ShouldGetPublicStaticVoidMethodParametersUsingReference()
	{
		var info = Reflect.GetMethod(() => Mock.PublicStaticVoidMethod);
		Assert.True(info == typeof(Mock).GetMethod("PublicStaticVoidMethod"));
	}

	[Fact]
	public void ShouldGetPublicStaticMethodUsingReference()
	{
		var info = Reflect.GetMethod<bool>(() => Mock.PublicStaticMethod);
		Assert.True(info == typeof(Mock).GetMethod("PublicStaticMethod"));
	}

	[Fact]
	public void ShouldGetNonPublicProperty()
	{
		var info = Reflect<ReflectFixture>.GetProperty(x => x.NonPublicProperty);
		Assert.True(info == typeof(ReflectFixture).GetProperty("NonPublicProperty", BindingFlags.Instance | BindingFlags.NonPublic));
	}

	[Fact]
	public void ShouldGetNonPublicField()
	{
		var info = Reflect<ReflectFixture>.GetField(x => x.NonPublicField);
		Assert.True(info == typeof(ReflectFixture).GetField("NonPublicField", BindingFlags.Instance | BindingFlags.NonPublic));
	}

	[Fact]
	public void ShouldGetNonPublicMethod()
	{
		var info = Reflect<ReflectFixture>.GetMethod(x => x.NonPublicMethod());
		Assert.True(info == typeof(ReflectFixture).GetMethod("NonPublicMethod", BindingFlags.Instance | BindingFlags.NonPublic));
	}

	[Fact]
	public void ShouldGetConsoleWriteLine()
	{
		var cw = Reflect.GetMethod(() => Console.WriteLine);
		Assert.Equal(typeof(Console).GetMethod("WriteLine", BindingFlags.Static | BindingFlags.Public, null, new Type[0], null), cw);
	}

	[Fact]
	public void ShouldGetShowView()
	{
		var cw = Reflect<IView>.GetMethod(v => v.Show);
		Assert.Equal(typeof(Console).GetMethod("WriteLine", BindingFlags.Static | BindingFlags.Public, null, new Type[0], null), cw);
	}

	public interface IView
	{
		void Show();
	}

	private int NonPublicField;

	private int NonPublicProperty
	{
		get { return NonPublicField; }
		set { NonPublicField = value; }
	}

	private object NonPublicMethod()
	{
		throw new NotImplementedException();
	}

	public class Mock
	{
		public int Value;
		public bool PublicField;
		private int valueProp;

		public Mock()
		{
		}

		public Mock(string foo, int bar)
		{
		}

		public int PublicProperty
		{
			get { return valueProp; }
			set { valueProp = value; }
		}

		public bool PublicMethodNoParameters()
		{
			throw new NotImplementedException();
		}

		public bool PublicMethodParameters(string foo, int bar)
		{
			throw new NotImplementedException();
		}

		public void PublicVoidMethod()
		{
			throw new NotImplementedException();
		}

		public void PublicVoidMethodParameters(string foo)
		{
			throw new NotImplementedException();
		}

		public static void PublicStaticVoidMethod()
		{
				throw new NotImplementedException();
		}

		public static void PublicStaticVoidMethodParameters(string foo)
		{
			throw new NotImplementedException();
		}

		public static bool PublicStaticMethod()
		{
			throw new NotImplementedException();
		}
	}
}
