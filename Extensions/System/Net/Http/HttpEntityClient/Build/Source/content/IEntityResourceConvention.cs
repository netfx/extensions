using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Net.Http.Entity
{
	/// <summary>
	/// Provides the convention to discover the resource name 
	/// for a given entity type. The resource name is used 
	/// by the <see cref="HttpEntityClient"/> to build 
	/// requests for the given entity by appending it to the 
	/// <see cref="HttpEntityClient.BaseAddress"/>.
	/// </summary>
	internal interface IEntityResourceConvention
	{
		/// <summary>
		/// Gets the name of the resource corresponding to the 
		/// given entity.
		/// </summary>
		string GetResourceName(Type entityType);
	}
}