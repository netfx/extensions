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

namespace NetFx.Patterns.EventStore.Tests
{
	/// <nuget id="netfx-Patterns.EventStore.Tests.xUnit" />
	public class EventBusSpec
	{
		[Fact]
		public void WhenNullHandlers_ThenThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new EventBus<SystemEvent>(default(IEnumerable<ISystemEventHandler>)));
		}

		[Fact]
		public void WhenNullEventStore_ThenThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new EventBus<SystemEvent>(default(IEventStore<SystemEvent>)));
		}

		[Fact]
		public void WhenNullAsyncRunner_ThenThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new EventBus<SystemEvent>(Enumerable.Empty<ISystemEventHandler>(), null));
		}

		[Fact]
		public void WhenPublishNullEvent_ThenThrows()
		{
			var bus = new EventBus<SystemEvent>(Enumerable.Empty<ISystemEventHandler>());

			Assert.Throws<ArgumentNullException>(() => bus.Publish(default(FooArgs)));
		}

		[Fact]
		public void WhenPublishingEvent_ThenSavesToStore()
		{
			var args = new FooArgs { Id = 5 };
			var store = new Mock<IEventStore<SystemEvent>>();

			var bus = new EventBus<SystemEvent>(store.Object);

			bus.Publish(args);

			store.Verify(x => x.Persist(args));
		}

		[Fact]
		public void WhenPublishingEvent_ThenSavesToStoreAndInvokesHandler()
		{
			var args = new FooArgs { Id = 5 };
			var store = new Mock<IEventStore<SystemEvent>>();
			var handler = new Mock<SystemEventHandler<BaseArgs>> { CallBase = true };

			var bus = new EventBus<SystemEvent>(store.Object, new[] { handler.Object });

			bus.Publish(args);

			store.Verify(x => x.Persist(args));
			handler.Verify(x => x.Handle(It.IsAny<BaseArgs>()));
		}

		[Fact]
		public void WhenPublishingEventWithAsyncHandler_ThenSavesToStoreAndInvokesHandler()
		{
			var args = new FooArgs { Id = 5 };
			var store = new Mock<IEventStore<SystemEvent>>();
			var handler = new Mock<SystemEventHandler<BaseArgs>> { CallBase = true };
			handler.Setup(x => x.IsAsync).Returns(true);

			var bus = new EventBus<SystemEvent>(store.Object, new[] { handler.Object });

			bus.Publish(args);

			store.Verify(x => x.Persist(args));
		}

		[Fact]
		public void WhenHandlerRegisteredForSpecificType_ThenHandlesOnRaise()
		{
			var args = new FooArgs { Id = 5 };
			var handler = new Mock<SystemEventHandler<FooArgs>> { CallBase = true };

			var bus = new EventBus<SystemEvent>(new[] { handler.Object });

			bus.Publish(args);

			handler.Verify(x => x.Handle(args));
		}

		[Fact]
		public void WhenHandlerRegisteredForBaseType_ThenHandlesOnRaise()
		{
			var args = new FooArgs { Id = 5 };
			var handler = new Mock<SystemEventHandler<BaseArgs>> { CallBase = true };

			var bus = new EventBus<SystemEvent>(new[] { handler.Object });

			bus.Publish(args);

			handler.Verify(x => x.Handle(It.IsAny<BaseArgs>()));
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForSpecificType_ThenInvokesAsyncRunner()
		{
			var args = new FooArgs { Id = 5 };
			var handler = new Mock<SystemEventHandler<FooArgs>> { CallBase = true };
			handler.Setup(x => x.IsAsync).Returns(true);
			var asyncCalled = false;
			Action<Action> asyncRunner = action => asyncCalled = true;

			var bus = new EventBus<SystemEvent>(new[] { handler.Object }, asyncRunner);

			bus.Publish(args);

			handler.Verify(x => x.Handle(args), Times.Never());
			Assert.True(asyncCalled);
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForBaseType_ThenHandlesOnRaise()
		{
			var args = new FooArgs { Id = 5 };
			var handler = new Mock<SystemEventHandler<BaseArgs>> { CallBase = true };
			handler.Setup(x => x.IsAsync).Returns(true);
			var asyncCalled = false;
			Action<Action> asyncRunner = action => asyncCalled = true;

			var bus = new EventBus<SystemEvent>(new[] { handler.Object }, asyncRunner);

			bus.Publish(args);

			handler.Verify(x => x.Handle(It.IsAny<BaseArgs>()), Times.Never());
			Assert.True(asyncCalled);
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForSpecificType_ThenCanUseDefaultAsynRunner()
		{
			var args = new FooArgs { Id = 5 };
			var handler = new Mock<SystemEventHandler<FooArgs>> { CallBase = true };
			handler.Setup(x => x.IsAsync).Returns(true);

			var bus = new EventBus<SystemEvent>(new[] { handler.Object });

			bus.Publish(args);

			handler.Verify(x => x.Handle(args), Times.Never());
		}

		[Fact]
		public void WhenDefaultEventsRaises_ThenDoesNothing()
		{
			EventBus<SystemEvent>.None.Publish(new FooArgs());
		}

		[Fact]
		public void WhenHandlerDoesNotImplementGenericHandlerInterface_ThenThrows()
		{
			var handler = new NonGenericHandler();

			Assert.Throws<ArgumentException>(() => new EventBus<SystemEvent>(new ISystemEventHandler[] { handler }));
		}

		[Fact]
		public void WhenHandlerExposesNullEventType_ThenThrows()
		{
			var handler = new Mock<ISystemEventHandler<FooArgs>>();

			Assert.Throws<ArgumentException>(() => new EventBus<SystemEvent>(new ISystemEventHandler[] { handler.Object }));
		}

		[Fact]
		public void WhenNullHandlerProvided_ThenThrows()
		{
			Assert.Throws<ArgumentException>(() => new EventBus<SystemEvent>(new ISystemEventHandler[] { null }));
		}

		private class HandlerBase : SystemEventHandler<FooArgs>
		{
			public override void Handle(FooArgs @event)
			{
				throw new NotImplementedException();
			}
		}

		private class Handler : HandlerBase
		{
		}

		private class NonGenericHandler : ISystemEventHandler
		{
			public bool IsAsync { get; private set; }

			public Type EventType { get { return typeof(BaseArgs); } }
		}
	}

	/// <nuget id="netfx-Patterns.EventSourcing.Tests.xUnit" />
	public class BaseArgs : SystemEvent
	{
	}

	/// <nuget id="netfx-Patterns.EventSourcing.Tests.xUnit" />
	public class FooArgs : BaseArgs
	{
		public int Id { get; set; }
	}
}