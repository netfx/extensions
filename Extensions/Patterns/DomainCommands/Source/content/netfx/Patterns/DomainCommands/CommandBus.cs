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
using System.Diagnostics;

/// <summary>
/// Default implementation of an <see cref="ICommandBus{TBaseCommand}"/> that 
/// invokes in-memory command handlers registered for a given command.
/// <para>
/// Handlers with <see cref="ICommandHandler.IsAsync"/> set to 
/// <see langword="true"/> are invoked through the optional 
/// async runner delegate passed to the constructor. If no async 
/// runner is specified, the <see cref="ThreadPool.QueueUserWorkItem(WaitCallback)"/>
/// will be used automatically.
/// </para>
/// </summary>
/// <typeparam name="TBaseCommand">The base type that all commands inherit from, 
/// or a common interface for all. Can even be <see cref="object"/> if no 
/// common interface is needed.</typeparam>
/// <nuget id="netfx-Patterns.DomainCommands.Core" />
partial class CommandBus<TBaseCommand> : ICommandBus<TBaseCommand>
{
	private Dictionary<Type, dynamic> commandHandlers = new Dictionary<Type, dynamic>();
	private Action<Action> asyncActionRunner;
	private IMessageStore<TBaseCommand> commandStore;

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandBus{TBaseCommand}"/> class with 
	/// the default async runner that enqueues work in the <see cref="ThreadPool"/>.
	/// </summary>
	/// <param name="commandHandlers">The command handlers.</param>
	public CommandBus(IEnumerable<ICommandHandler> commandHandlers)
		: this(null, commandHandlers, action => ThreadPool.QueueUserWorkItem(state => action()))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandBus{TBaseCommand}"/> class with 
	/// the given command storage implementation and list of available command handlers.
	/// </summary>
	/// <param name="commandStore">The command store.</param>
	/// <param name="commandHandlers">The command handlers.</param>
	public CommandBus(IMessageStore<TBaseCommand> commandStore, IEnumerable<ICommandHandler> commandHandlers)
		: this(commandStore, commandHandlers, action => ThreadPool.QueueUserWorkItem(state => action()))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandBus{TBaseCommand}"/> class with
	/// the given list of available command handlers and an async runner for command 
	/// handlers that must be executed asynchronously.
	/// </summary>
	/// <param name="commandHandlers">The available command handlers.</param>
	/// <param name="asyncActionRunner">The async action runner to use to invoke command handlers
	/// that have <see cref="ICommandHandler.IsAsync"/> set to <see langword="true"/>.</param>
	public CommandBus(IEnumerable<ICommandHandler> commandHandlers, Action<Action> asyncActionRunner)
		: this(null, commandHandlers, asyncActionRunner)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandBus{TBaseCommand}"/> class with
	/// the given command storage implementation, list of available command handlers 
	/// and an async runner for command handlers that must be executed asynchronously.
	/// </summary>
	/// <param name="commandStore">The command store.</param>
	/// <param name="commandHandlers">The command handlers.</param>
	/// <param name="asyncActionRunner">The async action runner to use to invoke command handlers
	/// that have <see cref="ICommandHandler.IsAsync"/> set to <see langword="true"/>.</param>
	public CommandBus(IMessageStore<TBaseCommand> commandStore, IEnumerable<ICommandHandler> commandHandlers, Action<Action> asyncActionRunner)
	{
		Guard.NotNull(() => commandHandlers, commandHandlers);
		Guard.NotNull(() => asyncActionRunner, asyncActionRunner);

		if (commandHandlers.Any(eh => eh == null))
			throw new ArgumentException("Invalid null handler found.", "commandHandlers");

		var duplicateHandlersForType = new List<KeyValuePair<Type, ICommandHandler>>();
		var nonGenericHandlers = new List<ICommandHandler>();
		var genericHandler = typeof(ICommandHandler<>);

		foreach (var handler in commandHandlers)
		{
			// Grab the type of body from the generic 
			// type argument, if any.
			var commandType = handler.GetType()
				.GetInterfaces()
				.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericHandler)
				.Select(x => x.GetGenericArguments()[0])
				.FirstOrDefault();

			if (commandType == null)
				nonGenericHandlers.Add(handler);
			else if (this.commandHandlers.ContainsKey(commandType))
				duplicateHandlersForType.Add(new KeyValuePair<Type, ICommandHandler>(commandType, handler));
			else
				this.commandHandlers.Add(commandType, handler);
		}

		var error = default(string);

		if (nonGenericHandlers.Any())
			error = string.Format(
				CultureInfo.CurrentCulture,
				"The following event handlers to not implement the generic interface ICommanHandler<TCommand>: {0}.",
				 string.Join(", ", nonGenericHandlers.Select(eh => eh.GetType().FullName)));

		foreach (var duplicateHandler in duplicateHandlersForType)
		{
			// "The following command handlers cannot be registered because another handler is already registered for the command type: {0}.",
			error += string.Format(CultureInfo.CurrentCulture,
				"The command handler '{0}' cannot be used because an existing handler '{1}' is already registered for the command type: {2}.",
				duplicateHandler, this.commandHandlers[duplicateHandler.Key], duplicateHandler.Key);
		}

		if (error != null)
			throw new ArgumentException(error);

		this.commandStore = commandStore;
		this.asyncActionRunner = asyncActionRunner;
	}

	/// <summary>
	/// Executes all handlers registered to receive the given command.
	/// </summary>
	/// <param name="command">The command payload.</param>
	/// <param name="headers">The headers for the command.</param>
	public virtual void Execute(TBaseCommand command, IDictionary<string, object> headers)
	{
		Guard.NotNull(() => command, command);
		Guard.NotNull(() => headers, headers);

		// Unlike events, commands are matched explicitly to the 
		// given T, and exactly one handler must be found.
		dynamic handler = null;
		if (!this.commandHandlers.TryGetValue(command.GetType(), out handler))
			throw new InvalidOperationException(string.Format(
				CultureInfo.CurrentCulture,
				"No command handler has been registered for command type '{0}'.",
				command.GetType()));

		if (handler.IsAsync)
			asyncActionRunner(() => OnExecute(handler, command, headers));
		else
			OnExecute(handler, command, headers);
	}

	/// <summary>
	/// Called when invoking the handler for a command with the given headers.
	/// </summary>
	/// <remarks>
	/// Derived classes can change the way handlers are invoked, optimize it, 
	/// or do pre/post processing right before/after the command is handled.
	/// </remarks>
	protected virtual void OnExecute(dynamic handler, dynamic command, IDictionary<string, object> headers)
	{
		var newActivityId = Guid.NewGuid();
		var oldActivityId = Trace.CorrelationManager.ActivityId;
		Trace.CorrelationManager.ActivityId = newActivityId;

		try
		{
			handler.Handle(command, headers);
		}
		finally
		{
			if (this.commandStore != null)
				// Persisting after execute (regardless of failure conditions)
				// allows handlers to augment the headers before execution 
				// reaches the target command in a chain of responsibility style.
				this.commandStore.Save(command, headers);

			Trace.CorrelationManager.ActivityId = oldActivityId;
		}
	}
}
