using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;

namespace NetFx.Patterns.DomainCommands
{
	public class DomainCommandsSpec
	{
		[Fact]
		public void WhenNoStoreSpecifiedAndMatchingHandlerRegistered_ThenExecuteSucceeds()
		{
			var command = new FooCommand { Id = 5 };
			var handler = Mock.Of<ICommandHandler<FooCommand>>(x => x.IsAsync == true);

			var bus = new CommandRegistry<IDomainCommand>(new[] { handler });

			bus.Execute(command);
		}

		[Fact]
		public void WhenHandlerRegisteredForSpecificType_ThenHandlesOnExecute()
		{
			var command = new FooCommand { Id = 5 };
			var handler = new Mock<CommandHandler<FooCommand>> { CallBase = true };

			var bus = new CommandRegistry<IDomainCommand>(new[] { handler.Object });

			bus.Execute(command);

			handler.Verify(x => x.Handle(command, It.IsAny<IDictionary<string, object>>()));
		}

		[Fact]
		public void WhenHandlerRegisteredForBaseType_ThenThrowsForExecuteWithDerivedClass()
		{
			var command = new FooCommand { Id = 5 };
			var handler = Mock.Of<ICommandHandler<BaseCommand>>();

			var bus = new CommandRegistry<IDomainCommand>(new[] { handler });

			Assert.Throws<InvalidOperationException>(() => bus.Execute(command));
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForBaseType_ThenThrowsForExecuteWithDerivedClass()
		{
			var command = new FooCommand { Id = 5 };
			var handler = Mock.Of<ICommandHandler<BaseCommand>>();

			var bus = new CommandRegistry<IDomainCommand>(new[] { handler }, action => action());

			Assert.Throws<InvalidOperationException>(() => bus.Execute(command));
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForSpecificType_ThenInvokesAsyncRunner()
		{
			var command = new FooCommand { Id = 5 };
			var handler = new Mock<CommandHandler<FooCommand>> { CallBase = true };
			handler.Setup(x => x.IsAsync).Returns(true);
			var asyncCalled = false;
			Action<Action> asyncRunner = action => asyncCalled = true;

			var bus = new CommandRegistry<IDomainCommand>(new[] { handler.Object }, asyncRunner);

			bus.Execute(command);

			handler.Verify(x => x.Handle(command, It.IsAny<IDictionary<string, object>>()), Times.Never());
			Assert.True(asyncCalled);
		}

		[Fact]
		public void WhenHandlerRegisteredForSpecificType_ThenPersistsToStore()
		{
			var command = new FooCommand { Id = 5 };
			var handler = Mock.Of<ICommandHandler<FooCommand>>();
			var store = new Mock<IMessageStore<IDomainCommand>>();
			var bus = new CommandRegistry<IDomainCommand>(store.Object, new[] { handler });

			bus.Execute(command);

			store.Verify(x => x.Save(command, It.IsAny<IDictionary<string, object>>()));
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForSpecificType_ThenCanUseDefaultAsyncRunner()
		{
			var command = new FooCommand { Id = 5 };
			var handlerCalled = false;
			var handler = new Mock<ICommandHandler<FooCommand>>();
			handler.Setup(x => x.IsAsync).Returns(true);
			handler.Setup(x => x.Handle(It.IsAny<FooCommand>(), It.IsAny<IDictionary<string, object>>()))
				.Callback<FooCommand, IDictionary<string, object>>((c, h) => handlerCalled = true);

			var bus = new CommandRegistry<IDomainCommand>(new[] { handler.Object });

			bus.Execute(command);

			while (!handlerCalled)
				System.Threading.Thread.Sleep(10);

			handler.Verify(x => x.Handle(command, It.IsAny<IDictionary<string, object>>()));
		}

		[Fact]
		public void WhenNoAsyncHandlerRegistereded_ThenInvokesOnThreadPool()
		{
			var command = new FooCommand { Id = 5 };
			var handler = new Mock<CommandHandler<FooCommand>> { CallBase = true };
			handler.Setup(x => x.IsAsync).Returns(true);
			var handlerCalled = false;
			handler.Setup(x => x.Handle(It.IsAny<FooCommand>(), It.IsAny<IDictionary<string, object>>()))
				.Callback<FooCommand, IDictionary<string, object>>((c, h) => handlerCalled = true);
			var bus = new CommandRegistry<IDomainCommand>(new[] { handler.Object });

			bus.Execute(command);

			while (!handlerCalled)
				System.Threading.Thread.Sleep(10);

			handler.Verify(x => x.Handle(command, It.IsAny<IDictionary<string, object>>()));
		}

		[Fact]
		public void WhenNoAsyncHandlerRegistereded_ThenInvokesOnThreadPoolAndPersistsToStore()
		{
			var command = new FooCommand { Id = 5 };
			var handler = new Mock<ICommandHandler<FooCommand>>();
			handler.Setup(x => x.IsAsync).Returns(true);
			var handlerCalled = false;
			handler.Setup(x => x.Handle(It.IsAny<FooCommand>(), It.IsAny<IDictionary<string, object>>()))
				.Callback<FooCommand, IDictionary<string, object>>((c, h) => handlerCalled = true);

			var store = new Mock<IMessageStore<IDomainCommand>>();

			var bus = new CommandRegistry<IDomainCommand>(store.Object, new[] { handler.Object });

			bus.Execute(command);

			while (!handlerCalled)
				System.Threading.Thread.Sleep(10);

			handler.Verify(x => x.Handle(command, It.IsAny<IDictionary<string, object>>()));
			store.Verify(x => x.Save(command, It.IsAny<IDictionary<string, object>>()));
		}

		[Fact]
		public void WhenHandlerDoesNotInheritFromGenericHandler_ThenThrows()
		{
			var handler = new NonGenericHandler();

			Assert.Throws<ArgumentException>(() => new CommandRegistry<IDomainCommand>(new ICommandHandler[] { handler }));
		}

		[Fact]
		public void WhenDuplicateHandlerIsRegisteredForSameCommandType_ThenThrows()
		{
			Assert.Throws<ArgumentException>(() =>
				new CommandRegistry<IDomainCommand>(new ICommandHandler[] { new FooCommandHandler(), new FooCommandHandler() }));
		}

		[Fact]
		public void WhenNullHandlerProvided_ThenThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() =>
				new CommandRegistry<IDomainCommand>(new ICommandHandler[] { null }));
		}

		[Fact]
		public void WhenExecuteExtensionOnNullBus_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				CommandRegistryExtensions.Execute(
					default(ICommandRegistry<BaseCommand>),
					new FooCommand()));
		}

