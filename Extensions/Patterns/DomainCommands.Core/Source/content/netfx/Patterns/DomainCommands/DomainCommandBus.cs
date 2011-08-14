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
/// Default implementation of an <see cref="IDomainCommandBus{TBaseCommand}"/> that 
/// invokes in-memory command handlers registered for a given command.
/// <para>
/// Handlers with <see cref="IDomainCommandHandler.IsAsync"/> set to 
/// <see langword="true"/> are invoked through the optional 
/// async runner delegate passed to the constructor. If no async 
/// runner is specified, the <see cref="ThreadPool.QueueUserWorkItem(WaitCallback)"/>
/// will be used automatically.
/// </para>
/// </summary>
/// <typeparam name="TBaseCommand">The base type that all persisted 
/// commands inherit from, or a common interface for all. Can even 
/// be <see cref="object"/> if the store can deal with any kind of 
/// command object.</typeparam>
/// <nuget id="netfx-Patterns.DomainCommands.Core" />
partial class DomainCommandBus<TBaseCommand> : IDomainCommandBus<TBaseCommand>
{
	private IEnumerable<IDomainCommandHandler> commandHandlers;
	private Action<Action> asyncActionRunner;
	private IDomainCommandStore<TBaseCommand> commandStore;

	/// <summary>
	/// Initializes the <see cref="None"/> null object 
	/// pattern property.
	/// </summary>
	static DomainCommandBus()
	{
		None = new NullBus();
	}

	/// <summary>
	/// Gets a default domain command bus implementation that 
	/// does nothing (a.k.a. Null Object Pattern).
	/// </summary>
	public static IDomainCommandBus<TBaseCommand> None { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainCommandBus{TBaseCommand}"/> class with 
	/// the default async runner that enqueues work in the <see cref="ThreadPool"/>.
	/// </summary>
	/// <param name="commandHandlers">The command handlers.</param>
	public DomainCommandBus(IEnumerable<IDomainCommandHandler> commandHandlers)
		: this(DomainCommandStore<TBaseCommand>.None, commandHandlers, action => ThreadPool.QueueUserWorkItem(state => action()))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainCommandBus{TBaseCommand}"/> class with 
	/// the given command storage implementation and list of available command handlers.
	/// </summary>
	/// <param name="commandStore">The command store.</param>
	/// <param name="commandHandlers">The command handlers.</param>
	public DomainCommandBus(IDomainCommandStore<TBaseCommand> commandStore, IEnumerable<IDomainCommandHandler> commandHandlers)
		: this(commandStore, commandHandlers, action => ThreadPool.QueueUserWorkItem(state => action()))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainCommandBus{TBaseCommand}"/> class with
	/// the given list of available command handlers and an async runner for command 
	/// handlers that must be executed asynchronously.
	/// </summary>
	/// <param name="commandHandlers">The available command handlers.</param>
	/// <param name="asyncActionRunner">The async action runner to use to invoke command handlers
	/// that have <see cref="DomainCommandHandler.IsAsync"/> set to <see langword="true"/>.</param>
	public DomainCommandBus(IEnumerable<IDomainCommandHandler> commandHandlers, Action<Action> asyncActionRunner)
		: this(DomainCommandStore<TBaseCommand>.None, commandHandlers, asyncActionRunner)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainCommandBus{TBaseCommand}"/> class with
	/// the given command storage implementation, list of available command handlers 
	/// and an async runner for command handlers that must be executed asynchronously.
	/// </summary>
	/// <param name="commandStore">The command store.</param>
	/// <param name="commandHandlers">The command handlers.</param>
	/// <param name="asyncActionRunner">The async action runner to use to invoke command handlers
	/// that have <see cref="DomainCommandHandler.IsAsync"/> set to <see langword="true"/>.</param>
	public DomainCommandBus(IDomainCommandStore<TBaseCommand> commandStore, IEnumerable<IDomainCommandHandler> commandHandlers, Action<Action> asyncActionRunner)
	{
		Guard.NotNull(() => commandStore, commandStore);
		Guard.NotNull(() => commandHandlers, commandHandlers);
		Guard.NotNull(() => asyncActionRunner, asyncActionRunner);

		if (commandHandlers.Any(eh => eh == null))
			throw new ArgumentException("Invalid null handler found.", "commandHandlers");

		if (commandHandlers.Any(eh => !ImplementsGenericHandler(eh.GetType())))
			throw new ArgumentException(string.Format(
				CultureInfo.CurrentCulture,
				"The following event handlers to not implement the generic interface IDomainEventHandler<TAggregateId, TEventArgs>: {0}.",
				 string.Join(", ", commandHandlers.Where(eh => !ImplementsGenericHandler(eh.GetType())).Select(eh => eh.GetType().FullName))),
				 "commandHandlers");

		this.commandStore = commandStore;
		this.commandHandlers = commandHandlers;
		this.asyncActionRunner = asyncActionRunner;
	}

	/// <summary>
	/// Executes all handlers registered to receive the given command.
	/// </summary>
	/// <typeparam name="T">Type of command, typically inferred by the compiler based on the received <paramref name="command"/>.</typeparam>
	/// <param name="command">The command payload.</param>
	public virtual void Execute<T>(T command)
		where T : TBaseCommand
	{
		// Unlike events, commands are matched explicitly to the 
		// given T. This allows conversion handlers to be registered, 
		// for example for Command_V1 to upgrade it and invoke Execute again 
		// with the converted Command_V2.
		var compatibleHandlers = this.commandHandlers.OfType<DomainCommandHandler<T>>();

		foreach (var handler in compatibleHandlers.Where(h => !h.IsAsync).AsParallel())
		{
			handler.Handle(command);
		}

		// Run background handlers through the runner.
		foreach (dynamic handler in compatibleHandlers.Where(h => h.IsAsync).AsParallel())
		{
			asyncActionRunner(() => handler.Handle(command));
		}
	}

	private bool ImplementsGenericHandler(Type type)
	{
		var genericHandler = typeof(IDomainCommandHandler<>);

		return type.GetInterfaces().Any(iface =>
			iface.IsGenericType &&
			iface.GetGenericTypeDefinition() == genericHandler);
	}

	/// <summary>
	/// Provides a null bus implementation 
	/// for use when no command bus has been configured.
	/// </summary>
	private class NullBus : IDomainCommandBus<TBaseCommand>
	{
		/// <summary>
		/// Does nothing.
		/// </summary>
		public void Execute<T>(T command) where T : TBaseCommand
		{
		}
	}
}
