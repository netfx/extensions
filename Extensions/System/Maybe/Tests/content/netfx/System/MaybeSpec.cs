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
using Xunit;

namespace NetFx.System
{
	public class MaybeSpec
	{
		public class Empty
		{
			[Fact]
			public void is_singleton()
			{
				Assert.Same(Maybe<String>.Empty, Maybe<String>.Empty);
			}
		}

		public class From
		{
			[Fact]
			public void simple()
			{
				Maybe<String> ms = Maybe<String>.From("value");
				Assert.True(ms.HasValue);
				Assert.Equal("value", ms.GetValueOrDefault());
			}

			[Fact]
			public void when_called_with_null_param_then_returns_Empty()
			{
				Maybe<String> ms = Maybe<String>.From(null);
				Assert.False(ms.HasValue);
				Assert.Same(Maybe<String>.Empty, ms);
			}
		}

		public class From_Nullable_T
		{
			[Fact]
			public void simple()
			{
				Maybe<int> ms = Maybe.From((int?)3);
				Assert.True(ms.HasValue);
				Assert.Equal(3, ms.GetValueOrDefault());
			}

			[Fact]
			public void when_called_with_null_param_then_returns_Empty()
			{
				Maybe<int> ms = Maybe.From<int>((int?)null);
				Assert.False(ms.HasValue);
				Assert.Same(Maybe<int>.Empty, ms);
			}
		}


		public class Bind
		{
			[Fact]
			public void throws_when_project_is_null()
			{
				Maybe<String> ms = Maybe.From("3");
				Assert.Throws<ArgumentNullException>(() =>
				{
					Maybe<String> ms2 = ms.Bind((Func<String, Maybe<String>>)null);
				});
			}

			[Fact]
			public void string_to_string()
			{
				Maybe<String> ms = Maybe.From("3");
				Maybe<String> ms2 = ms.Bind(t => Maybe.From(t + "_"));
				Assert.True(ms2.HasValue);
				Assert.Equal("3_", ms2.GetValueOrDefault());
			}

			[Fact]
			public void int_to_string()
			{
				Maybe<int> mi = Maybe.From(3);
				Maybe<String> ms = mi.Bind(i => Maybe.From(i.ToString()));
				Assert.Equal("3", ms.GetValueOrDefault());
			}

			[Fact]
			public void Nullable_int_to_string()
			{
				Maybe<int?> mi = Maybe.From<int?>((int?)3);
				Maybe<String> ms = mi.Bind(i => Maybe.From(i.ToString()));
				Assert.Equal("3", ms.GetValueOrDefault());
			}

			[Fact]
			public void Nullable_int_null_to_string()
			{
				Maybe<int?> mi = Maybe.From<int?>((int?)null);
				Maybe<String> ms = mi.Bind(i => Maybe.From(i.ToString()));
				Assert.Equal(Maybe<String>.Empty, ms);
			}
		}

		public class Map
		{

			[Fact]
			public void throws_when_project_is_null()
			{
				Maybe<String> ms = Maybe.From("3");
				Assert.Throws<ArgumentNullException>(() =>
				{
					Maybe<String> ms2 = ms.Map((Func<String, String>)null);
				});

			}

			[Fact]
			public void simple()
			{
				Maybe<String> ms = Maybe.From("3");
				Maybe<int> mi = ms.Map(s => int.Parse(s));
				Assert.Equal(3, mi.GetValueOrDefault());
			}

			[Fact]
			public void chain()
			{
				Maybe<int?> ms = Maybe.From<int?>(null);
				var result = 
					ms.Map<int>(i => (int)i)
					.Map(i => i.ToString())
					.Map(s => int.Parse(s));

				Assert.Equal(Maybe<int>.Empty, result);
			}

		}
		
		public class Select
		{
			[Fact]
			public void works_with_query_comprehension_syntax()
			{
				var maybe =
					from m in Maybe.From("value")
					select m + "1";

				Assert.Equal("value1", maybe.GetValueOrDefault());
			}
		}

		public class As
		{
			[Fact]
			public void when_not_Empty_and_cast_is_legal_then_succeeds()
			{
				Maybe<Bar> maybe = Maybe.From(new Bar());
				Maybe<Foo> maybeFoo = maybe.As<Foo>();
				Assert.NotNull(maybeFoo.GetValueOrDefault());
			}

			[Fact]
			public void when_not_Empty_and_cast_is_not_legal_then_throws()
			{
				var maybe = Maybe.From(new Foo());
				Assert.Throws<InvalidCastException>(() =>
				{
					var maybeBar = maybe.As<Bar>();
				});
			}

			[Fact]
			public void when_Empty_then_cast_always_succeeds()
			{
				var maybe = Maybe<Foo>.From(null);
				var maybeBar = maybe.As<Bar>();
				Assert.Same(Maybe<Bar>.Empty, maybeBar);
			}

			public class Foo
			{

			}

