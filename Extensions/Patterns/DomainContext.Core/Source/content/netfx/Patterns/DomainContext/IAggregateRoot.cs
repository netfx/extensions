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

/// <summary>
/// Base interface that must be implemented by all aggregate root entities. 
/// </summary>
/// <remarks>
/// This interface simply provides the <see cref="IsDeleted"/> on top of 
/// what <see cref="IIdentifiable{T}"/> provides, enforcing the rule that that 
/// aggregate are never deleted using the domain model, they are 
/// simply marked as deleted.
/// <para>
/// Accessing the underlying context implementation though, aggregate 
/// roots can be deleted anyway as usual in EF/NH (i.e. in migration scenarios, 
/// tests, etc., it might be needed).
/// </para>
/// </remarks>
/// <nuget id="netfx-Patterns.DomainContext" />
public partial interface IAggregateRoot<TId> : IIdentifiable<TId>
{
	/// <summary>
	/// Gets or sets a value indicating whether the entity is deleted. 
	/// Aggregate root entities are never deleted using the domain model, they are 
	/// simply marked as deleted.
	/// </summary>
	bool IsDeleted { get; set; }
}
