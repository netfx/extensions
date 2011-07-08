using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;

/// <nuget id="netfx-Patterns.DomainEvents.Tests" />
public class DomainEventsSpec
{
	[Fact]
	public void WhenNullHandlers_ThenThrows()
	{
		Assert.Throws<ArgumentNullException>(() => new DomainEventBus(null));
	}

	[Fact]
	public void WhenNullAsyncRunner_ThenThrows()
	{
		Assert.Throws<ArgumentNullException>(() => new DomainEventBus(Enumerable.Empty<DomainEventHandler>(), null));
	}

	[Fact]
	public void WhenPublishNullEvent_ThenThrows()
	{
		var bus = new DomainEventBus(Enumerable.Empty<DomainEventHandler>());

		Assert.Throws<ArgumentNullException>(() => bus.Publish(null));
	}

	[Fact]
	public void WhenHandlerRegisteredForSpecificType_ThenHandlesOnRaise()
	{
		var args = new FooArgs { Id = 5 };
		var handler = new Mock<DomainEventHandler<FooArgs>> { CallBase = true };

		var bus = new DomainEventBus(new[] { handler.Object });

		bus.Publish(args);

		handler.Verify(x => x.Handle(args));
	}

	[Fact]
	public void WhenHandlerRegisteredForBaseType_ThenHandlesOnRaise()
	{
		var args = new FooArgs { Id = 5 };
		var handler = new Mock<DomainEventHandler<BaseArgs>> { CallBase = true };

		var bus = new DomainEventBus(new[] { handler.Object });

		bus.Publish(args);

		handler.Verify(x => x.Handle(args));
	}

	[Fact]
	public void WhenAsyncHandlerRegisteredForSpecificType_ThenInvokesAsyncRunner()
	{
		var args = new FooArgs { Id = 5 };
		var handler = new Mock<DomainEventHandler<FooArgs>> { CallBase = true };
		handler.Setup(x => x.IsAsync).Returns(true);
		var asyncCalled = false;
		Action<Action> asyncRunner = action => asyncCalled = true;

		var bus = new DomainEventBus(new[] { handler.Object }, asyncRunner);

		bus.Publish(args);

		handler.Verify(x => x.Handle(args), Times.Never());
		Assert.True(asyncCalled);
	}

	[Fact]
	public void WhenAsyncHandlerRegisteredForBaseType_ThenHandlesOnRaise()
	{
		var args = new FooArgs { Id = 5 };
		var handler = new Mock<DomainEventHandler<BaseArgs>> { CallBase = true };
		handler.Setup(x => x.IsAsync).Returns(true);
		var asyncCalled = false;
		Action<Action> asyncRunner = action => asyncCalled = true;

		var bus = new DomainEventBus(new[] { handler.Object }, asyncRunner);

		bus.Publish(args);

		handler.Verify(x => x.Handle(args), Times.Never());
		Assert.True(asyncCalled);
	}

	[Fact]
	public void WhenAsyncHandlerRegisteredForSpecificType_ThenCanUseDefaultAsynRunner()
	{
		var args = new FooArgs { Id = 5 };
		var handler = new Mock<DomainEventHandler<FooArgs>> { CallBase = true };
		handler.Setup(x => x.IsAsync).Returns(true);

		var bus = new DomainEventBus(new[] { handler.Object });

		bus.Publish(args);

		handler.Verify(x => x.Handle(args), Times.Never());
	}

	[Fact]
	public void WhenDefaultDomainEventsRaises_ThenDoesNothing()
	{
		DomainEventBus.None.Publish(new FooArgs());
	}

	[Fact]
	public void WhenHandlerDoesNotInheritFromGenericHandler_ThenThrows()
	{
		var handler = new NonGenericHandler();

		Assert.Throws<ArgumentException>(() => new DomainEventBus(new DomainEventHandler[] { handler }));
	}

	[Fact]
	public void WhenHandlerExposesNullEventType_ThenThrows()
	{
		var handler = new Mock<DomainEventHandler<FooArgs>>();

		Assert.Throws<ArgumentException>(() => new DomainEventBus(new DomainEventHandler[] { handler.Object }));
	}

	[Fact]
	public void WhenNullHandlerProvided_ThenThrows()
	{
		Assert.Throws<ArgumentException>(() => new DomainEventBus(new DomainEventHandler[] { null }));
	}

	private class HandlerBase : DomainEventHandler<FooArgs>
	{
		public override void Handle(FooArgs @event)
		{
		}

		public override bool IsAsync { get { return false; } }
	}

	private class Handler : HandlerBase
	{
	}

	private class NonGenericHandler : DomainEventHandler
	{
		public override bool IsAsync { get { return false; } }

		public override Type EventType { get { return typeof(FooArgs); } }
	}
}

public class BaseArgs : DomainEvent
{
}

public class FooArgs : BaseArgs
{
	public int Id { get; set; }
}
