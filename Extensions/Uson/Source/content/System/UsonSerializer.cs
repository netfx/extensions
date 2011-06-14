using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Collections;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Converters;

internal class UsonSerializer
{
	private static Dictionary<Type, string> defaultPropertyMap = new Dictionary<Type, string>();
	private JsonSerializer serializer;

	public static UsonSerializer Create(JsonSerializerSettings settings)
	{
		return new UsonSerializer(JsonSerializer.Create(settings));
	}

	public UsonSerializer()
		: this(JsonSerializer.Create(new JsonSerializerSettings
		{
			DefaultValueHandling = DefaultValueHandling.Ignore,
			NullValueHandling = NullValueHandling.Ignore,
			Converters = { new IsoDateTimeConverter() },
		}))
	{
	}

	private UsonSerializer(JsonSerializer serializer)
	{
		this.serializer = serializer;
	}

	public T Deserialize<T>(string uson)
		where T : new()
	{
		var reader = new UsonReader<T>(uson);

		return this.serializer.Deserialize<T>(reader);
	}

	public string Serialize<T>(T value)
	{
		var writer = new StringWriter();
		var uson = new UsonWriter(writer);

		uson.DefaultProperty = defaultPropertyMap.GetOrAdd(typeof(T), key =>
		{
			return typeof(T)
				.GetCustomAttributes(true)
				.OfType<DefaultPropertyAttribute>()
				.Select(x => x.Name)
				.FirstOrDefault() ?? "";
		});

		this.serializer.Serialize(uson, value);

		uson.Flush();

		return writer.ToString();
	}
}