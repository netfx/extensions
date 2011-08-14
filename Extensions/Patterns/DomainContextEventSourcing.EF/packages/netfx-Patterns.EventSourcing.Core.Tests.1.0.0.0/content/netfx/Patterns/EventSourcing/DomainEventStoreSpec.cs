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
			DomainEventStore<int>.None.Save(null, null);
		}

		[Fact]
		public void WhenSavingChanges_ThenNoOp()
		{
			DomainEventStore<int>.None.SaveChanges();
		}

		[Fact]
		public void WhenQuerying_ThenReturnsEmpty()
		{
			Assert.False(DomainEventStore<int>.None.Query(null).Any());
		}
	}
}
