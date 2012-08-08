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
    using System.Reflection;
    using System.Text.RegularExpressions;

    static partial class StringlyScopeExtensions
    {
        /// <summary>
        /// Adds the given type to the scope of used types.
        /// </summary>
        public static void AddType(this IStringlyScope scope, Type type)
        {
            scope.AddType(SafeGenericTypeName(type));
        }

        /// <summary>
        /// Adds the given types to the scope of used types.
        /// </summary>
        public static void AddTypes(this IStringlyScope scope, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                scope.AddType(type);
            }
        }

        /// <summary>
        /// Adds all the exported (public) types in the given assembly to the scope of used types.
        /// </summary>
        public static void AddTypes(this IStringlyScope scope, Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                scope.AddType(type);
            }
        }

        /// <summary>
        /// Gets the safe shortened name of a type that can be used in a given context,
        /// considering the already determined <see cref="IStringlyScope.SafeImports"/>, 
        /// such as in code generation.
        /// </summary>
        public static string GetTypeName(this IStringlyScope scope, Type type)
        {
            return scope.GetTypeName(SafeGenericTypeName(type));
        }

        /// <summary>
        /// If the type is a generic type, renders all the T's with the generic 
        /// type definition name. Otherwise, returns just the type full name.
        /// </summary>
        internal static string SafeGenericTypeName(Type type)
        {
            var fullName = type.FullName ?? type.ToString();

            if (type.IsGenericTypeDefinition)
                // Grab the part before the ` arity separator
                return fullName.Substring(0, fullName.IndexOf('`')) +
                    "<" +
                    // `2 means two generic parameters, which would become <,>. 
                    // `1 would be <> (no coma)
                    new string(',', int.Parse(ClrGenericsArity.Match(fullName).Value) - 1) +
                    ">";

            if (!type.IsGenericType)
                return fullName;

            // Renders open generics with their proper generic argument type name.
            return fullName.Substring(0, fullName.IndexOf('`')) +
                "`" + type.GetGenericArguments().Length + "[" +
                String.Join(",", type.GetGenericArguments().Select(t => "[" + SafeGenericTypeName(t) + "]")) +
                "]";
        }

        private static readonly Regex ClrGenericsArity = new Regex(@"(?<=`)\d{1,}", RegexOptions.Compiled);
    }
}
