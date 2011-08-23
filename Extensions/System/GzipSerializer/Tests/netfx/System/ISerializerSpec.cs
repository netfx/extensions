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
	public abstract partial class SerializerSpec
	{
		internal abstract ISerializer CreateSerializer();

		[Fact]
		public void WhenSerializing_ThenCanRoundTrip()
		{
			var serializer = CreateSerializer();
			var stream = new MemoryStream();

			var graph = new User
			{
				Id = 5, 
				Products= new []
				{
					new Product { Id = 1 },
					new Product { Id = 2 },
				}
			};

			serializer.Serialize(stream, graph);

			stream.Position = 0;

			var deserialized = serializer.Deserialize<User>(stream);

			Assert.Equal(graph.Id, deserialized.Id);
			Assert.Equal(graph.Products.Count(), deserialized.Products.Count());
			Assert.Equal(graph.Products.First().Id, deserialized.Products.First().Id);
		}

		[Fact]
		public void WhenSerializingNullGraph_ThenThrowsArgumentNullException()
		{
			var serializer = CreateSerializer();
			var stream = new MemoryStream();

			Assert.Throws<ArgumentNullException>(() => serializer.Serialize(stream, default(User)));
		}

		[Fact]
		public void WhenSerializingToNullStream_ThenThrowsArgumentNullException()
		{
			var serializer = CreateSerializer();

			Assert.Throws<ArgumentNullException>(() => serializer.Serialize(null, new User()));
		}

		[Fact]
		public void WhenDeserializingFromNullStream_ThenThrowsArgumentNullException()
		{
			var serializer = CreateSerializer();

			Assert.Throws<ArgumentNullException>(() => serializer.Deserialize<User>(null));			
		}

		public partial class User
		{
			public int Id { get; set; }
			public IEnumerable<Product> Products { get; set; }
		}

		public partial class Product
		{
			public int Id { get; set; }
		}
	}
}