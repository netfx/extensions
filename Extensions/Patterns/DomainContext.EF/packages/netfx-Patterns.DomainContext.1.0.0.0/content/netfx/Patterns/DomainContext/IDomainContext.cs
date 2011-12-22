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
/// Provides access to the domain aggregate roots.
/// </summary>
/// <nuget id="netfx-Patterns.DomainContext" />
partial interface IDomainContext<TId> : IDisposable
{
	/// <summary>
	/// Saves all changes made in this context to the underlying database.
	/// </summary>
	void Commit();

	/// <summary>
	/// Finds the aggregate root with the specified id.
	/// </summary>
	/// <returns>The found aggregate or <see langword="null"/>.</returns>
	T Find<T>(TId id) where T : class, IAggregateRoot<TId>;
	
	/// <summary>
	/// Inserts or updates the specified aggregate root.
	/// </summary>
	void Persist<T>(T entity) where T : class, IAggregateRoot<TId>;

	/// <summary>
	/// Creates a new instance of an aggregate root.
	/// </summary>
	/// <remarks>
	/// Although not strictly required, using this method for creating new 
	/// aggregate roots allows the context to perform additional initialization 
	/// if needed, such as injecting the <see cref="IDomainContext{TId}"/> into an 
	/// entity that implements <see cref="IDomainContextAccessor{TDomainContext}"/>, tracking 
	/// the entity changes, create a proxy for it, etc.
	/// </remarks>
	/// <typeparam name="T">Type of aggregate root to instantiate.</typeparam>
	T New<T>(Action<T> initializer = null) where T : class, IAggregateRoot<TId>;

	/// <summary>
	/// Logically deletes the specified aggregate root.
	/// </summary>
	void Delete<T>(TId id) where T : class, IAggregateRoot<TId>;

	/// <summary>
	/// Logically deletes the aggregate root with the specified identifier.
	/// </summary>
	void Delete<T>(T entity) where T : class, IAggregateRoot<TId>;
}
