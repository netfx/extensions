#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Implementations of this interface are able to serialize and 
/// deserialize an object graph from a given storage type.
/// </summary>
///	<nuget id="netfx-System.ISerializer" />
internal partial interface ISerializer<TStorage>
{
	/// <summary>
	/// Deserializes an object graph from the provided storage.
	/// </summary>
	/// <typeparam name="T">The type of object to be deserialized.</typeparam>
	/// <param name="storage">The storage from which the object will be reconstructed.</param>
	/// <returns>The deserialized object.</returns>
	T Deserialize<T>(TStorage storage);

	/// <summary>
	/// Serializes the provided object graph and writes it to the storage.
	/// </summary>
	/// <typeparam name="T">The type of object to be serialized</typeparam>
	/// <param name="storage">The storage where the object should be persisted.</param>
	/// <param name="graph">The object graph to be serialized.</param>
	void Serialize<T>(TStorage storage, T graph);
}