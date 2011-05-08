using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

internal class FilteringReflectionCatalogSpec
{
	[Fact]
	public void WhenPartIsFiltered_ThenItsExportCannotBeRetrieved()
	{
		var catalog = new TypeCatalog(typeof(Foo));
		var filtered = new FilteringReflectionCatalog(catalog)
		{
			PartFilter = part => part.PartType != typeof(Foo),
		};

		var container = new CompositionContainer(filtered);

		var exports = container.GetExports<IFoo>();

		Assert.False(exports.Any());
	}

	[Fact]
	public void WhenExportIsFiltered_ThenPartIsAvailableButNotExport()
	{
		var catalog = new TypeCatalog(typeof(Foo));
		var filtered = new FilteringReflectionCatalog(catalog)
		{
			ExportFilter = export => !(export.ExportingMember.MemberType == System.Reflection.MemberTypes.Property),
		};

		var container = new CompositionContainer(filtered);

		var exports = container.GetExports<IFoo>();
		var barExports = container.GetExports<IBar>();

		Assert.True(exports.Any());
		Assert.False(barExports.Any());
	}

	[Fact]
	public void WhenPartIsShared_ThenGetExportGetsSame()
	{
		var catalog = new TypeCatalog(typeof(SharedFoo));
		var filtered = new FilteringReflectionCatalog(catalog);

		var container = new CompositionContainer(filtered);

		var export1 = container.GetExportedValue<SharedFoo>();
		var export2 = container.GetExportedValue<SharedFoo>();

		Assert.Same(export1, export2);
	}

	[Fact]
	public void WhenPartIsNonShared_ThenGetExportGetsDifferent()
	{
		var catalog = new TypeCatalog(typeof(NonSharedFoo));
		var filtered = new FilteringReflectionCatalog(catalog);

		var container = new CompositionContainer(filtered);

		var export1 = container.GetExportedValue<NonSharedFoo>();
		var export2 = container.GetExportedValue<NonSharedFoo>();

		Assert.NotSame(export1, export2);
	}

	[PartCreationPolicy(CreationPolicy.Shared)]
	[Export(typeof(SharedFoo))]
	public class SharedFoo { }

	[PartCreationPolicy(CreationPolicy.NonShared)]
	[Export(typeof(NonSharedFoo))]
	public class NonSharedFoo { }


	[Export(typeof(IFoo))]
	public class Foo : IFoo
	{
		public Foo()
		{
			this.Bar = new Bar();
		}

		[Export(typeof(IBar))]
		public IBar Bar { get; set; }
	}

	public interface IFoo { }

	public class Bar : IBar { }
	public interface IBar { }

}
