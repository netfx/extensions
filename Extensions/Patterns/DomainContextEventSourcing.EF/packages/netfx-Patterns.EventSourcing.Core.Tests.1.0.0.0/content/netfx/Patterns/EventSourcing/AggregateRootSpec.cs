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
using Xunit;

namespace NetFx.Patterns.EventSourcing.Core.Tests
{
	/// <nuget id="netfx-Patterns.EventSourcing.Core.Tests"/>
	public class AggregateRootSpec
	{
		[Fact]
		public void WhenDomainActionPerformed_ThenRootChangesStateThroughEvent()
		{
			var root = new TestRoot();
			root.Publish(5);

			Assert.Equal(5, root.LatestVersion);
			Assert.True(root.GetChanges().Any());
			Assert.True(root.GetChanges().OfType<TestPublished>().Any(x => x.Version == 5));

			root.AcceptChanges();

			Assert.False(root.GetChanges().Any());
		}

		[Fact]
		public void WhenLoadingFromEvent_ThenRootChangesState()
		{
			var root = new TestRoot();
			var events = new TimestampedEventArgs[] { new TestPublished { Version = 5 } };

			root.Load(events);

			Assert.Equal(5, root.LatestVersion);
			Assert.False(root.GetChanges().Any());

			// This should be no-op now.
			root.AcceptChanges();

			Assert.False(root.GetChanges().Any());
		}

		/// <nuget id="netfx-Patterns.EventSourcing.Tests" />
		internal class TestRoot : AggregateRoot<Guid>
		{
			public TestRoot()
			{
				Handles<TestPublished>(this.Apply);
			}

			public void Publish(int version)
			{
				if (version < 0)
					throw new ArgumentException();

				base.ApplyChange(new TestPublished { Version = version });
			}

			public int LatestVersion { get; set; }

			private void Apply(TestPublished published)
			{
				this.LatestVersion = published.Version;
			}
		}

		/// <nuget id="netfx-Patterns.EventSourcing.Tests" />
		internal class TestPublished : TimestampedEventArgs
		{
			public int Version { get; set; }
		}
	}
}