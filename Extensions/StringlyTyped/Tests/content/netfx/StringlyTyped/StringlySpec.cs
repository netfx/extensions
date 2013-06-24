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

namespace NetFx.StringlyTyped
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class StringlySpec
    {
        [Fact]
        public void WhenClassContainsGenerics_ThenCovertsToSimpleCodeName()
        {
            var name = typeof(Dictionary<string, List<KeyValuePair<string, IEnumerable<bool>>>>).ToTypeName();

            Assert.Equal("Dictionary<String, List<KeyValuePair<String, IEnumerable<Boolean>>>>", name);
        }

        [Fact]
        public void WhenClassContainsGenerics_ThenCovertsToFullCodeName()
        {
            var name = typeof(Dictionary<string, List<KeyValuePair<string, IEnumerable<bool>>>>).ToTypeFullName();

            Assert.Equal("System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<System.String, System.Collections.Generic.IEnumerable<System.Boolean>>>>", name);
        }

        [Fact]
        public void WhenNestedTypeToTypeName_ThenReplacesPlusWithDot()
        {
            var name = typeof(NestedType).ToTypeName();

            Assert.Equal("StringlySpec.NestedType", name);
        }

        [Fact]
        public void WhenNestedTypeToTypeFullName_ThenReplacesPlusWithDot()
        {
            var name = typeof(NestedType).ToTypeFullName();

            Assert.Equal("NetFx.StringlyTyped.StringlySpec.NestedType", name);
        }

        [Fact]
        public void WhenTwoDeclaringOpenGenericWithNestedGenericTypeToTypeFullName_ThenReplacesGenericArgumentsWithNames()
        {
            var name = typeof(TwoGeneric<,>.NestedTwoGeneric<,>).ToTypeFullName();

            Assert.Equal("NetFx.StringlyTyped.TwoGeneric<T, I>.NestedTwoGeneric<R, S>", name);
        }

        [Fact]
        public void WhenTwoDeclaringGenericWithNestedGenericTypeToTypeFullName_ThenReplacesArgumentsWithFullNames()
        {
            var name = typeof(TwoGeneric<string, int>.NestedTwoGeneric<bool, int>).ToTypeFullName();

            Assert.Equal("NetFx.StringlyTyped.TwoGeneric<System.String, System.Int32>.NestedTwoGeneric<System.Boolean, System.Int32>", name);
        }

        [Fact]
        public void WhenTwoDeclaringGenericWithNestedGenericTypeToTypeName_ThenReplacesArgumentsWithNames()
        {
            var name = typeof(TwoGeneric<string, int>.NestedTwoGeneric<bool, int>).ToTypeName();

            Assert.Equal("TwoGeneric<String, Int32>.NestedTwoGeneric<Boolean, Int32>", name);
        }

        [Fact]
        public void WhenOpenGenericNestedTypeToTypeName_ThenReplacesPlusWithDot()
        {
            var name = typeof(NestedGeneric<>).ToTypeName();

            Assert.Equal("StringlySpec.NestedGeneric<T>", name);
        }

        [Fact]
        public void WhenGenericNestedTypeToTypeName_ThenReplacesPlusWithDot()
        {
            var name = typeof(NestedGeneric<NestedType>).ToTypeName();

            Assert.Equal("StringlySpec.NestedGeneric<StringlySpec.NestedType>", name);
        }

        [Fact]
        public void WhenNestedOpenGenericTypeToTypeFullName_ThenReplacesPlusWithDot()
        {
            var name = typeof(NestedGeneric<>).ToTypeFullName();

            Assert.Equal("NetFx.StringlyTyped.StringlySpec.NestedGeneric<T>", name);
        }

        [Fact]
        public void WhenGenericNestedTypeToTypeFullName_ThenReplacesPlusWithDot()
        {
            var name = typeof(NestedGeneric<NestedType>).ToTypeFullName();

            Assert.Equal("NetFx.StringlyTyped.StringlySpec.NestedGeneric<NetFx.StringlyTyped.StringlySpec.NestedType>", name);
        }

        [Fact]
        public void WhenIsGenericDefinition_ThenGetsTs()
        {
            var type = typeof(Dictionary<,>);
            var name = type.ToTypeName();

            Assert.Equal("Dictionary<TKey, TValue>", name);
        }

        [Fact]
        public void WhenAddingTypesToScopeWithNoNamespace_ThenSucceeds()
        {
            var scope = Stringly.BeginScope();
            scope.AddType("Foo");
            scope.AddType("Bar");

            Assert.Equal("Foo", scope.GetTypeName("Foo"));
            Assert.Equal("Bar", scope.GetTypeName("Bar"));
        }

        [Fact]
        public void WhenSimplifyingGenericType_ThenAddsUsingsAndSimplifiesGenericParameterType()
        {
            var scope = Stringly.BeginScope();
            scope.AddType(typeof(IEnumerable<NonNestedType>));

            Assert.Equal("IEnumerable<NonNestedType>", scope.GetTypeName(typeof(IEnumerable<NonNestedType>)));
            Assert.True(scope.SafeImports.Contains(typeof(IEnumerable<>).Namespace));
            Assert.True(scope.SafeImports.Contains(typeof(NonNestedType).Namespace));
        }

        [Fact]
        public void WhenSimplifyingOpenGenericType_ThenRendersValidCSharp()
        {
            var scope = Stringly.BeginScope();
            scope.AddType(typeof(IEnumerable<>));

            Assert.Equal("IEnumerable<>", scope.GetTypeName(typeof(IEnumerable<>)));
            Assert.True(scope.SafeImports.Contains(typeof(IEnumerable<>).Namespace));
        }

        [Fact]
        public void WhenSimplifyingOpenGenericTypeWithTwoParameters_ThenRendersValidCSharp()
        {
            var scope = Stringly.BeginScope();
            scope.AddType(typeof(IDictionary<,>));

            Assert.Equal("IDictionary<,>", scope.GetTypeName(typeof(IDictionary<,>)));
            Assert.True(scope.SafeImports.Contains(typeof(IDictionary<,>).Namespace));
        }

        [Fact]
        public void WhenSimplifyingGenericTypeWithNestedTypeParameter_ThenRemovesPlusFromNestedTypeName()
        {
            var scope = Stringly.BeginScope();
            scope.AddType(typeof(IEnumerable<NestedType>));

            Assert.Equal("IEnumerable<StringlySpec.NestedType>", scope.GetTypeName(typeof(IEnumerable<NestedType>)));
            Assert.True(scope.SafeImports.Contains(typeof(IEnumerable<>).Namespace));
            Assert.True(scope.SafeImports.Contains(typeof(NestedType).Namespace));
            Assert.False(scope.SafeImports.Contains(typeof(StringlySpec).FullName), "The nested type parent should not be mistaken for a namespace.");
        }

        [Fact]
        public void WhenSimplifyingGenericTypeWithCollidingParameter_ThenKeepsParameterFullName()
        {
            var scope = Stringly.BeginScope();
            scope.AddType(typeof(IEnumerable<StringConverter>));
            scope.AddType(typeof(System.ComponentModel.StringConverter));

            Assert.Equal("IEnumerable<NetFx.StringlyTyped.StringConverter>", scope.GetTypeName(typeof(IEnumerable<StringConverter>)));
            Assert.True(scope.SafeImports.Contains(typeof(IEnumerable<>).Namespace));
            Assert.False(scope.SafeImports.Contains(typeof(StringConverter).Namespace));
        }

        [Fact]
        public void WhenSimplifyingAllCoreLib_ThenAddsUsingForGenericsAndNonGenericComparable()
        {
            var scope = Stringly.BeginScope();
            scope.AddTypes(typeof(string).Assembly);
            scope.AddTypes(new[] { typeof(IComparable), typeof(IComparable<string>) });

            Assert.Equal("IComparable<String>", scope.GetTypeName(typeof(IComparable<string>)));
            Assert.Equal("IComparable", scope.GetTypeName(typeof(IComparable)));
            Assert.True(scope.SafeImports.Contains(typeof(IComparable<>).Namespace));
        }

        [Fact]
        public void WhenGettingTypeNameOfTypeNotAdded_ThenReturnsValidCodeName()
        {
            var scope = Stringly.BeginScope();

            Assert.Equal("System.Lazy<System.String>", scope.GetTypeName(typeof(Lazy<string>)));
        }

        [Fact]
        public void WhenGettingTypeNameOfTypeNotAddedWithAddedGeneric_ThenReturnsSimplifiedGenericParameter()
        {
            var scope = Stringly.BeginScope();
            scope.AddType(typeof(string));
            scope.AddType(typeof(bool));

            Assert.Equal("System.Lazy<String>", scope.GetTypeName(typeof(Lazy<string>)));
            Assert.Equal("System.Lazy<System.Collections.Generic.KeyValuePair<String, Boolean>>", scope.GetTypeName(typeof(Lazy<KeyValuePair<string, bool>>)));
        }

        [Fact]
        public void WhenAddingOpenGeneric_ThenSimplifiesConcreteGeneric()
        {
            var scope = Stringly.BeginScope();
            scope.AddType(typeof(Lazy<>));

            Assert.Equal("Lazy<System.String>", scope.GetTypeName(typeof(Lazy<string>)));
        }

        [Fact]
        public void WhenAddingAssembly_ThenSafeUsingsDoNotContainGenerics()
        {
            var scope = Stringly.BeginScope();
            scope.AddType(typeof(IEnumerable<string>));
            scope.AddType(typeof(IEnumerable));

            Assert.False(scope.SafeImports.Any(s => s.IndexOf('[') != -1));
        }

        [Fact]
        public void WhenSimplifyingTwoGenerics_ThenSimplifiesAllParameters()
        {
            var scope = Stringly.BeginScope();
            var type = typeof(IList<KeyValuePair<string, StringConverter>>);

            scope.AddType(type);
            scope.AddType(typeof(System.ComponentModel.StringConverter));

            Assert.Equal("IList<KeyValuePair<String, NetFx.StringlyTyped.StringConverter>>", scope.GetTypeName(type));
            Assert.True(scope.SafeImports.Contains(typeof(IEnumerable<>).Namespace));
        }

        [Fact]
        public void WhenSimplifyingMultipleGenerics_ThenSimplifiesAllParameters()
        {
            var scope = Stringly.BeginScope();
            var type = typeof(IDictionary<IList<KeyValuePair<string, StringConverter>>, NestedType>);

            scope.AddType(type);
            scope.AddType(typeof(System.ComponentModel.StringConverter));

            Assert.Equal("IDictionary<IList<KeyValuePair<String, NetFx.StringlyTyped.StringConverter>>, StringlySpec.NestedType>", scope.GetTypeName(type));
            Assert.True(scope.SafeImports.Contains(typeof(IEnumerable<>).Namespace));
            Assert.True(scope.SafeImports.Contains(typeof(StringlySpec).Namespace));
        }

        [Fact]
        public void WhenSimplifyingTypeMap_ThenOnlySimplifiesNonCollidingTypeNames()
        {
            var scope = Stringly.BeginScope();

            scope.AddType("Foo.A");
            scope.AddType("Foo.B");
            scope.AddType("Bar.A");

            Assert.Equal("Foo.A", scope.GetTypeName("Foo.A"));
            Assert.Equal("B", scope.GetTypeName("Foo.B"));
            Assert.Equal("Bar.A", scope.GetTypeName("Bar.A"));
        }

        [Fact]
        public void WhenSimplifyingTypeMap_ThenUniqueTypeNamesAreSimplified()
        {
            var scope = Stringly.BeginScope();

            scope.AddType("Foo.A");
            scope.AddType("Bar.B");

            Assert.Equal("A", scope.GetTypeName("Foo.A"));
            Assert.Equal("B", scope.GetTypeName("Bar.B"));
        }

        [Fact]
        public void WhenGettingSafeUsings_ThenOnlyGetsNamespacesFromSimplifiedTypeNames()
        {
            var scope = Stringly.BeginScope();

            scope.AddType("Foo.A");
            scope.AddType("Foo.B");
            scope.AddType("Bar.A");
            scope.AddType("Baz.C");

            Assert.True(scope.SafeImports.Contains("Foo"));
            Assert.True(scope.SafeImports.Contains("Baz"));
            Assert.False(scope.SafeImports.Contains("Bar"));
        }

        [Fact]
        public void WhenSimplifyingAssemblyQualifiedName_ThenAddsUsingAndSimplifiesTypeName()
        {
            var scope = Stringly.BeginScope();
            scope.AddType("Foo.Bar, Foo");

            Assert.Equal("Bar", scope.GetTypeName("Foo.Bar, Foo"));
            Assert.True(scope.SafeImports.Contains("Foo"));
        }

        [Fact]
        public void When_Adding_Null_TypeName_Then_Throws()
        {
            var scope = Stringly.BeginScope();

            Assert.Throws<ArgumentException>(() => scope.AddType(null));
        }

        [Fact]
        public void When_Adding_Empty_TypeName_Then_Throws()
        {
            var scope = Stringly.BeginScope();

            Assert.Throws<ArgumentException>(() => scope.AddType(""));
        }

        [Fact]
        public void When_Resolving_Open_Generic_Parameter_Then_Adds_Name()
        {
            var scope = Stringly.BeginScope();

            var genericParam = typeof(IEnumerable<>).GetGenericTypeDefinition().GetGenericArguments()[0];

            scope.AddType(genericParam);

            Assert.Equal("T", scope.GetTypeName(genericParam));
        }

        [Fact]
        public void When_Adding_Open_Generic_Type_Then_Resolves_Without_Param_Name()
        {
            var scope = Stringly.BeginScope();

            var genericType = typeof(Func<>);

            scope.AddType(genericType);

            Assert.Equal("Func<>", scope.GetTypeName(genericType));
        }

        [Fact]
        public void When_Adding_CSharp_Generic_Then_Simplifies_Generic_Parameters()
        {
            var scope = Stringly.BeginScope();

            scope.AddType("System.Device.Location.GeoCoordinate");
            scope.AddType("System.Device.Location.GeoPositionChangedEventArgs<System.Device.Location.GeoCoordinate>");
            scope.AddType("System.EventHandler<System.Device.Location.GeoPositionChangedEventArgs<System.Device.Location.GeoCoordinate>>");
            scope.AddType("System.Device.Location.GeoCoordinate");
            scope.AddType("System.Device.Location.GeoPositionChangedEventArgs<System.Device.Location.GeoCoordinate>");

            Assert.Equal("GeoPositionChangedEventArgs<GeoCoordinate>",
                scope.GetTypeName("System.Device.Location.GeoPositionChangedEventArgs<System.Device.Location.GeoCoordinate>"));
        }

        public class NestedType { }
        public class NestedGeneric<T> { }
    }

    public class NonNestedType { }
    public class StringConverter { }

    public class TwoGeneric<T, I>
    {
        public class NestedTwoGeneric<R, S>
        {
        }
    }
}