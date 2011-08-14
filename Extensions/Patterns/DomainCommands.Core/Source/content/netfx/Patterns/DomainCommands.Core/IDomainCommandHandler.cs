using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Base interface part of the infrastructure. Concrete 
/// handlers should inherit <see cref="DomainCommandHandler{TCommand}"/> or 
/// implement <see cref="IDomainCommandHandler{TCommand}"/> instead.
/// </summary>
/// <nuget id="netfx-Patterns.DomainCommands.Core" />
partial interface IDomainCommandHandler
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
partial interface IDomainCommandHandler<TCommand> : IDomainCommandHandler
{
	/// <summary>
	/// Handles the specified command.
	/// </summary>
	void Handle(TCommand command);
}