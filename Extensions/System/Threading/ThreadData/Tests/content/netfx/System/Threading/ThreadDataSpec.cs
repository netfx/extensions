using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

internal class ThreadDataSpec
{
	[Fact]
	public void WhenDataIsSet_ThenCanRetrieveIt()
	{
		var data = new Foo { Id = 5 };

		using (Thread.CurrentThread.SetData(data))
		{
			var saved = Thread.CurrentThread.GetData<Foo>();

			Assert.Same(data, saved);
		}
	}

	[Fact]
	public void WhenDisposableIsDisposed_ThenDataIsRemoved()
	{
		var data = new Foo { Id = 5 };

		using (Thread.CurrentThread.SetData(data))
		{
		}

		var saved = Thread.CurrentThread.GetData<Foo>();

		Assert.Null(saved);
	}

	[Fact]
	public void WhenCurrentDataIsSet_ThenCanRetrieveIt()
	{
		var data = new Foo { Id = 5 };

		using (ThreadData.SetData(data))
		{
			var saved = ThreadData.GetData<Foo>();

			Assert.Same(data, saved);
		}
	}

	[Fact]
	public void WhenCurrentDisposableIsDisposed_ThenDataIsRemoved()
	{
		var data = new Foo { Id = 5 };

		using (ThreadData.SetData(data))
		{
		}

		var saved = ThreadData.GetData<Foo>();

		Assert.Null(saved);
	}

	[Fact]
	public void WhenMultipleThreadsSetData_ThenEachAccessesDifferentData()
	{
		Action action = () =>
		{
			var data = ThreadData.GetData<Foo>();
			Assert.Null(data);

			data = new Foo { Id = Thread.CurrentThread.ManagedThreadId };
			ThreadData.SetData(data);

			var threadData = ThreadData.GetData<Foo>();

			Assert.NotNull(threadData);
			Assert.Same(threadData, data);
		};

		var tasks = Enumerable.Range(0, 20).Select(x => Task.Factory.StartNew(action)).ToArray();

		Task.WaitAll(tasks);
	}

	public class Foo
	{
		public int Id { get; set; }
	}
}