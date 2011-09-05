﻿#region BSD License
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
	public class DomainEventsSpec
	{
		[Fact]
		public void WhenNullHandlers_ThenThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new DomainEventBus<int, DomainEvent>(default(IEnumerable<IDomainEventHandler>)));
		}

		[Fact]
		public void WhenNullEventStore_ThenThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new DomainEventBus<int, DomainEvent>(default(IDomainEventStore<int, DomainEvent>)));
		}

		[Fact]
		public void WhenNullAsyncRunner_ThenThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new DomainEventBus<int, DomainEvent>(Enumerable.Empty<IDomainEventHandler>(), null));
		}

		[Fact]
		public void WhenPublishNullEvent_ThenThrows()
		{
			var bus = new DomainEventBus<int, DomainEvent>(Enumerable.Empty<IDomainEventHandler>());

			Assert.Throws<ArgumentNullException>(() => bus.Publish(new TestAggregate(), default(FooArgs)));
		}

		[Fact]
		public void WhenPublishNullAggregate_ThenThrows()
		{
			var bus = new DomainEventBus<int, DomainEvent>(Enumerable.Empty<IDomainEventHandler>());

			Assert.Throws<ArgumentNullException>(() => bus.Publish((AggregateRoot<int, DomainEvent>)null, new FooArgs()));
		}

		[Fact]
		public void WhenPublishingEvent_ThenSavesToStore()
		{
			var args = new FooArgs { Id = 5 };
			var aggregate = new TestAggregate();
			var store = new Mock<IDomainEventStore<int, DomainEvent>>();

			var bus = new DomainEventBus<int, DomainEvent>(store.Object);

			bus.Publish(aggregate, args);

			store.Verify(x => x.Persist(aggregate, args));
		}

		[Fact]
		public void WhenPublishingEvent_ThenSavesToStoreAndInvokesHandler()
		{
			var args = new FooArgs { Id = 5 };
			var aggregate = new TestAggregate();
			var store = new Mock<IDomainEventStore<int, DomainEvent>>();
			var handler = new Mock<DomainEventHandler<int, BaseArgs>> { CallBase = true };

			var bus = new DomainEventBus<int, DomainEvent>(store.Object, new[] { handler.Object });

			bus.Publish(aggregate, args);

			store.Verify(x => x.Persist(aggregate, args));
			handler.Verify(x => x.Handle(5, It.IsAny<BaseArgs>()));
		}

		[Fact]
		public void WhenPublishingEventWithAsyncHandler_ThenSavesToStoreAndInvokesHandler()
		{
			var args = new FooArgs { Id = 5 };
			var aggregate = new TestAggregate();
			var store = new Mock<IDomainEventStore<int, DomainEvent>>();
			var handler = new Mock<DomainEventHandler<int, BaseArgs>> { CallBase = true };
			handler.Setup(x => x.IsAsync).Returns(true);

			var bus = new DomainEventBus<int, DomainEvent>(store.Object, new[] { handler.Object });

			bus.Publish(aggregate, args);

			store.Verify(x => x.Persist(aggregate, args));
		}

		[Fact]
		public void WhenHandlerRegisteredForSpecificType_ThenHandlesOnRaise()
		{
			var args = new FooArgs { Id = 5 };
			var handler = new Mock<DomainEventHandler<int, FooArgs>> { CallBase = true };

			var bus = new DomainEventBus<int, DomainEvent>(new[] { handler.Object });

			bus.Publish(new TestAggregate(), args);

			handler.Verify(x => x.Handle(5, args));
		}

		[Fact]
		public void WhenHandlerRegisteredForBaseType_ThenHandlesOnRaise()
		{
			var args = new FooArgs { Id = 5 };
			var handler = new Mock<DomainEventHandler<int, BaseArgs>> { CallBase = true };

			var bus = new DomainEventBus<int, DomainEvent>(new[] { handler.Object });

			bus.Publish(new TestAggregate(), args);

			handler.Verify(x => x.Handle(5, It.IsAny<BaseArgs>()));
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForSpecificType_ThenInvokesAsyncRunner()
		{
			var args = new FooArgs { Id = 5 };
			var handler = new Mock<DomainEventHandler<int, FooArgs>> { CallBase = true };
			handler.Setup(x => x.IsAsync).Returns(true);
			var asyncCalled = false;
			Action<Action> asyncRunner = action => asyncCalled = true;

			var bus = new DomainEventBus<int, DomainEvent>(new[] { handler.Object }, asyncRunner);

			bus.Publish(new TestAggregate(), args);

			handler.Verify(x => x.Handle(5, args), Times.Never());
			Assert.True(asyncCalled);
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForBaseType_ThenHandlesOnRaise()
		{
			var args = new FooArgs { Id = 5 };
			var handler = new Mock<DomainEventHandler<int, BaseArgs>> { CallBase = true };
			handler.Setup(x => x.IsAsync).Returns(true);
			var asyncCalled = false;
			Action<Action> asyncRunner = action => asyncCalled = true;

			var bus = new DomainEventBus<int, DomainEvent>(new[] { handler.Object }, asyncRunner);

			bus.Publish(new TestAggregate(), args);

			handler.Verify(x => x.Handle(5, It.IsAny<BaseArgs>()), Times.Never());
			Assert.True(asyncCalled);
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForSpecificType_ThenCanUseDefaultAsynRunner()
		{
			var args = new FooArgs { Id = 5 };
			var handler = new Mock<DomainEventHandler<int, FooArgs>> { CallBase = true };
			handler.Setup(x => x.IsAsync).Returns(true);

			var bus = new DomainEventBus<int, DomainEvent>(new[] { handler.Object });

			bus.Publish(new TestAggregate(), args);

			handler.Verify(x => x.Handle(5, args), Times.Never());
		}

		[Fact]
		public void WhenDefaultDomainEventsRaises_ThenDoesNothing()
		{
			DomainEventBus<int, DomainEvent>.None.Publish(new TestAggregate(), new FooArgs());
		}

		[Fact]
		public void WhenHandlerDoesNotInheritFromGenericHandler_ThenThrows()
		{
			var handler = new NonGenericHandler();

			Assert.Throws<ArgumentException>(() => new DomainEventBus<int, DomainEvent>(new IDomainEventHandler[] { handler }));
		}

		[Fact]
		public void WhenHandlerExposesNullEventType_ThenThrows()
		{
			var handler = new Mock<IDomainEventHandler<int, FooArgs>>();

			Assert.Throws<ArgumentException>(() => new DomainEventBus<int, DomainEvent>(new IDomainEventHandler[] { handler.Object }));
		}

		[Fact]
		public void WhenNullHandlerProvided_ThenThrows()
		{
			Assert.Throws<ArgumentException>(() => new DomainEventBus<int, DomainEvent>(new IDomainEventHandler[] { null }));
		}

		private class HandlerBase : DomainEventHandler<int, FooArgs>
		{
			public override bool IsAsync { get { return false; } }

			public override void Handle(int aggregateId, FooArgs @event)
			{
				throw new NotImplementedException();
			}
		}

		private class Handler : HandlerBase
		{
		}

		private class NonGenericHandler : IDomainEventHandler
		{
			public bool IsAsync { get { return false; } }

			public Type EventType { get { return typeof(FooArgs); } }
		}
	}

	public class BaseArgs : DomainEvent
	{
	}

	public class FooArgs : BaseArgs
	{
		public int Id { get; set; }
	}

	internal class TestAggregate : AggregateRoot<int, DomainEvent>
	{
		public TestAggregate()
		{
			this.Id = 5;
		}
	}
}