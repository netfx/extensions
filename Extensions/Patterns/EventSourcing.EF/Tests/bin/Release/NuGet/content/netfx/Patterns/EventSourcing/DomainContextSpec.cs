using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Xunit;
using Moq;

namespace NetFx.Patterns.EventSourcing.EF
{
	/// <nuget id="netfx-Patterns.EventSourcing.EF" />
	public class DomainContextSpec
	{
		[Fact]
		public void WhenPersisting_ThenSubmitsToBus()
		{
			var bus = new Mock<IDomainEventBus>();
			var id = 0;

			// Some other code creates the aggregate root.
			using (var context = new TestContext(bus.Object))
			{
				var entity = new TestEntity();
				context.Save(entity);
				context.SaveChanges();

				id = entity.Id;
			}

			bus.Verify(x => x.Publish<int>(It.IsAny<TestEntity>(), It.IsAny<DomainEvent>()), Times.Never());

			// Now it's time to do something on the domain
			using (var context = new TestContext(bus.Object))
			{
				var entity = context.Find<TestEntity>(id);

				entity.Publish("Foo");

				context.SaveChanges();
			}

			bus.Verify(x => x.Publish<int>(It.IsAny<TestEntity>(), It.IsAny<DomainEvent>()), Times.Once());
		}

		[Fact]
		public void WhenPersisting_ThenAcceptsChangeEvents()
		{
			var bus = new Mock<IDomainEventBus>();
			var id = 0;

			// Some other code creates the aggregate root.
			using (var context = new TestContext(bus.Object))
			{
				var entity = new TestEntity();
				context.Save(entity);
				context.SaveChanges();

				id = entity.Id;
			}

			bus.Verify(x => x.Publish<int>(It.IsAny<TestEntity>(), It.IsAny<DomainEvent>()), Times.Never());

			// Now it's time to do something on the domain
			using (var context = new TestContext(bus.Object))
			{
				var entity = context.Find<TestEntity>(id);

				entity.Publish("Foo");

				context.SaveChanges();
			}

			bus.Verify(x => x.Publish<int>(It.IsAny<TestEntity>(), It.IsAny<DomainEvent>()), Times.Once());
		}
	}

	public class TestContext : DomainContext<TestContext, int>
	{
		public TestContext(IDomainEventBus bus)
			: base(bus)
		{
		}

		public virtual DbSet<TestEntity> Tests { get; set; }
	}

	public class PublishEvent : DomainEvent<int>
	{
		public PublishEvent(int id, string title)
		{
			this.AggregateId = id;
			this.Title = title;
		}

		public string Title { get; private set; }
	}

	public class TestEntity : AggregateRoot<int>
	{
		static TestEntity()
		{
			Database.SetInitializer<TestContext>(new DropCreateDatabaseIfModelChanges<TestContext>());
		}

		public string Title { get; set; }

		public void Publish(string title)
		{
			if (string.IsNullOrEmpty(title))
				throw new ArgumentException();

			this.ApplyEvent(new PublishEvent(this.Id, title), this.Apply);
		}

		private void Apply(PublishEvent publish)
		{
			this.Title = publish.Title;
		}
	}
}
