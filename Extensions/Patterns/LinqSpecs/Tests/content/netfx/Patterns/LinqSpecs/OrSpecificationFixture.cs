using System.Linq;
using Xunit;
using SharpTestsEx;

namespace netfx.Patterns.QuerySpecs
{
	public class OrSpecificationFixture
	{
		[Fact]
		public void or_should_work()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithN = LinqSpec.For<string>(n => n.EndsWith("n"));

			var result = new SampleRepository()
				.Retrieve(startWithJ | endsWithN);

			result.Satisfy(r => Enumerable.Contains(r, "Jose")
			                    && Enumerable.Contains(r, "Julian")
			                    && !Enumerable.Contains(r, "Manuel"));
		}

		[Fact]
		public void or_operator_should_work()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithN = LinqSpec.For<string>(n => n.EndsWith("n"));

			var result = new SampleRepository()
				.Retrieve(startWithJ || endsWithN);

			result.Satisfy(r => r.Contains("Jose")
								&& r.Contains("Julian")
								&& !r.Contains("Manuel"));
		}

		[Fact]
		public void equals_return_true_when_both_sides_are_equals()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithE = LinqSpec.For<string>(n => n.EndsWith("e"));
			var spec = startWithJ || endsWithE;

			spec.Should().Be.EqualTo(startWithJ || endsWithE);

			spec.Should("orelse is not conmutable")
				.Not.Be.EqualTo(endsWithE || startWithJ);
		}

		[Fact]
		public void equals_return_false_when_both_sides_are_not_equals()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithE = LinqSpec.For<string>(n => n.EndsWith("e"));
			var endsWithF = LinqSpec.For<string>(n => n.EndsWith("f"));
			var spec = startWithJ || endsWithE;

			spec.Should().Not.Be.EqualTo(startWithJ || endsWithF);
		}

		[Fact]
		public void should_equal_self()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithE = LinqSpec.For<string>(n => n.EndsWith("e"));
			var spec = startWithJ || endsWithE;

			Assert.True(spec.Equals(spec));
			Assert.Equal(spec.GetHashCode(), spec.GetHashCode());
		}

		[Fact]
		public void should_not_equal_null()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithE = LinqSpec.For<string>(n => n.EndsWith("e"));
			var spec = startWithJ || endsWithE;

			Assert.False(spec.Equals(null));
		}

		[Fact]
		public void should_not_equal_other_type()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithE = LinqSpec.For<string>(n => n.EndsWith("e"));
			var spec = startWithJ || endsWithE;

			Assert.False(spec.Equals(startWithJ && endsWithE));
		}
	}
}