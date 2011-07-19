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
using NetFx.Patterns.EventSourcing.Core.Tests;
using System.Data.Entity;

namespace NetFx.Patterns.EventSourcing.EF.Tests
{
	///	<netfx id="netfx-Patterns.EventSourcing.EF.Tests" />
	public class DomainEventStoreSpec
	{
		[Fact]
		public void WhenSavingEvent_ThenCanRetrieveIt()
		{
			Database.SetInitializer(new DropCreateDatabaseAlways<DomainEventStore>());
			IDomainEventStore<int> store = new DomainEventStore("EventSourcing.EF");
			var product = new Product(5, "DevStore");
			product.Publish(1);

			foreach (var e in product.GetChanges())
			{
				store.Save(product, e);
			}

			store.SaveChanges();

			var events = store.Query().ToList();

			Assert.Equal(2, events.Count);
			Assert.True(events.OfType<Product.CreatedEvent>().Any(x => x.Id == 5 && x.Title == "DevStore"));
			Assert.True(events.OfType<Product.PublishedEvent>().Any(x => x.Version == 1));
		}

		[Fact]
		public void WhenSavingMultipleEvents_ThenCanLoadSpecificAggregate()
		{
			Database.SetInitializer(new DropCreateDatabaseAlways<DomainEventStore>());
			IDomainEventStore<int> store = new DomainEventStore("EventSourcing.EF");
			var product = new Product(5, "DevStore");
			product.Publish(1);

			foreach (var e in product.GetChanges())
			{
				store.Save(product, e);
			}

			product = new Product(6, "WoVS");
			product.Publish(1);
			product.Publish(2);
			product.Publish(3);

			foreach (var e in product.GetChanges())
			{
				store.Save(product, e);
			}

			store.SaveChanges();

			var saved = new Product();
			saved.Load(store.Query().For<Product>(6));

			Assert.Equal(3, saved.Version);
			Assert.Equal("WoVS", saved.Title);
			Assert.Equal(6, saved.Id);
		}

	}

	public class DomainEventStore : DomainEventStore<int, StoredEvent>
	{
		public DomainEventStore(string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
		}
	}

	public class StoredEvent : StoredEvent<int>
	{
	}
}