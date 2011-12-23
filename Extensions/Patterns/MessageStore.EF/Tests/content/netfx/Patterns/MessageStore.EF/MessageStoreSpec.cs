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
using Moq;
using System.Threading;
using System.Collections;
using System.Data.Entity;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetFx.Patterns.MessageStore.EF
{
	public class MessageStoreSpec : IDisposable
	{
		private MessageStore<Message> store;
		private ISerializer serializer = new SimpleSerializer();

		private Func<DateTime> utcNow = () => DateTime.UtcNow;
		private IDictionary<string, object> emptyHeaders = new Dictionary<string, object>();

		static MessageStoreSpec()
		{
			Database.SetInitializer(new DropCreateDatabaseAlways<MessageStore<Message>>());
		}

		public MessageStoreSpec()
		{
			this.Store = CreateStore();

			this.Store.Save(new CreateProduct { Id = 5, Title = "DevStore" }, emptyHeaders);
			this.Store.Save(new PublishProduct { Version = 1 }, emptyHeaders);
			this.Store.Save(new PublishProduct { Version = 2 }, emptyHeaders);
			this.Store.Save(new PublishProduct { Version = 3 }, emptyHeaders);

			this.Store.Save(new CreateProduct { Id = 6, Title = "WoVS" }, emptyHeaders);
			this.Store.Save(new PublishProduct { Version = 1 }, emptyHeaders);
			this.Store.Save(new PublishProduct { Version = 2 }, emptyHeaders);
		}

		private MessageStore<Message> Store
		{
			get
			{
				return this.store ?? (this.store = CreateStore());
			}
			set
			{
				if (this.store != null)
					this.store.Dispose();

				this.store = value;
			}
		}

		public void Dispose()
		{
			if (this.store != null)
				this.store.Dispose();

			//Database.Delete("MessageStoreSpec");
		}

		private MessageStore<Message> CreateStore()
		{
			var clock = new Mock<IClock>();
			clock.Setup(x => x.UtcNow).Returns(() => this.utcNow());

			var store = new MessageStore<Message>("MessageStoreSpec", this.serializer) { SystemClock = clock.Object };

			store.Database.Initialize(true);

			return store;
		}

		[Fact]
		public void WhenNoClockProvided_ThenUsesUtcNow()
		{
			this.Store = new MessageStore<Message>("MessageStoreSpec", this.serializer);

			this.Store.Save(new CreateProduct { Id = 5, Title = "DevStore" }, emptyHeaders);

			Thread.Sleep(1);

			Assert.True(DateTime.UtcNow.Ticks > this.Store.Messages.First().Timestamp.Ticks);
		}

		[Fact]
		public void WhenGettingEmptyQueryEnumerable_ThenEmptyList()
		{
			using (var store = CreateStore())
			{
				var enumerable = store.Query().Execute() as IEnumerable;

				Assert.False(enumerable.GetEnumerator().MoveNext());
			}
		}

		[Fact]
		public void WhenGettingAllMessages_ThenSucceeds()
		{
			var messages = this.Store.Messages.Select(x => x.Id);

			Assert.Equal(7, messages.Count());
		}

		[Fact]
		public void WhenFilteringByMessageType_ThenSucceeds()
		{
			var messages = this.Store.Query().OfType<CreateProduct>().Execute();

			Assert.Equal(2, messages.Count());
		}

		[Fact]
		public void WhenFilteringByDateSince_ThenSucceeds()
		{
			this.Store = CreateStore();

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			this.Store.Save(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			this.Store.Save(new Deactivate(), emptyHeaders);

			this.utcNow = () => when;
			this.Store.Save(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			this.Store.Save(new Deactivate(), emptyHeaders);

			var messages = this.Store.Query().OfType<Deactivate>().Since(when).Execute();

			Assert.Equal(2, messages.Count());
		}

		[Fact]
		public void WhenFilteringByDateSinceExclusive_ThenSucceeds()
		{
			this.Store = CreateStore();

			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			this.Store.Save(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			this.Store.Save(new Deactivate(), emptyHeaders);

			this.utcNow = () => when;
			this.Store.Save(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			this.Store.Save(new Deactivate(), emptyHeaders);

			var messages = Store.Query().OfType<Deactivate>().Since(when).ExclusiveRange().Execute();

			Assert.Equal(1, messages.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntil_ThenSucceeds()
		{
			this.Store = CreateStore();
			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			this.Store.Save(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			this.Store.Save(new Deactivate(), emptyHeaders);

			this.utcNow = () => when;
			this.Store.Save(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			this.Store.Save(new Deactivate(), emptyHeaders);

			var messages = Store.Query().OfType<Deactivate>().Until(when).Execute();

			Assert.Equal(3, messages.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntilExclusive_ThenSucceeds()
		{
			this.Store = CreateStore();
			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			this.Store.Save(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			this.Store.Save(new Deactivate(), emptyHeaders);

			this.utcNow = () => when;
			this.Store.Save(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			this.Store.Save(new Deactivate(), emptyHeaders);

			var messages = Store.Query().OfType<Deactivate>().Until(when).ExclusiveRange().Execute();

			Assert.Equal(2, messages.Count());
		}

		[Serializable]
		public class Deactivate : Message
		{
			public override string ToString()
			{
				return this.GetType().Name;
			}
		}

		[Serializable]
		public abstract class Message { }

		[Serializable]
		public class CreateProduct : Message
		{
			public int Id { get; set; }
			public string Title { get; set; }

			public override string ToString()
			{
				return string.Format("Created new product with Id={0} and Title='{1}'.",
					this.Id, this.Title);
			}
		}

		[Serializable]
		public class PublishProduct : Message
		{
			public int Version { get; set; }

			public override string ToString()
			{
				return "Published new product version " + this.Version + ".";
			}
		}

		private class SimpleSerializer : ISerializer
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
}