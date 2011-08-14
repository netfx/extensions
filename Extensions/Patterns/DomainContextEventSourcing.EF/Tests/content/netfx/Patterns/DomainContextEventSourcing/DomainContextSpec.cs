using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Xunit;
using Moq;

namespace NetFx.Patterns.DomainContextEventSourcing.EF
{
	/// <nuget id="netfx-Patterns.EventSourcing.EF" />
	public class DomainContextSpec
	{
		[Fact]
		public void WhenPersisting_ThenSubmitsToBus()
		{
			var bus = new Mock<IDomainEventBus<int>>();
			var id = 0;

			// Some other code creates the aggregate root.
			using (var context = new TestContext(bus.Object))
			{
				var entity = new TestEntity();
				context.Save(entity);
				context.SaveChanges();

				id = entity.Id;
			}

			bus.Verify(x => x.Publish(It.IsAny<TestEntity>(), It.IsAny<TimestampedEventArgs>()), Times.Never());

			// Now it's time to do something on the domain
			using (var context = new TestContext(bus.Object))
			{
				var entity = context.Find<TestEntity>(id);

				entity.Publish("Foo");

				context.SaveChanges();
			}

			bus.Verify(x => x.Publish(It.IsAny<TestEntity>(), It.IsAny<TimestampedEventArgs>()), Times.Once());
		}

		[Fact]
		public void WhenPersisting_ThenAcceptsChangeEvents()
		{
			var bus = new Mock<IDomainEventBus<int>>();
			var id = 0;

			// Some other code creates the aggregate root.
			using (var context = new TestContext(bus.Object))
			{
				var entity = new TestEntity();
				context.Save(entity);
				context.SaveChanges();

				id = entity.Id;
			}

			bus.Verify(x => x.Publish(It.IsAny<TestEntity>(), It.IsAny<TimestampedEventArgs>()), Times.Never());

			// Now it's time to do something on the domain
			using (var context = new TestContext(bus.Object))
			{
				var entity = context.Find<TestEntity>(id);

				entity.Publish("Foo");

				context.SaveChanges();
			}

			bus.Verify(x => x.Publish(It.IsAny<TestEntity>(), It.IsAny<TimestampedEventArgs>()), Times.Once());
		}
	}

	/// <nuget id="netfx-Patterns.EventSourcing.EF" />
	internal class TestContext : DomainContext<TestContext, int>
	{
		public TestContext(IDomainEventBus<int> bus)
			: base(bus)
		{
		}

		public virtual DbSet<TestEntity> Tests { get; set; }
	}

	/// <nuget id="netfx-Patterns.EventSourcing.EF" />
	internal class PublishEvent : TimestampedEventArgs
	{
		public PublishEvent(int id, string title)
		{
			this.ProductId = id;
			this.Title = title;
		}

		public int ProductId { get; set; }
		public string Title { get; private set; }
	}

	/// <nuget id="netfx-Patterns.EventSourcing.EF" />
	internal class TestEntity : AggregateRoot<int>
	{
		static TestEntity()
		{
			Database.SetInitializer<TestContext>(new DropCreateDatabaseIfModelChanges<TestContext>());
		}

		public TestEntity()
		{
			this.Handles<PublishEvent>(this.OnPublished);
		}

		public string Title { get; set; }

		public void Publish(string title)
		{
			if (string.IsNullOrEmpty(title))
				throw new ArgumentException();

			this.ApplyChange(new PublishEvent(this.Id, title));
		}

		private void OnPublished(PublishEvent publish)
		{
			this.Title = publish.Title;
		}
	}
}
