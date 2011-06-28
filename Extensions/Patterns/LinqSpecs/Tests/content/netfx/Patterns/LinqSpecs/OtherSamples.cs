using System.Collections.Generic;
using Enumerable = System.Linq.Enumerable;
using Xunit;
using SharpTestsEx;
using System.Linq.Expressions;
using System;

namespace netfx.Patterns.QuerySpecs
{
	public class OtherSamples
	{
		[Fact]
		public void can_cast_expression_as_queryspec()
		{
			Expression<Func<string, bool>> startExpr = n => n.StartsWith("M");
			Expression<Func<string, bool>> endExpr = n => n.EndsWith("n");
			
			LinqSpec<string> startWithM = startExpr;
			LinqSpec<string> endsWithN = endExpr;

			IEnumerable<string> result = new SampleRepository()
				.Retrieve(startWithM | !endsWithN);

			result.Satisfy(r =>
				Enumerable.Contains(r, "Jose") &&
				!Enumerable.Contains(r, "Julian") &&
				Enumerable.Contains(r, "Manuel"));

		}

		[Fact]
		public void combination_sample()
		{
			var startWithM = LinqSpec.For<string>(n => n.StartsWith("M"));
			var endsWithN = LinqSpec.For<string>(n => n.EndsWith("n"));

			IEnumerable<string> result = new SampleRepository()
				.Retrieve(startWithM | !endsWithN);

			result.Satisfy(r =>
				Enumerable.Contains(r, "Jose") &&
				!Enumerable.Contains(r, "Julian") &&
				Enumerable.Contains(r, "Manuel"));
		}

		[Fact]
		public void query_sample()
		{
			var startWithM = new StartsWithQuery("M");
			var endsWithN = LinqSpec.For<string>(n => n.EndsWith("n"));

			IEnumerable<string> result = new SampleRepository()
				.Retrieve(startWithM | !endsWithN);

			result.Satisfy(r =>
				Enumerable.Contains(r, "Jose") &&
				!Enumerable.Contains(r, "Julian") &&
				Enumerable.Contains(r, "Manuel"));
		}

		[Fact]
		public void can_compare_negated_custom_query()
		{
			var query = new StartsWithQuery("M");

			var spec1 = !query;
			var spec2 = !query;

			spec1.Should().Be.EqualTo(spec2);
		}

		internal class StartsWithQuery : LinqSpec<string>
		{
			private Expression<Func<string, bool>> spec;

			public StartsWithQuery(string start)
			{
				this.Start = start;

				this.spec = s => s.StartsWith(start);
			}

			public string Start { get; private set; }

			public override Expression<Func<string, bool>> Expression { get { return this.spec; } }

			public override bool Equals(object obj)
			{
				if (Object.Equals(null, obj) ||
					obj.GetType() != this.GetType())
					return false;

				return ((StartsWithQuery)obj).Start.Equals(this.Start);
			}

			public override int GetHashCode()
			{
				return this.Start.GetHashCode();
			}
		}
	}
}