namespace Tests.content.netfx.Patterns.LinqSpecs
{
	using System.Linq;
	using global::netfx.Patterns.QuerySpecs;
	using SharpTestsEx;
	using Xunit;

	public class OnPropertyTest
	{
		[Fact]
		public void CombineIntSpec()
		{
			var intGreaterThan4Spec = LinqSpec.For<int>(i => i > 4);
			var stringLongerThan4CharsSpec = LinqSpec.OnProperty<string, int>(s => s.Length, intGreaterThan4Spec);

			var result = new SampleRepository()
				.Retrieve(stringLongerThan4CharsSpec);

			result.Satisfy(r => !r.Contains("Jose")
			                    && r.Contains("Julian")
			                    && r.Contains("Manuel"));
		}
	}
}