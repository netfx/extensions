#region BSD License
/* 
Copyright (c) 2010, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationServer.Http;
using System.Net.Http.Headers;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Collections;
using System.Net;

/// <summary>
/// A <see cref="MediaTypeFormatter"/> that supports the following media types:
/// "text/json", "application/json" and "application/bson" (for binary Json).
/// </summary>
internal class JsonNetMediaTypeFormatter : MediaTypeFormatter
{
	private JsonSerializerSettings serializerSettings;

	/// <summary>
	/// Initializes a new instance of the <see cref="JsonNetMediaTypeFormatter"/> class with 
	/// <see cref="ReferenceLoopHandling.Ignore "/> for the Json serializer.
	/// </summary>
	public JsonNetMediaTypeFormatter()
		: this(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="JsonNetMediaTypeFormatter"/> class with 
	/// the specified Json serializer settings.
	/// </summary>
	public JsonNetMediaTypeFormatter(JsonSerializerSettings serializerSettings)
	{
		Guard.NotNull(() => serializerSettings, serializerSettings);

		this.serializerSettings = serializerSettings;

		this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/json"));
		this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
		this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/bson"));
	}

	/// <summary>
	/// Serializes the given value to the stream using Json.
	/// </summary>
	public override void OnWriteToStream(Type type, object value, Stream stream, HttpContentHeaders contentHeaders, TransportContext context)
	{
		var serializer = JsonSerializer.Create(this.serializerSettings);
		// NOTE: we don't dispose or close the writer as that would 
		// close the stream, which is used by the rest of the pipeline.
		var writer = GetWriter(contentHeaders, stream);

		if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IQueryable<>)))
		{
			serializer.Serialize(writer, ((IEnumerable)value).OfType<object>().ToList());
		}
		else
		{
			serializer.Serialize(writer, value);
		}

		writer.Flush();
	}

	/// <summary>
	/// Deserializes the stream as Json.
	/// </summary>
	public override object OnReadFromStream(Type type, Stream stream, HttpContentHeaders contentHeaders)
	{
		var serializer = JsonSerializer.Create(this.serializerSettings);
		var reader = GetReader(contentHeaders, stream);

		var result = serializer.Deserialize(reader, type);

		return result;
	}

	private static JsonReader GetReader(HttpContentHeaders contentHeaders, Stream stream)
	{
		if (contentHeaders.ContentType.MediaType.EndsWith("json"))
			return new JsonTextReader(new StreamReader(stream));
		else
			return new BsonReader(stream);
	}

	private JsonWriter GetWriter(HttpContentHeaders contentHeaders, Stream stream)
	{
		if (contentHeaders.ContentType.MediaType.EndsWith("json"))
			return new JsonTextWriter(new StreamWriter(stream));
		else
			return new BsonWriter(stream);
	}
}