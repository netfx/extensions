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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Implements the core NETFx ISerializer interface using the .NET framework runtime BinaryFormatter.
/// </summary>
///	<nuget id="netfx-System.BinarySerializer" />
partial class BinarySerializer : ISerializer
{
	private BinaryFormatter formatter;

	/// <summary>
	/// Initializes a new instance of the <see cref="BinarySerializer"/> class 
	/// with a default <see cref="BinaryFormatter"/>.
	/// </summary>
	public BinarySerializer()
		: this(new BinaryFormatter())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="BinarySerializer"/> class
	/// with the given formatter.
	/// </summary>
	/// <param name="formatter">The binary formatter to use.</param>
	public BinarySerializer(BinaryFormatter formatter)
	{
		Guard.NotNull(() => formatter, formatter);

		this.formatter = formatter;
	}

	/// <summary>
	/// Deserializes an object graph of type <typeparamref name="T"/> 
	/// from the given stream.
	/// </summary>
	/// <typeparam name="T">The type of object graph to deserialize.</typeparam>
	/// <param name="stream">The serialized binary stream.</param>
	public T Deserialize<T>(Stream stream)
	{
		return (T)this.formatter.Deserialize(stream);
	}

	/// <summary>
	/// Serializes the given object graph as a binary stream.
	/// </summary>
	/// <typeparam name="T">The type of object graph to serialize, inferred by the 
	///  compiler from the passed-in <paramref name="graph"/>.</typeparam>
	/// <param name="stream">The stream to serialize into.</param>
	/// <param name="graph">The object graph to serialize.</param>
	public void Serialize<T>(Stream stream, T graph)
	{
		this.formatter.Serialize(stream, graph);
	}
}