using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;

/// <nuget id="netfx-Patterns.DomainCommands.Tests" />
public class DomainCommandsSpec
{
	[Fact]
	public void WhenHandlerRegisteredForSpecificType_ThenHandlesOnExecute()
	{
		var command = new FooCommand { Id = 5 };
		var handler = new Mock<DomainCommandHandler<FooCommand>> { CallBase = true };

		var bus = new DomainCommandBus<IDomainCommand>(new[] { handler.Object });

		bus.Execute(command);

		handler.Verify(x => x.Handle(command));
	}

	[Fact]
	public void WhenHandlerRegisteredForBaseType_ThenDoesNotInvokeHandlesOnExecute()
	{
		var command = new FooCommand { Id = 5 };
		var handler = new Mock<DomainCommandHandler<BaseCommand>> { CallBase = true };

		var bus = new DomainCommandBus<IDomainCommand>(new[] { handler.Object });

		bus.Execute(command);

		handler.Verify(x => x.Handle(command), Times.Never());
	}

	[Fact]
	public void WhenAsyncHandlerRegisteredForSpecificType_ThenInvokesAsyncRunner()
	{
		var command = new FooCommand { Id = 5 };
		var handler = new Mock<DomainCommandHandler<FooCommand>> { CallBase = true };
		handler.Setup(x => x.IsAsync).Returns(true);
		var asyncCalled = false;
		Action<Action> asyncRunner = action => asyncCalled = true;

		var bus = new DomainCommandBus<IDomainCommand>(new[] { handler.Object }, asyncRunner);

		bus.Execute(command);

		handler.Verify(x => x.Handle(command), Times.Never());
		Assert.True(asyncCalled);
	}

	[Fact]
	public void WhenAsyncHandlerRegisteredForBaseType_ThenDoesNotInvokeHandlesOnExecute()
	{
		var command = new FooCommand { Id = 5 };
		var handler = new Mock<DomainCommandHandler<BaseCommand>> { CallBase = true };
		handler.Setup(x => x.IsAsync).Returns(true);
		var asyncCalled = false;
		Action<Action> asyncRunner = action => asyncCalled = true;

		var bus = new DomainCommandBus<IDomainCommand>(new[] { handler.Object }, asyncRunner);

		bus.Execute(command);

		handler.Verify(x => x.Handle(command), Times.Never());
		Assert.False(asyncCalled);
	}

	[Fact]
	public void WhenAsyncHandlerRegisteredForSpecificType_ThenCanUseDefaultAsyncRunner()
	{
		var command = new FooCommand { Id = 5 };
		var handler = new Mock<DomainCommandHandler<FooCommand>> { CallBase = true };
		var handlerCalled = false;
		handler.Setup(x => x.IsAsync).Returns(true);
		handler.Setup(x => x.Handle(It.IsAny<FooCommand>())).Callback<FooCommand>(c => handlerCalled = true);

		var bus = new DomainCommandBus<IDomainCommand>(new[] { handler.Object });

		bus.Execute(command);

		while (!handlerCalled)
			System.Threading.Thread.Sleep(10);

		handler.Verify(x => x.Handle(command));
	}

	[Fact]
	public void WhenNoAsyncHandlerRegistereded_ThenInvokesOnThreadPool()
	{
		var command = new FooCommand { Id = 5 };
		var handler = new Mock<DomainCommandHandler<FooCommand>> { CallBase = true };
		handler.Setup(x => x.IsAsync).Returns(true);
		var handlerCalled = false;
		handler.Setup(x => x.Handle(It.IsAny<FooCommand>())).Callback<FooCommand>(c => handlerCalled = true);
		var bus = new DomainCommandBus<IDomainCommand>(DomainCommandStore<IDomainCommand>.None, new[] { handler.Object });

		bus.Execute(command);

		while (!handlerCalled)
			System.Threading.Thread.Sleep(10);

		handler.Verify(x => x.Handle(command));
	}


	[Fact]
	public void WhenDefaultDomainCommandsExecutes_ThenDoesNothing()
	{
		DomainCommandBus<IDomainCommand>.None.Execute(new FooCommand());
	}

	[Fact]
	public void WhenHandlerDoesNotInheritFromGenericHandler_ThenThrows()
	{
		var handler = new NonGenericHandler();

		Assert.Throws<ArgumentException>(() => new DomainCommandBus<IDomainCommand>(new DomainCommandHandler[] { handler }));
	}

	[Fact]
	public void WhenNullHandlerProvided_ThenThrows()
	{
		Assert.Throws<ArgumentException>(() => new DomainCommandBus<IDomainCommand>(new DomainCommandHandler[] { null }));
	}

	[Fact]
	public void WhenPersistingToNullStore_ThenNothingHappens()
	{
		DomainCommandStore<IDomainCommand>.None.Persist(new FooCommand());
	}

	[Fact]
	public void WhenCommittingNullStore_ThenNothingHappens()
	{
		DomainCommandStore<IDomainCommand>.None.Commit();
	}

	private class HandlerBase : DomainCommandHandler<FooCommand>
	{
		public override void Handle(FooCommand @event)
		{
		}

		public override bool IsAsync { get { return false; } }
	}

	private class Handler : HandlerBase
	{
	}

	private class NonGenericHandler : DomainCommandHandler
	{
		public override bool IsAsync { get { return false; } }
	}
}

public interface IDomainCommand
{
}

public class BaseCommand : IDomainCommand
{
}

public class FooCommand : BaseCommand
{
	public int Id { get; set; }
}
