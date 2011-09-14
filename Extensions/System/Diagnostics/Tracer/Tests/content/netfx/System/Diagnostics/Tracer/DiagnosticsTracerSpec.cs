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
using System.Diagnostics.Extensibility;
using Moq;
using System.Diagnostics.Events;
using System.Diagnostics;

namespace NetFx.System.Diagnostics.Tracer
{
	public class DiagnosticsTracerSpec
	{
		private readonly ITracer tracer = new DiagnosticsTracer();

		[Fact]
		public void WhenGettingSource_ThenReturnsNonNull()
		{
			var source = tracer.GetSourceEntryFor("Foo");

			Assert.NotNull(source);
		}

		[Fact]
		public void WhenAddingListener_ThenCanTraceEvent()
		{
			var listener = new Mock<TraceListener>();
			var source = tracer.GetSourceEntryFor("Foo");

			source.Configuration.Switch.Level = SourceLevels.Information;
			source.Configuration.Listeners.Add(listener.Object);

			source.TraceSource.Trace(new MessageTraceEvent(TraceEventType.Information, 0, "Hello"));

			listener.Verify(x => x.TraceEvent(
				It.IsAny<TraceEventCache>(),
				"Foo",
				TraceEventType.Information,
				0,
				"Hello",
				null));
		}

		[Fact]
		public void WhenAddingListenerThroughTracer_ThenTracesEventPassingOriginalSourceName()
		{
			var listener = new Mock<TraceListener>();

			tracer.AddListener("Foo", listener.Object);
			
			var source = tracer.GetSourceEntryFor("Foo");

			source.Configuration.Switch.Level = SourceLevels.Information;

			source.TraceSource.Trace(new MessageTraceEvent(TraceEventType.Information, 0, "Hello"));

			listener.Verify(x => x.TraceEvent(
				It.IsAny<TraceEventCache>(),
				"Foo",
				TraceEventType.Information,
				0,
				"Hello",
				null));
		}

		[Fact]
		public void WhenRemovingListenerThroughTracer_ThenStopsTracingOnListener()
		{
			var listener = new Mock<TraceListener>();
			listener.Setup(x => x.Name).Returns("Mock");

			tracer.AddListener("Foo", listener.Object);
			tracer.RemoveListener("Foo", listener.Object.Name);

			var source = tracer.GetSourceEntryFor("Foo");

			source.Configuration.Switch.Level = SourceLevels.Information;

			source.TraceSource.Trace(new MessageTraceEvent(TraceEventType.Information, 0, "Hello"));

			listener.Verify(x => x.TraceEvent(
				It.IsAny<TraceEventCache>(), It.IsAny<string>(),
				TraceEventType.Information,
				It.IsAny<int>(), It.IsAny<string>(), null), Times.Never());
		}

		[Fact]
		public void WhenAddingListenerToNewSource_ThenCreatesOnTheFly()
		{
			var listener = new Mock<TraceListener>("Listener");
			var sourceName = Guid.NewGuid().ToString();

			tracer.AddListener(sourceName, listener.Object);

			var source = tracer.GetSourceEntryFor("Foo");
			Assert.False(source.Configuration.Listeners.Any(t => t.Name == "Listener"));
		}

		[Fact]
		public void WhenRemovingListenerToNewSource_ThenCreatesOnTheFly()
		{
			var listener = new Mock<TraceListener>("Listener");

			tracer.RemoveListener(Guid.NewGuid().ToString(), listener.Object);
		}

		[Fact]
		public void WhenAddingListenerAndReplacingSwitch_ThenCanTraceEvent()
		{
			var listener = new Mock<TraceListener>();
			var source = tracer.GetSourceEntryFor("Foo");

			source.Configuration.Switch = new SourceSwitch("FooSwitch", ((int)SourceLevels.Information).ToString());
			source.Configuration.Listeners.Add(listener.Object);

			source.TraceSource.Trace(new MessageTraceEvent(TraceEventType.Information, 0, "Hello"));

			listener.Verify(x => x.TraceEvent(
				It.IsAny<TraceEventCache>(),
				"Foo",
				TraceEventType.Information,
				0,
				"Hello",
				null));
		}

		[Fact]
		public void WhenGettingEntryConfigurationName_ThenMatchesTraceSourceName()
		{
			Assert.Equal("Foo", tracer.GetSourceEntryFor("Foo").Configuration.Name);
		}

		[Fact]
		public void WhenFlushingSource_ThenFlushesListeners()
		{
			var listener = new Mock<TraceListener>("Listener");
			tracer.AddListener("Foo", listener.Object);
			var source = tracer.GetSourceEntryFor("Foo");

			source.TraceSource.Flush();

			listener.Verify(x => x.Flush());
		}

		[Fact]
		public void WhenTracingTransferEvent_ThenInvokesTraceTransferOnListener()
		{
			var listener = new Mock<TraceListener>("Listener");
			tracer.AddListener("Foo", listener.Object);
			var source = tracer.GetSourceEntryFor("Foo");
			var relatedId = Guid.NewGuid();
			source.Configuration.Switch.Level = SourceLevels.All;

			source.TraceSource.Trace(new TransferTraceEvent(relatedId, "Transfer"));

			listener.Verify(x => x.TraceTransfer(It.IsAny<TraceEventCache>(), "Foo", 0, "Transfer", relatedId));
		}

		[Fact]
		public void WhenTracingDataEvent_ThenInvokesTraceDataOnListener()
		{
			var listener = new Mock<TraceListener>("Listener");
			tracer.AddListener("Foo", listener.Object);
			var source = tracer.GetSourceEntryFor("Foo");
			source.Configuration.Switch.Level = SourceLevels.All;
			var data = new object();

			source.TraceSource.Trace(new DataTraceEvent(TraceEventType.Information, 5, data));

			listener.Verify(x => x.TraceData(It.IsAny<TraceEventCache>(), "Foo", TraceEventType.Information, 5, data));
		}

		[Fact]
		public void WhenManipulatingTraceListenersCollection_ThenSucceeds()
		{
			var listener = Mock.Of<TraceListener>();
			var source = tracer.GetSourceEntryFor("Foo");

			source.Configuration.Listeners.Clear();

			Assert.Equal(0, source.Configuration.Listeners.Count);

			source.Configuration.Listeners.Add(listener);

			Assert.Equal(1, source.Configuration.Listeners.Count);

			source.Configuration.Listeners.Remove(listener);

			Assert.Equal(0, source.Configuration.Listeners.Count);
		}
	}
}

