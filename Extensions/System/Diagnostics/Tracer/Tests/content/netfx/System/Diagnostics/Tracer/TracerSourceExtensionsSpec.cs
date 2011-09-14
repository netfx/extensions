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
using Moq;
using System.Diagnostics.Events;
using System.Diagnostics;

namespace NetFx.System.Diagnostics.Tracer
{
	public class TracerSourceExtensionsSpec
	{
		[Fact]
		public void WhenTraceErrorWithNullSource_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				TracerSourceExtensions.TraceError(null, new InvalidOperationException()));

			Assert.Throws<ArgumentNullException>(() =>
				TracerSourceExtensions.TraceError(null, new InvalidOperationException(), "foo"));

			Assert.Throws<ArgumentNullException>(() =>
				TracerSourceExtensions.TraceError(null, new InvalidOperationException(), "foo {0}", "bar"));
		}

		[Fact]
		public void WhenTraceWithNullSource_ThenThrowsArgumentNullException()
		{
			AssertThrowsWithNullSource(TracerSourceExtensions.TraceError);
			AssertThrowsWithNullSource(TracerSourceExtensions.TraceInformation);
			AssertThrowsWithNullSource(TracerSourceExtensions.TraceWarning);
			AssertThrowsWithNullSource(TracerSourceExtensions.TraceVerbose);
		}

		private void AssertThrowsWithNullSource(Action<ITraceSource, string> trace)
		{
			Assert.Throws<ArgumentNullException>(() => trace(null, ""));
		}

		[Fact]
		public void WhenTraceWithNullSourceWithFormat_ThenThrowsArgumentNullException()
		{
			AssertThrowsWithNullSourceWithFormat(TracerSourceExtensions.TraceError);
			AssertThrowsWithNullSourceWithFormat(TracerSourceExtensions.TraceInformation);
			AssertThrowsWithNullSourceWithFormat(TracerSourceExtensions.TraceWarning);
			AssertThrowsWithNullSourceWithFormat(TracerSourceExtensions.TraceVerbose);
		}

		private void AssertThrowsWithNullSourceWithFormat(Action<ITraceSource, string, object[]> trace)
		{
			Assert.Throws<ArgumentNullException>(() => trace(null, "foo {0}", new object[] { "bar" }));
		}

		[Fact]
		public void WhenTraceDataWithNullSource_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				TracerSourceExtensions.TraceData(null, TraceEventType.Verbose, new object()));
		}

		[Fact]
		public void WhenTracingData_ThenSucceeds()
		{
			var source = new Mock<ITraceSource>();
			var data = new object();

			TracerSourceExtensions.TraceData(source.Object, TraceEventType.Verbose, data);

			source.Verify(x => x.Trace(It.Is<DataTraceEvent>(t => t.Type == TraceEventType.Verbose && t.Data == data)));
		}

		[Fact]
		public void WhenTracingError_ThenSucceeds()
		{
			var source = new Mock<ITraceSource>();

			TracerSourceExtensions.TraceError(source.Object, new InvalidOperationException());

			source.Verify(x => x.Trace(It.Is<TraceEvent>(t => t.Type == TraceEventType.Error)));
		}

		[Fact]
		public void WhenTracingErrorWithValidMessage_ThenSucceeds()
		{
			var source = new Mock<ITraceSource>();

			TracerSourceExtensions.TraceError(source.Object, new InvalidOperationException(), "Foo");

			source.Verify(x => x.Trace(It.Is<TraceEvent>(t => t.Type == TraceEventType.Error)));
		}

		[Fact]
		public void WhenTracingErrorWithValidMessageFormat_ThenSucceeds()
		{
			var source = new Mock<ITraceSource>();

			TracerSourceExtensions.TraceError(source.Object, new InvalidOperationException(), "Foo {0}", "Bar");

			source.Verify(x => x.Trace(It.Is<TraceEvent>(t => t.Type == TraceEventType.Error)));
		}

		[Fact]
		public void WhenTraceWithValidMessage_ThenSucceeds()
		{
			TraceWithMessage(TracerSourceExtensions.TraceError, TraceEventType.Error);
			TraceWithMessage(TracerSourceExtensions.TraceInformation, TraceEventType.Information);
			TraceWithMessage(TracerSourceExtensions.TraceWarning, TraceEventType.Warning);
			TraceWithMessage(TracerSourceExtensions.TraceVerbose, TraceEventType.Verbose);
		}

		[Fact]
		public void WhenTraceWithValidMessageFormat_ThenSucceeds()
		{
			TraceWithMessageFormat(TracerSourceExtensions.TraceError, TraceEventType.Error);
			TraceWithMessageFormat(TracerSourceExtensions.TraceInformation, TraceEventType.Information);
			TraceWithMessageFormat(TracerSourceExtensions.TraceWarning, TraceEventType.Warning);
			TraceWithMessageFormat(TracerSourceExtensions.TraceVerbose, TraceEventType.Verbose);
		}

		private void TraceWithMessage(Action<ITraceSource, string> trace, TraceEventType type)
		{
			var source = new Mock<ITraceSource>();
			
			trace(source.Object, "foo");

			source.Verify(x => x.Trace(It.Is<TraceEvent>(t => t.Type == type)));
		}

		private void TraceWithMessageFormat(Action<ITraceSource, string, object[]> trace, TraceEventType type)
		{
			var source = new Mock<ITraceSource>();
			
			trace(source.Object, "foo {0}", new object[] { "bar" });

			source.Verify(x => x.Trace(It.Is<TraceEvent>(t => t.Type == type)));
		}

		[Fact]
		public void WhenTraceErrorWithNullFormat_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				TracerSourceExtensions.TraceError(Mock.Of<ITraceSource>(), "bar {0}", null));

			Assert.Throws<ArgumentNullException>(() =>
				TracerSourceExtensions.TraceError(Mock.Of<ITraceSource>(), new InvalidOperationException(), "bar {0}", null));
		}

		public class GivenAnActivity
		{
			private Mock<ITraceSource> tracer = new Mock<ITraceSource>();

			[Fact]
			public void WhenStartingActivityWithFormat_ThenSetsActivityIdToNonEmpty()
			{
				using (tracer.Object.StartActivity("foo {0}", "bar"))
				{
					Assert.NotEqual(Guid.Empty, Trace.CorrelationManager.ActivityId);
				}
			}

			[Fact]
			public void WhenStartingActivity_ThenSetsActivityIdToNonEmpty()
			{
				using (tracer.Object.StartActivity("foo"))
				{
					Assert.NotEqual(Guid.Empty, Trace.CorrelationManager.ActivityId);
				}
			}

			[Fact]
			public void WhenStartingActivity_ThenTracesStartEvent()
			{
				using (tracer.Object.StartActivity("foo"))
				{
					this.tracer.Verify(t => t.Trace(It.Is<TraceEvent>(e => e.Type == TraceEventType.Start)));
				}
			}

			[Fact]
			public void WhenDisposingActivity_ThenTracesStopEvent()
			{
				using (tracer.Object.StartActivity("foo"))
				{
				}

				this.tracer.Verify(t => t.Trace(It.Is<TraceEvent>(e => e.Type == TraceEventType.Stop)));
			}

			[Fact]
			public void WhenDisposingActivity_ThenRestoresOriginalActivityId()
			{
				var originalId = Trace.CorrelationManager.ActivityId;

				using (tracer.Object.StartActivity("foo"))
				{
					Assert.NotEqual(originalId, Trace.CorrelationManager.ActivityId);
				}

				Assert.Equal(originalId, Trace.CorrelationManager.ActivityId);
			}
		}

		public class GivenAPreviousActivity
		{
			private Mock<ITraceSource> tracer = new Mock<ITraceSource>();

			public GivenAPreviousActivity()
			{
				Trace.CorrelationManager.ActivityId = Guid.NewGuid();
			}

			[Fact]
			public void WhenStartingActivity_ThenTracesTransferEventToNewId()
			{
				var originalId = Trace.CorrelationManager.ActivityId;
				var newId = default(Guid);

				using (tracer.Object.StartActivity("foo"))
				{
					newId = Trace.CorrelationManager.ActivityId;

					this.tracer.Verify(t => t.Trace(It.Is<TransferTraceEvent>(e =>
						e.Type == TraceEventType.Transfer &&
						e.RelatedActivityId == newId)));
				}
			}

			[Fact]
			public void WhenStartingActivityWithFormat_ThenTracesTransferEventToNewId()
			{
				var originalId = Trace.CorrelationManager.ActivityId;
				var newId = default(Guid);

				using (tracer.Object.StartActivity("foo {0}", "bar"))
				{
					newId = Trace.CorrelationManager.ActivityId;

					this.tracer.Verify(t => t.Trace(It.Is<TransferTraceEvent>(e =>
						e.Type == TraceEventType.Transfer &&
						e.RelatedActivityId == newId)));
				}
			}

			[Fact]
			public void WhenDisposingActivity_ThenTracesTransferEventBackToOld()
			{
				var originalId = Trace.CorrelationManager.ActivityId;
				var newId = default(Guid);

				using (tracer.Object.StartActivity("foo"))
				{
					newId = Trace.CorrelationManager.ActivityId;
				}

				this.tracer.Verify(t => t.Trace(It.Is<TransferTraceEvent>(e =>
					e.Type == TraceEventType.Transfer &&
					e.RelatedActivityId == originalId)));
			}
		}
	}
}