			public class Bar : Foo
			{

			}
		}

		public class Coalesce
		{
			[Fact]
			public void when_this_not_Empty_then_returns_this()
			{
				var m = Maybe.From("value");
				var m2 = m.Coalesce(Maybe.From("that"));
				Assert.Equal("value", m2.GetValueOrDefault());
			}

			[Fact]
			public void when_this_Empty_then_returns_other()
			{
				var m = Maybe<String>.Empty;
				var m2 = m.Coalesce(Maybe.From("that"));
				Assert.Equal("that", m2.GetValueOrDefault());
			}


			[Fact]
			public void when_this_and_other_are_Empty_then_returns_Empty()
			{
				var m = Maybe<String>.Empty;
				var m2 = m.Coalesce(Maybe<String>.Empty);
				Assert.Equal(Maybe<String>.Empty, m2);
			}

			[Fact]
			public void throws_when_other_is_null()
			{
				var m = Maybe<String>.Empty;
				Assert.Throws<ArgumentNullException>(() =>
				{
					var m2 = m.Coalesce(null);
				});
			}


		}

		public class SelectMany
		{
			[Fact]
			public void works_with_query_comprehension_syntax()
			{
				var maybe =
					from s1 in Maybe.From((string)null)
					from s2 in Maybe.From("value")
					select s1 ?? s2;

				Assert.Equal(null, maybe.GetValueOrDefault());
			}

			[Fact]
			public void when_s1_is_Empty_then_result_is_Empty()
			{
				var maybe =
					from s1 in Maybe.From((string)null)
					from s2 in Maybe.From("value")
					select s1 ?? s2;

				Assert.Equal(null, maybe.GetValueOrDefault());
			}


			[Fact]
			public void when_s2_is_Empty_then_result_is_Empty()
			{
				var maybe =
					from s1 in Maybe.From("value")
					from s2 in Maybe.From((string)null)
					select s1 ?? s2;

				Assert.Equal(null, maybe.GetValueOrDefault());
			}

			[Fact]
			public void when_neither_are_Empty_then_result_is_not_Empty()
			{
				var maybe =
					from s1 in Maybe.From("value")
					from s2 in Maybe.From("value2")
					select s1 ?? s2;

				Assert.Equal("value", maybe.GetValueOrDefault());
			}


		}

		public class GetValueOrDefault
		{
			[Fact]
			public void when_has_value_then_returns_value()
			{
				var m = Maybe.From("value");
				Assert.Equal("value", m.GetValueOrDefault());
			}

			[Fact]
			public void when_has_no_value_then_returns_default()
			{
				var m = Maybe<String>.Empty;
				Assert.Equal(null, m.GetValueOrDefault());
			}
		}

		public class GetValueOrDefault_T
		{
			[Fact]
			public void when_has_value_then_returns_value()
			{
				var m = Maybe.From("value");
				Assert.Equal("value", m.GetValueOrDefault("my default value"));
			}

			[Fact]
			public void when_has_no_value_then_returns_default()
			{
				var m = Maybe<String>.Empty;
				Assert.Equal("my default value", m.GetValueOrDefault("my default value"));
			}
		}

		public class Equals_MaybeT
		{
			[Fact]
			public void empty_equals_empty()
			{
				Assert.True(Maybe<String>.Empty.Equals(Maybe<String>.Empty));
			}

			[Fact]
			public void equivalent_amplified_values_are_equivalent()
			{
				var m1 = Maybe.From(1);
				var m2 = Maybe.From(1);

				Assert.True(m1.Equals(m2));
				Assert.True(m2.Equals(m1));
			}

			[Fact]
			public void non_equivalent_amplified_values_are_not_equivalent()
			{
				var m1 = Maybe.From(1);
				var m2 = Maybe.From(2);

				Assert.False(m1.Equals(m2));
				Assert.False(m2.Equals(m1));
			}

			[Fact]
			public void non_Empty_amplified_value_not_equal_to_Empty()
			{
				var m1 = Maybe.From(1);
				Assert.False(m1.Equals(Maybe<int>.Empty));
				Assert.False(Maybe<int>.Empty.Equals(m1));
			}

			[Fact]
			public void non_null_not_equal_to_actual_null()
			{
				var m1 = Maybe.From(1);
				Assert.False(m1.Equals((Maybe<int>)null));
				Assert.False(Maybe<int>.Empty.Equals((Maybe<int>)null));
			}
		}

		public class Equals_object
		{
			[Fact]
			public void empty_equals_empty()
			{
				Assert.True(Maybe<String>.Empty.Equals((object)Maybe<String>.Empty));
			}

			[Fact]
			public void equivalent_amplified_values_are_equivalent()
			{
				var m1 = Maybe.From(1);
				var m2 = Maybe.From(1);

				Assert.True(m1.Equals((object)m2));
				Assert.True(m2.Equals((object)m1));
			}

