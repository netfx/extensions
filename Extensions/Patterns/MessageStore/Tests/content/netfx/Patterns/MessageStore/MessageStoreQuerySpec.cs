using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Collections;
using Moq;

namespace NetFx.Patterns.SystemEventStore.Tests
{
	public class MessageStoreQueryExtensionSpec
	{
		private IMessageStore<Message> store;

		public MessageStoreQueryExtensionSpec()
		{
			this.store = Mock.Of<IMessageStore<Message>>();
		}

		[Fact]
		public void WhenEmptyQuery_ThenSinceUntilAreNull()
		{
			var query = store.Query().Criteria;

			Assert.Null(query.Since);
			Assert.Null(query.Until);
		}

		[Fact]
		public void WhenEmptyQuery_ThenIsExclusiveRangeIsFalse()
		{
			var query = store.Query().Criteria;

			Assert.False(query.IsExclusiveRange);
		}

		[Fact]
		public void WhenSettingExclusiveRange_ThenSetsIsExclusiveRange()
		{
			var query = store.Query().ExclusiveRange().Criteria;

			Assert.True(query.IsExclusiveRange);
		}

		[Fact]
		public void WhenSettingSince_ThenPopulatesSinceCriteria()
		{
			var date = DateTime.UtcNow;
			var query = store.Query().Since(date).Criteria;

			Assert.Equal(date, query.Since);
		}

		[Fact]
		public void WhenSettingUntil_ThenPopulatesUntilCriteria()
		{
			var date = DateTime.UtcNow;
			var query = store.Query().Until(date).Criteria;

			Assert.Equal(date, query.Until);
		}

		[Fact]
		public void WhenSpecifyingOfType_ThenAddsMessageType()
		{
			var query = store.Query().OfType<CreateUser>();

			Assert.Equal(1, query.Criteria.MessageTypes.Count);
			Assert.Equal(typeof(CreateUser), query.Criteria.MessageTypes.First());
		}

		[Fact]
		public void WhenSpecifyingMultipleOfType_ThenAddsAllMessageTypes()
		{
			var query = store.Query().OfType<CreateUser>().OfType<DeactivateUser>();

			Assert.Equal(2, query.Criteria.MessageTypes.Count);
			Assert.True(query.Criteria.MessageTypes.Any(type => type == typeof(CreateUser)));
			Assert.True(query.Criteria.MessageTypes.Any(type => type == typeof(DeactivateUser)));
		}

		[Fact]
		public void WhenSpecifyingDuplicateOfType_ThenAddsOnlyOnce()
		{
			var query = store.Query().OfType<CreateUser>().OfType<CreateUser>();

			Assert.Equal(1, query.Criteria.MessageTypes.Count);
		}

		[Fact]
		public void WhenExecuting_ThenInvokesStoreQueryWithCriteria()
		{
			var query = store.Query()
				.Since(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
				.OfType<CreateUser>();

			var criteria = query.Criteria;

			query.Execute();

			Mock.Get(this.store)
				.Verify(x => x.Query(criteria));
		}

		public class Message { }
		public class CreateUser : Message { }
		public class DeactivateUser : Message { }
	}
}
