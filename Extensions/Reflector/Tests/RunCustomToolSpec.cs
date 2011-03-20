using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using EnvDTE;

namespace Tests
{
	public class RunCustomToolSpec
	{
		public void RunCustomTool()
		{
			var dte = (DTE)AppDomain.CurrentDomain.GetData("DTE");
			Project p;
			
			var item = (dynamic)dte.Solution.FindProjectItem("Reflect.Overloads.tt");

			item.Object.RunCustomTool();

			Assert.NotNull(item);
		}
	}
}
