#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;

namespace NetFx.Patterns.MessageBus.Tests
{
	public class MessageBusSpec
	{
		[Fact]
		public void WhenNullHandlers_ThenThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new MessageBus<Message>(default(IEnumerable<IMessageHandler>)));
		}

		[Fact]
		public void WhenNullAsyncRunner_ThenThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new MessageBus<Message>(Enumerable.Empty<IMessageHandler>(), null));
		}

		[Fact]
		public void WhenPublishNullMessage_ThenThrows()
		{
			var bus = new MessageBus<Message>(Enumerable.Empty<IMessageHandler>());

			Assert.Throws<ArgumentNullException>(() => bus.Publish(default(FooMessage)));
		}

		[Fact]
		public void WhenHandlerRegisteredForBaseType_ThenInvokesHandler()
		{
			var message = new FooMessage { Id = 5 };
			var handler = new Mock<IMessageHandler<BaseMessage>>();

			var bus = new MessageBus<Message>(new[] { handler.Object });

			bus.Publish(message);

			handler.Verify(x => x.Handle(It.IsAny<BaseMessage>(), It.IsAny<IDictionary<string, object>>()));
		}

		[Fact]
		public void WhenHandlerRegisteredForSpecificType_ThenInvokesHandler()
		{
			var message = new FooMessage { Id = 5 };
			var handler = new Mock<IMessageHandler<FooMessage>>();

			var bus = new MessageBus<Message>(new[] { handler.Object });

			bus.Publish(message);

			handler.Verify(x => x.Handle(It.IsAny<FooMessage>(), It.IsAny<IDictionary<string, object>>()));
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForSpecificType_ThenInvokesAsyncRunner()
		{
			var message = new FooMessage { Id = 5 };
			var handler = new Mock<IMessageHandler<FooMessage>>();
			handler.Setup(x => x.IsAsync).Returns(true);
			var asyncCalled = false;
			Action<Action> asyncRunner = action => asyncCalled = true;

			var bus = new MessageBus<Message>(new[] { handler.Object }, asyncRunner);

			bus.Publish(message);

			handler.Verify(x => x.Handle(It.IsAny<FooMessage>(), It.IsAny<IDictionary<string, object>>()), Times.Never());
			Assert.True(asyncCalled);
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForBaseType_ThenInvokesAsyncRunner()
		{
			var message = new FooMessage { Id = 5 };
			var handler = new Mock<IMessageHandler<BaseMessage>>();
			handler.Setup(x => x.IsAsync).Returns(true);
			var asyncCalled = false;
			Action<Action> asyncRunner = action => asyncCalled = true;

			var bus = new MessageBus<Message>(new[] { handler.Object }, asyncRunner);

			bus.Publish(message);

			handler.Verify(x => x.Handle(It.IsAny<FooMessage>(), It.IsAny<IDictionary<string, object>>()), Times.Never());
			Assert.True(asyncCalled);
		}

		[Fact]
		public void WhenAsyncHandlerRegisteredForSpecificType_ThenCanUseDefaultAsynRunner()
		{
			var message = new FooMessage { Id = 5 };
			var handler = new Mock<IMessageHandler<FooMessage>>();
			handler.Setup(x => x.IsAsync).Returns(true);

			var bus = new MessageBus<Message>(new[] { handler.Object });

			bus.Publish(message);

			handler.Verify(x => x.Handle(It.IsAny<FooMessage>(), It.IsAny<IDictionary<string, object>>()), Times.Never());
		}

		[Fact]
		public void WhenHandlerDoesNotInheritFromGenericHandler_ThenThrows()
		{
			var handler = new NonGenericHandler();

			Assert.Throws<ArgumentException>(() => new MessageBus<Message>(new IMessageHandler[] { handler }));
		}

		[Fact]
		public void WhenNullHandlerProvided_ThenThrows()
		{
			Assert.Throws<ArgumentException>(() => new MessageBus<Message>(new IMessageHandler[] { null }));
		}

		[Fact]
		public void WhenPublishMessageWithoutHeaders_ThenCreatesNewHeaders()
		{
			var message = new FooMessage { Id = 5 };
			var handler = new Mock<IMessageHandler<BaseMessage>>();

			var bus = new MessageBus<Message>(new[] { handler.Object });

			bus.Publish(message);

			handler.Verify(x => x.Handle(message, It.Is<IDictionary<string, object>>(d => d != null && d.Count == 0)));
		}

		[Fact]
		public void WhenPublishMessagesWithoutHeaders_ThenCreatesNewHeaders()
		{
			var handler = new Mock<IMessageHandler<BaseMessage>>();

			var bus = new MessageBus<Message>(new[] { handler.Object });

			bus.Publish(new Message[] { new FooMessage(), new BaseMessage() });

			handler.Verify(x => x.Handle(
				It.IsAny<BaseMessage>(), 
				It.Is<IDictionary<string, object>>(d => d != null && d.Count == 0)), 
				Times.Exactly(2));
		}

		[Fact]
		public void WhenPublishMessagesWithHeaders_ThenPublishesWithSameHeadersForAll()
		{
			var handler = new Mock<IMessageHandler<BaseMessage>>();
			var headers = new Dictionary<string, object>();
			var bus = new MessageBus<Message>(new[] { handler.Object });

			bus.Publish(new Message[] { new FooMessage(), new BaseMessage() }, headers);

			handler.Verify(x => x.Handle(
				It.IsAny<BaseMessage>(),
				It.Is<IDictionary<string, object>>(d => Object.ReferenceEquals(headers, d))),
				Times.Exactly(2));
		}

		private class HandlerBase : MessageHandler<FooMessage>
		{
			public override bool IsAsync { get { return false; } }

			public override void Handle(FooMessage message, IDictionary<string, object> headers)
			{
				throw new NotImplementedException();
			}
		}

		private class Handler : HandlerBase
		{
		}

		private class NonGenericHandler : IMessageHandler
		{
			public bool IsAsync { get { return false; } }
		}
	}

	public class Message
	{
	}

	public class BaseMessage : Message
	{
	}

	public class FooMessage : BaseMessage
	{
		public int Id { get; set; }
	}
}