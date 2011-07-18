using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Linq.Expressions;

public class ExpressionCombinerSpec
{
	[Fact]
	public void WhenCombiningAndExpressions_ThenSucceeds()
	{
		var foo = new Foo { Name = "kzu" };

		Expression<Func<Foo, bool>> first = x => x.Name.StartsWith("k");

		Assert.False(first.And(x => x.Name.EndsWith("z")).Compile().Invoke(foo));
		Assert.True(first.And(x => x.Name.EndsWith("u")).Compile().Invoke(foo));
	}

	[Fact]
	public void WhenCombiningOrExpressions_ThenSucceeds()
	{
		var foo = new Foo { Name = "kzu" };

		Expression<Func<Foo, bool>> first = x => x.Name.StartsWith("k");

		Assert.True(first.Or(x => x.Name.EndsWith("z")).Compile().Invoke(foo));

		first = x => x.Name.StartsWith("z");
	
		Assert.True(first.Or(x => x.Name.EndsWith("u")).Compile().Invoke(foo));
		Assert.False(first.Or(x => x.Name.EndsWith("k")).Compile().Invoke(foo));
	}

	public class Foo
	{
		public string Name { get; set; }
	}
}
