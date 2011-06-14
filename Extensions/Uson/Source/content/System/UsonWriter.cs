using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

internal class UsonWriter : JsonWriter
{
	private char quoteChar;
	private TextWriter writer;
	private Stack<string> path = new Stack<string>();
	private bool isArray;

	public UsonWriter(TextWriter writer)
	{
		this.writer = writer;
		this.quoteChar = '"';
	}

	/// <summary>
	/// Gets or sets the optional default property at the root level object 
	/// that can be omitted from serialization.
	/// </summary>
	public string DefaultProperty { get; set; }

	/// <summary>
	/// Closes this stream and the underlying stream.
	/// </summary>
	public override void Close()
	{
		base.Close();
		if (base.CloseOutput && (this.writer != null))
		{
			this.writer.Close();
		}
	}

	/// <summary>
	/// Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.
	/// </summary>
	public override void Flush()
	{
		this.writer.Flush();
	}

	/// <summary>
	/// Writes the property name of a name/value pair on a Json object.
	/// </summary>
	public override void WritePropertyName(string name)
	{
		base.WritePropertyName(name);
		path.Push(name);
	}

	/// <summary>
	/// Writes the specified end token.
	/// </summary>
	protected override void WriteEnd(JsonToken token)
	{
		switch (token)
		{
			case JsonToken.EndArray:
				this.isArray = false;
				this.path.Pop();
				break;
			default:
				break;
		}
	}

	private void WriteValueInternal(string value, JsonToken token)
	{
		var property = string.Join(".", this.path.Reverse());

		if (!property.Equals(this.DefaultProperty, StringComparison.OrdinalIgnoreCase))
		{
			this.writer.Write(property);
			this.writer.Write(':');
		}

		this.writer.Write(value);
		if (!this.isArray)
			this.path.Pop();
	}

	/// <summary>
	/// Writes a <see cref="T:System.String" /> value.
	/// </summary>
	public override void WriteValue(string value)
	{
		base.WriteValue(value);
		if (value == null)
		{
			this.WriteValueInternal(JsonConvert.Null, JsonToken.Null);
		}
		else
		{
			//JavaScriptUtils.WriteEscapedJavaScriptString(this.writer, value, this.quoteChar, true);
			this.WriteValueInternal(this.quoteChar + value + this.quoteChar, JsonToken.String);
		}
	}

	/// <summary>
	/// Writes a null value.
	/// </summary>
	public override void WriteNull()
	{
		base.WriteNull();
		WriteValueInternal(JsonConvert.Null, JsonToken.Null);
	}

	/// <summary>
	/// Writes the beginning of a Json array.
	/// </summary>
	public override void WriteStartArray()
	{
		base.WriteStartArray();
		this.isArray = true;
	}

	/// <summary>
	/// Writes an undefined value.
	/// </summary>
	public override void WriteUndefined()
	{
		base.WriteUndefined();
		WriteValueInternal(JsonConvert.Undefined, JsonToken.Undefined);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Boolean" /> value.
	/// </summary>
	public override void WriteValue(bool value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Boolean);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Byte" /> value.
	/// </summary>
	public override void WriteValue(byte value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Char" /> value.
	/// </summary>
	public override void WriteValue(char value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Decimal" /> value.
	/// </summary>
	public override void WriteValue(decimal value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Double" /> value.
	/// </summary>
	public override void WriteValue(double value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Int16" /> value.
	/// </summary>
	public override void WriteValue(short value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Int32" /> value.
	/// </summary>
	public override void WriteValue(int value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Int64" /> value.
	/// </summary>
	public override void WriteValue(long value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.SByte" /> value.
	/// </summary>
	public override void WriteValue(sbyte value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Single" /> value.
	/// </summary>
	public override void WriteValue(float value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.UInt16" /> value.
	/// </summary>
	public override void WriteValue(ushort value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.UInt32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt32" /> value to write.</param>
	public override void WriteValue(uint value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.UInt64" /> value.
	/// </summary>
	public override void WriteValue(ulong value)
	{
		base.WriteValue(value);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
	}

	/// <summary>
	/// Writes the JSON value delimiter.
	/// </summary>
	protected override void WriteValueDelimiter()
	{
		base.WriteValueDelimiter();
		this.writer.Write(' ');
	}

	/// <summary>
	/// Writes out the given white space.
	/// </summary>
	public override void WriteWhitespace(string ws)
	{
		base.WriteWhitespace(ws);
		this.writer.Write(ws);
	}

	#region Unsupported

	public override void WriteComment(string text)
	{
		throw new NotSupportedException();
	}

	public override void WriteValue(DateTime value)
	{
		throw new NotSupportedException("Native dates are not supported. Please provide a date time converter with the serializer settings.");
	}

	public override void WriteValue(DateTimeOffset value)
	{
		throw new NotSupportedException("Native dates are not supported. Please provide a date time converter with the serializer settings.");
	}

	public override void WriteRaw(string json)
	{
		throw new NotSupportedException();
	}

	public override void WriteRawValue(string json)
	{
		throw new NotSupportedException();
	}

	public override void WriteStartConstructor(string name)
	{
		throw new NotSupportedException();
	}

	#endregion
}