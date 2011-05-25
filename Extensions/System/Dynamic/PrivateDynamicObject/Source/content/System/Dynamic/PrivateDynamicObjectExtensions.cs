using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Dynamic
{
	public static class DynamicObjectExtensions
	{
		public static dynamic AsPrivateDynamic(this object obj)
		{
			if (obj == null)
				return null;

			return new PrivateDynamicObject(obj);
		}
	}
}