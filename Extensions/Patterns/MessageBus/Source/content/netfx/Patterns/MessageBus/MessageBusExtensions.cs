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

/// <summary>
/// Adds usability overloads to <see cref="IMessageBus{TBaseMessage}"/>.
/// </summary>
static partial class MessageBusExtensions
{
	/// <summary>
	/// Publishes the specified message to the bus so that all
	/// relevant subscribers are notified.
	/// </summary>
	/// <typeparam name="TBaseMessage">The common base type or interface implemented by message payloads.</typeparam>
	/// <param name="bus">The bus to publish messages to.</param>
	/// <param name="message">The message to publish.</param>
	public static void Publish<TBaseMessage>(this IMessageBus<TBaseMessage> bus, TBaseMessage message)
	{
		Guard.NotNull(() => bus, bus);
		
		bus.Publish(message, new Dictionary<string, object>());
	}

	/// <summary>
	/// Publishes the specified messages to the bus so that all 
	/// relevant subscribers are notified.
	/// </summary>
	/// <typeparam name="TBaseMessage">The common base type or interface implemented by message payloads.</typeparam>
	/// <param name="bus">The bus to publish messages to.</param>
	/// <param name="messages">The messages to publish.</param>
	public static void Publish<TBaseMessage>(this IMessageBus<TBaseMessage> bus, IEnumerable<TBaseMessage> messages)
	{
		Guard.NotNull(() => bus, bus);
		Guard.NotNull(() => messages, messages);

		foreach (var message in messages)
		{
			bus.Publish(message, new Dictionary<string, object>());
		}
	}

	/// <summary>
	/// Publishes the specified messages to the bus so that all
	/// relevant subscribers are notified.
	/// </summary>
	/// <typeparam name="TBaseMessage">The common base type or interface implemented by message payloads.</typeparam>
	/// <param name="bus">The bus to publish messages to.</param>
	/// <param name="messages">The messages to publish.</param>
	/// <param name="headers">The headers associated with all messages. The same set of headers is used for each message.</param>
	public static void Publish<TBaseMessage>(this IMessageBus<TBaseMessage> bus, IEnumerable<TBaseMessage> messages, IDictionary<string, object> headers)
	{
		Guard.NotNull(() => bus, bus);
		Guard.NotNull(() => messages, messages);
		Guard.NotNull(() => headers, headers);

		foreach (var message in messages)
		{
			bus.Publish(message, headers);
		}
	}
}