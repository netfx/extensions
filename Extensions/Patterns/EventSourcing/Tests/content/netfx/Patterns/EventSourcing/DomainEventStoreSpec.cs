using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NetFx.Patterns.EventSourcing.Tests
{
	public class DomainEventStoreSpec
	{
		[Fact]
		public void WhenPersistingEvent_ThenNoOp()
		{
			DomainEventStore<int, DomainEvent>.None.Persist(null);
		}

		[Fact]
		public void WhenPersistingAggregateEvent_ThenNoOp()
		{
			DomainEventStore<int, DomainEvent>.None.Persist(null, null);
		}

		[Fact]
		public void WhenCommit_ThenNoOp()
		{
			DomainEventStore<int, DomainEvent>.None.Commit();
		}

		[Fact]
		public void WhenQuerying_ThenReturnsEmpty()
		{
			Assert.False(DomainEventStore<int, DomainEvent>.None.Query(null).Any());
		}

		[Fact]
		public void WhenQuerying_ThenReturnsEmptyNonAggregate()
		{
			Assert.False(((IEventStore<DomainEvent>)DomainEventStore<int, DomainEvent>.None).Query(null).Any());
		}
	}
}
