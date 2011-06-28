using System.Collections.Generic;
using System.Linq;
using Xunit;
using SharpTestsEx;

namespace netfx.Patterns.QuerySpecs
{
	//Note; no matter if you are using & operator, or && operator.. both works as an &&.
	public class AndSpecificationFixture
	{
		[Fact]
		public void and_should_work()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithE = LinqSpec.For<string>(n => n.EndsWith("e"));          
			var specfication = startWithJ && endsWithE;

			IEnumerable<string> result = new SampleRepository()
				.Retrieve(specfication);

			result.Satisfy(r => r.Contains("Jose")
			                    && !r.Contains("Julian")
			                    && !r.Contains("Manuel"));
		}

		[Fact]
		public void and_operator_should_work()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithE = LinqSpec.For<string>(n => n.EndsWith("e"));


			// & or && both operators behave as &&.

			IEnumerable<string> result = new SampleRepository()
				.Retrieve(startWithJ & endsWithE);

			result.Satisfy(r => r.Contains("Jose")
								&& !r.Contains("Julian")
								&& !r.Contains("Manuel"));

		}

		[Fact]
		public void equals_return_true_when_both_sides_are_equals()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithE = LinqSpec.For<string>(n => n.EndsWith("e"));
			var spec = startWithJ & endsWithE;

			spec.Should().Be.EqualTo(startWithJ & endsWithE);

			spec.Should("andalso is not conmutable")
				.Not.Be.EqualTo(endsWithE & startWithJ);
		}

		[Fact]
		public void equals_return_false_when_both_sides_are_not_equals()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithE = LinqSpec.For<string>(n => n.EndsWith("e"));
			var endsWithF = LinqSpec.For<string>(n => n.EndsWith("f"));
			var spec = startWithJ & endsWithE;

			spec.Should().Not.Be.EqualTo(startWithJ & endsWithF);
		}

		[Fact]
		public void should_equals_itself()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithE = LinqSpec.For<string>(n => n.EndsWith("e"));
			var spec = startWithJ & endsWithE;

			Assert.True(spec.Equals(spec));
			Assert.Equal(spec.GetHashCode(), spec.GetHashCode());
		}

		[Fact]
		public void should_not_equals_null()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithE = LinqSpec.For<string>(n => n.EndsWith("e"));
			var spec = startWithJ & endsWithE;

			Assert.False(spec.Equals(null));
		}

		[Fact]
		public void should_not_equals_othertype()
		{
			var startWithJ = LinqSpec.For<string>(n => n.StartsWith("J"));
			var endsWithE = LinqSpec.For<string>(n => n.EndsWith("e"));
			var spec = startWithJ & endsWithE;

			Assert.False(spec.Equals(startWithJ | endsWithE));
		}
	}
}