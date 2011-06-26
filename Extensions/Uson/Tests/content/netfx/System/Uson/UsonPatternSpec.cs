using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Text.RegularExpressions;

namespace Tests
{
	public class UsonPatternSpec
	{
		[Fact]
		public void WhenParsingContentValue_ThenRetrievesValueOnlyGroup()
		{
			var value = "foo";
			var match = new UsonPattern().Matches(value).OfType<Match>().FirstOrDefault();

			Assert.NotNull(match);
			Assert.False(match.Groups[UsonPattern.NameGroup].Success);
			Assert.True(match.Groups[UsonPattern.ValueGroup].Success);
			Assert.Equal("foo", match.Groups[UsonPattern.ValueGroup].Value);
		}

		[Fact]
		public void WhenParsingQuotedContent_ThenRetrievesAsSingleValueOnlyMatch()
		{
			var value = "\"hello world\"";
			var matches = new UsonPattern().Matches(value).OfType<Match>();
			var match = matches.FirstOrDefault();

			Assert.Equal(1, matches.Count());
			Assert.NotNull(match);
			Assert.False(match.Groups[UsonPattern.NameGroup].Success);
			Assert.True(match.Groups[UsonPattern.ValueGroup].Success);
			Assert.Equal("\"hello world\"", match.Groups[UsonPattern.ValueGroup].Value);
		}

		[Fact]
		public void WhenUnquotingEmptyContent_ThenReturnsEmpty()
		{
			var value = "";
			Assert.Equal("", UsonPattern.Unquote(value));
		}

		[Fact]
		public void WhenUnquotingUnbalanced_ThenRemovesStartingQuote()
		{
			var value = "\"hello";
			Assert.Equal("hello", UsonPattern.Unquote(value));
		}

		[Fact]
		public void WhenUnquotingStartQuoteOnly_ThenReturnsEmpty()
		{
			var value = "\"";
			Assert.Equal("", UsonPattern.Unquote(value));
		}

		[Fact]
		public void WhenUnquotingContent_ThenRemovesDoubleQuotes()
		{
			var value = "\"hello world\"";
			Assert.Equal("hello world", UsonPattern.Unquote(value));
		}

		[Fact]
		public void WhenUnquotingContent_ThenRemovesSingleQuotes()
		{
			var value = "'hello world'";
			Assert.Equal("hello world", UsonPattern.Unquote(value));
		}

		[Fact]
		public void WhenUnquotingContent_ThenPreservesMiddleQuotes()
		{
			var value = "\"Joe's house\"";
			Assert.Equal("Joe's house", UsonPattern.Unquote(value));
		}

		[Fact]
		public void WhenParsingSingleValue_ThenRetrievesNameValueGroups()
		{
			var value = "tag:foo";
			var match = new UsonPattern().Matches(value).OfType<Match>().FirstOrDefault();

			Assert.NotNull(match);
			Assert.True(match.Groups[UsonPattern.NameGroup].Success);
			Assert.True(match.Groups[UsonPattern.ValueGroup].Success);
			Assert.Equal("tag", match.Groups[UsonPattern.NameGroup].Value);
			Assert.Equal("foo", match.Groups[UsonPattern.ValueGroup].Value);
		}

		[Fact]
		public void WhenParsingSingleValueWithEquals_ThenRetrievesNameValueGroups()
		{
			var value = "tag=foo";
			var match = new UsonPattern().Matches(value).OfType<Match>().FirstOrDefault();

			Assert.NotNull(match);
			Assert.True(match.Groups[UsonPattern.NameGroup].Success);
			Assert.True(match.Groups[UsonPattern.ValueGroup].Success);
			Assert.Equal("tag", match.Groups[UsonPattern.NameGroup].Value);
			Assert.Equal("foo", match.Groups[UsonPattern.ValueGroup].Value);
		}

		[Fact]
		public void WhenParsingDoubleQuotedPropertyValue_ThenRetrievesNameValueGroups()
		{
			var value = "tag:\"hello world\"";
			var match = new UsonPattern().Matches(value).OfType<Match>().FirstOrDefault();

			Assert.NotNull(match);
			Assert.True(match.Groups[UsonPattern.NameGroup].Success);
			Assert.True(match.Groups[UsonPattern.ValueGroup].Success);
			Assert.Equal("tag", match.Groups[UsonPattern.NameGroup].Value);
			Assert.Equal("\"hello world\"", match.Groups[UsonPattern.ValueGroup].Value);
		}

		[Fact]
		public void WhenParsingSingleQuotedPropertyValue_ThenRetrievesNameValueGroups()
		{
			var value = "tag:'hello world'";
			var match = new UsonPattern().Matches(value).OfType<Match>().FirstOrDefault();

			Assert.NotNull(match);
			Assert.True(match.Groups[UsonPattern.NameGroup].Success);
			Assert.True(match.Groups[UsonPattern.ValueGroup].Success);
			Assert.Equal("tag", match.Groups[UsonPattern.NameGroup].Value);
			Assert.Equal("'hello world'", match.Groups[UsonPattern.ValueGroup].Value);
		}

		[Fact]
		public void WhenParsingMultiplePropertyValues_ThenRetrievesNameValueGroups()
		{
			var value = "tag:wpf platform:vspro";
			var matches = new UsonPattern().Matches(value).OfType<Match>().ToList();

			Assert.Equal(2, matches.Count);
			Assert.Equal("tag", matches[0].Groups[UsonPattern.NameGroup].Value);
			Assert.Equal("wpf", matches[0].Groups[UsonPattern.ValueGroup].Value);
			Assert.Equal("platform", matches[1].Groups[UsonPattern.NameGroup].Value);
			Assert.Equal("vspro", matches[1].Groups[UsonPattern.ValueGroup].Value);
		}

		[Fact]
		public void WhenParsingQuotedTimespanValue_ThenRetrievesNameValueGroups()
		{
			var value = "timeout:\"10:00:00\"";
			var match = new UsonPattern().Matches(value).OfType<Match>().FirstOrDefault();

			Assert.NotNull(match);
			Assert.True(match.Groups[UsonPattern.NameGroup].Success);
			Assert.True(match.Groups[UsonPattern.ValueGroup].Success);
			Assert.Equal("timeout", match.Groups[UsonPattern.NameGroup].Value);
			Assert.Equal("\"10:00:00\"", match.Groups[UsonPattern.ValueGroup].Value);
		}

		[Fact]
		public void WhenParsingSingleQuotedValueWithSpaces_ThenRetrievesFullValue()
		{
			//AnalysisConsole.exe Source:"C:\Data\Message1.txt" Source:"C:\Data\Message2.txt" Database:"Data Source=.\\SQLExpress;Initial Catalog=AnalysisDatabase;Integrated Security=True;" Assembly:"C:\Temp\MyQueries1.dll" Assembly:"C:\Temp\MyQueries2.dll"
			var value = "Database:'Data Source=.\\SQLExpress;Initial Catalog=AnalysisDatabase;Integrated Security=True;'";
			var match = new UsonPattern().Matches(value).OfType<Match>().FirstOrDefault();

			Assert.NotNull(match);
			Assert.True(match.Groups[UsonPattern.NameGroup].Success);
			Assert.True(match.Groups[UsonPattern.ValueGroup].Success);
			Assert.Equal("Database", match.Groups[UsonPattern.NameGroup].Value);
			Assert.True(match.Groups[UsonPattern.ValueGroup].Value.Contains("Integrated Security"), "Failed to find 'Integrated Security' in " + match.Groups[UsonPattern.ValueGroup].Value);
		}
	}
}
