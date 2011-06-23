using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace Tests
{
	public class StringObjectSpec
	{
		[Fact]
		public void WhenDeserializingStringProperty_ThenSucceeds()
		{
			var serializer = new UsonSerializer();

			var value = serializer.Deserialize<Options>("Tag:foo");

			Assert.Equal("foo", value.Tag);
		}

		[Fact(Skip = "Waiting on a Json.NET fix: http://json.codeplex.com/workitem/20965")]
		public void WhenDeserializingMissingProperty_ThenSetsDefaultValue()
		{
			var serializer = new UsonSerializer();

			var value = serializer.Deserialize<Options>("");

			Assert.Equal(10, value.Count);
		}

		[Fact]
		public void WhenDeserializingIntProperty_ThenSucceeds()
		{
			var serializer = new UsonSerializer();

			var value = serializer.Deserialize<Options>("Count:25");

			Assert.Equal(25, value.Count);
		}

		[Fact]
		public void WhenDeserializing_ThenIgnoresNonExistentProperty()
		{
			var serializer = new UsonSerializer();
			var uson = "foo:bar";

			var json = serializer.Deserialize<QueryOptions>(uson);
		}

		[Fact]
		public void WhenDeserializing_ThenReadsJson()
		{
			var serializer = new UsonSerializer();
			var uson = "tag:wpf owner.id:25 parent.Name:kzu";

			var json = serializer.Deserialize<QueryOptions>(uson);

			Assert.Equal("wpf", json.Tag);
			Assert.Equal(25, json.Owner.Id);
			Assert.Equal("kzu", json.Parent.Name);
		}

		[Fact]
		public void WhenDeserializing_ThenLastSetterWins()
		{
			var serializer = new UsonSerializer();
			var uson = "tag:wpf owner.id:25 parent.Name:kzu owner.id:10";

			var json = serializer.Deserialize<QueryOptions>(uson);

			Assert.Equal(10, json.Owner.Id);
		}

		[Fact]
		public void WhenRenderingJson_ThenSucceeds()
		{
			var uson = "foo bar baz tag:wpf owner.id:25 parent.Name:kzu owner.id:10";
			var reader = new UsonReader<QueryOptions>(uson);

			var json = JObject.ReadFrom(reader);

			Console.WriteLine(json.ToString());
		}

		[Fact]
		public void WhenDeserializing_ThenAddsValuesToList()
		{
			var serializer = new UsonSerializer();
			var uson = "content:foo content:bar";

			var json = serializer.Deserialize<QueryOptions>(uson);

			Assert.Equal(2, json.Content.Count);
			Assert.Equal("foo", json.Content[0]);
			Assert.Equal("bar", json.Content[1]);
		}

		[Fact]
		public void WhenDeserializingWithoutPropertyName_ThenAddsValuesToDefaultProperty()
		{
			var serializer = new UsonSerializer();
			var uson = "foo bar";

			var json = serializer.Deserialize<QueryOptions>(uson);

			Assert.Equal(2, json.Content.Count);
			Assert.Equal("foo", json.Content[0]);
			Assert.Equal("bar", json.Content[1]);
		}

		[Fact]
		public void WhenSerializingDefaultPropertyName_ThenDoesNotRenderIt()
		{
			var serializer = new UsonSerializer();

			var json = serializer.Serialize(new QueryOptions { Content = { "foo", "bar" } });

			Assert.False(json.Contains("Content"));
		}

		[Fact]
		public void WhenSerializing_ThenRoundrips()
		{
			var options = new QueryOptions
			{
				Content = { "foo", "bar" },
				Tag = "wpf",
				Max = 10,
				Owner = new User
				{
					Id = 5,
					Name = "kzu",
				},
			};

			var json = new UsonSerializer().Serialize(options);

			var value = new UsonSerializer().Deserialize<QueryOptions>(json);

			Assert.Equal(options.Content.Count, value.Content.Count);
			Assert.Equal(options.Tag, value.Tag);
			Assert.Equal(options.Max, value.Max);
			Assert.Equal(options.Owner.Id, value.Owner.Id);
			Assert.Equal(options.Owner.Name, value.Owner.Name);
		}

		[Fact]
		public void WhenDeserializingTimespan_ThenParsesValue()
		{
			var serializer = new UsonSerializer();
			var uson = "timeout:\"00:05:00\"";

			var json = serializer.Deserialize<QueryOptions>(uson);

			json.Timeout = TimeSpan.FromMinutes(5);

			Assert.Equal(TimeSpan.FromMinutes(5), json.Timeout);
		}

		public class Options
		{
			[DefaultValue(10)]
			public int Count { get; set; }
			public string Tag { get; set; }
		}

		[DefaultProperty("Content")]
		public class QueryOptions
		{
			public QueryOptions()
			{
				this.Content = new List<string>();
				this.When = DateTimeOffset.Now;
				this.Max = 25;
			}

			public List<string> Content { get; set; }
			public string Tag { get; set; }
			[DefaultValue(25)]
			public int Max { get; set; }
			public int? Take { get; set; }
			public DateTimeOffset When { get; set; }
			public TimeSpan Timeout { get; set; }
			public User Owner { get; set; }
			public User Parent { get; set; }
		}

		public class User
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}
	}
}