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
using System.Threading;
using System.Globalization;

/// <summary>
/// Default implementation of an <see cref="IMessageBus{TBaseMessage}"/> that 
/// invokes handlers as messages are published, and where handlers are 
/// run in-process.
/// </summary>
/// <remarks>
/// This class implements what is known as a Content-Based Publish/Subscribe
/// message bus, where subscribers are matched based on the type of message 
/// (content) they can handle, as specified by the generic type parameter to 
/// <see cref="IMessageHandler{TBaseMessage}"/>.
/// <para>
/// Handlers with <see cref="IMessageHandler.IsAsync"/> set to 
/// <see langword="true"/> are invoked through the optional 
/// async runner delegate passed to the constructor.
/// </para>
/// </remarks>
/// <typeparam name="TBaseMessage">The common base type or interface implemented by message payloads.</typeparam>
/// <nuget id="netfx-Patterns.MessageBus" />
partial class MessageBus<TBaseMessage> : IMessageBus<TBaseMessage>
{
	private Action<Action> asyncActionRunner;
	private List<HandlerDescriptor> messageHandlers;

	/// <summary>
	/// Initializes a new instance of the <see cref="MessageBus{TBaseMessage}"/> class with 
	/// the given set of message handlers, and uses the default async runner that 
	/// enqueues work in the <see cref="ThreadPool"/>.
	/// </summary>
	/// <param name="messageHandlers">The message handlers.</param>
	public MessageBus(IEnumerable<IMessageHandler> messageHandlers)
		: this(messageHandlers, action => ThreadPool.QueueUserWorkItem(state => action()))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MessageBus{TBaseMessage}"/> class with 
	/// the given set of message handlers and a specific async runner.
	/// </summary>
	/// <param name="messageHandlers">The message handlers.</param>
	/// <param name="asyncActionRunner">The async action runner to use to invoke log handlers 
	/// that have <see cref="IMessageHandler.IsAsync"/> set to <see langword="true"/>.</param>
	public MessageBus(IEnumerable<IMessageHandler> messageHandlers, Action<Action> asyncActionRunner)
	{
		Guard.NotNull(() => messageHandlers, messageHandlers);
		Guard.NotNull(() => asyncActionRunner, asyncActionRunner);

		if (messageHandlers.Any(eh => eh == null))
			throw new ArgumentException("Invalid null handler found.", "messageHandlers");

		var genericHandler = typeof(IMessageHandler<>);

		this.messageHandlers = messageHandlers.Select(handler =>
			new HandlerDescriptor
			{
				Handler = handler, 
				// Grab the type of body from the generic 
				// type argument, if any.
				MessageType = handler.GetType()
					.GetInterfaces()
					.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericHandler)
					.Select(x => x.GetGenericArguments()[0])
					.FirstOrDefault()
			})
			.ToList();

		var invalidHandlers = this.messageHandlers.Where(x => x.MessageType == null).ToList();
		if (invalidHandlers.Any())
			throw new ArgumentException(string.Format(
				CultureInfo.CurrentCulture,
				"The following message handlers to not implement the generic interface IMessageHandler<TBody>: {0}.", 
				 string.Join(", ", invalidHandlers.Select(handler => handler.GetType().FullName))),
				 "messageHandlers");

		this.asyncActionRunner = asyncActionRunner;
	}

	/// <summary>
	/// Publishes the specified message to the bus so that all 
	/// relevant subscribers are notified.
	/// </summary>
	/// <param name="message">The message to publish.</param>
	/// <param name="headers">The headers associated with the message.</param>
	public virtual void Publish(TBaseMessage message, IDictionary<string, object> headers)
	{
		Guard.NotNull(() => message, message);
		Guard.NotNull(() => headers, headers);

		var compatibleHandlers = this.messageHandlers.Where(h => h.MessageType.IsAssignableFrom(message.GetType())).ToList();
		dynamic dynamicMessage = message;

		// By making both handler and message dynamic, we allow message handlers to 
		// subscribe to base classes
		foreach (dynamic handler in compatibleHandlers.Where(h => !h.Handler.IsAsync).Select(x => x.Handler).AsParallel())
		{
			handler.Handle(dynamicMessage, headers);
		}

		// Run background handlers through the async runner.
		foreach (dynamic handler in compatibleHandlers.Where(h => h.Handler.IsAsync).Select(x => x.Handler).AsParallel())
		{
			asyncActionRunner(() => handler.Handle(dynamicMessage, headers));
		}
	}

	private class HandlerDescriptor
	{
		/// <summary>
		/// Gets or sets the type of message payload the handler can process, 
		/// retrieved from the handler TMessage generic parameter.
		/// </summary>
		public Type MessageType { get; set; }

		/// <summary>
		/// Gets or sets the handler.
		/// </summary>
		public IMessageHandler Handler { get; set; }
	}
}
