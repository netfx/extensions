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
/// Factory class to create <see cref="ValueEventArgs{T}"/> from a value.
/// </summary>
///	<nuget id="netfx-System.ValueEventArgs" />
static partial class ValueEventArgs
{
	/// <summary>
	/// Creates a typed <see cref="ValueEventArgs{T}"/> for the given piece of data.
	/// </summary>
	public static ValueEventArgs<T> Create<T>(this T data)
	{
		return new ValueEventArgs<T>(data);
	}
}

/// <summary>
/// An EventArgs class that exposes a single Value property of a type T.
/// </summary>
/// <typeparam name="T">Type of value exposed by the argument class.</typeparam>
/// <nuget id="netfx-System.ValueEventArgs"/>
partial class ValueEventArgs<T> : EventArgs, IEquatable<ValueEventArgs<T>>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ValueEventArgs{T}"/> class.
	/// </summary>
	public ValueEventArgs(T value)
	{
		this.Value = value;
	}

	/// <summary>
	/// Gets value of the event.
	/// </summary>
	public T Value { get; private set; }

	/// <summary>
	/// Checks if the current instance is equal to the other.
	/// </summary>
	public bool Equals(ValueEventArgs<T> other)
	{
		return ValueEventArgs<T>.Equals(this, other);
	}

	/// <summary>
	/// Checks if the current instance is equal to the given object.
	/// </summary>
	public override bool Equals(object obj)
	{
		return ValueEventArgs<T>.Equals(this, obj as ValueEventArgs<T>);
	}

	private static bool Equals(ValueEventArgs<T> obj1, ValueEventArgs<T> obj2)
	{
		if (Object.Equals(null, obj1) ||
			Object.Equals(null, obj2) ||
			Object.Equals(null, obj1.Value) ||
			Object.Equals(null, obj2.Value) ||
			obj1.GetType() != obj2.GetType())
			return false;

		if (Object.ReferenceEquals(obj1.Value, obj2.Value)) return true;

		return Object.Equals(obj1.Value, obj2.Value);
	}

	/// <summary>
	/// Returns a hash code for this instance.
	/// </summary>
	public override int GetHashCode()
	{
		return this.Value != null ? this.Value.GetHashCode() : base.GetHashCode();
	}
}
