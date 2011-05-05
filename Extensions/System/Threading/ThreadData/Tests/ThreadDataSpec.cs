using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Threading;

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

	public class Foo
	{
		public int Id { get; set; }
	}
}
