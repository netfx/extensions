using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NetFx.System
{
	public class AppDomainDataSpec
	{
		[Fact]
		public void WhenDataIsSet_ThenCanRetrieveIt()
		{
			var data = new Foo { Id = 5 };

			using (AppDomain.CurrentDomain.SetData(data))
			{
				var saved = AppDomain.CurrentDomain.GetData<Foo>();

				Assert.Same(data, saved);
			}
		}

		[Fact]
		public void WhenDisposableIsDisposed_ThenDataIsRemoved()
		{
			var data = new Foo { Id = 5 };

			using (AppDomain.CurrentDomain.SetData(data))
			{
			}

			var saved = AppDomain.CurrentDomain.GetData<Foo>();

			Assert.Null(saved);
		}

		[Fact]
		public void WhenCurrentDataIsSet_ThenCanRetrieveIt()
		{
			var data = new Foo { Id = 5 };

			using (AppDomainData.SetData(data))
			{
				var saved = AppDomainData.GetData<Foo>();

				Assert.Same(data, saved);
			}
		}

		[Fact]
		public void WhenCurrentDisposableIsDisposed_ThenDataIsRemoved()
		{
			var data = new Foo { Id = 5 };

			using (AppDomainData.SetData(data))
			{
			}

			var saved = AppDomainData.GetData<Foo>();

			Assert.Null(saved);
		}


		public class Foo
		{
			public int Id { get; set; }
		}
	}
}