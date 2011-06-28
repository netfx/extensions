using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Data.Entity;
using Moq;
using System.ComponentModel.DataAnnotations;

public class DomainEventsContextSpec
{
	public DomainEventsContextSpec()
	{
		Database.SetInitializer(new DropCreateDatabaseAlways<TestDomainEventsContext>());
		using (var db = new TestDomainEventsContext(Mock.Of<IDomainEvents>()))
		{
			db.Database.Initialize(true);
		}
	}

	[Fact]
	public void WhenEntityRaisesEvents_ThenTheyAreDispatchedAfterSubmitChanges()
	{
		var events = new Mock<IDomainEvents>();
		using (var context = new TestDomainEventsContext(events.Object))
		{
			var handler = new Mock<IDomainEventHandler<FooArgs>>();

			var foo = context.New<FooWithEvents>();
			foo.RaiseSomeEvent();

			events.Verify(x => x.Raise(It.IsAny<FooEventContextArgs>()), Times.Never());

			context.Save(foo);

			events.Verify(x => x.Raise(It.IsAny<FooEventContextArgs>()), Times.Never());

			context.SaveChanges();

			events.Verify(x => x.Raise(It.IsAny<FooEventContextArgs>()));
		}
	}

	[Fact]
	public void WhenNoEventsSpecifiedForContext_ThenEntityCanRaiseEventsWithNoOpBehavior()
	{
		var events = new Mock<IDomainEvents>();
		using (var context = new TestDomainEventsContext())
		{
			var handler = new Mock<IDomainEventHandler<FooArgs>>();

			var foo = context.New<FooWithEvents>();
			foo.RaiseSomeEvent();

			context.Save(foo);
			context.SaveChanges();
		}
	}
}

public class TestDomainEventsContext : DomainContext<TestDomainEventsContext, long>
{
	public TestDomainEventsContext()
	{
	}

	public TestDomainEventsContext(IDomainEvents events)
		: base(events)
	{
	}

	public DbSet<FooWithEvents> Foos { get; set; }
}

public class FooWithEvents : IAggregateRoot<long>, IDomainEventsAccessor
{
	public long Id { get; set; }
	public bool IsDeleted { get; set; }

	public string Name { get; set; }

	private FooWithEvents()
		: this(DomainEvents.None)
	{
	}

	public FooWithEvents(IDomainEvents events)
	{
		this.Events = events;
	}

	public void RaiseSomeEvent()
	{
		this.Events.Raise(new FooEventContextArgs());
	}

	[NotMapped]
	public IDomainEvents Events { get; set; }
}

public class FooEventContextArgs
{
	public long Id { get; set; }
}
