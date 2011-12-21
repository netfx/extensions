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
using System.Threading;

namespace NetFx.Patterns.EventSourcing.Tests
{
	public class EventsSpec
	{
		[Fact]
		public void WhenNullHandlers_ThenThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new EventBus<int, DomainEvent>(Mock.Of<IEventStore<int, DomainEvent>>(), default(IEnumerable<IEventHandler>)));
		}

		[Fact]
		public void WhenNullEventStore_ThenThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new EventBus<int, DomainEvent>(default(IEventStore<int, DomainEvent>), Enumerable.Empty<IEventHandler>()));
		}

		[Fact]
		public void WhenNullAsyncRunner_ThenThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new EventBus<int, DomainEvent>(Mock.Of<IEventStore<int, DomainEvent>>(), Enumerable.Empty<IEventHandler>(), null));
		}

		[Fact]
		public void WhenPublishNullAggregate_ThenThrows()
		{
			var bus = new EventBus<int, DomainEvent>(Mock.Of<IEventStore<int, DomainEvent>>(), Enumerable.Empty<IEventHandler>());

			Assert.Throws<ArgumentNullException>(() => bus.PublishChanges((AggregateRoot<int, DomainEvent>)null));
		}

		[Fact]
		public void WhenPublishingAggregateWithChangedEvent_ThenSavesToStore()
		{
			var aggregate = new TestAggregate();
			var store = new Mock<IEventStore<int, DomainEvent>>();

			var bus = new EventBus<int, DomainEvent>(store.Object, Enumerable.Empty<IEventHandler>());

			aggregate.Foo();
			bus.PublishChanges(aggregate);

			store.Verify(x => x.SaveChanges(aggregate));
		}

		[Fact]
		public void WhenPublishingAggregateWithChangedEvent_ThenAcceptsChanges()
		{
			var aggregate = new TestAggregate();
			var store = new Mock<IEventStore<int, DomainEvent>>();

			var bus = new EventBus<int, DomainEvent>(store.Object, Enumerable.Empty<IEventHandler>());

			bus.PublishChanges(aggregate);

			Assert.False(aggregate.GetChanges().Any());
		}

		[Fact]
		public void WhenPublishingEvent_ThenInvokesHandler()
		{
			var aggregate = new TestAggregate();
			aggregate.Foo();
			var store = new Mock<IEventStore<int, DomainEvent>>();
			var handler = new Mock<EventHandler<int, BaseEvent>> { CallBase = true };

			var bus = new EventBus<int, DomainEvent>(store.Object, new[] { handler.Object });

			bus.PublishChanges(aggregate);

			handler.Verify(x => x.Handle(5, It.IsAny<BaseEvent>()));
		}

		[Fact]
		public void WhenHandlerRegisteredForSpecificType_ThenHandlesOnRaise()
		{
			var handler = new Mock<EventHandler<int, FooEvent>> { CallBase = true };
			var bus = new EventBus<int, DomainEvent>(Mock.Of<IEventStore<int, DomainEvent>>(), new[] { handler.Object });
			var aggregate = new TestAggregate();
			aggregate.Foo();

			bus.PublishChanges(aggregate);

			handler.Verify(x => x.Handle(5, It.IsAny<FooEvent>()));
		}

		[Fact]
		public void WhenHandlerRegisteredForBaseType_ThenHandlesOnRaise()
		{
			var handler = new Mock<EventHandler<int, BaseEvent>> { CallBase = true };
			var bus = new EventBus<int, DomainEvent>(Mock.Of<IEventStore<int, DomainEvent>>(), new[] { handler.Object });
			var aggregate = new TestAggregate();
			aggregate.Foo();

			bus.PublishChanges(aggregate);

			handler.Verify(x => x.Handle(5, It.IsAny<BaseEvent>()));
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForSpecificType_ThenInvokesAsyncRunner()
		{
			var handler = new Mock<EventHandler<int, FooEvent>> { CallBase = true };
			handler.Setup(x => x.IsAsync).Returns(true);
			var asyncCalled = false;
			Action<Action> asyncRunner = action => asyncCalled = true;
			var aggregate = new TestAggregate();
			aggregate.Foo();

			var bus = new EventBus<int, DomainEvent>(Mock.Of<IEventStore<int, DomainEvent>>(), new[] { handler.Object }, asyncRunner);

			bus.PublishChanges(aggregate);

			handler.Verify(x => x.Handle(5, It.IsAny<FooEvent>()), Times.Never());
			Assert.True(asyncCalled);
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForBaseType_ThenHandlesOnRaise()
		{
			var handler = new Mock<EventHandler<int, BaseEvent>> { CallBase = true };
			handler.Setup(x => x.IsAsync).Returns(true);
			var asyncCalled = false;
			Action<Action> asyncRunner = action => asyncCalled = true;
			var aggregate = new TestAggregate();
			aggregate.Foo();

			var bus = new EventBus<int, DomainEvent>(Mock.Of<IEventStore<int, DomainEvent>>(), new[] { handler.Object }, asyncRunner);

			bus.PublishChanges(aggregate);

			handler.Verify(x => x.Handle(5, It.IsAny<BaseEvent>()), Times.Never());
			Assert.True(asyncCalled);
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForSpecificType_ThenCanUseDefaultAsynRunner()
		{
			var handler = new Mock<EventHandler<int, FooEvent>> { CallBase = true };
			var asyncCalled = false;
			handler.Setup(x => x.IsAsync).Returns(true);
			handler.Setup(x => x.Handle(It.IsAny<int>(), It.IsAny<FooEvent>()))
				.Callback(() => asyncCalled = true);
			var aggregate = new TestAggregate();
			aggregate.Foo();

			var bus = new EventBus<int, DomainEvent>(Mock.Of<IEventStore<int, DomainEvent>>(), new[] { handler.Object });

			bus.PublishChanges(aggregate);

			while (!asyncCalled)
			{
				Thread.Sleep(1);
			}

			handler.Verify(x => x.Handle(5, It.IsAny<FooEvent>()));
		}

		[Fact]
		public void WhenHandlerDoesNotInheritFromGenericHandler_ThenThrows()
		{
			var handler = new NonGenericHandler();

			Assert.Throws<ArgumentException>(() => new EventBus<int, DomainEvent>(Mock.Of<IEventStore<int, DomainEvent>>(), new IEventHandler[] { handler }));
		}

		[Fact]
		public void WhenNullHandlerProvided_ThenThrows()
		{
			Assert.Throws<ArgumentException>(() => new EventBus<int, DomainEvent>(Mock.Of<IEventStore<int, DomainEvent>>(), new IEventHandler[] { null }));
		}

		private class HandlerBase : EventHandler<int, FooEvent>
		{
			public override bool IsAsync { get { return false; } }

			public override void Handle(int aggregateId, FooEvent @event)
			{
				throw new NotImplementedException();
			}
		}

		private class Handler : HandlerBase
		{
		}

		private class NonGenericHandler : IEventHandler
		{
			public bool IsAsync { get { return false; } }

			public Type EventType { get { return typeof(FooEvent); } }
		}
	}

	internal class BaseEvent : DomainEvent
	{
	}

	internal class FooEvent : BaseEvent
	{
		public int Id { get; set; }
	}

	internal class TestAggregate : AggregateRoot<int, DomainEvent>
	{
		public TestAggregate()
		{
			this.Id = 5;
		}

		public void Foo()
		{
			base.Raise(new FooEvent { Id = this.Id });
		}

		public void Base()
		{
			base.Raise(new BaseEvent());
		}
	}
}