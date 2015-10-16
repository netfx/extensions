using System.Collections.Generic;
using System.Diagnostics;

namespace NetFx
{
	[DebuggerDisplay ("{Name} = {Value}")]
	class ResourceValue
	{
		public ResourceValue ()
		{
			this.Format = new List<string> ();
		}
		public string Name { get; set; }
		public string Value { get; set; }

		public bool HasFormat { get; set; }
		public bool IsIndexed { get; set; }
		public List<string> Format { get; private set; }
	}
}