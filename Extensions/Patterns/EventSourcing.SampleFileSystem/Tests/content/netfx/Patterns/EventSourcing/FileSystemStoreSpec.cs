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

namespace NetFx.Patterns.EventSourcing
{
	public class FileSystemStoreSpec
	{
		private ISerializer serializer = new JsonSerializer(new Newtonsoft.Json.JsonSerializer
		{
			TypeNameAssemblyFormat= System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple, 
			TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,
		});

		[Fact]
		public void WhenSavingObject_ThenPersistsEvents()
		{
			var path = "Store";
			var store = new FileSystemStore<int, DomainEvent>(path, this.serializer);

			var product = new Product(5, "Hello");
			product.Publish(1);

			store.SaveChanges(product);
		}


		internal class DomainEvent { }

		internal class Product : DomainObject<int, DomainEvent>
		{
			private Product()
			{
				this.Handles<ProductCreated>(this.OnCreated);
				this.Handles<ProductPublished>(this.OnPublished);
			}

			public Product(int id, string title)
				: this()
			{
				this.Apply(new ProductCreated { Id = id, Title = title });
			}

			public string Title { get; private set; }
			public int Version { get; set; }

			public void Publish(int version)
			{
				this.Apply(new ProductPublished { Version = version });
			}

			private void OnCreated(ProductCreated args)
			{
				this.Id = args.Id;
				this.Title = args.Title;
			}

			private void OnPublished(ProductPublished args)
			{
				this.Version = args.Version;
			}
		}

		internal class ProductPublished : DomainEvent
		{
			public int Version { get; set; }
		}

		internal class ProductCreated : DomainEvent
		{
			public int Id { get; set; }
			public string Title { get; set; }
		}
	}
}