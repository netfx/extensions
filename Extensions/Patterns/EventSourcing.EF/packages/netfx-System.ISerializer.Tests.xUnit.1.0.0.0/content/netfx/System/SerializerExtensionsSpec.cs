#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;

namespace NetFx.System
{
	///	<nuget id="netfx-System.ISerializer.Tests.xUnit" />
	public class SerializerExtensionsSpec
	{
		[Fact]
		public void WhenDeserializingWithNullSerializer_ThenThrowsArgumentNullException()
		{
			var serializer = default(ISerializer);

			Assert.Throws<ArgumentNullException>(() => serializer.Deserialize<Data>(new byte[0]));
		}

		[Fact]
		public void WhenDeserializingNullByteArray_ThenThrowsArgumentNullException()
		{
			var serializer = new FakeSerializer();

			Assert.Throws<ArgumentNullException>(() => serializer.Deserialize<Data>((byte[])null));
		}

		[Fact]
		public void WhenDeserializingEmptyByteArray_ThenReturnsDefaultOfT()
		{
			var serializer = new FakeSerializer();
			var result = serializer.Deserialize<Data>(new byte[0]);

			Assert.Null(result);
		}

		[Fact]
		public void WhenDeserializing_ThenInvokesSerializer()
		{
			var data = new Data();
			var serializer = new FakeSerializer(data);
			var result = serializer.Deserialize<Data>(new byte[] { 25, 20 });

			Assert.Same(data, result);
		}

		[Fact]
		public void WhenSerializingWithNullSerializer_ThenThrowsArgumentNullException()
		{
			var serializer = default(ISerializer);

			Assert.Throws<ArgumentNullException>(() => serializer.Serialize(new Data()));
		}

		[Fact]
		public void WhenSerializingNullGraph_ThenThrowsArgumentNullException()
		{
			var serializer = new FakeSerializer();

			Assert.Throws<ArgumentNullException>(() => serializer.Serialize<Data>(null));
		}

		[Fact]
		public void WhenSerializing_ThenInvokesSerializer()
		{
			var serializer = new FakeSerializer();
			var data = new Data();

			serializer.Serialize(data);

			Assert.Same(data, serializer.Data);
		}

		class FakeSerializer : ISerializer
		{
			public object Data { get; set; }

			public FakeSerializer(object data = null)
			{
				this.Data = data;
			}

			public T Deserialize<T>(Stream stream)
			{
				return (T)this.Data;
			}

			public void Serialize<T>(Stream stream, T graph)
			{
				this.Data = graph;
			}
		}

		partial class Data { }
	}
}
