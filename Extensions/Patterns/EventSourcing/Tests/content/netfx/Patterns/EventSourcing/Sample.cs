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
using Xunit;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;

namespace NetFx.Patterns.EventSourcing.Core.Tests
{
	/// <nuget id="netfx-Patterns.EventSourcing.Core.Tests" />
	public class Sample
	{
		[Fact]
		public void WhenEventPersisted_ThenCanObserveIt()
		{
			var store = new MemoryEventStore<int>();
			var product = new Product(5, "DevStore");
			product.Publish(1);
			product.Publish(2);
			product.Publish(3);
			product.GetChanges().ToList()
				.ForEach(e => store.Save(product, e));

			product = new Product(6, "WoVS");
			product.Publish(1);
			product.Publish(2);
			product.GetChanges().ToList()
				.ForEach(e => store.Save(product, e));

			var product2 = new Product();
			product2.Load(store.Query().For<Product>(6));

			Assert.Equal(product.Id, product2.Id);
			Assert.Equal(product.Version, product2.Version);

			var events = store.Query().For<Product>(5).OfType<Product.PublishedEvent>();
			Assert.Equal(3, events.Count());

			Console.WriteLine("For product 5, of type published:");
			foreach (var e in events)
			{
				Console.WriteLine("\t" + e);
			}

			events = store.Query().For<Product>().OfType<Product.PublishedEvent>();
			Assert.Equal(5, events.Count());

			Console.WriteLine();
			Console.WriteLine("For all products, of type published:");
			foreach (var e in events)
			{
				Console.WriteLine("\t" + e);
			}

			events = store.Query().For<Product>();
			Assert.Equal(7, events.Count());
			
			Console.WriteLine();
			Console.WriteLine("For all products, all event types:");
			foreach (var e in events)
			{
				Console.WriteLine("\t" + e);
			}

			events = store.Query().OfType<Product.CreatedEvent>();
			Assert.Equal(2, events.Count());

			Console.WriteLine();
			Console.WriteLine("Products created events:");
			foreach (var e in events)
			{
				Console.WriteLine("\t" + e);
			}

			//product.Load(store.Events.For<Product>(23));

			//var player = new DomainEventPlayer(store);
			//player.Observe<ProductPublishedEvent>();

			//player.Replay(from: null, to: null);
			//store.Events.Where(
			//store.Events.Where(x => x.AggregateType == "Product" &&
			//    x.EventType == "Published");
		}
	}
}