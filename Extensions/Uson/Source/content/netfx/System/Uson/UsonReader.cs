using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Regex = UsonPattern;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ComponentModel;

internal class UsonReader<T> : JsonTextReader
{
	private static readonly Regex regex = new Regex();
	private static Dictionary<Type, string> defaultPropertyMap = new Dictionary<Type, string>();

	public UsonReader(string value)
		: base(new StringReader(PatternToJson(value)))
	{
	}

	private static string PatternToJson(string uson)
	{
		var matches = regex.Matches(uson)
			.OfType<Match>()
			.Select(m => new PropertyMatch(m))
			.Select(m => SetDefaultProperty(m, typeof(T)))
			.OrderBy(m => m.Path)
			.ThenBy(m => m.Name)
			.ToList();

		var root = new JObject();
		var rootType = typeof(T);
		var currentTarget = root;
		var currentType = rootType;
		var currentPath = "";

		foreach (var match in matches)
		{
			if (!match.Path.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
			{
				var paths = match.Path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
				// We start again from the root.
				currentTarget = root;
				currentType = rootType;
				foreach (var path in paths)
				{
					var propInfo = currentType.GetProperties().FirstOrDefault(x => x.Name.Equals(path, StringComparison.OrdinalIgnoreCase));
					if (propInfo == null)
						// Like JSon, we just ignore extra junk.
						continue;

					var property = currentTarget.Property(path);
					var propValue = default(JObject);
					if (property == null)
					{
						propValue = new JObject();
						currentTarget.Add(path, propValue);
					}
					else
					{
						propValue = (JObject)property.Value;
					}

					currentTarget = propValue;
					currentType = propInfo.PropertyType;
				}

				currentPath = match.Path;
			}

			var matchProp = currentType.GetProperties().FirstOrDefault(x => x.Name.Equals(match.Name, StringComparison.OrdinalIgnoreCase));
			if (matchProp == null)
				// Like JSon, we just ignore extra junk.
				continue;

			if (matchProp.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(matchProp.PropertyType))
			{
				// Array style.
				var existingProp = currentTarget.Property(match.Name);
				if (existingProp != null)
					((JArray)existingProp.Value).Add(match.Value);
				else
					currentTarget.Add(match.Name, new JArray(match.Value));
			}
			else
			{
				var existingProp = currentTarget.Property(match.Name);
				if (existingProp != null)
					existingProp.Value = match.Value;
				else
					currentTarget.Add(match.Name, match.Value);
			}
		}

		return root.ToString();
	}

	private static PropertyMatch SetDefaultProperty(PropertyMatch match, Type ownerType)
	{
		if (match.IsDefault)
		{
			match.Name = defaultPropertyMap.GetOrAdd(ownerType, key =>
			{
				return ownerType
					.GetCustomAttributes(true)
					.OfType<DefaultPropertyAttribute>()
					.Select(x => x.Name)
					.FirstOrDefault() ?? "";
			});
		}

		return match;
	}

	private class PropertyMatch
	{
		public PropertyMatch(Match match)
		{
			this.IsDefault = !match.Groups[Regex.NameGroup].Success;
			this.Name = match.Groups[Regex.NameGroup].Value;
			this.Path = "";
			this.Value = Regex.Unquote(match.Groups[Regex.ValueGroup].Value);

			if (this.Name.Contains('.'))
			{
				var paths = this.Name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

				this.Path = string.Join(".", paths.Take(paths.Length - 1));
				this.Name = this.Name.Replace(this.Path, "");
			}

			if (this.Name.StartsWith("."))
				this.Name = this.Name.Substring(1);
		}

		public bool IsDefault { get; set; }

		public string Path { get; set; }
		public string Name { get; set; }
		public string Value { get; set; }
	}
}
