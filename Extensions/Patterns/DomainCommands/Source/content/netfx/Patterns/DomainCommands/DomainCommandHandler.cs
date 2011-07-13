using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Base class part of the infrastructure. Concrete 
/// handlers should inherit <see cref="DomainCommandHandler{T}"/> instead.
/// </summary>
/// <nuget id="netfx-Patterns.DomainCommands" />
public abstract partial class DomainCommandHandler
{
	/// <summary>
	/// Gets a value indicating whether this handler should be executed asynchronously.
	/// </summary>
	public virtual bool IsAsync { get { return false; } }
}

/// <summary>
/// Base class for domain command handlers that subscribe to specific type of command.
/// </summary>
/// <typeparam name="T">Type of command this handler can process.</typeparam>
/// <nuget id="netfx-Patterns.DomainCommands" />
public abstract partial class DomainCommandHandler<T> : DomainCommandHandler
	where T : DomainCommand
{
	/// <summary>
	/// Handles the specified command.
	/// </summary>
	public abstract void Handle(T command);
}