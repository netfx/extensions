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
using System.Diagnostics;
using System.Diagnostics.Extensibility;
using Moq;

namespace NetFx.System.Diagnostics.Tracer
{
	public class TracerExtensibilitySpec
	{
		[Fact]
		public void WhenSettingNullTracer_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				TracerExtensibility.SetTracer(null));
		}

		[Fact]
		public void WhenReplacingTracer_ThenTracersInvokeIt()
		{
			var tracer = new Mock<ITracer> { DefaultValue = DefaultValue.Mock };

			using (TracerExtensibility.SetTracer(tracer.Object))
			{
				global::System.Diagnostics.Tracer.GetSourceFor<IComparable>();

				tracer.Verify(x => x.GetSourceEntryFor("*"));
				tracer.Verify(x => x.GetSourceEntryFor("System"));
				tracer.Verify(x => x.GetSourceEntryFor("System.IComparable"));
			}
		}

		[Fact]
		public void WhenDisposingReplacedTracerResult_ThenRevertsToExisting()
		{
			var tracer = new Mock<ITracer> { DefaultValue = DefaultValue.Mock };

			using (TracerExtensibility.SetTracer(tracer.Object))
			{
			}

			global::System.Diagnostics.Tracer.GetSourceFor<IComparable>();

			tracer.Verify(x => x.GetSourceEntryFor("*"), Times.Never());
			tracer.Verify(x => x.GetSourceEntryFor("System"), Times.Never());
			tracer.Verify(x => x.GetSourceEntryFor("System.IComparable"), Times.Never());
		}

		[Fact]
		public void WhenSettingTracingLevelWithNullSourceName_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				TracerExtensibility.SetTracingLevel(null, SourceLevels.All));
		}

		[Fact]
		public void WhenSettingTracingLevel_ThenConfigurationIsSet()
		{
			TracerExtensibility.SetTracingLevel("Foo", SourceLevels.Warning);

			Assert.Equal(SourceLevels.Warning, global::System.Diagnostics.Tracer.Instance.GetSourceEntryFor("Foo").Configuration.Switch.Level);
		}

		[Fact]
		public void WhenAddListenerWithNullSourceName_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				TracerExtensibility.AddListener(null, new ConsoleTraceListener()));
		}

		[Fact]
		public void WhenAddNullListener_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				TracerExtensibility.AddListener("Foo", null));
		}

		[Fact]
		public void WhenAddListener_ThenAddsItToEntryConfiguration()
		{
			var listener = new ConsoleTraceListener();
			TracerExtensibility.AddListener("Foo", listener);

			var entry = global::System.Diagnostics.Tracer.Instance.GetSourceEntryFor("Foo");

			Assert.True(entry.Configuration.Listeners.Contains(listener));
		}

		[Fact]
		public void WhenRemoveAddedListener_ThenRemovesItFromEntryConfiguration()
		{
			var listener = new ConsoleTraceListener { Name = "Bar" };
			TracerExtensibility.AddListener("Foo", listener);

			var entry = global::System.Diagnostics.Tracer.Instance.GetSourceEntryFor("Foo");

			Assert.True(entry.Configuration.Listeners.Contains(listener));

			TracerExtensibility.RemoveListener("Foo", "Bar");

			Assert.False(entry.Configuration.Listeners.Contains(listener));
		}

		[Fact]
		public void WhenRemoveListenerWithNullSourceName_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				TracerExtensibility.RemoveListener(null, "Foo"));
		}

		[Fact]
		public void WhenRemoveListenerWithNullListenerName_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				TracerExtensibility.RemoveListener("Foo", null));
		}

		[Fact]
		public void WhenRemoveListenerWithNullTracer_ThenThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				TracerExtensibility.RemoveListener(null, "Foo", "Bar"));
		}
	}
}