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

namespace NetFx.Patterns.Adapter
{
    /// <summary>
    /// Base marker interface implemented by all adapters.
    /// </summary>
    ///	<nuget id="netfx-Patterns.Adapter.Interfaces" />
    partial interface IAdapter
    {
    }

    /// <summary>
    /// Interface implemented by adapters that know how to expose a 
    /// type as a different interface.
    /// </summary>
    /// <typeparam name="TFrom">The type that this adapter supports adapting from.</typeparam>
    /// <typeparam name="TTo">The type that this adapter adapts to.</typeparam>
    ///	<nuget id="netfx-Patterns.Adapter.Interfaces" />
    partial interface IAdapter<TFrom, TTo> : IAdapter
    {
        /// <summary>
        /// Adapts the specified object to the <typeparamref name="TTo"/> type.
        /// </summary>
        TTo Adapt(TFrom from);
    }
}