using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
	/// <summary>
	/// Static class that contains a convenience factory method for 
	/// <see cref="Maybe{T}" />,.
	/// </summary>
	static partial class Maybe
	{
		/// <summary>
		/// Convenience factory method that provides type inference.
		/// Constructs a <see cref="Maybe{T}" />.
		/// </summary>
		/// <typeparam name="T">The type of Maybe to construct.</typeparam>
		/// <param name="value">The value to use as the underlying value for the constructed <see cref="Maybe{T}" />.</param>
		/// <returns>A <see cref="Maybe{T}" /> from the given value.</returns>
		public static Maybe<T> From<T>(T value)
		{
			return Maybe<T>.From(value);
		}

		/// <summary>
		/// Factory method that creates a <see cref="Maybe{T}" /> from
		/// a <see cref="Nullable{T}" />.  If the nullable has no value, then <see cref="Maybe{T}.Empty" />
		/// is returned.
		/// </summary>
		/// <param name="value">The value to project to a <see cref="Maybe{T}" />.  This value may be null.</param>
		/// <returns>A <see cref="Maybe{T}" />.</returns>
		public static Maybe<T> From<T>(Nullable<T> value)
			where T : struct
		//This method could be a member of Maybe<T>, but
		//it would have to be written like this:
		//		public static Maybe<U> From<U>(Nullable<U> value)
		//			 where U : struct, T
		//And that looks weird.
		{
			if (!value.HasValue)
			{
				return Maybe<T>.Empty;
			}
			else
			{
				return Maybe<T>.From(value.Value);
			}
		}
	}
}
