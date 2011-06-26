using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NetFx.Templates.Projects.OpenSource.Extension
{
	public class Replacement
	{
		public static string Key<TProperty>(Expression<Func<ExtensionInformationModel, TProperty>> property)
		{
			return "$" + Reflect<ExtensionInformationModel>.GetPropertyName(property) + "$";
		}
	}
}
