using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using Moq;
using Newtonsoft.Json;

namespace Tests
{
	public class UsonWriterSpecs
	{
		[Fact]
		public void WhenWritingComment_ThenThrowsNotSupported()
		{
			var writer = new UsonWriter(new StringWriter());

			Assert.Throws<NotSupportedException>(() => writer.WriteComment("foo"));
		}

		[Fact]
		public void WhenWritingDateTimeValue_ThenThrowsNotSupported()
		{
			var writer = new UsonWriter(new StringWriter());

			Assert.Throws<NotSupportedException>(() => writer.WriteValue(DateTime.Now));
		}

		[Fact]
		public void WhenWritingDateTimeOffsetValue_ThenThrowsNotSupported()
		{
			var writer = new UsonWriter(new StringWriter());

			Assert.Throws<NotSupportedException>(() => writer.WriteValue(DateTimeOffset.Now));
		}

		[Fact]
		public void WhenWritingRawJson_ThenThrowsNotSupported()
		{
			var writer = new UsonWriter(new StringWriter());

			Assert.Throws<NotSupportedException>(() => writer.WriteRaw(""));
		}

		[Fact]
		public void WhenWritingRawValue_ThenThrowsNotSupported()
		{
			var writer = new UsonWriter(new StringWriter());

			Assert.Throws<NotSupportedException>(() => writer.WriteRawValue(""));
		}

		[Fact]
		public void WhenWritingStartConstructor_ThenThrowsNotSupported()
		{
			var writer = new UsonWriter(new StringWriter());

			Assert.Throws<NotSupportedException>(() => writer.WriteStartConstructor("foo"));
		}

		[Fact]
		public void WhenClosingWriter_ThenClosesUnderlying()
		{
			var inner = new Mock<TextWriter>();
			var writer = new UsonWriter(inner.Object);

			writer.Close();

			inner.Verify(x => x.Close());
		}

		[Fact]
		public void WhenWritingAllValues_ThenSucceeds()
		{
			var settings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Include,
				DefaultValueHandling = DefaultValueHandling.Include,
			};

			var uson = UsonSerializer.Create(settings);

			Console.WriteLine(uson.Serialize(new Options()));
		}

		public class Options
		{
			public bool BoolValue { get; set; }
			public byte ByteValue { get; set; }
			public char CharValue { get; set; }
			public decimal DecimalValue { get; set; }
			public double DoubleValue { get; set; }
			public short ShortValue { get; set; }
			public int IntValue { get; set; }
			public long LongValue { get; set; }
			public sbyte SByteValue { get; set; }
			public float FloatValue { get; set; }
			public ushort UShortValue { get; set; }
			public uint UIntValue { get; set; }
			public ulong ULongValue { get; set; }

			public object NullObject { get; set; }
		}
	}
}
