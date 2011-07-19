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
using System.Linq.Expressions;

/// <summary>
/// Allows consumers of <see cref="IDomainContext{TId}"/> queryable 
/// aggregate roots to include/prefetch certain properties or 
/// property paths. This class allows testing of such extension 
/// method by exposing a replaceable <see cref="IIncluder"/> 
/// interface that concrete domain context implementations 
/// replace on their constructors.
/// </summary>
static partial class QueryableExtensions
{
	internal static IIncluder Includer = new NullIncluder();

	/// <summary>
	/// Specifies that the given property should be included/prefetched for a queryable from an <see cref="IDomainContext{TId}"/>.
	/// </summary>
	/// <typeparam name="T">Type of queryable entity, inferred by the compiler from the <paramref name="source"/> argument.</typeparam>
	/// <typeparam name="TProperty">The type of the property, inferred by the compiler from the <paramref name="path"/> expression.</typeparam>
	/// <param name="source">The queryable source to include/prefetch properties from.</param>
	/// <param name="path">The property path expression.</param>
	public static IQueryable<T> Include<T, TProperty>(this IQueryable<T> source, Expression<Func<T, TProperty>> path)
		where T : class
	{
		return Includer.Include(source, path);
	}

	/// <summary>
	/// Specifies that the given property should be included/prefetched for a queryable from an <see cref="IDomainContext{TId}"/>.
	/// </summary>
	/// <typeparam name="T">Type of queryable entity, inferred by the compiler from the <paramref name="source"/> argument.</typeparam>
	/// <param name="source">The queryable source to include/prefetch properties from.</param>
	/// <param name="path">The property path expression.</param>
	public static IQueryable<T> Include<T>(this IQueryable<T> source, string path)
		where T : class
	{
		return Includer.Include(source, path);
	}

	/// <summary>
	/// Provides the <c>Include</c> extension method for queryables. Does 
	/// nothing by default.
	/// </summary>
	public interface IIncluder
	{
		/// <summary>
		/// Specifies that the given property should be included/prefetched.
		/// </summary>
		/// <typeparam name="T">Type of queryable entity, inferred by the compiler from the <paramref name="source"/> argument.</typeparam>
		/// <typeparam name="TProperty">The type of the property, inferred by the compiler from the <paramref name="path"/> expression.</typeparam>
		/// <param name="source">The queryable source to include/prefetch properties from.</param>
		/// <param name="path">The property path expression.</param>
		IQueryable<T> Include<T, TProperty>(IQueryable<T> source, Expression<Func<T, TProperty>> path)
			where T : class;

		/// <summary>
		/// Specifies that the given property should be included/prefetched.
		/// </summary>
		/// <typeparam name="T">Type of queryable entity, inferred by the compiler from the <paramref name="source"/> argument.</typeparam>
		/// <param name="source">The queryable source to include/prefetch properties from.</param>
		/// <param name="path">The property path expression.</param>
		IQueryable<T> Include<T>(IQueryable<T> source, string path)
			where T : class;
	}

	private class NullIncluder : IIncluder
	{
		public IQueryable<T> Include<T, TProperty>(IQueryable<T> source, Expression<Func<T, TProperty>> path)
			where T : class
		{
			return source;
		}

		public IQueryable<T> Include<T>(IQueryable<T> source, string path) where T : class
		{
			return source;
		}
	}
}
