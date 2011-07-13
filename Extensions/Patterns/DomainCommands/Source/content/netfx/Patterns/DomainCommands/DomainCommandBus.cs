using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

/// <summary>
/// Default implementation of an <see cref="IDomainCommandBus"/> that 
/// invokes command handlers registered for a given command.
/// <para>
/// Handlers with <see cref="DomainCommandHandler.IsAsync"/> set to 
/// <see langword="true"/> are invoked through the optional 
/// async runner delegate passed to the constructor.
/// </para>
/// </summary>
/// <nuget id="netfx-Patterns.DomainCommands" />
public partial class DomainCommandBus : IDomainCommandBus
{
	private IEnumerable<DomainCommandHandler> commandHandlers;
	private Action<Action> asyncActionRunner;

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
	public static IDomainCommandBus None { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainCommandBus"/> class with 
	/// the default async runner that enqueues work in the <see cref="ThreadPool"/>.
	/// </summary>
	/// <param name="commandHandlers">The command handlers.</param>
	public DomainCommandBus(IEnumerable<DomainCommandHandler> commandHandlers)
		: this(commandHandlers, action => ThreadPool.QueueUserWorkItem(state => action()))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainCommandBus"/> class with 
	/// the given async runner.
	/// </summary>
	/// <param name="commandHandlers">The command handlers.</param>
	/// <param name="asyncActionRunner">The async action runner to use to invoke command handlers 
	/// that have <see cref="DomainCommandHandler.IsAsync"/> set to <see langword="true"/>.</param>
	public DomainCommandBus(IEnumerable<DomainCommandHandler> commandHandlers, Action<Action> asyncActionRunner)
	{
		if (commandHandlers.Any(eh =>
			eh == null ||
			!InheritsFromGenericHandler(eh.GetType())))
			throw new ArgumentException("commandHandlers");

		this.commandHandlers = commandHandlers;
		this.asyncActionRunner = asyncActionRunner;
	}

	/// <summary>
	/// Executes all handlers registered to receive the given command.
	/// </summary>
	/// <typeparam name="T">Type of command, typically inferred by the compiler based on the received <paramref name="command"/>.</typeparam>
	/// <param name="command">The command payload.</param>
	public virtual void Execute<T>(T command)
		where T : DomainCommand
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

	private bool InheritsFromGenericHandler(Type type)
	{
		var baseType = type.BaseType;
		while (baseType != typeof(object))
		{
			if (baseType.IsGenericType &&
				baseType.GetGenericTypeDefinition() == typeof(DomainCommandHandler<>))
				return true;

			baseType = baseType.BaseType;
		}

		return false;
	}

	/// <summary>
	/// Provides a null <see cref="IDomainCommandBus"/> implementation 
	/// for use when no command bus has been configured.
	/// </summary>
	private class NullBus : IDomainCommandBus
	{
		/// <summary>
		/// Does nothing.
		/// </summary>
		public void Execute<T>(T command) where T : DomainCommand
		{
		}
	}
}
