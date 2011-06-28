using System.Linq;
using Xunit;
using SharpTestsEx;

namespace netfx.Patterns.QuerySpecs
{
	public class NegateSpecificationFixture
	{
		[Fact]
		public void negate_should_work()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var specification = !startWithJ;

			var result = new SampleRepository()
				.Retrieve(specification);

			result.Satisfy(r => !r.Contains("Jose")
								&& !r.Contains("Julian")
								&& r.Contains("Manuel"));
		}

		[Fact]
		public void negate_operator_should_work()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			
			var result = new SampleRepository()
				.Retrieve(!startWithJ);

			result.Satisfy(r => !r.Contains("Jose")
								&& !r.Contains("Julian")
								&& r.Contains("Manuel"));
		}

		[Fact]
		public void equals_return_true_when_the_negated_spec_are_equals()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));

			var spec = !startWithJ;

			spec.Should().Be.EqualTo(!startWithJ);

		}

		[Fact]
		public void equals_return_false_when_the_negated_spec_are_not_equals()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var anotherAdHocSpec = LinqSpec.For<string>(n => n.StartsWith("dasdas"));

			var spec = !startWithJ;

			spec.Should().Not.Be.EqualTo(!anotherAdHocSpec);

		}

		[Fact]
		public void should_equals_itself()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var spec = !startWithJ;

			Assert.True(spec.Equals(spec));
			Assert.Equal(spec.GetHashCode(), spec.GetHashCode());
		}

		[Fact]
		public void should_notequals_null()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var spec = !startWithJ;

			Assert.False(spec.Equals(null));
		}

		[Fact]
		public void should_notequals_othertype()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var spec = !startWithJ;

			Assert.False(spec.Equals(startWithJ));
		}
	}
}