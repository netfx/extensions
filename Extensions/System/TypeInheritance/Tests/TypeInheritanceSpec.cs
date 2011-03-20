using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Windows;
using System.Windows.Controls;

internal class TypeInheritanceSpec
{
	[Fact]
	public void WhenWindowInheritance_ThenIWindowServiceIsFirstLevel()
	{
		var tree = typeof(Window).GetInheritanceTree();

		Assert.Equal(1, tree.Inheritance.Count);
		Assert.Equal(typeof(ContentControl), tree.Inheritance.First().Type);
	}

	public class GivenATypeHierarchy
	{
		[Fact]
		public void ThenBaseTypeIsFirst()
		{
			Assert.Equal(typeof(Base), typeof(Derived).GetInheritanceTree().Inheritance.First().Type);
		}

		[Fact]
		public void ThenBaseTypeInterfacesDoesNotAppearOnRoot()
		{
			Assert.False(typeof(Derived).GetInheritanceTree().Inheritance.Any(t => t.Type == typeof(IBase)));
			Assert.False(typeof(Derived).GetInheritanceTree().Inheritance.Any(t => t.Type == typeof(IMarker)));
		}

		[Fact]
		public void ThenInterfaceInheritedTypeDoesNotAppearOnRoot()
		{
			Assert.False(typeof(Base).GetInheritanceTree().Inheritance.Any(t => t.Type == typeof(IMarker)));
		}

		[Fact]
		public void ThenInterfaceInheritedTypeAppearsUnderParentInterface()
		{
			var baseHier = typeof(Base).GetInheritanceTree().Inheritance.First(t => t.Type == typeof(IBase)).Type.GetInheritanceTree();

			Assert.True(baseHier.Inheritance.First().Type == typeof(IFormattable));
		}

		[Fact]
		public void WhenDerivedClassOverridesAllInterfaceMembers_ThenInterfaceAppearsInDerived()
		{
			Assert.True(typeof(Derived).GetInheritanceTree().Inheritance.Any(t => t.Type == typeof(ICloneable)));
		}

		[Fact]
		public void WhenDerivedClassOverridesSomeInterfaceMembers_ThenInterfaceDoesNotAppearsInDerived()
		{
			Assert.False(typeof(Derived).GetInheritanceTree().Inheritance.Any(t => t.Type == typeof(IBase)));
		}

		[Fact]
		public void WhenDerivedClassImplementsExplicitlyBaseInterface_ThenInterfaceAppearsInDerived()
		{
			Assert.True(typeof(Derived).GetInheritanceTree().Inheritance.Any(t => t.Type == typeof(IFormattable)));
		}

		[Fact]
		public void WhenMarkerInterfaceImplementedByBase_ThenInterfaceDoesNotAppearsInDerived()
		{
			Assert.False(typeof(Derived).GetInheritanceTree().Inheritance.Any(t => t.Type == typeof(IMarker)));
			Assert.False(typeof(Base).GetInheritanceTree().Inheritance.Any(t => t.Type == typeof(IMarker)));
			Assert.True(typeof(Base).GetInheritanceTree().Inheritance.First(t => t.Type == typeof(IBase)).Inheritance.Any(t => t.Type == typeof(IMarker)));
		}

		public interface IMarker { }
		public interface IBase : IFormattable, IMarker
		{
			string Name { get; set; }
			bool IsActive { get; set; }
		}
		public class Base : IBase, ICloneable
		{
			public virtual string Name { get; set; }
			public virtual bool IsActive { get; set; }
			public virtual object Clone() { return this; }

			public string ToString(string format, IFormatProvider formatProvider)
			{
				return this.Name;
			}
		}
		public class Derived : Base, IFormattable
		{
			public override object Clone()
			{
				return base.Clone();
			}

			string IFormattable.ToString(string format, IFormatProvider formatProvider)
			{
				return this.ToString();
			}

			public override bool IsActive
			{
				get { return base.IsActive; }
				set { base.IsActive = value; }
			}
		}
	}

	private static void PrintTypeList(TypeInheritance list, int indentLevel)
	{
		Console.WriteLine(
			Enumerable.Range(0, indentLevel).Aggregate("", (s, level) => s += '\t') +
			list.Type.Name + " (" + list.Distance + ")");

		indentLevel++;

		foreach (var inner in list.Inheritance)
		{
			PrintTypeList(inner, indentLevel);
		}
	}
}
