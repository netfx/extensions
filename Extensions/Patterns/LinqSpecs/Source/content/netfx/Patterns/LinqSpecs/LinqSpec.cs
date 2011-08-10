using System;
using System.Linq;
using System.Linq.Expressions;
using Linq = System.Linq.Expressions;
using System.Collections.Generic;

/// <summary>
/// Allows creating and combining query specifications using logical And and Or 
/// operators.
/// </summary>
/// <nuget id="netfx-Patterns.LinqSpecs" />
static partial class LinqSpec
{
	/// <summary>
	/// Creates a custom ad-hoc <see cref="LinqSpec{T}"/> for the given <typeparamref name="T"/>.
	/// </summary>
	public static LinqSpec<T> For<T>(Expression<Func<T, bool>> specification)
	{
		return specification;
	}

	/// <summary>
	/// Converts the given expression to a linq query specification. Typically 
	/// not needed as the expression can be converted implicitly to a linq 
	/// specification by just assigning it or passing it as such to another method.
	/// </summary>
	public static LinqSpec<T> Spec<T>(this Expression<Func<T, bool>> specification)
	{
		return specification;
	}
}

/// <summary>
/// Base class for query specifications that can be combined using logical And and Or 
/// operators. For custom ad-hoc queries, use the static <see cref="LinqSpec.For{T}"/> method.
/// </summary>
abstract partial class LinqSpec<T>
{
	/// <summary>
	/// Gets the expression that defines this query. Typically accessing 
	/// this property is not needed as the query spec can be converted 
	/// implicitly to an expression by just assigning it or passing it as 
	/// such to another method.
	/// </summary>
	public abstract Expression<Func<T, bool>> Expression { get; }

	/// <summary>
	/// Allows to combine two query specifications using a logical And operation.
	/// </summary>
	public static LinqSpec<T> operator &(LinqSpec<T> spec1, LinqSpec<T> spec2)
	{
		return new AndSpec(spec1, spec2);
	}

	public static bool operator false(LinqSpec<T> spec1)
	{
		return false; // no-op. & and && do exactly the same thing.
	}

	public static bool operator true(LinqSpec<T> spec1)
	{
		return false; // no - op. & and && do exactly the same thing.
	}

	/// <summary>
	/// Allows to combine two query specifications using a logical Or operation.
	/// </summary>
	public static LinqSpec<T> operator |(LinqSpec<T> spec1, LinqSpec<T> spec2)
	{
		return new OrSpec(spec1, spec2);
	}

	/// <summary>
	/// Negates the given expression.
	/// </summary>
	public static LinqSpec<T> operator !(LinqSpec<T> spec1)
	{
		return new NegateSpec<T>(spec1);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="LinqSpec{T}"/> to a linq expression.
	/// </summary>
	public static implicit operator Expression<Func<T, bool>>(LinqSpec<T> spec)
	{
		return spec.Expression;
	}

	/// <summary>
	/// Performs an implicit conversion from a linq expression to <see cref="LinqSpec&lt;T&gt;"/>.
	/// </summary>
	public static implicit operator LinqSpec<T>(Expression<Func<T, bool>> expression)
	{
		return new AdHocSpec(expression);
	}

	/// <summary>
	/// The <c>And</c> specification.
	/// </summary>
	private class AndSpec : LinqSpec<T>, IEquatable<AndSpec>
	{
		private readonly Expression<Func<T, bool>> expression;
		private LinqSpec<T> spec1;
		private LinqSpec<T> spec2;

		public AndSpec(LinqSpec<T> spec1, LinqSpec<T> spec2)
		{
			this.spec1 = spec1;
			this.spec2 = spec2;

			// combines the expressions without the need for Expression.Invoke which fails on EntityFramework
			this.expression = spec1.Expression.And(spec2.Expression);
		}

		public override Expression<Func<T, bool>> Expression { get { return this.expression; } }

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(null, obj)) return false;
			if (Object.ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;

			return Equals((LinqSpec<T>.AndSpec)obj);
		}

		public override int GetHashCode()
		{
			return spec1.GetHashCode() ^ spec2.GetHashCode();
		}

		public bool Equals(LinqSpec<T>.AndSpec other)
		{
			return this.spec1.Equals(other.spec1) &&
				this.spec2.Equals(other.spec2);
		}
	}

	/// <summary>
	/// The <c>Or</c> specification.
	/// </summary>
	private class OrSpec : LinqSpec<T>, IEquatable<OrSpec>
	{
		private readonly Expression<Func<T, bool>> expression;
		private LinqSpec<T> spec1;
		private LinqSpec<T> spec2;

		public OrSpec(LinqSpec<T> spec1, LinqSpec<T> spec2)
		{
			this.spec1 = spec1;
			this.spec2 = spec2;
			this.expression = spec1.Expression.Or(spec2.Expression);
		}

		public override Expression<Func<T, bool>> Expression { get { return this.expression; } }

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(null, obj)) return false;
			if (Object.ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;

			return Equals((OrSpec)obj);
		}

		public override int GetHashCode()
		{
			return spec1.GetHashCode() ^ spec2.GetHashCode();
		}

		public bool Equals(OrSpec other)
		{
			return this.spec1.Equals(other.spec1) &&
				this.spec2.Equals(other.spec2);
		}

	}

	/// <summary>
	/// Negates the given query specification.
	/// </summary>
	private class NegateSpec<TArg> : LinqSpec<TArg>, IEquatable<NegateSpec<TArg>>
	{
		private readonly Expression<Func<TArg, bool>> expression;
		private LinqSpec<TArg> spec;

		public NegateSpec(LinqSpec<TArg> spec)
		{
			this.spec = spec;
			this.expression = Linq.Expression.Lambda<Func<TArg, bool>>(
				Linq.Expression.Not(spec.Expression.Body), spec.Expression.Parameters);
		}

		public override Expression<Func<TArg, bool>> Expression { get { return this.expression; } }

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(null, obj)) return false;
			if (Object.ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;

			return Equals((LinqSpec<T>.NegateSpec<TArg>)obj);
		}

		public override int GetHashCode()
		{
			return spec.GetHashCode();
		}

		public bool Equals(LinqSpec<T>.NegateSpec<TArg> other)
		{
			return this.spec.Equals(other.spec);
		}
	}

	private class AdHocSpec : LinqSpec<T>, IEquatable<AdHocSpec>
	{
		private readonly Expression<Func<T, bool>> specification;

		public AdHocSpec(Expression<Func<T, bool>> specification)
		{
			this.specification = specification;
		}

		public override Expression<Func<T, bool>> Expression { get { return this.specification; } }

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(null, obj)) return false;
			if (Object.ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;

			return Equals((AdHocSpec)obj);
		}

		public override int GetHashCode()
		{
			return this.specification.GetHashCode();
		}

		public bool Equals(AdHocSpec other)
		{
			return this.specification.Equals(other.specification);
		}
	}
}