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

namespace NetFx.StringlyTypedSpec
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class GivenStringlyTypes
    {
        [Fact]
        public void WhenClassContainsGenerics_ThenCovertsToSimpleCodeName()
        {
            var name = typeof(Dictionary<string, List<KeyValuePair<string, IEnumerable<bool>>>>).ToCodeName();

            Assert.Equal("Dictionary<String, List<KeyValuePair<String, IEnumerable<Boolean>>>>", name);
        }

        [Fact]
        public void WhenClassContainsGenerics_ThenCovertsToFullCodeName()
        {
            var name = typeof(Dictionary<string, List<KeyValuePair<string, IEnumerable<bool>>>>).ToCodeFullName();

            Assert.Equal("System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<System.String, System.Collections.Generic.IEnumerable<System.Boolean>>>>", name);
        }

        [Fact]
        public void WhenNestedTypeToCodeName_ThenReplacesPlusWithDot()
        {
            var name = typeof(NestedType).ToCodeName();

            Assert.Equal("GivenStringlyTypes.NestedType", name);
        }

        [Fact]
        public void WhenNestedTypeToCodeFullName_ThenReplacesPlusWithDot()
        {
            var name = typeof(NestedType).ToCodeFullName();

            Assert.Equal("NetFx.StringlyTypedSpec.GivenStringlyTypes.NestedType", name);
        }

        [Fact]
        public void WhenOpenGenericNestedTypeToCodeName_ThenReplacesPlusWithDot()
        {
            var name = typeof(NestedGeneric<>).ToCodeName();

            Assert.Equal("GivenStringlyTypes.NestedGeneric<T>", name);
        }

        [Fact]
        public void WhenGenericNestedTypeToCodeName_ThenReplacesPlusWithDot()
        {
            var name = typeof(NestedGeneric<NestedType>).ToCodeName();

            Assert.Equal("GivenStringlyTypes.NestedGeneric<GivenStringlyTypes.NestedType>", name);
        }

        [Fact]
        public void WhenNestedOpenGenericTypeToCodeFullName_ThenReplacesPlusWithDot()
        {
            var name = typeof(NestedGeneric<>).ToCodeFullName();

            Assert.Equal("NetFx.StringlyTypedSpec.GivenStringlyTypes.NestedGeneric<T>", name);
        }

        [Fact]
        public void WhenGenericNestedTypeToCodeFullName_ThenReplacesPlusWithDot()
        {
            var name = typeof(NestedGeneric<NestedType>).ToCodeFullName();

            Assert.Equal("NetFx.StringlyTypedSpec.GivenStringlyTypes.NestedGeneric<NetFx.StringlyTypedSpec.GivenStringlyTypes.NestedType>", name);
        }

        [Fact]
        public void WhenIsGenericDefinition_ThenGetsTs()
        {
            var type = typeof(Dictionary<,>);
            var name = type.ToCodeName();

            Assert.Equal("Dictionary<TKey, TValue>", name);
        }

        [Fact]
        public void WhenAddingTypesToScopeWithNoNamespace_ThenSucceeds()
        {
            var scope = StringlyTyped.BeginScope();
            scope.AddType("Foo");
            scope.AddType("Bar");

            var context = scope.Build();

            Assert.Equal("Foo", context.GetCodeName("Foo"));
            Assert.Equal("Bar", context.GetCodeName("Bar"));
        }

        [Fact]
        public void WhenSimplifyingGenericType_ThenAddsUsingsAndSimplifiesGenericParameterType()
        {
            var scope = StringlyTyped.BeginScope();
            scope.AddType(typeof(IEnumerable<NonNestedType>));
            var context = scope.Build();

            Assert.Equal("IEnumerable<NonNestedType>", context.GetCodeName(typeof(IEnumerable<NonNestedType>)));
            Assert.True(context.SafeImports.Contains(typeof(IEnumerable<>).Namespace));
            Assert.True(context.SafeImports.Contains(typeof(NonNestedType).Namespace));
        }

        [Fact]
        public void WhenSimplifyingOpenGenericType_ThenRendersValidCSharp()
        {
            var scope = StringlyTyped.BeginScope();
            scope.AddType(typeof(IEnumerable<>));
            var context = scope.Build();

            Assert.Equal("IEnumerable<>", context.GetCodeName(typeof(IEnumerable<>)));
            Assert.True(context.SafeImports.Contains(typeof(IEnumerable<>).Namespace));
        }

        [Fact]
        public void WhenSimplifyingGenericTypeWithNestedTypeParameter_ThenRemovesPlusFromNestedTypeName()
        {
            var scope = StringlyTyped.BeginScope();
            scope.AddType(typeof(IEnumerable<NestedType>));
            var context = scope.Build();

            Assert.Equal("IEnumerable<GivenStringlyTypes.NestedType>", context.GetCodeName(typeof(IEnumerable<NestedType>)));
            Assert.True(context.SafeImports.Contains(typeof(IEnumerable<>).Namespace));
            Assert.True(context.SafeImports.Contains(typeof(NestedType).Namespace));
            Assert.False(context.SafeImports.Contains(typeof(GivenStringlyTypes).FullName), "The nested type parent should not be mistaken for a namespace.");
        }

        [Fact]
        public void WhenSimplifyingGenericTypeWithCollidingParameter_ThenKeepsParameterFullName()
        {
            var scope = StringlyTyped.BeginScope();
            scope.AddType(typeof(IEnumerable<StringConverter>));
            scope.AddType(typeof(System.ComponentModel.StringConverter));
            var context = scope.Build();

            Assert.Equal("IEnumerable<NetFx.StringlyTypedSpec.StringConverter>", context.GetCodeName(typeof(IEnumerable<StringConverter>)));
            Assert.True(context.SafeImports.Contains(typeof(IEnumerable<>).Namespace));
            Assert.False(context.SafeImports.Contains(typeof(StringConverter).Namespace));
        }

        [Fact]
        public void WhenSimplifyingAllCoreLib_ThenAddsUsingForGenericsAndNonGenericComparable()
        {
            var scope = StringlyTyped.BeginScope();
            scope.AddTypes(typeof(string).Assembly);
            scope.AddTypes(new[] { typeof(IComparable), typeof(IComparable<string>) });
            var context = scope.Build();

            Assert.Equal("IComparable<String>", context.GetCodeName(typeof(IComparable<string>)));
            Assert.Equal("IComparable", context.GetCodeName(typeof(IComparable)));
            Assert.True(context.SafeImports.Contains(typeof(IComparable<>).Namespace));
        }

        [Fact]
        public void WhenGettingTypeNameOfTypeNotAdded_ThenReturnsValidCodeName()
        {
            var scope = StringlyTyped.BeginScope();
            var context = scope.Build();

            Assert.Equal("System.Lazy<System.String>", context.GetCodeName(typeof(Lazy<string>)));
        }

        [Fact]
        public void WhenGettingTypeNameOfTypeNotAddedWithAddedGeneric_ThenReturnsSimplifiedGenericParameter()
        {
            var scope = StringlyTyped.BeginScope();
            scope.AddType(typeof(string));
            scope.AddType(typeof(bool));
            var context = scope.Build();

            Assert.Equal("System.Lazy<String>", context.GetCodeName(typeof(Lazy<string>)));
            Assert.Equal("System.Lazy<System.Collections.Generic.KeyValuePair<String, Boolean>>", context.GetCodeName(typeof(Lazy<KeyValuePair<string, bool>>)));
        }

        [Fact]
        public void WhenAddingOpenGeneric_ThenSimplifiesConcreteGeneric()
        {
            var scope = StringlyTyped.BeginScope();
            scope.AddType(typeof(Lazy<>));
            var context = scope.Build();

            Assert.Equal("Lazy<System.String>", context.GetCodeName(typeof(Lazy<string>)));
        }

        [Fact]
        public void WhenAddingAssembly_ThenSafeUsingsDoNotContainGenerics()
        {
            var scope = StringlyTyped.BeginScope();
            scope.AddType(typeof(IEnumerable<string>));
            scope.AddType(typeof(IEnumerable));
            var context = scope.Build();

            Assert.False(context.SafeImports.Any(s => s.IndexOf('[') != -1));
        }

        [Fact]
        public void WhenSimplifyingTwoGenerics_ThenSimplifiesAllParameters()
        {
            var scope = StringlyTyped.BeginScope();
            var type = typeof(IList<KeyValuePair<string, StringConverter>>);

            scope.AddType(type);
            scope.AddType(typeof(System.ComponentModel.StringConverter));

            var context = scope.Build();

            Assert.Equal("IList<KeyValuePair<String, NetFx.StringlyTypedSpec.StringConverter>>", context.GetCodeName(type));
            Assert.True(context.SafeImports.Contains(typeof(IEnumerable<>).Namespace));
        }

        [Fact]
        public void WhenSimplifyingMultipleGenerics_ThenSimplifiesAllParameters()
        {
            var scope = StringlyTyped.BeginScope();
            var type = typeof(IDictionary<IList<KeyValuePair<string, StringConverter>>, NestedType>);

            scope.AddType(type);
            scope.AddType(typeof(System.ComponentModel.StringConverter));

            var context = scope.Build();

            Assert.Equal("IDictionary<IList<KeyValuePair<String, NetFx.StringlyTypedSpec.StringConverter>>, GivenStringlyTypes.NestedType>", context.GetCodeName(type));
            Assert.True(context.SafeImports.Contains(typeof(IEnumerable<>).Namespace));
            Assert.True(context.SafeImports.Contains(typeof(GivenStringlyTypes).Namespace));
        }

        [Fact]
        public void WhenSimplifyingTypeMap_ThenOnlySimplifiesNonCollidingTypeNames()
        {
            var scope = StringlyTyped.BeginScope();

            scope.AddType("Foo.A");
            scope.AddType("Foo.B");
            scope.AddType("Bar.A");

            var context = scope.Build();

            Assert.Equal("Foo.A", context.GetCodeName("Foo.A"));
            Assert.Equal("B", context.GetCodeName("Foo.B"));
            Assert.Equal("Bar.A", context.GetCodeName("Bar.A"));
        }

        [Fact]
        public void WhenSimplifyingTypeMap_ThenUniqueTypeNamesAreSimplified()
        {
            var scope = StringlyTyped.BeginScope();

            scope.AddType("Foo.A");
            scope.AddType("Bar.B");

            var context = scope.Build();

            Assert.Equal("A", context.GetCodeName("Foo.A"));
            Assert.Equal("B", context.GetCodeName("Bar.B"));
        }

        [Fact]
        public void WhenGettingSafeUsings_ThenOnlyGetsNamespacesFromSimplifiedTypeNames()
        {
            var scope = StringlyTyped.BeginScope();

            scope.AddType("Foo.A");
            scope.AddType("Foo.B");
            scope.AddType("Bar.A");
            scope.AddType("Baz.C");

            var context = scope.Build();

            Assert.True(context.SafeImports.Contains("Foo"));
            Assert.True(context.SafeImports.Contains("Baz"));
            Assert.False(context.SafeImports.Contains("Bar"));
        }

        [Fact]
        public void WhenSimplifyingAssemblyQualifiedName_ThenAddsUsingAndSimplifiesTypeName()
        {
            var scope = StringlyTyped.BeginScope();
            scope.AddType("Foo.Bar, Foo");
            var context = scope.Build();

            Assert.Equal("Bar", context.GetCodeName("Foo.Bar, Foo"));
            Assert.True(context.SafeImports.Contains("Foo"));
        }

        public class NestedType { }
        public class NestedGeneric<T> { }
    }

    public class NonNestedType { }
    public class StringConverter { }
}