			[Fact]
			public void non_equivalent_amplified_values_are_not_equivalent()
			{
				var m1 = Maybe.From(1);
				var m2 = Maybe.From(2);

				Assert.False(m1.Equals((object)m2));
				Assert.False(m2.Equals((object)m1));
			}

			[Fact]
			public void non_Empty_amplified_value_not_equal_to_Empty()
			{
				var m1 = Maybe.From(1);
				Assert.False(m1.Equals((object)Maybe<int>.Empty));
				Assert.False(Maybe<int>.Empty.Equals((object)m1));
			}

			[Fact]
			public void non_null_not_equal_to_actual_null()
			{
				var m1 = Maybe.From(1);
				Assert.False(m1.Equals((object)null));
				Assert.False(Maybe<int>.Empty.Equals((object)null));
			}

		}

		public class EqualsOperator_MaybeT_MaybeT
		{
			[Fact]
			public void empty_equals_empty()
			{
				Assert.True(Maybe<String>.Empty == Maybe<String>.Empty);
			}

			[Fact]
			public void equivalent_amplified_values_are_equivalent()
			{
				var m1 = Maybe.From(1);
				var m2 = Maybe.From(1);

				Assert.True(m1 == m2);
				Assert.True(m2 == m1);
			}

			[Fact]
			public void non_equivalent_amplified_values_are_not_equivalent()
			{
				var m1 = Maybe.From(1);
				var m2 = Maybe.From(2);

				Assert.False(m1 == m2);
				Assert.False(m2 == m1);
			}

			[Fact]
			public void non_Empty_amplified_value_not_equal_to_Empty()
			{
				var m1 = Maybe.From(1);
				Assert.False(m1 == Maybe<int>.Empty);
				Assert.False(Maybe<int>.Empty == m1);
			}

			[Fact]
			public void non_null_not_equal_to_actual_null()
			{
				var m1 = Maybe.From(1);
				Assert.False(m1 == (Maybe<int>)null);
				Assert.False(Maybe<int>.Empty == (Maybe<int>)null);
			}

			[Fact]
			public void null_equals_null()
			{
				Assert.True(((Maybe<String>)null) == ((Maybe<String>)null));
			}
		}

		public class EqualsOperator_MaybeT_Object
		{
			[Fact]
			public void empty_equals_empty()
			{
				Assert.True(Maybe<String>.Empty == (object)Maybe<String>.Empty);
			}

			[Fact]
			public void equivalent_amplified_values_are_equivalent()
			{
				var m1 = Maybe.From(1);
				var m2 = Maybe.From(1);

				Assert.True(m1 == (object)m2);
				Assert.True(m2 == (object)m1);
			}

			[Fact]
			public void non_equivalent_amplified_values_are_not_equivalent()
			{
				var m1 = Maybe.From(1);
				var m2 = Maybe.From(2);

				Assert.False(m1 == (object)m2);
				Assert.False(m2 == (object)m1);
			}

			[Fact]
			public void non_Empty_amplified_value_not_equal_to_Empty()
			{
				var m1 = Maybe.From(1);
				Assert.False(m1 == (object)Maybe<int>.Empty);
				Assert.False(Maybe<int>.Empty == (object)m1);
			}

			[Fact]
			public void non_null_not_equal_to_actual_null()
			{
				var m1 = Maybe.From(1);
				Assert.False(m1 == (object)null);
				Assert.False(Maybe<int>.Empty == (object)null);
			}

			[Fact]
			public void null_equals_null()
			{
				Assert.True(((Maybe<String>)null) == ((object)null));
			}
		}


		public class EqualsOperator_Object_MaybeT
		{
			[Fact]
			public void empty_equals_empty()
			{
				Assert.True(((object)Maybe<String>.Empty) == Maybe<String>.Empty);
			}

			[Fact]
			public void equivalent_amplified_values_are_equivalent()
			{
				var m1 = Maybe.From(1);
				var m2 = Maybe.From(1);

				Assert.True(((object)m1) == m2);
				Assert.True(((object)m2) == m1);
			}

			[Fact]
			public void non_equivalent_amplified_values_are_not_equivalent()
			{
				var m1 = Maybe.From(1);
				var m2 = Maybe.From(2);

				Assert.False(((object)m1) == m2);
				Assert.False(((object)m2) == m1);
			}

			[Fact]
			public void non_Empty_amplified_value_not_equal_to_Empty()
			{
				var m1 = Maybe.From(1);
				Assert.False(((object)m1) == (Maybe<int>.Empty));
				Assert.False(Maybe<int>.Empty == (object)m1);
			}

			[Fact]
			public void non_null_not_equal_to_actual_null()
			{
				var m1 = Maybe.From(1);
				Assert.False(null == m1);
				Assert.False(null == Maybe<int>.Empty);
			}

			[Fact]
			public void null_equals_null()
			{
				Assert.True(null == ((Maybe<String>)null));
			}
		}


	}
}