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
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using NetFx.Patterns.EventSourcing.Tests;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace NetFx.Patterns.EventSourcing.EF.Tests
{
	public class EventStoreSpec
	{
		[Fact]
		public void WhenSavingEvent_ThenCanRetrieveIt()
		{
			Database.SetInitializer(new DropCreateDatabaseAlways<EventStore>());
			using (var store = new EventStore("EventSourcing.EF", new BinarySerializer()))
			{
				store.Database.Initialize(true);
			}

			var id = Guid.NewGuid();
			using (var store = new EventStore("EventSourcing.EF", new BinarySerializer()))
			{
				var product = new Product(id, "DevStore");
				product.Publish(1);

				store.SaveChanges(product);

				var events = store.Query().Execute().ToList();

				Assert.Equal(2, events.Count);
				Assert.True(events.OfType<Product.CreatedEvent>().Any(x => x.Id == id && x.Title == "DevStore"));
				Assert.True(events.OfType<Product.PublishedEvent>().Any(x => x.Version == 1));
			}
		}

		[Fact]
		public void WhenSavingMultipleEvents_ThenCanLoadSpecificObject()
		{
			Database.SetInitializer(new DropCreateDatabaseAlways<EventStore>());
			using (var store = new EventStore("EventSourcing.EF", new BinarySerializer()))
			{
				store.Database.Initialize(true);
			}

			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			using (var store = new EventStore("EventSourcing.EF", new BinarySerializer()))
			{
				var product = new Product(id1, "DevStore");
				product.Publish(1);

				store.SaveChanges(product);

				product = new Product(id2, "WoVS");
				product.Publish(1);
				product.Publish(2);
				product.Publish(3);

				store.SaveChanges(product);

				var events = store.Query().For<Product>(id2).Execute().ToList();
				var saved = new Product(events);

				Assert.Equal(3, saved.Version);
				Assert.Equal("WoVS", saved.Title);
				Assert.Equal(id2, saved.Id);
			}
		}

		[Fact]
		public void WhenSavingEvents_ThenAcceptsChangesOnObject()
		{
			Database.SetInitializer(new DropCreateDatabaseAlways<EventStore>());
			using (var store = new EventStore("EventSourcing.EF", new BinarySerializer()))
			{
				store.Database.Initialize(true);
			}

			var id = Guid.NewGuid();
			using (var store = new EventStore("EventSourcing.EF", new BinarySerializer()))
			{
				var product = new Product(id, "DevStore");
				product.Publish(1);

				store.SaveChanges(product);

				var events = store.Query().Execute().ToList();

				Assert.Equal(2, events.Count);
				Assert.True(events.OfType<Product.CreatedEvent>().Any(x => x.Id == id && x.Title == "DevStore"));
				Assert.True(events.OfType<Product.PublishedEvent>().Any(x => x.Version == 1));
			}
		}

		private class BinarySerializer : ISerializer
		{
			public T Deserialize<T>(global::System.IO.Stream stream)
			{
				return (T)new BinaryFormatter().Deserialize(stream);
			}

			public void Serialize<T>(global::System.IO.Stream stream, T graph)
			{
				new BinaryFormatter().Serialize(stream, graph);
			}
		}
	}

	internal class EventStore : EventStore<DomainEvent>
	{
		public EventStore(ISerializer serializer)
			: base("EventStore", serializer)
		{
		}

		public EventStore(string nameOrConnectionString, ISerializer serializer)
			: base(nameOrConnectionString, serializer)
		{
		}
	}
}