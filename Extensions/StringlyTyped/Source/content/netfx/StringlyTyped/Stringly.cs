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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Utilities for rendering .NET types as C# type names, with support for generics, 
    /// nested types, type name simplification via "using" scopes, etc.
    /// </summary>
    ///	<nuget id="StringlyTyped" />
    ///	<devdoc>
    /// Note: to make this class public in your assembly, 
    /// declare in another file a partial class that makes 
    /// it public, like so:
    /// public static partial class StringlyTyped { }
    /// Don't modify this file directly, as that would 
    /// prevent further updates via NuGet.
    /// </devdoc>
    static partial class Stringly
    {
        /// <summary>
        /// Gets the C# name for the type, including proper rendering of generics, 
        /// using the simple name of types.
        /// </summary>
        /// <remarks>
        /// For example, for a generic enumerable of boolean, which has a full name of 
        /// <c>System.Collections.Generic.IEnumerable`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]</c>, 
        /// this method returns IEnumerable&lt;Boolean&gt;.
        /// </remarks>
        /// <param name="type" this="true">The type to convert to a simple C# type name.</param>
        public static string ToTypeName(this Type type)
        {
            var name = type.DeclaringType == null || type.IsGenericParameter ?
                type.Name : type.DeclaringType.Name + "." + type.Name;

            if (!type.IsGenericType)
                return name;

            return name.Substring(0, name.IndexOf('`')) +
                "<" +
                String.Join(", ", type.GetGenericArguments().Select(t => ToTypeName(t))) +
                ">";
        }

        /// <summary>
        /// Gets the C# name for the type, including proper rendering of generics, 
        /// using full names of types.
        /// </summary>
        /// <remarks>
        /// For example, for a generic enumerable of boolean, which has a full name of 
        /// <c>System.Collections.Generic.IEnumerable`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]</c>, 
        /// this method returns System.Collections.Generic.IEnumerable&lt;System.Boolean&gt;.
        /// </remarks>
        /// <param name="type" this="true">The type to convert to a C# full type name.</param>
        public static string ToTypeFullName(this Type type)
        {
            var name = type.IsGenericParameter ? type.Name : type.FullName.Replace('+', '.');

            if (!type.IsGenericType)
                return name;

            return name.Substring(0, name.IndexOf('`')) +
                "<" +
                String.Join(", ", type.GetGenericArguments().Select(t => ToTypeFullName(t))) +
                ">";
        }

        /// <summary>
        /// Creates a new stringly scope, which can be used to register types 
        /// that will be used as strings, so that safe code imports can be 
        /// determined and type names can be shortened to the safest form.
        /// </summary>
        /// <remarks>
        /// The scope takes into account all type names in use, and determines 
        /// what namespaces can be safely removed from certain type names 
        /// without introducing ambiguities.
        /// </remarks>
        public static IStringlyScope BeginScope()
        {
            return new StringlyScope();
        }

        private class StringlyScope : IStringlyScope
        {
            /// <summary>
            /// Checks whether the type contains the &lt; and &gt; characters that denotes a C# generic type.
            /// </summary>
            private static readonly Func<string, bool> IsCSharpGenericType = type => type.IndexOf('<') != -1 || type.IndexOf('>') != -1;

            /// <summary>
            /// Checks whether the type contains the ` but also the generic type arguments with [[ and ]]. 
            /// This is the CLR/Reflection representation of generic tpyes.
            /// </summary>
            private static readonly Func<string, bool> IsClrFullGenericType = type => Expressions.ClrGenericsArityAndBracket.IsMatch(type);

            /// <summary>
            /// Checks whether the type contains the `1 (`arity) which denotes an open generic type.
            /// </summary>
            private static readonly Func<string, bool> IsClrOpenGenericType = type => Expressions.ClrGenericsArity.IsMatch(type) && type.IndexOf('[') == -1;

            /// <summary>
            /// Checks whether the type is a nested type by finding the '+' symbol.
            /// </summary>
            private static readonly Func<string, bool> IsNestedType = type => type.IndexOf('+') != -1;

            /// <summary>
            /// Gets the type map built so far, where the keys are the full type names, 
            /// and the value is the type name to use in code generation.
            /// </summary>
            private Dictionary<string, string> typeNameMap;

            /// <summary>
            /// Determined imports that can be removed from full type names 
            /// safely.
            /// </summary>
            private List<string> safeImports;

            /// <summary>
            /// All registered types so far with the scope.
            /// </summary>
            private List<string> registeredTypes = new List<string>();

            public void AddType(string typeFullName)
            {
                if (string.IsNullOrEmpty(typeFullName))
                    throw new ArgumentException("Type to add cannot be null or empty.");

                this.registeredTypes.Add(typeFullName);
                // Clear the calculated state.
                typeNameMap = null;
                safeImports = null;
            }

            /// <summary>
            /// Gets the list of safe imports for type map in use.
            /// </summary>
            public IEnumerable<string> SafeImports
            {
                get
                {
                    EnsureBuilt();
                    return safeImports;
                }
            }

            /// <summary>
            /// Gets the name of the type that can be used in code generation, considering
            /// the already determined <see cref="SafeImports"/>.
            /// </summary>
            public string GetTypeName(string fullName)
            {
                EnsureBuilt();

                var typeName = fullName;
                if (!this.typeNameMap.TryGetValue(typeName, out typeName))
                {
                    typeName = Expressions.ComaWithoutSpace.Replace(
                        Expressions.ClrGenericsArity.Replace(
                            Expressions.CSharpGenericParameters.Replace(ClrToCSharpGeneric(fullName), match =>
                                this.typeNameMap.ContainsKey(match.Value) ? this.typeNameMap[match.Value] : match.Value)
                        , ""), ", ");
                }

                return typeName;
            }

            private void EnsureBuilt()
            {
                if (safeImports == null || typeNameMap == null)
                {
                    typeNameMap = new Dictionary<string, string>();
                    foreach (var type in registeredTypes)
                    {
                        typeNameMap[type] = type;
                    }
                    this.safeImports = new List<string>();

                    this.BuildSafeTypeNames();
                    this.BuildSafeImports();
                }
            }

            /// <summary>
            /// Searches the type map for potential simplifications to type names. 
            /// All type names that are unique across all used namespaces are 
            /// turned into their simple type name (without the namespace).
            /// </summary>
            private void BuildSafeTypeNames()
            {
                // Generics have special notation.
                foreach (var genericParameter in this.typeNameMap.Keys
                    // Add CLR-style generics (with `n[[...]] notation)
                    .Where(IsClrFullGenericType)
                    // Transform to C#-style generics
                    .Select(fullName => ClrToCSharpGeneric(fullName))
                    // Add the ones that are already C# generics
                    .Concat(this.typeNameMap.Keys.Where(t => t.IndexOf('<') != -1))
                    .SelectMany(fullName => Expressions.CSharpGenericParameters.Matches(fullName).Cast<Match>())
                    .Select(parameterMatch => parameterMatch.Value)
                    .ToList())
                {
                    // Add all full name of generic parameters.
                    // Note that this also includes the generic type itself.
                    this.typeNameMap[genericParameter] = genericParameter;
                }

                // Open generics also have special notation, and we 
                // add the simple open generic without arity 
                // to the lookup/simplification table as the 
                // import can then benefit concrete generics
                // This makes the behavior for open generics 
                // compatible with the behavior for concrete ones.
                foreach (var genericParameter in this.typeNameMap.Keys
                    .Where(IsClrOpenGenericType)
                    .Select(fullName => fullName.Substring(0, fullName.IndexOf('`')))
                    .ToList())
                {
                    this.typeNameMap[genericParameter] = genericParameter;
                }

                // Build the list of simple type names from the dictionary for counting.
                // Add only non-generic first, as the actual generic parameters have 
                // already been added above
                var allSimpleTypeNames = this.typeNameMap.Keys
                    .Where(type => !IsClrFullGenericType(type) && type.IndexOf('<') == -1)
                    .Select(type => ToSimpleName(type))
                    .ToList();

                foreach (var type in this.typeNameMap.Keys.Where(name => !IsClrFullGenericType(name)).ToArray())
                {
                    var simpleTypeName = ToSimpleName(type);

                    // Only make the value map for this entry the simple name if there's 
                    // only one type with that simple name (no collisions)
                    if (allSimpleTypeNames.Count(s => s == simpleTypeName) == 1)
                        this.typeNameMap[type] = simpleTypeName;
                }

                // Now do the replacement on the generic parameters for CLR-style types
                foreach (var type in this.typeNameMap.Keys.Where(IsClrFullGenericType).ToArray())
                {
                    var sanitized = ClrToCSharpGeneric(this.typeNameMap[type]);
                    this.typeNameMap[type] = Expressions.CSharpGenericParameters.Replace(sanitized, match => this.typeNameMap[match.Value]);
                }

                // And C# style tpyes
                foreach (var type in this.typeNameMap.Keys.Where(t => t.IndexOf('<') != -1).ToArray())
                {
                    var sanitized = ClrToCSharpGeneric(this.typeNameMap[type]);
                    this.typeNameMap[type] = Expressions.CSharpGenericParameters.Replace(sanitized, match => this.typeNameMap[match.Value]);
                }

                // Remove the assembly on full type names that were not simplified and are not generics
                foreach (var pair in this.typeNameMap.Where(t => !IsCSharpGenericType(t.Value) && t.Value.IndexOf(',') != -1).ToList())
                {
                    this.typeNameMap[pair.Key] = pair.Value.Substring(0, pair.Value.IndexOf(','));
                }

                // Remove the '+' from nested type names.
                foreach (var type in this.typeNameMap.Keys.Where(IsNestedType).ToArray())
                {
                    this.typeNameMap[type] = this.typeNameMap[type].Replace('+', '.');
                }

                // Replace `arity of open generics with brackets and comas, i.e. IEnumerable`1 with IEnumerable<>
                foreach (var type in this.typeNameMap.Keys.Where(IsClrOpenGenericType).ToList())
                {
                    this.typeNameMap[type] = Expressions.ClrGenericsArity.Replace(this.typeNameMap[type], match =>
                            "<" + new string(Enumerable.Range(0, int.Parse(match.Value.Substring(1)) - 1).Select(_ => ',').ToArray()) + ">");
                }

                // Finally add whitespaces between generic parameters and remove the arity for a C# valid identifier
                foreach (var type in this.typeNameMap.Keys.Where(IsClrFullGenericType).ToList())
                {
                    this.typeNameMap[type] = Expressions.ClrGenericsArity.Replace(
                        Expressions.ComaWithoutSpace.Replace(this.typeNameMap[type], ", "), "");
                }
            }

            /// <summary>
            /// From the type map, finds those namespaces that can be safely imported ("using" in C#) 
            /// without causing type collisions among the type names in the map.
            /// </summary>
            private void BuildSafeImports()
            {
                Func<string, bool> hasNamespace = typeName =>
                    typeName.TakeWhile(c => c != '<' && c != ',').Any(c => c == '.');

                Func<string, string> namespaceSelector = typeName =>
                {
                    var simpleName = new string(typeName.TakeWhile(c => c != '<' && c != ',').ToArray());
                    // The hasNamespace filter already ensures we do have a dot.
                    return simpleName.Substring(0, simpleName.LastIndexOf('.'));
                };

                var fullNames = this.typeNameMap.Keys
                    .Where(type => !IsClrFullGenericType(type))
                    .Where(hasNamespace)
                    .Select(namespaceSelector)
                    .GroupBy(ns => ns)
                    .ToDictionary(group => group.Key, group => group.Count());

                var finalNames = this.typeNameMap.Values
                    .Where(type => !IsClrFullGenericType(type))
                    .Where(hasNamespace)
                    .Select(namespaceSelector)
                    .GroupBy(ns => ns)
                    .ToDictionary(group => group.Key, group => group.Count());

                foreach (var fullName in fullNames)
                {
                    if (!finalNames.ContainsKey(fullName.Key) ||
                        fullName.Value > finalNames[fullName.Key])
                        this.safeImports.Add(fullName.Key);
                }
            }

            /// <summary>
            /// Sanitizes the generics.
            /// </summary>
            /// <param name="typeName">Name of the type.</param>
            private string ClrToCSharpGeneric(string typeName)
            {
                // Remove full assembly names
                typeName = Expressions.FullAssemblyName.Replace(typeName, string.Empty);

                var nodes = new HashSet<TypeName>();
                var stack = new Stack<TypeName>();
                // Push root.
                stack.Push(new TypeName(null, -1));
                for (int i = 0; i < typeName.Length; i++)
                {
                    var c = typeName[i];
                    if (c == '[')
                    {
                        // Marks the start of a new type parameter, 
                        // or the enclosing bracket on a simple type 
                        // that we'll need to remove later (denoted 
                        // by the lack of child type names).
                        var node = new TypeName(stack.Peek(), i);
                        nodes.Add(node);
                        stack.Push(node);
                    }
                    else if (c == ']')
                    {
                        stack.Pop().EndBracket = i;
                    }
                    else if (c != ',')
                    {
                        // The coma will show up between generic type 
                        // parameters, but we skip as it does not make 
                        // up for a type name for the current type which 
                        // would be just the enclosing brackets that we 
                        // seek to remove.
                        stack.Peek().Name += c;
                    }
                }

                var indexesToSkip = new List<int>();
                foreach (var node in nodes.Where(x => x.Children.Count == 0).ToArray())
                {
                    indexesToSkip.Add(node.StartBracket);
                    indexesToSkip.Add(node.EndBracket);

                    var parent = node.Parent;
                    while (parent != null)
                    {
                        // If the node has no length, we 
                        // don't remove, as it would the 
                        // valid marker for generic args.
                        if (parent.Name.Length != 0)
                        {
                            indexesToSkip.Add(parent.StartBracket);
                            indexesToSkip.Add(parent.EndBracket);
                        }
                        parent = parent.Parent;
                    }
                }

                var sanitizedName = new string(Enumerable.Range(0, typeName.Length)
                    .Where(i => !indexesToSkip.Contains(i))
                    .Select(i => typeName[i])
                    .Select(c => c == '[' ? '<' : c == ']' ? '>' : c)
                    .ToArray());

                return Expressions.ClrGenericsArity.Replace(sanitizedName, "");
            }

            private static string ToSimpleName(string type)
            {
                var simpleTypeName = type;
                if (simpleTypeName.IndexOf(',') != -1)
                    simpleTypeName = simpleTypeName.Substring(0, simpleTypeName.IndexOf(','));

                simpleTypeName = simpleTypeName.Substring(simpleTypeName.LastIndexOf('.') + 1);

                return simpleTypeName;
            }

            /// <summary>
            /// Helper class used to build the hierarchy of type names in a generic type name.
            /// </summary>
            private class TypeName
            {
                public TypeName(TypeName parent, int startBracket)
                {
                    this.Parent = parent;
                    if (parent != null)
                        this.Parent.Children.Add(this);

                    this.StartBracket = startBracket;
                    this.EndBracket = -1;
                    this.Children = new List<TypeName>();
                    this.Name = "";
                }
                public string Name { get; set; }
                public TypeName Parent { get; set; }
                public int StartBracket { get; set; }
                public int EndBracket { get; set; }
                public List<TypeName> Children { get; set; }
            }

            private static class Expressions
            {
                /// <summary>
                /// Matches the generic type and its parameter type names with C# syntax: 
                /// for System.IEnumerable{System.Boolean}, matches System.IEnumerable and System.Boolean
                /// </summary>
                public static readonly Regex CSharpGenericParameters = new Regex(@"[^<,>]+?(?=(<|,|>|$))", RegexOptions.Compiled);

                /// <summary>
                /// Matches two identifiers that are separated by a coma but without a whitespace after the coma, such as Boolean,String.
                /// </summary>
                public static readonly Regex ComaWithoutSpace = new Regex(@"(?<=[^,]),(?=[^\s])", RegexOptions.Compiled);

                /// <summary>
                /// Matches the assembly part of an assembly qualified type name (i.e. , mscorlib, Version=..., Culture=..., PublicKeyToken=...).
                /// </summary>
                public static readonly Regex FullAssemblyName = new Regex(@",[^\[]+?,\s?Version.*?,\s?Culture.*?,\s?PublicKeyToken.*?(?=\])", RegexOptions.Compiled);

                /// <summary>
                /// Matches the arity part of a generics type, like IEnumerable`1 (would match `1).
                /// </summary>
                public static readonly Regex ClrGenericsArity = new Regex(@"`\d{1,}", RegexOptions.Compiled);

                /// <summary>
                /// Matches the arity part of a generics type, like IEnumerable`1[[System.Boolean]] (would match `1[).
                /// </summary>
                public static readonly Regex ClrGenericsArityAndBracket = new Regex(@"`\d{1,}\[", RegexOptions.Compiled);
            }
        }
    }
}