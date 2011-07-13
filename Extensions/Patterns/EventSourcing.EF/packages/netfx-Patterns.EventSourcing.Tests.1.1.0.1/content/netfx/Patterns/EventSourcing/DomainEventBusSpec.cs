#region BSD License
/* 
Copyright (c) 2010, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;

namespace NetFx.Patterns.EventSourcing.Tests
{
	/// <nuget id="netfx-Patterns.EventSourcing.Tests" />
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

			Assert.Throws<ArgumentNullException>(() => bus.Publish(new TestAggregate(), null));
		}

		[Fact]
		public void WhenPublishNullAggregate_ThenThrows()
		{
			var bus = new DomainEventBus(Enumerable.Empty<DomainEventHandler>());

			Assert.Throws<ArgumentNullException>(() => bus.Publish((AggregateRoot<int>)null, new FooArgs()));
		}

		[Fact]
		public void WhenHandlerRegisteredForSpecificType_ThenHandlesOnRaise()
		{
			var args = new FooArgs { Id = 5 };
			var handler = new Mock<DomainEventHandler<FooArgs>> { CallBase = true };

			var bus = new DomainEventBus(new[] { handler.Object });

			bus.Publish(new TestAggregate(), args);

			handler.Verify(x => x.Handle(args));
		}

		[Fact]
		public void WhenHandlerRegisteredForBaseType_ThenHandlesOnRaise()
		{
			var args = new FooArgs { Id = 5 };
			var handler = new Mock<DomainEventHandler<BaseArgs>> { CallBase = true };

			var bus = new DomainEventBus(new[] { handler.Object });

			bus.Publish(new TestAggregate(), args);

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

			bus.Publish(new TestAggregate(), args);

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

			bus.Publish(new TestAggregate(), args);

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

			bus.Publish(new TestAggregate(), args);

			handler.Verify(x => x.Handle(args), Times.Never());
		}

		[Fact]
		public void WhenDefaultDomainEventsRaises_ThenDoesNothing()
		{
			DomainEventBus.None.Publish(new TestAggregate(), new FooArgs());
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

	/// <nuget id="netfx-Patterns.EventSourcing.Tests" />
	internal class BaseArgs : DomainEvent
	{
	}

	/// <nuget id="netfx-Patterns.EventSourcing.Tests" />
	internal class FooArgs : BaseArgs
	{
		public int Id { get; set; }
	}

	/// <nuget id="netfx-Patterns.EventSourcing.Tests" />
	internal class TestAggregate : AggregateRoot<int>
	{
	}
}