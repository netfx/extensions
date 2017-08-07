﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Linq = System.Linq.Expressions;
using System.Collections.Generic;

/// <summary>
/// Allows creating and combining query specifications using logical And and Or 
/// operators.
/// </summary>
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
	/// <typeparam name="T">Type of specification value being constrained.</typeparam>
	/// <param name="specification" this="true">The specification expression to convert to a spec.</param>
	/// <nuget id="netfx-Patterns.LinqSpecs"/>
	public static LinqSpec<T> Spec<T>(this Expression<Func<T, bool>> specification)
	{
		return specification;
	}

	/// <summary>
	/// Allows reusing a specification for another Type by applying it to a property
	/// of the Type being targetted by the new specification.
	/// </summary>
	/// <typeparam name="T">The type of the new LinqSpec to be created</typeparam>
	/// <typeparam name="TNestedEntity">The type of the property on the T which is being accessed</typeparam>
	/// <param name="propertyExpression">The property expression to access property on T. Usually a lambda like t => t.MyProp</param>
	/// <param name="spec">A LinqSpec for type TNestedEntity</param>
	public static LinqSpec<T> OnProperty<T, TNestedEntity>(Expression<Func<T, TNestedEntity>> propertyExpression, LinqSpec<TNestedEntity> spec)
	{
		var replacer = new ParameterReplaceVisitor(spec.Expression.Parameters.First(), propertyExpression.Body);
		var newExpression = replacer.Visit(spec.Expression.Body);
		var exp = Expression.Lambda<Func<T, bool>>(newExpression, propertyExpression.Parameters);
		return For(exp);
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
	/// The classic specification pattern evaluator.
	/// This compiles and invokes the expression from the LinqSpec
	/// so that is can be evaluated in code rather than passed to LinqToX.
	/// If you find yourself doing this frequently you may want to consider
	/// using a non-Linq specification which does not store the rule as an
	/// expression, but as a compiled method instead.
	/// </summary>
	/// <param name="entity">The T under test</param>
	public bool IsSatisfiedBy(T entity)
	{
		return Expression.Compile().Invoke(entity);
	}

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