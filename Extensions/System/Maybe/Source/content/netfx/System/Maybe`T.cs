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
using System.Linq.Expressions;
using System.Text;

namespace System
{
	// NOTE: the <nuget id="" /> documentation element is used to determine 
	// which nuget provides what classes. This is important as netfx packages 
	// can use other netfx packages, therefore generating XML documentation 
	// for them too. We need to tell apart the actual types provides by 
	// this nuget, hence the id.

	/// <summary>
	/// An implementation of the Maybe monad.
	/// </summary>
	///	<nuget id="netfx-System.Maybe" />
	sealed partial class Maybe<T> : IEquatable<Maybe<T>>
	{
		static Maybe()
		{
			_empty = new Maybe<T>();
		}

		private static readonly Maybe<T> _empty;

		/// <summary>
		/// The empty value.
		/// </summary>
		public static Maybe<T> Empty
		{
			get
			{
				return _empty;
			}
		}

		/// <summary>
		/// Factory method that creates a <see cref="Maybe{T}" />
		/// that wraps the given T value.  If that value is null, 
		/// then <see cref="Maybe{T}.Empty" /> is returned.
		/// </summary>
		/// <param name="value">The value to wrap in a <see cref="Maybe{T}" />.  
		/// This value may be null.</param>
		/// <returns>A <see cref="Maybe{T}" /> wrapping the given value.</returns>
		public static Maybe<T> From(T value)
		{
			if (value == null)
			{
				return Maybe<T>.Empty;
			}
			else
			{
				return new Maybe<T>(value);
			}
		}

		/// <summary>
		/// Constructor for the Empty value.
		/// </summary>
		private Maybe()
		{
			this.HasValue = false;
		}

		/// <summary>
		/// Constructor for non-Empty values.
		/// </summary>
		/// <param name="value"></param>
		private Maybe(T value)
		{
			this.Value = value;
			this.HasValue = true;
		}

		private T Value { get; set; }


		/// <summary>
		/// Indicates whether or not the current instance has an underlying value.
		/// Returns true if there is an underlying value, or false if there is not.
		/// HasValue returns false if and only if the current instance is equal to
		/// <see cref="Maybe{T}.Empty" />.
		/// </summary>
		public bool HasValue { get; private set; }


		/// <summary>
		/// Returns the <typeparamref name="T"/> value underlying this <see cref="Maybe{T}" />
		/// if there is one; otherise, returns the specified default value for T.
		/// </summary>
		/// <param name="default">The default value to return if the current <see cref="Maybe{T}" />
		/// is Empty.</param>
		/// <returns> The <typeparamref name="T"/> value underlying this <see cref="Maybe{T}" />
		/// if there is one; otherise, returns the specified default T value.</returns>
		public T GetValueOrDefault(T @default)
		{
			if (HasValue)
			{
				return Value;
			}
			else
			{
				return @default;
			}
		}

		/// <summary>
		/// Returns the <typeparamref name="T"/> value underlying this <see cref="Maybe{T}" />
		/// if there is one; otherise, returns default(T).
		/// </summary>
		/// <returns> The <typeparamref name="T"/> value underlying this <see cref="Maybe{T}" />
		/// if there is one; otherise, returns default(T).</returns>
		public T GetValueOrDefault()
		{
			return GetValueOrDefault(default(T));
		}

		/// <summary>
		/// If Empty, returns <see cref="Maybe{U}.Empty" />;
		/// otherwise, projects the current underlying value from 
		/// <typeparamref name="T"/> to <typeparamref name="U"/>
		/// using the given projection and returns that value
		/// wrapped in a <see cref="Maybe{U}" />.
		/// </summary>
		/// <typeparam name="U">The output type.</typeparam>
		/// <param name="projection">The projection to use
		/// to create a <see cref="Maybe{U}" /> from the value of the current <see cref="Maybe{T}" />.
		/// </param>
		/// <returns>The value T projected onto <see cref="Maybe{U}" />.</returns>
		public Maybe<U> Bind<U>(Func<T, Maybe<U>> projection)
		{
			Guard.NotNull(() => projection, projection);
			if (!HasValue)
			{
				return Maybe<U>.Empty;
			}
			else
			{
				Maybe<U> maybeU = projection(this.Value);
				return maybeU;
			}
		}

		/// <summary>
		/// Maps a <see cref="Maybe{T}" /> to a <see cref="Maybe{U}" />.
		/// </summary>
		/// <typeparam name="U">The type to project the underlying value to.</typeparam>
		/// <param name="project">The projection to use.</param>
		/// <returns>A <see cref="Maybe{U}" />.</returns>
		public Maybe<U> Map<U>(Func<T, U> project)
		{
			Guard.NotNull(() => project, project);
			return Bind(t => Maybe.From(project(t)));
		}


		/// <summary>
		/// Creates a <see cref="Maybe{U}" /> from
		/// the underlying value of the current <see cref="Maybe{T}" />,
		/// if it is not Empty.  If the current <see cref="Maybe{T}" />
		/// is Empty, returns <see cref="Maybe{U}.Empty" />.
		/// </summary>
		/// <typeparam name="U">The underlying type of the resultant Maybe.</typeparam>
		/// <returns>A <see cref="Maybe{U}" /></returns>
		public Maybe<U> As<U>()
		//It would be awfully nice if the language let us write
		// "where U > T" here, but it does not.
		{
			//This method would be better as an extension method,
			//except that it would require either changing the signature to 
			//[As<T, U>()], which would be obnoxious, or creating a non-generic
			//base class for Maybe<T> and creating an extension method for
			//*that* with the signature [As<U>()], which isn't worth it.


			return Map(t => (U)(object)t);
		}

		#region Equality
		
		/// <summary>
		/// Determines whether the given System.Object is equivalent to the current
		/// <see cref="Maybe{T}" />.
		/// </summary>
		/// <param name="obj">The object against which to compare the current <see cref="Maybe{T}" />.</param>
		/// <returns>True if the object is equivalent to the current <see cref="Maybe{T}" />; false othewise.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as Maybe<T>);
		}

		private const int HASH_CONSTANT = 29;

		/// <summary>
		/// Returns the hash code (for use in a dictionary or hash map) of the current <see cref="Maybe{T}" />.
		/// </summary>
		/// <returns>The hash code of the current <see cref="Maybe{T}" />.</returns>
		public override int GetHashCode()
		{
			if (!HasValue)
			{
				return HASH_CONSTANT;
			}
			else
			{
				return this.Value.GetHashCode() ^ HASH_CONSTANT;
			}
		}

		/// <summary>
		/// Compares the current <see cref="Maybe{T}" /> to another <see cref="Maybe{T}" />, returning true if they are equivalent,
		/// or false otherwise.
		/// <see cref="Maybe{T}" /> objects are equivalent if and only if both instances are <see cref="Maybe{T}.Empty" /> or
		/// both instances' underlying values are equivalent.
		/// </summary>
		/// <param name="other">The other <see cref="Maybe{T}" /> to compare against this instance.</param>
		/// <returns>True if the object are equivalent; false otherwise.</returns>
		public bool Equals(Maybe<T> other)
		{
			if (IsNull(other))
			{
				return false;
			}
			else if (IsEmpty(this)
				&& IsEmpty(other))
			{
				return true;
			}
			else if (IsEmpty(this)
				|| IsEmpty(other))
			{
				return false;
			}
			else
			{
				return this.Value.Equals(other.Value);
			}
		}

		public static bool operator ==(Maybe<T> left, Maybe<T> right)
		{
			return EqualsOperatorImpl(left, right);
		}

		public static bool operator !=(Maybe<T> left, Maybe<T> right)
		{
			return !EqualsOperatorImpl(left, right);
		}

		public static bool operator ==(Maybe<T> left, object right)
		{
			return EqualsOperatorImpl(left, right as Maybe<T>);
		}

		public static bool operator !=(Maybe<T> left, object right)
		{
			return !EqualsOperatorImpl(left, right as Maybe<T>);
		}

		public static bool operator ==(object left, Maybe<T> right)
		{
			return EqualsOperatorImpl(right, left as Maybe<T>);
		}

		public static bool operator !=(object left, Maybe<T> right)
		{
			return !EqualsOperatorImpl(right, left as Maybe<T>);
		}

		private static bool EqualsOperatorImpl(Maybe<T> left, Maybe<T> right)
		{
			if (IsNull(left) && IsNull(right))
			{
				return true;
			}
			else if (IsNull(left) ^ IsNull(right))
			{
				return false;
			}
			else
			{
				return left.Equals(right);
			}
		}

		private static bool IsNull(object o)
		{
			return ReferenceEquals(o, null);
		}

		private static bool IsEmpty<U>(Maybe<U> m)
		{
			  return !m.HasValue;
		}
		#endregion
	}

}