using System.Linq;
using SharpTestsEx;
using Xunit;

namespace netfx.Patterns.QuerySpecs
{
	public class AdHocSpecificationFixture
	{
		[Fact]
		public void simple_adhoc_should_work()
		{
			var specification = LinqSpec.For<string>(n => n.StartsWith("J"));

			var result = new SampleRepository()
								.Retrieve(specification);

			result.Satisfy(r => r.Contains("Jose")
							  && r.Contains("Julian")
							  && !r.Contains("Manuel"));
		}

		[Fact]
		public void simple_adhoc_equals_itself()
		{
			var specification = LinqSpec.For<string>(n => n.StartsWith("J"));

			Assert.True(specification.Equals(specification));
			Assert.Equal(specification.GetHashCode(), specification.Expression.GetHashCode());
		}

		[Fact]
		public void should_not_equals_null()
		{
			var specification = LinqSpec.For<string>(n => n.StartsWith("J"));

			Assert.False(specification.Equals(null));
		}

		[Fact]
		public void should_not_equals_othertype()
		{
			var specification = LinqSpec.For<string>(n => n.StartsWith("J"));

			Assert.False(specification.Equals(specification | LinqSpec.For<string>(n => true)));
		}
	}
}