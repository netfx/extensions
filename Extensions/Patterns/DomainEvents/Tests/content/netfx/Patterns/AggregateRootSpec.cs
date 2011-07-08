using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

/// <nuget id="netfx-Patterns.DomainEvents.Tests" />
public class AggregateRootSpec
{
	[Fact]
	public void WhenDomainActionPerformed_ThenRootChangesStateThroughEvent()
	{
		var root = new TestRoot();
		root.Publish(5);

		Assert.Equal(5, root.LatestVersion);
		Assert.True(root.GetChanges().Any());
		Assert.True(root.GetChanges().OfType<TestPublished>().Any(x => x.Version == 5));

		root.AcceptChanges();

		Assert.False(root.GetChanges().Any());
	}

	[Fact]
	public void WhenLoadingFromEvent_ThenRootChangesState()
	{
		var root = new TestRoot();
		var events = new DomainEvent[] { new TestPublished { Version = 5 } };

		root.LoadFrom(events);

		Assert.Equal(5, root.LatestVersion);
		Assert.False(root.GetChanges().Any());

		// This should be no-op now.
		root.AcceptChanges();

		Assert.False(root.GetChanges().Any());
	}


	public class TestRoot : AggregateRoot<Guid>
	{
		public void Publish(int version)
		{
			if (version < 0)
				throw new ArgumentException();

			base.ApplyChange(new TestPublished { Version = version }, this.Apply);
		}

		public int LatestVersion { get; set; }

		private void Apply(TestPublished published)
		{
			this.LatestVersion = published.Version;
		}
	}

	public class TestPublished : DomainEvent
	{
		public int Version { get; set; }
	}
}
