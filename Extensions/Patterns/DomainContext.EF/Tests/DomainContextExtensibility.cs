using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Data.Entity;

namespace Tests
{
	public class DomainContextExtensibility
	{
		public DomainContextExtensibility()
		{
			Database.SetInitializer<TestContext>(new DropCreateDatabaseAlways<TestContext>());
			using (var db = new TestContext())
			{
				db.Database.Initialize(true);
			}
		}

		[Fact]
		public void WhenContextCreated_ThenInvokesExtensibilityHook()
		{
			using (var context = new TestContext())
			{
				Assert.True(context.OnContextCreatedCalled);
			}
		}

		[Fact]
		public void WhenContextSavesChanges_ThenInvokesExtensibilityHooks()
		{
			using (var context = new TestContext())
			{
				context.Persist(new Foo());

				context.SaveChanges();

				Assert.True(context.OnContextSavingChangesCalled);
				Assert.True(context.OnContextSavedChangesCalled);
			}
		}

		[Fact]
		public void WhenSavingEntity_ThenInvokesExtensibilityHooks()
		{
			using (var context = new TestContext())
			{
				var foo = new Foo();
				context.Persist(foo);

				Assert.Equal(1, context.OnEntitySavingCalls.Count);
				Assert.Equal(1, context.OnEntitySavedCalls.Count);
				Assert.Same(foo, context.OnEntitySavingCalls[0]);
				Assert.Same(foo, context.OnEntitySavedCalls[0]);
			}
		}

		[Fact]
		public void WhenRootWithDependentsAdded_ThenInvokesExtensibilityHooksForRootOnly()
		{
			using (var context = new TestContext())
			{
				var foo = new Foo { Content = new Content() };
				context.Persist(foo);

				Assert.Equal(1, context.OnEntitySavingCalls.Count);
				Assert.Equal(1, context.OnEntitySavedCalls.Count);
				Assert.Same(foo, context.OnEntitySavingCalls[0]);
				Assert.Same(foo, context.OnEntitySavedCalls[0]);
			}
		}

		[Fact]
		public void WhenRootWithDependentsUpdated_ThenInvokesExtensibilityHooksForBoth()
		{
			var foo = new Foo { Content = new Content() };
			using (var context = new TestContext())
			{
				context.Persist(foo);

				context.SaveChanges();
			}

			using (var context = new TestContext())
			{
				var saved = context.Find<Foo>(foo.Id);

				saved.Content.Payload = "foo";

				context.Persist(saved);
				context.SaveChanges();

				Assert.Equal(2, context.OnEntitySavingCalls.Count);
				Assert.Equal(2, context.OnEntitySavedCalls.Count);
				Assert.Same(saved, context.OnEntitySavingCalls[0]);
				Assert.Same(saved.Content, context.OnEntitySavingCalls[1]);
				Assert.Same(saved.Content, context.OnEntitySavedCalls[0]);
				Assert.Same(saved, context.OnEntitySavedCalls[1]);
			}
		}

	}
}
