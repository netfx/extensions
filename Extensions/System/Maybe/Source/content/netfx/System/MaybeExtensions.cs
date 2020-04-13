using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
	static partial class MaybeExtensions
	{
		/// <summary>
		/// Provides support for query comprehension syntax of the following form:
		///	var value =	
		///		from x in [Maybe{X}]
		///		select x.ToString();
		/// </summary>
		/// <typeparam name="T">The underlying type of the <see cref="Maybe{T}" />.</typeparam>
		/// <typeparam name="U">The underlying type of the <see cref="Maybe{U}" /> that results 
		/// from the projection <paramref name="project"/>.</typeparam>
		/// <param name="this" this="true">The current Maybe.</param>
		/// <param name="project">The projection that will be used to create a <typeparamref name="U"/> from
		/// the value <typeparamref name="T"/> that underlies the current Maybe instance.
		/// If the current <see cref="Maybe{T}" /> is Empty, then <see cref="Maybe{U}.Empty" />
		/// is returned instead.</param>
		/// <returns>If the current <see cref="Maybe{T}" /> is not Empty, returns the T value this instance wraps
		/// projected onto U using the specified projection; returns <see cref="Maybe{U}.Empty" />" otherwise.
		/// </returns>
		public static Maybe<U> Select<T, U>(this Maybe<T> @this, Func<T, U> project)
		{
			Guard.NotNull(() => @this, @this);
			return @this.Map(project);
		}

		/// <summary>
		/// Provides support for query comprehension syntax of the following form:
		///	var value =	
		///		from x in [Maybe{X}]
		///		from y in [Maybe{Y}]
		///		select x.ToString() + y.ToString();
		/// </summary>
		/// <typeparam name="T">The type of the current Maybe.</typeparam>
		/// <typeparam name="U">The type of the Maybe that results from the projection <paramref name="project"/>.</typeparam>
		/// <typeparam name="V">The type of Maybe that results from the combination of the <see cref="Maybe{T}" /> and the
		/// projected <see cref="Maybe{U}" />.</typeparam>
		/// <param name="this" this="true">The current Maybe.</param>
		/// <param name="project">The projection that will be used to create a <see cref="Maybe{U}" /> from
		/// the current <see cref="Maybe{T}" />.</param>
		/// <param name="combine">The function that logically "combines" the <see cref="Maybe{T}" /> and
		/// the projected <see cref="Maybe{U}" />.
		/// </param>
		/// <returns>The combination of the <paramref name="combine"/> function,
		/// or <see cref="Maybe{V}.Empty" /> if either of the current <see cref="Maybe{T}" />
		/// and the projected <see cref="Maybe{U}" /> is Empty.
		/// </returns>
		public static Maybe<V> SelectMany<T, U, V>(this Maybe<T> @this, Func<T, Maybe<U>> project, Func<T, U, V> combine)
		{
			if (!@this.HasValue)
			{
				return Maybe<V>.Empty;
			}

			var maybeU = @this.Bind(project);
			if (!maybeU.HasValue)
			{
				return Maybe<V>.Empty;
			}

			var v = combine(@this.GetValueOrDefault(), maybeU.GetValueOrDefault());
			return Maybe.From(v);
		}

		/// <summary>
		/// Returns the current <see cref="Maybe{T}" /> if it is not Empty; otherwise,
		/// returns <paramref name="other"/>.
		/// </summary>
		/// <param name="this" this="true">The current <see cref="Maybe{T}" />.</param>
		/// <param name="other">The <see cref="Maybe{T}" /> to return if the current
		/// <see cref="Maybe{T}" /> is Empty.</param>
		/// <returns>The current <see cref="Maybe{T}" /> if it is not Empty; otherwise,
		/// returns <paramref name="other"/>.</returns>
		public static Maybe<T> Coalesce<T>(this Maybe<T> @this, Maybe<T> other)
		{
			Guard.NotNull(() => other, other);
			if (@this.HasValue)
			{
				return @this;
			}
			else
			{
				return other;
			}
		}

	}

}