		[Fact]
		public void WhenExecuteExtensionOnNullBusWithEmptyCommands_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				CommandRegistryExtensions.Execute(
					default(ICommandRegistry<BaseCommand>),
					Enumerable.Empty<BaseCommand>()));
		}

		[Fact]
		public void WhenExecuteExtensionWithNullCommands_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				CommandRegistryExtensions.Execute(
					Mock.Of<ICommandRegistry<BaseCommand>>(),
					default(IEnumerable<BaseCommand>)));
		}

		[Fact]
		public void WhenExecuteExtensionWithNullHeadersAndCommands_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				CommandRegistryExtensions.Execute(
					Mock.Of<ICommandRegistry<BaseCommand>>(),
					Enumerable.Empty<BaseCommand>(),
					default(IDictionary<string, object>)));
		}

		[Fact]
		public void WhenExecuteExtensionWithHeadersAndCommands_ThenCopiesDictionary()
		{
			var headers = new Dictionary<string, object>();
			headers.Add("IP", "localhost");

			var bus = new Mock<ICommandRegistry<BaseCommand>>();

			CommandRegistryExtensions.Execute(
					bus.Object,
					new BaseCommand[] { new FooCommand() },
					headers);

			bus.Verify(x => x.Execute(It.IsAny<BaseCommand>(), It.Is<IDictionary<string, object>>(dict => Object.ReferenceEquals(dict, headers))), Times.Never());
			bus.Verify(x => x.Execute(It.IsAny<BaseCommand>(), It.Is<IDictionary<string, object>>(dict =>
				dict.ContainsKey("IP") && ((string)dict["IP"]) == "localhost")));
		}

		[Fact]
		public void WhenExecuteExtensionWithCommands_ThenCreatesNewHeadersForEach()
		{
			var headers = new HashSet<IDictionary<string, object>>();
			var bus = new Mock<ICommandRegistry<BaseCommand>>();

			bus.Setup(x => x.Execute(It.IsAny<BaseCommand>(), It.IsAny<IDictionary<string, object>>()))
				.Callback((BaseCommand cmd, IDictionary<string, object> h) => headers.Add(h));

			CommandRegistryExtensions.Execute(
					bus.Object,
					new BaseCommand[] { new FooCommand(), new FooCommand() });

			Assert.Equal(2, headers.Count);
			var dupe = new Dictionary<string, object>();
			headers.Add(dupe);
			headers.Add(dupe);

			Assert.Equal(3, headers.Count);
		}

		[Fact]
		public void WhenExecutedCommandThrows_ThenSavesExceptionAsHeader()
		{
			var headers = new Dictionary<string, object>();
			headers.Add("IP", "localhost");

			var handler = new Mock<ICommandHandler<FooCommand>>();
			var store = new Mock<IMessageStore<IDomainCommand>>();
			var registry = new CommandRegistry<IDomainCommand>(store.Object, new[] { handler.Object });

			handler.Setup(x => x.Handle(It.IsAny<FooCommand>(), headers))
				.Throws(new ArgumentException("foo"));

			registry.Execute(new FooCommand(), headers);

			Assert.True(headers.ContainsKey("Exception"));
		}

		private class FooCommandHandler : CommandHandler<FooCommand>
		{
			public override void Handle(FooCommand command, IDictionary<string, object> headers)
			{
			}

			public override bool IsAsync { get { return false; } }
		}

		private class Handler : FooCommandHandler
		{
		}

		private class NonGenericHandler : ICommandHandler
		{
			public bool IsAsync { get { return false; } }
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
}