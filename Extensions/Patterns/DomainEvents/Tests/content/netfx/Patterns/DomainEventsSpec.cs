using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;

public class DomainEventsSpec
{
	[Fact]
	public void WhenHandlerRegisteredForSpecificType_ThenHandlesOnRaise()
	{
		var args = new FooArgs { Id = 5 };
		var handler = new Mock<DomainEventHandler<FooArgs>>();

		var events = new DomainEvents(new[] { handler.Object });

		events.Raise(args);

		handler.Verify(x => x.Handle(args));
	}

	[Fact]
	public void WhenHandlerRegisteredForBaseType_ThenHandlesOnRaise()
	{
		var args = new FooArgs { Id = 5 };
		var handler = new Mock<DomainEventHandler<BaseArgs>>();

		var events = new DomainEvents(new[] { handler.Object });

		events.Raise(args);

		handler.Verify(x => x.Handle(args));
	}

	[Fact]
	public void WhenAsyncHandlerRegisteredForSpecificType_ThenInvokesScheduler()
	{
		var args = new FooArgs { Id = 5 };
		var handler = new Mock<DomainEventHandler<FooArgs>>();
		handler.Setup(x => x.IsAsync).Returns(true);
		var scheduler = new Mock<IDomainEventScheduler>();

		var events = new DomainEvents(new[] { handler.Object }, scheduler.Object);

		events.Raise(args);

		handler.Verify(x => x.Handle(args), Times.Never());
		scheduler.Verify(x => x.Schedule(It.IsAny<Action>()));
	}

	[Fact]
	public void WhenAsyncHandlerRegisteredForBaseType_ThenHandlesOnRaise()
	{
		var args = new FooArgs { Id = 5 };
		var handler = new Mock<DomainEventHandler<BaseArgs>>();
		handler.Setup(x => x.IsAsync).Returns(true);
		var scheduler = new Mock<IDomainEventScheduler>();

		var events = new DomainEvents(new[] { handler.Object }, scheduler.Object);

		events.Raise(args);

		handler.Verify(x => x.Handle(args), Times.Never());
		scheduler.Verify(x => x.Schedule(It.IsAny<Action>()));
	}

	[Fact]
	public void WhenAsyncHandlerRegisteredForSpecificType_ThenCanUseDefaultScheduler()
	{
		var args = new FooArgs { Id = 5 };
		var handler = new Mock<DomainEventHandler<FooArgs>>();
		handler.Setup(x => x.IsAsync).Returns(true);

		var events = new DomainEvents(new[] { handler.Object });

		events.Raise(args);

		handler.Verify(x => x.Handle(args), Times.Never());
	}

	[Fact]
	public void WhenDefaultDomainEventsRaises_ThenDoesNothing()
	{
		DomainEvents.None.Raise(new FooArgs());
	}
}

public class BaseArgs
{
}

public class FooArgs : BaseArgs
{
	public int Id { get; set; }
}
