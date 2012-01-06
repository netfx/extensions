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
using System.Reactive;

namespace NetFx.System.Reactive
{
	public class EventSpec
	{
		[Fact]
		public void WhenCreatingEvent_ThenCanCastToCovariantBase()
		{
			IEvent<FooArgs> args = new Event<object, FooArgs>(this, new FooArgs());
			var covariant = (IEvent<EventArgs>)args;

			Assert.NotNull(covariant);
		}

		[Fact]
		public void WhenCreatingEventFromFactory_ThenCanPassGenericSender()
		{
			IEvent<FooArgs> args = Event.Create(this, new FooArgs());
			var covariant = (IEvent<EventArgs>)args;

			Assert.NotNull(covariant);
		}

		[Fact]
		public void WhenCreatingEventFromFactory_ThenIsCovariantOnSender()
		{
			IEvent<EventSpec, FooArgs> args = Event.Create(this, new FooArgs());
			var covariant = (IEvent<object, EventArgs>)args;

			Assert.NotNull(covariant);
		}

		public class FooArgs : EventArgs { }
	}
}