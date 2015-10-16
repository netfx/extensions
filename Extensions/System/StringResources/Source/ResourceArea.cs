using System.Collections.Generic;
using System.Diagnostics;

namespace NetFx
{
	[DebuggerDisplay ("Name = {Name}, NestedAreas = {NestedAreas.Count}, Values = {Values.Count}")]
	class ResourceArea
	{
		public ResourceArea ()
		{
			this.NestedAreas = new List<ResourceArea> ();
			this.Values = new List<ResourceValue> ();
		}

		public string Name { get; set; }
		public string Prefix { get; set; }
		public List<ResourceArea> NestedAreas { get; private set; }
		public List<ResourceValue> Values { get; private set; }
	}
}