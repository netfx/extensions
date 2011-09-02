using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Collections;
using Moq;
using System.Threading;

namespace NetFx.Patterns.MessageStore.Memory
{
	public class MemoryMessageStoreSpec
	{
		private MemoryMessageStore<Message> store;
		private Func<DateTime> utcNow = () => DateTime.UtcNow;
		private IDictionary<string, object> emptyHeaders = new Dictionary<string, object>();

		public MemoryMessageStoreSpec()
		{
			this.store = new MemoryMessageStore<Message>(() => this.utcNow());

			this.store.Persist(new CreateProduct { Id = 5, Title = "DevStore" }, emptyHeaders);
			this.store.Persist(new PublishProduct { Version = 1 }, emptyHeaders);
			this.store.Persist(new PublishProduct { Version = 2 }, emptyHeaders);
			this.store.Persist(new PublishProduct { Version = 3 }, emptyHeaders);

			this.store.Persist(new CreateProduct { Id = 6, Title = "WoVS" }, emptyHeaders);
			this.store.Persist(new PublishProduct { Version = 1 }, emptyHeaders);
			this.store.Persist(new PublishProduct { Version = 2 }, emptyHeaders);
		}

		[Fact]
		public void WhenCommitting_ThenNoOp()
		{
			this.store.Commit();
		}

		[Fact]
		public void WhenNoDateTimeFuncProvided_ThenUsesUtcNow()
		{
			this.store = new MemoryMessageStore<Message>();

			this.store.Persist(new CreateProduct { Id = 5, Title = "DevStore" }, emptyHeaders);

			Thread.Sleep(1);

			Assert.True(DateTime.UtcNow.Ticks > this.store.AllMessages.First().Timestamp.Ticks);
		}

		[Fact]
		public void WhenGettingEmptyQueryEnumerable_ThenEmptyList()
		{
			var store = new MemoryMessageStore<Message>(() => this.utcNow());
			var enumerable = store.Query() as IEnumerable;

			Assert.False(enumerable.GetEnumerator().MoveNext());
		}

		[Fact]
		public void WhenGettingAllMessages_ThenSucceeds()
		{
			var messages = store.AllMessages.Select(x => x.ToString());

			Assert.Equal(7, messages.Count());
		}

		[Fact]
		public void WhenFilteringByMessageType_ThenSucceeds()
		{
			var messages = store.Query().OfType<CreateProduct>();

			Assert.Equal(2, messages.Count());
		}

		[Fact]
		public void WhenFilteringByMessageType_ThenCanAccessCriteria()
		{
			var messages = store.Query().OfType<CreateProduct>();

			Assert.Equal(1, messages.Criteria.MessageTypes.Count);
		}

		[Fact]
		public void WhenFilteringByDateSince_ThenSucceeds()
		{
			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			store.Persist(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			store.Persist(new Deactivate(), emptyHeaders);

			this.utcNow = () => when;
			store.Persist(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			store.Persist(new Deactivate(), emptyHeaders);

			var messages = store.Query().OfType<Deactivate>().Since(when);

			Assert.Equal(2, messages.Count());
		}

		[Fact]
		public void WhenFilteringByDateSinceExclusive_ThenSucceeds()
		{
			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			store.Persist(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			store.Persist(new Deactivate(), emptyHeaders);

			this.utcNow = () => when;
			store.Persist(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			store.Persist(new Deactivate(), emptyHeaders);

			var messages = store.Query().OfType<Deactivate>().Since(when).ExclusiveRange();

			Assert.Equal(1, messages.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntil_ThenSucceeds()
		{
			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			store.Persist(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			store.Persist(new Deactivate(), emptyHeaders);

			this.utcNow = () => when;
			store.Persist(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			store.Persist(new Deactivate(), emptyHeaders);

			var messages = store.Query().OfType<Deactivate>().Until(when);

			Assert.Equal(3, messages.Count());
		}

		[Fact]
		public void WhenFilteringByDateUntilExclusive_ThenSucceeds()
		{
			var when = DateTime.Today.Subtract(TimeSpan.FromDays(5)).ToUniversalTime();

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
			store.Persist(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(6)).ToUniversalTime();
			store.Persist(new Deactivate(), emptyHeaders);

			this.utcNow = () => when;
			store.Persist(new Deactivate(), emptyHeaders);

			this.utcNow = () => DateTime.Today.Subtract(TimeSpan.FromDays(4)).ToUniversalTime();
			store.Persist(new Deactivate(), emptyHeaders);

			var messages = store.Query().OfType<Deactivate>().Until(when).ExclusiveRange();

			Assert.Equal(2, messages.Count());
		}

		public class Deactivate : Message
		{
			public override string ToString()
			{
				return this.GetType().Name;
			}
		}

		public abstract class Message { }

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

		public class PublishProduct : Message
		{
			public int Version { get; set; }

			public override string ToString()
			{
				return "Published new product version " + this.Version + ".";
			}
		}
	}
}
