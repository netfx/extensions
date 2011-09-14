using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Collections;
using Moq;
using System.Reflection;

namespace NetFx.Patterns.SystemEventStore.Tests
{
	public class MessageStoreQueryExtensionSpec
	{
		private IMessageStore<Message> store;
		private Func<object, MessageStoreQueryCriteria> GetCriteria = query =>
			(MessageStoreQueryCriteria)query.GetType().GetField("criteria", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(query);

		public MessageStoreQueryExtensionSpec()
		{
			this.store = Mock.Of<IMessageStore<Message>>();
		}

		[Fact]
		public void WhenEmptyQuery_ThenSinceUntilAreNull()
		{
			var query = GetCriteria(store.Query());

			Assert.Null(query.Since);
			Assert.Null(query.Until);
		}

		[Fact]
		public void WhenEmptyQuery_ThenIsExclusiveRangeIsFalse()
		{
			var query = GetCriteria(store.Query());

			Assert.False(query.IsExclusiveRange);
		}

		[Fact]
		public void WhenSettingExclusiveRange_ThenSetsIsExclusiveRange()
		{
			var query = GetCriteria(store.Query().ExclusiveRange());

			Assert.True(query.IsExclusiveRange);
		}

		[Fact]
		public void WhenSettingSince_ThenPopulatesSinceCriteria()
		{
			var date = DateTime.UtcNow;
			var query = GetCriteria(store.Query().Since(date));

			Assert.Equal(date, query.Since);
		}

		[Fact]
		public void WhenSettingUntil_ThenPopulatesUntilCriteria()
		{
			var date = DateTime.UtcNow;
			var query = GetCriteria(store.Query().Until(date));

			Assert.Equal(date, query.Until);
		}

		[Fact]
		public void WhenSpecifyingOfType_ThenAddsMessageType()
		{
			var criteria = GetCriteria(store.Query().OfType<CreateUser>());

			Assert.Equal(1, criteria.MessageTypes.Count);
			Assert.Equal(typeof(CreateUser), criteria.MessageTypes.First());
		}

		[Fact]
		public void WhenSpecifyingMultipleOfType_ThenAddsAllMessageTypes()
		{
			var criteria = GetCriteria(store.Query().OfType<CreateUser>().OfType<DeactivateUser>());

			Assert.Equal(2, criteria.MessageTypes.Count);
			Assert.True(criteria.MessageTypes.Any(type => type == typeof(CreateUser)));
			Assert.True(criteria.MessageTypes.Any(type => type == typeof(DeactivateUser)));
		}

		[Fact]
		public void WhenSpecifyingDuplicateOfType_ThenAddsOnlyOnce()
		{
			var criteria = GetCriteria(store.Query().OfType<CreateUser>().OfType<CreateUser>());

			Assert.Equal(1, criteria.MessageTypes.Count);
		}

		[Fact]
		public void WhenExecuting_ThenInvokesStoreQueryWithCriteria()
		{
			var query = store.Query()
				.Since(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
				.OfType<CreateUser>();

			var criteria = GetCriteria(query);

			query.Execute();

			Mock.Get(this.store)
				.Verify(x => x.Query(criteria));
		}

		public class Message { }
		public class CreateUser : Message { }
		public class DeactivateUser : Message { }
	}
}
