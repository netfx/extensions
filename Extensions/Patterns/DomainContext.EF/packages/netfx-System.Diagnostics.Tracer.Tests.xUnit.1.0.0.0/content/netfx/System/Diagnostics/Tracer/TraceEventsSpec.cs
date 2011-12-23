using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Diagnostics.Events;
using System.Diagnostics;

namespace NetFx.System.Diagnostics.Tracer
{
	public class TraceEventsSpec
	{
		[Fact]
		public void WhenExceptionMessageHasOnlyException_ThenMessageContainsExceptionMessageAndStacktrace()
		{
			var exception = new InvalidOperationException("Foo");
			var traceEvent = new ExceptionTraceEvent(TraceEventType.Error, 0, exception);

			Assert.Contains("Foo", traceEvent.MessageOrFormat);
			Assert.Contains(exception.ToString(), traceEvent.MessageOrFormat);
		}

		[Fact]
		public void WhenExceptionMessageHasMessage_ThenMessageContainsMessageAndExceptionMessageAndStacktrace()
		{
			var exception = new InvalidOperationException("Foo");
			var traceEvent = new ExceptionTraceEvent(TraceEventType.Error, 0, exception, "Bar");

			Assert.Contains("Bar", traceEvent.MessageOrFormat);
			Assert.Contains("Foo", traceEvent.MessageOrFormat);
			Assert.Contains(exception.ToString(), traceEvent.MessageOrFormat);
		}

		[Fact]
		public void WhenExceptionMessageHasMessageFormat_ThenMessageContainsFormattedMessageAndExceptionMessageAndStacktrace()
		{
			var exception = new InvalidOperationException("Foo");
			var traceEvent = new ExceptionTraceEvent(TraceEventType.Error, 0, exception, "Hello {0}", "World");

			Assert.Contains("Hello World", traceEvent.MessageOrFormat);
			Assert.Contains("Foo", traceEvent.MessageOrFormat);
			Assert.Contains(exception.ToString(), traceEvent.MessageOrFormat);
		}

		[Fact]
		public void WhenMessageEventHasEmptyFormatArgs_ThenNoFormattingIsPerformed()
		{
			var traceEvent = new MessageTraceEvent(TraceEventType.Information, 0, "Foo", new object[0]);

			Assert.Contains("Foo", traceEvent.MessageOrFormat);
		}

		[Fact]
		public void WhenMessageToStringInvoked_ThenContainsEventTypeAndMessage()
		{
			var traceEvent = new MessageTraceEvent(TraceEventType.Information, 0, "Foo", new object[0]);

			Assert.Contains(TraceEventType.Information.ToString(), traceEvent.ToString());
			Assert.Contains("Foo", traceEvent.ToString());
		}

		[Fact]
		public void WhenMessageWithFormatToStringInvoked_ThenContainsEventTypeAndFormattedMessge()
		{
			var traceEvent = new MessageTraceEvent(TraceEventType.Information, 0, "Hello {0}", "World");

			Assert.Contains("Hello World", traceEvent.ToString());
		}

		[Fact]
		public void WhenTransferEventHasMessage_ThenMessageOrFormatExposesIt()
		{
			var traceEvent = new TransferTraceEvent(Guid.NewGuid(), "Foo");

			Assert.Contains("Foo", traceEvent.MessageOrFormat);
		}

		[Fact]
		public void WhenTransferEventHasMessageFormat_ThenMessageOrFormatExposesIt()
		{
			var traceEvent = new TransferTraceEvent(Guid.NewGuid(), "Hello {0}", "World");

			Assert.Contains("Hello World", traceEvent.MessageOrFormat);
		}

	}
}
