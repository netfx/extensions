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
using System.Linq.Expressions;

/// <summary>
/// Interface implemented by domain command stores.
/// </summary>
/// <typeparam name="TBaseCommand">The base type that all persisted 
/// commands inherit from, or a common interface for all. Can even 
/// be <see cref="object"/> if the store can deal with any kind of 
/// command object.</typeparam>
/// <remarks>
/// This interface is intentionally simple and devoid of
/// context information on the <see cref="Persist"/> operation,
/// because the intended design is that specific implementations
/// should get whatever context they need in their constructors
/// as dependencies (i.e. the current user) or by augmenting
/// the commands themselves.
/// </remarks>
/// <nuget id="netfx-Patterns.DomainCommands.Core"/>
partial interface IDomainCommandStore<TBaseCommand>
{
	/// <summary>
	/// Notifies the store that the given command 
	/// should be persisted when <see cref="Commit"/> is called.
	/// </summary>
	/// <param name="command">The command instance to persist.</param>
	void Persist(TBaseCommand command);

	/// <summary>
	/// Persists all <see cref="Persist"/>s performed so far, effectively commiting 
	/// the changes to the underlying store in a unit-of-work style.
	/// </summary>
	void Commit();
}
