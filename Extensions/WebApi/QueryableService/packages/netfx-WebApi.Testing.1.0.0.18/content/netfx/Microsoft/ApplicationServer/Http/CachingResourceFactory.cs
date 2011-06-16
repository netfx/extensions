#region BSD License
/* 
Copyright (c) 2010, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationServer.Http.Description;
using System.Collections.Concurrent;
using System.Net.Http;

namespace Microsoft.ApplicationServer.Http
{
	/// <summary>
	/// A resource factory that creates services only once and returns the same 
	/// instance afterwards. Appropriately wraps whatever existing factory 
	/// exists or creates an <see cref="ActivatorResourceFactory"/> if null 
	/// is passed in the constructor.
	/// </summary>
	internal class CachingResourceFactory : IResourceFactory
	{
		private ConcurrentDictionary<Type, object> cachedTypes = new ConcurrentDictionary<Type, object>();
		private IResourceFactory originalFactory;

		public CachingResourceFactory(IResourceFactory originalFactory = null)
		{
			this.originalFactory = originalFactory ?? (IResourceFactory)new ActivatorResourceFactory();
		}

		public object GetInstance(Type serviceType, System.ServiceModel.InstanceContext instanceContext, HttpRequestMessage request)
		{
			return cachedTypes.GetOrAdd(serviceType, type => this.originalFactory.GetInstance(serviceType, instanceContext, request));
		}

		public void ReleaseInstance(System.ServiceModel.InstanceContext instanceContext, object service)
		{
			// We never release, as we're caching it.
		}
	}
}
