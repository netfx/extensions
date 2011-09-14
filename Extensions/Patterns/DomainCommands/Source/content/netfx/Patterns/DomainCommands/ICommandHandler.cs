using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Base interface part of the infrastructure. Concrete 
/// handlers should inherit <see cref="CommandHandler{TCommand}"/> or 
/// implement <see cref="ICommandHandler{TCommand}"/> instead.
/// </summary>
/// <nuget id="netfx-Patterns.DomainCommands.Core" />
partial interface ICommandHandler
{
	/// <summary>
	/// Gets a value indicating whether this handler should be executed asynchronously.
	/// </summary>
	bool IsAsync { get; }
}

/// <summary>
/// Base interface for domain command handlers that subscribe to specific type of command.
/// </summary>
/// <typeparam name="TCommand">Type of command this handler can process.</typeparam>
/// <nuget id="netfx-Patterns.DomainCommands.Core" />
partial interface ICommandHandler<TCommand> : ICommandHandler
{
	/// <summary>
	/// Handles the specified command.
	/// </summary>
	/// <param name="command">The command to handle.</param>
	/// <param name="headers">The headers associated with the command.</param>
	void Handle(TCommand command, IDictionary<string, object> headers);
}