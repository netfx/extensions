using System;

/// <summary>
/// Interface implemented by the component that coordinates 
/// command handler invocation when a subscribed command is executed.
/// </summary>
/// <nuget id="netfx-Patterns.DomainCommands" />
public partial interface IDomainCommandBus
{
	/// <summary>
	/// Executes the specified command.
	/// </summary>
	void Execute<T>(T command) where T : DomainCommand;
}
