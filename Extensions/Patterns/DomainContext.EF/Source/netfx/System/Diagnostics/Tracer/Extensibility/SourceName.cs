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
using System.Collections.Generic;
using System.Linq;

namespace System.Diagnostics.Extensibility
{
	/// <summary>
	/// Provides methods and constants for dealing with trace source names.
	/// </summary>
	static partial class SourceName
	{
		/// <summary>
		/// The global default trace source name that is used by all sources requested through 
		/// the <see cref="Tracer"/>, which equals <c>*</c> (an asterisk).
		/// </summary>
		public const string Default = "*";

		/// <summary>
		/// The trace source that is used to trace unexpected errors that happen during the 
		/// tracing operations, which never affect the running application. Tracing failures 
		/// should NEVER cause an application to fail.
		/// </summary>
		/// <remarks>
		/// Add listeners to this trace source name in order to receive logs for tracing 
		/// infrastructure errors, such as listener failing to log, etc.
		/// </remarks>
		public const string Tracer = "Tracer";

		/// <summary>
		/// Gets the single trace source name that corresponds to the given type <typeparamref name="T"/>.
		/// </summary>
		public static string For<T>()
		{
			return For(typeof(T));
		}

		/// <summary>
		/// Gets the single trace source name that corresponds to the given type <paramref name="type"/>
		/// </summary>
		public static string For(Type type)
		{
			if (type.IsGenericType)
			{
				var genericName = type.GetGenericTypeDefinition().FullName;

				return genericName.Substring(0, genericName.IndexOf('`')) +
					"<" +
					string.Join(",", type.GetGenericArguments().Select(t => For(t)).ToArray()) +
					">";
			}

			return type.FullName;
		}

		/// <summary>
		/// Gets the list of trace source names that are used to inherit trace source logging for the given type <typeparamref name="T"/>.
		/// </summary>
		public static IEnumerable<string> CompositeFor<T>()
		{
			return CompositeFor(typeof(T));
		}

		/// <summary>
		/// Gets the list of trace source names that are used to inherit trace source logging for the given type <paramref name="type"/>.
		/// </summary>
		public static IEnumerable<string> CompositeFor(Type type)
		{
			var name = For(type);

			yield return SourceName.Default;

			var indexOfGeneric = name.IndexOf('<');
			var indexOfLastDot = name.LastIndexOf('.');

			if (indexOfGeneric == -1 && indexOfLastDot == -1)
			{
				yield return name;
				yield break;
			}

			var parts = default(string[]);

			if (indexOfGeneric == -1)
				parts = name
					.Substring(0, name.LastIndexOf('.'))
					.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			else
				parts = name
					.Substring(0, indexOfGeneric)
					.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 1; i <= parts.Length; i++)
			{
				yield return string.Join(".", parts, 0, i);
			}

			yield return name;
		}
	}
}
