using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NetFx.Patterns.EventSourcing.Core.Tests
{
	public class DomainEventStoreSpec
	{
		[Fact]
		public void WhenSaving_ThenNoOp()
		{
			DomainEventStore<int, DomainEvent>.None.Persist(null, null);
		}

		[Fact]
		public void WhenSavingChanges_ThenNoOp()
		{
			DomainEventStore<int, DomainEvent>.None.Commit();
		}

		[Fact]
		public void WhenQuerying_ThenReturnsEmpty()
		{
			Assert.False(DomainEventStore<int, DomainEvent>.None.Query(null).Any());
		}
	}
}
