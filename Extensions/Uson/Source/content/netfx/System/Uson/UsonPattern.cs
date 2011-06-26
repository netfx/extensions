using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Regular expression pattern that matches USON (or Uson, user string object notation).
/// </summary>
internal class UsonPattern : Regex
{
	// extracting component patterns to make it explicit what they match
	private static readonly RegexPart NonWhitespace = "[\\S]+";
	private static readonly RegexPart DoubleQuotedValue = "\"[^\"]+\"";
	private static readonly RegexPart SingleQuotedValue = "'[^\']+'";

	private static readonly RegexPart Value = Group(DoubleQuotedValue | SingleQuotedValue | NonWhitespace);

	private static readonly RegexPart Expression = Group(Group(NameGroup, NonWhitespace + "?") + "[:=]") + "?" + Group(ValueGroup, Value);

	/// <summary>
	/// Name of the "name" group/part of a value, i.e. "tag" in "tag:wpf".
	/// </summary>
	public const string NameGroup = "name";
	/// <summary>
	/// Name of the "value" group/part of a value, i.e. "wpf" in "tag:wpf".
	/// </summary>
	public const string ValueGroup = "value";

	/// <summary>
	/// Initializes a new instance of the <see cref="UsonPattern"/> class.
	/// </summary>
	public UsonPattern()
		: base(Expression, RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture)
	{
	}

	/// <summary>
	/// Removes wrapping quotes (single or double) that may exist around the value.
	/// </summary>
	public static string Unquote(string value)
	{
		if (string.IsNullOrEmpty(value))
			return value;

		if (value[0] == '\"' || value[0] == '\'')
			value = value.Substring(1);

		if (string.IsNullOrEmpty(value))
			return value;

		if (value[value.Length - 1] == '\"' || value[value.Length - 1] == '\'')
			value = value.Substring(0, value.Length - 1);

		return value;
	}

	private static RegexPart Group(string name, string pattern)
	{
		return "(?<" + name + ">" + pattern + ")";
	}

	private static RegexPart Group(string pattern)
	{
		return "(" + pattern + ")";
	}

	private class RegexPart
	{
		private string pattern;

		internal RegexPart(string pattern)
		{
			this.pattern = pattern;
		}

		/// <summary>
		/// Alternates the current pattern with the given one
		/// </summary>
		public static RegexPart operator |(RegexPart builder, string alternate)
		{
			return builder.Alternate(alternate);
		}

		/// <summary>
		/// Alternates this part with the given value. Equivalent to just 
		/// using the "|" operator on two values: QuotedPart | NonWhitespacePart;
		/// </summary>
		public RegexPart Alternate(string alternate)
		{
			return new RegexPart(this.pattern + "|" + alternate);
		}

		/// <summary>
		/// Alternates this part with the given value. Equivalent to just 
		/// using the "|" operator on two values: QuotedPart | NonWhitespacePart;
		/// </summary>
		public RegexPart Alternate(RegexPart alternate)
		{
			return new RegexPart(this.pattern + "|" + alternate);
		}

		public RegexPart Alternate(params string[] alternates)
		{
			return Alternate(string.Join("|", alternates));
		}

		/// <summary>
		/// Returns the built pattern. Not necessary to call this method, as 
		/// the <see cref="RegexPart"/> can be assigned to a string variable 
		/// using implicit type conversion.
		/// </summary>
		public override string ToString()
		{
			return this.pattern;
		}

		/// <summary>
		/// Returns the built pattern. 
		/// </summary>
		public static implicit operator string(RegexPart builder)
		{
			return builder.ToString();
		}

		/// <summary>
		/// Returns the built pattern. 
		/// </summary>
		public static implicit operator RegexPart(string pattern)
		{
			return new RegexPart(pattern);
		}
	}
}
