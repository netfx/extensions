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
using System.Diagnostics;
using System.Diagnostics.Extensibility;

namespace NetFx.System.Diagnostics.Tracer
{
	public class SourceNameSpec
	{
		[Fact]
		public void WhenSimpleTypeName_ThenReturnsFullName()
		{
			var name = SourceName.For<ICloneable>();

			Assert.Equal("System.ICloneable", name);
		}

		[Fact]
		public void WhenGenericType_ThenReturnsCSharpFullName()
		{
			var name = SourceName.For<IComparable<string>>();

			Assert.Equal("System.IComparable<System.String>", name);
		}

		[Fact]
		public void WhenNestedGenericType_ThenReturnNestedCSharpFullName()
		{
			var name = SourceName.For<IDictionary<KeyValuePair<string, int>, IComparable<bool>>>();

			Assert.Equal("System.Collections.Generic.IDictionary<System.Collections.Generic.KeyValuePair<System.String,System.Int32>,System.IComparable<System.Boolean>>", name);
		}

		[Fact]
		public void WhenSimpleTypeName_ThenCompositeContainsAllSegmentsAndDefaultSource()
		{
			var names = SourceName.CompositeFor<SourceNameSpec>();

			Assert.Equal(6, names.Count());
		}

		[Fact]
		public void WhenGenericTypeName_ThenCompositeContainsSegmentsButNoSplitFromGenericArgument()
		{
			var names = SourceName.CompositeFor<IComparable<string>>();

			Assert.Equal(4, names.Count());
			Assert.True(names.Contains("System"));
			Assert.True(names.Contains("System.IComparable"));
			Assert.True(names.Contains("System.IComparable<System.String>"));
		}

		[Fact]
		public void WhenTypeNameHasNoNamespace_ThenCompositeContainsDefaultAndTypeName()
		{
			var names = SourceName.CompositeFor<SourceNameWithoutNamespace>();

			Assert.Equal(2, names.Count());
			Assert.True(names.Contains(SourceName.Default));
			Assert.True(names.Contains("SourceNameWithoutNamespace"));
		}
	}
}

public class SourceNameWithoutNamespace { }