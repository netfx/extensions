using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Net.Http.Entity
{
	/// <summary>
	/// How to format the pluralized resource name when applying the 
	/// <see cref="PluralizerResourceConvention"/> convention to discover 
	/// the resource name corresponding to an entity type.
	/// </summary>
	public enum PluralizerResourceFormat
	{
		/// <summary>
		/// Example: turns 'customerAddress' into 'CustomerAddress'.
		/// </summary>
		PascalCase,

		/// <summary>
		/// Example: turns 'CustomerAddress' into 'customerAddress'.
		/// </summary>
		CamelCase,

		/// <summary>
		/// Example: turns 'CustomerAddress' into 'customeraddress'.
		/// </summary>
		LowerCase,
	}
}
