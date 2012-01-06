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

namespace NetFx.Patterns.EventSourcing
{
	public class DomainObjectAutoWireSpec
	{
		[Fact]
		public void WhenAutoWiredDomainActionPerformed_ThenRootChangesStateThroughEvent()
		{
			var root = new AutoWiredTestRoot();
			root.Publish(5);

			Assert.Equal(5, root.LatestVersion);
			Assert.True(root.GetEvents().Any());
			Assert.True(root.GetEvents().OfType<TestPublished>().Any(x => x.Version == 5));
		}

		[Fact]
		public void WhenSubsequentAutoWiredDomainActionPerformed_ThenRootChangesStateThroughEvent()
		{
			var root = new AutoWiredTestRoot();
			root.Publish(5);

			Assert.True(root.IsPublished);
			
			root.Unpublish();

			Assert.False(root.IsPublished);
		}

		[Fact]
		public void WhenComparingManualVsAutoWired_ThenItIsAsFastAsManual()
		{
			// warmup
			var manual = new ManualWiredTestRoot();
			var auto = new AutoWiredTestRoot();

			var repeat = 500000;
			var watch = Stopwatch.StartNew();

			for (int i = 0; i < repeat; i++)
			{
				new ManualWiredTestRoot().Publish(5);
			}

			watch.Stop();

			var averageManual = watch.ElapsedTicks / repeat;
			Console.WriteLine("Average per publish (Manual): {0}", watch.ElapsedTicks / repeat);

			watch = Stopwatch.StartNew();

			for (int i = 0; i < repeat; i++)
			{
				new AutoWiredTestRoot().Publish(5);
			}

			watch.Stop();

			Console.WriteLine("Average per publish (Autowire): {0}", watch.ElapsedTicks / repeat);

			var averageAuto = watch.ElapsedTicks / repeat;

			// We're as fast as manual.
			Assert.True(averageAuto <= averageManual);
		}

		internal class AutoWiredTestRoot : DomainObject<Guid, DomainEvent>
		{
			public AutoWiredTestRoot()
			{
				this.AutoWireHandlers();
			}

			public void Publish(int version)
			{
				if (version < 0)
					throw new ArgumentException();

				base.Apply(new TestPublished { Version = version });
			}

			public void Unpublish()
			{
				if (!this.IsPublished)
					throw new ArgumentException();

				base.Apply(new TestUnpublished());
			}

			public int LatestVersion { get; set; }
			public bool IsPublished { get; set; }

			private void OnPublished(TestPublished published)
			{
				this.LatestVersion = published.Version;
				this.IsPublished = true;
			}

			private void OnUnpublished(TestUnpublished published)
			{
				this.IsPublished = false;
			}
		}

		internal class ManualWiredTestRoot : DomainObject<Guid, DomainEvent>
		{
			public ManualWiredTestRoot()
			{
				Initialize(this);
			}

			public void Publish(int version)
			{
				if (version < 0)
					throw new ArgumentException();

				base.Apply(new TestPublished { Version = version });
			}

			public int LatestVersion { get; set; }

			private void OnPublished(TestPublished published)
			{
				this.LatestVersion = published.Version;
			}

			private static void Initialize(DomainObject<Guid, DomainEvent> @this)
			{
				var typed= (ManualWiredTestRoot)@this;
				typed.Handles<TestPublished>(typed.OnPublished);
			}
		}

		public class DomainEvent { }

		public class TestUnpublished : DomainEvent
		{
		}

		public class TestPublished : DomainEvent
		{
			public int Version { get; set; }
		}
	}
}