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
    using System.Collections.Generic;

    /// <summary>
    /// Provides a stringly scope, which can be used to register types 
    /// that will be used as strings, so that safe code imports can be 
    /// determined and type names can be shortened to the safest form.
    /// </summary>
    /// <remarks>
    /// The scope takes into account all type names in use, and determines 
    /// what namespaces can be safely removed from certain type names 
    /// without introducing ambiguities.
    /// </remarks>
    partial interface IStringlyScope
    {
        /// <summary>
        /// Gets the list of safe imports that have been determined so 
        /// far given the added types.
        /// </summary>
        IEnumerable<string> SafeImports { get; }

        /// <summary>
        /// Adds the given type to the scope if it wasn't added already.
        /// </summary>
        void AddType(string fullName);

        /// <summary>
        /// Gets the name of the type that can be used in code generation, considering 
        /// the already determined <see cref="SafeImports"/>.
        /// </summary>
        string GetTypeName(string fullName);
    }
}
