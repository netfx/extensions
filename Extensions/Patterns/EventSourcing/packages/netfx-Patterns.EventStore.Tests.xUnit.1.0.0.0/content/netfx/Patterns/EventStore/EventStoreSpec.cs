using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NetFx.Patterns.EventStore.Tests
{
	public class EventStoreSpec
	{
		[Fact]
		public void WhenSaving_ThenNoOp()
		{
			EventStore<DomainEvent>.None.Persist(null);
		}

		[Fact]
		public void WhenSavingChanges_ThenNoOp()
		{
			EventStore<DomainEvent>.None.Commit();
		}

		[Fact]
		public void WhenQuerying_ThenReturnsEmpty()
		{
			Assert.False(EventStore<DomainEvent>.None.Query(null).Any());
		}
	}
}
