using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Net.Http
{
	/// <summary>
	/// Provides the <see cref="ToQueryResponse"/> extension method to execute 
	/// the query and retrieve the additional response information like 
	/// total count and the original <see cref="HttpResponseMessage"/>.
	/// </summary>
	internal static class QueryableExtension
	{
		/// <summary>
		/// Executes the query and retrieves the full response information 
		/// together with the actual result.
		/// </summary>
		public static IHttpEntityQueryResponse<T> ToQueryResponse<T>(this IQueryable<T> query)
		{
			if (!(query is IHttpEntityQuery<T>))
				throw new ArgumentException("Query is not an HTTP entity query.", "query");

			var httpQuery = (IHttpEntityQuery<T>)query;

			return httpQuery.Execute();
		}
	}
}
