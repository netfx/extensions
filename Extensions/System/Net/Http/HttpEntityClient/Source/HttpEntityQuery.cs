using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace System.Net.Http.Entity
{
	/// <summary>
	/// Represents a typed query to a REST service, that is executed 
	/// when the <see cref="Where"/> method is invoked with a predicate.
	/// </summary>
	internal class HttpEntityQuery<T>
	{
		private Func<Expression<Func<T, bool>>, IEnumerable<T>> invoker;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpEntityQuery&lt;T&gt;"/> class.
		/// </summary>
		public HttpEntityQuery(Func<Expression<Func<T, bool>>, IEnumerable<T>> invoker)
		{
			this.invoker = invoker;
		}

		/// <summary>
		/// Retrieves the matching entities from the service.
		/// </summary>
		public IEnumerable<T> Where(Expression<Func<T, bool>> predicate)
		{
			return this.invoker.Invoke(predicate);
		}
	}
}