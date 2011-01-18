using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Collections.ObjectModel;

internal class CovariantExtensionsSpec
{
    [Fact]
    public void ShouldConvertCollections()
    {
        var barcol = new Collection<IBar>();
        IList<IFoo> foocol = barcol.ToCovariant<IBar, IFoo>();
        ICollection<IFoo> foo2 = barcol.ToCovariant<IBar, IFoo>();
        IEnumerable<IFoo> foo3 = barcol.ToCovariant<IBar, IFoo>();
    }

    interface IFoo { }
    interface IBar : IFoo { }
}
