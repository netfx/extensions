using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace NetFx.Templates.Projects.OpenSource.Extension
{
	internal static class ExtensionTitleSuggestion
	{
		private static readonly Dictionary<Regex, string> Areas = new Dictionary<Regex, string>
		{
			{ new Regex("System\\.Xml.*", RegexOptions.Compiled), "XML" },
			{ new Regex("System\\.Web.*", RegexOptions.Compiled), "Web" },
			{ new Regex("System\\.Data.*", RegexOptions.Compiled), "Data" },
			{ new Regex("System\\.ServiceModel.*", RegexOptions.Compiled), "WCF" },
			{ new Regex("System\\.Windows\\.Input.*", RegexOptions.Compiled), "WPF" },
			{ new Regex("PresentationCore", RegexOptions.Compiled), "WPF" },
			{ new Regex("System\\.Windows\\.Controls.*", RegexOptions.Compiled), "WPF" },
			{ new Regex("System\\.Windows\\.Media.*", RegexOptions.Compiled), "WPF" },
			{ new Regex("System\\.Xaml.*", RegexOptions.Compiled), "XAML" },
		};

		public static string Suggest(string areaPath, string projectName)
		{
			var ns = string.Join(".", areaPath.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries));
			var areaName = (from area in Areas
							where area.Key.IsMatch(ns)
							select area.Value)
						   .FirstOrDefault();

			if (areaName != null)
				return "NETFx " + areaName + " " + projectName;
			else
				return "NETFx " + projectName;
		}
	}
}
