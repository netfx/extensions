using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NetFx
{
	static class ResourceFile
	{
		private static Regex FormatExpression = new Regex ("{(?<name>[^{}]+)}", RegexOptions.Compiled);

		public static ResourceArea Build (string fileName, string rootArea)
		{
			return Build (
				XDocument.Load (fileName)
					.Root.Elements ("data")
					.Where (e => e.Attribute ("type") == null),
				rootArea);
		}

		public static ResourceArea Build (IEnumerable<XElement> data, string rootArea)
		{
			var root = new ResourceArea { Name = rootArea, Prefix = "" };
			foreach (var element in data) {
				//  Splits: ([resouce area]_)*[resouce name]
				var nameAttribute = element.Attribute ("name").Value;
				var valueElement = element.Element ("value").Value;
				var areaParts = nameAttribute.Split (new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
				if (areaParts.Length <= 1) {
					root.Values.Add (GetValue (nameAttribute, valueElement));
				} else {
					var area = GetArea (root, areaParts.Take (areaParts.Length - 1));
					var value = GetValue (areaParts.Skip (areaParts.Length - 1).First (), valueElement);

					area.Values.Add (value);
				}
			}

			SortArea (root);

			return root;
		}

		private static void SortArea (ResourceArea area)
		{
			area.Values.Sort ((left, right) => left.Name.CompareTo (right.Name));
			foreach (var nested in area.NestedAreas) {
				SortArea (nested);
			}
		}

		private static ResourceArea GetArea (ResourceArea area, IEnumerable<string> areaPath)
		{
			var currentArea = area;
			foreach (var areaName in areaPath) {
				var existing = currentArea.NestedAreas.FirstOrDefault (a => a.Name == areaName);
				if (existing == null) {
					if (currentArea.Values.Any (v => v.Name == areaName))
						throw new ArgumentException (string.Format (
							"Area name '{0}' is already in use as a value name under area '{1}'.",
							areaName, currentArea.Name));

					existing = new ResourceArea { Name = areaName, Prefix = currentArea.Prefix + areaName + "_" };
					currentArea.NestedAreas.Add (existing);
				}

				currentArea = existing;
			}

			return currentArea;
		}

		private static ResourceValue GetValue (string resourceName, string resourceValue)
		{
			var value = new ResourceValue { Name = resourceName, Value = resourceValue };

			value.HasFormat = FormatExpression.IsMatch (resourceValue);

			// Parse parameter names
			if (value.HasFormat) {
				value.Format.AddRange (FormatExpression
					.Matches (resourceValue)
					.OfType<Match> ()
					.Select (match => match.Groups["name"].Value)
					.Distinct ());
			}

			int index;
			// Find if there are any index values
			value.IsIndexed = value.Format.Any (format => int.TryParse (format, out index));

			// If there are mixed names and indexes, treat it as unformatted.
			if (value.IsIndexed && value.Format.Any (format => !int.TryParse (format, out index)))
				value.HasFormat = false;

			return value;
		}
	}
}