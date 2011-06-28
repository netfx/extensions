using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace netfx.Patterns.QuerySpecs
{
	internal class SampleRepository : ReadOnlyCollection<string>
	{
		public SampleRepository()
			: base(new[] { "Jose", "Manuel", "Julian" })
		{ }

		public IEnumerable<string> Retrieve(LinqSpec<string> specfication)
		{
			return this.AsQueryable().Where(specfication);
		}
	}
}