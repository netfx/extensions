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
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace NetFx.System
{
	public class AmbientSingletonSpec
	{
        [Fact]
        public void WhenReusingIdentifier_ThenCanAccessSameValue()
        {
            var identifier = Guid.NewGuid();
            var singleton1 = new AmbientSingleton<string>("foo", identifier);
            var singleton2 = new AmbientSingleton<string>(identifier);

            Assert.Equal("foo", singleton1.Value);
            Assert.Equal("foo", singleton2.Value);
            Assert.Same(singleton1.Value, singleton2.Value);
        }

		[Fact]
		public void WhenSpecifyingGlobalDefault_ThenReturnsItFromSingleton()
		{
			var singleton = new AmbientSingleton<string>("foo");

			Assert.Equal("foo", singleton.Value);
		}

		[Fact]
		public void WhenNoGlobalDefaultSpecified_ThenReturnsDefaultValue()
		{
			var singleton = new AmbientSingleton<string>();

			Assert.Equal(null, singleton.Value);
		}

		[Fact]
		public void WhenSpecifyingAmbientValue_ThenOverridesGlobalDefault()
		{
			var singleton = new AmbientSingleton<string>("foo");

			singleton.Value = "bar";

			Assert.Equal("bar", singleton.Value);
		}

		[Fact]
		public void WhenSpecifyingAmbientValue_ThenDoesNotOverridesOtherCallContextGlobalDefault()
		{
			var singleton = new AmbientSingleton<string>("foo");

			var value1 = "";
			var value2 = "";

			Action action1 = () =>
			{
				singleton.Value = "bar";
				value1 = singleton.Value;
			};

			Action action2 = () => value2 = singleton.Value;

			var tasks = new[] { Task.Factory.StartNew(action1), Task.Factory.StartNew(action2) };

			Task.WaitAll(tasks);

			Assert.Equal("bar", value1);
			Assert.Equal("foo", value2);
		}

		[Fact]
		public void WhenUsingFactory_ThenCanCreateSingleton()
		{
			var s1 = AmbientSingleton.Create("foo");
			var s2 = AmbientSingleton.Create(() => "bar");

			Assert.Equal("foo", s1.Value);
			Assert.Equal("bar", s2.Value);
		}

		[ThreadStatic]
		private static string threadStaticData;

		private static ThreadLocal<string> threadLocalData = new ThreadLocal<string>(() => "foo");

		[Fact(Skip = "Only to see how fast it is :)")]
		public void WhenTestingSpeedOfCallContext_ThenComparesWellAgainstThreadData()
		{
			var iterations = 5000;
			var data = "foo";
			var threadSlot = Thread.AllocateDataSlot();

			Action threadData = () =>
			{
				Thread.SetData(threadSlot, data);
				var state = Thread.GetData(threadSlot);
				Assert.Equal(state, data);
			};

			Action callContext = () =>
			{
				CallContext.LogicalSetData("slot", data);
				var state = CallContext.LogicalGetData("slot");
				Assert.Equal(state, data);
			};

			Action threadStatic = () =>
			{
				threadStaticData = data;
				var state = threadStaticData;
				Assert.Equal(state, data);
			};

			Action threadLocal = () =>
			{
				threadLocalData = new ThreadLocal<string>(() => data);
				var state = threadLocalData.Value;
				Assert.Equal(state, data);
			};

			// It's best if we test this in multi-threaded scenarios too.
			Time("Thread Data", threadData, iterations);
			Time("CallContext", callContext, iterations);
			Time("ThreadStatic", threadStatic, iterations);
			Time("ThreadLocal", threadLocal, iterations);
		}

		private void Time(string title, Action action, int iterations)
		{
 			var actions = Enumerable.Range(0, iterations).Select(i => action).ToArray();

			var watch = Stopwatch.StartNew();
			Parallel.Invoke(actions);
			watch.Stop();

			Console.WriteLine("{0}: {1}", title, (double)watch.ElapsedTicks / (double)iterations);
		}

		public interface IClock { }

		public class SystemClock : IClock
		{
			private static readonly AmbientSingleton<IClock> instance = new AmbientSingleton<IClock>(() => new SystemClock());

			private SystemClock() { }

			public static IClock Instance
			{
				get { return instance.Value; }
				internal set { instance.Value = value; }
			}
		}

	}
}