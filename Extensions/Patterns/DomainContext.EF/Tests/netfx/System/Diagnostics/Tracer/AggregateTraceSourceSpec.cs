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
using Moq;
using Xunit;
using System.Diagnostics;
using System.Diagnostics.Events;
using System.Diagnostics.Extensibility;

namespace NetFx.System.Diagnostics.Tracer
{
	public class AggregateTraceSourceSpec
	{
		private Mock<ITraceSource> first = new Mock<ITraceSource>();
		private Mock<ITraceSource> second = new Mock<ITraceSource>();
		private AggregateTraceSource aggregate;

		public AggregateTraceSourceSpec()
		{
			this.first = new Mock<ITraceSource> { DefaultValue = DefaultValue.Mock };
			this.second = new Mock<ITraceSource> { DefaultValue = DefaultValue.Mock };

			this.first.SetupAllProperties();
			this.second.SetupAllProperties();

			this.aggregate = new AggregateTraceSource("Tracer", new[] { this.first.Object, this.second.Object });
		}

		[Fact]
		public void WhenFlushing_ThenFlushesEach()
		{
			this.aggregate.Flush();

			this.first.Verify(x => x.Flush());
			this.second.Verify(x => x.Flush());
		}

		[Fact]
		public void WhenTracing_ThenTracesEach()
		{
			var ev = new MessageTraceEvent(TraceEventType.Information, 0, "foo");

			this.aggregate.Trace(ev);

			this.first.Verify(x => x.Trace(ev));
			this.second.Verify(x => x.Trace(ev));
		}

		[Fact]
		public void WhenAggregateToStringInvoked_ThenContainsOriginalSourceName()
		{
			Assert.Contains("Tracer", this.aggregate.ToString());
		}
	}
}
