using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Xunit;

namespace Tests.Validations
{
	public class ValidationsSpec
	{
		public ValidationsSpec()
		{
			Database.SetInitializer<TestContext>(new DropCreateDatabaseAlways<TestContext>());
			using (var db = new TestContext())
			{
				db.Database.Initialize(true);
			}
		}

		[Fact]
		public void WhenSavingInvalidEntity_ThenThrowsValidationException()
		{
			var foo = new AggregateRootWithRequiredProperty();
			using (var db = new TestContext())
			{
				db.Persist(foo);
				try
				{
					db.SaveChanges();
					Assert.False(true, "Didn't throw");
				}
				catch (DbEntityValidationException ex)
				{
					Assert.Contains("Name", ex.Message);
				}
			}
		}

		[Fact]
		public void WhenNonAggregateRootQueryable_ThenThrowsNotSupportedException()
		{
			var init = Assert.Throws<TypeInitializationException>(() => new InvalidContextRoot());
			Assert.True(init.InnerException is NotSupportedException);
		}

		[Fact]
		public void WhenInvalidContextIdType_ThenThrowsNotSupportedException()
		{
			var init = Assert.Throws<TypeInitializationException>(() => new ContextWithInvalidIdType());
			Assert.True(init.InnerException is NotSupportedException);
		}

		[Fact]
		public void WhenSavingNonIdentifiableReferenceEntry_ThenThrowsNotSupportedException()
		{
			System.Data.Entity.Database.SetInitializer<ValidContextWithNonIdentiableDependentEntity>(
				new System.Data.Entity.DropCreateDatabaseAlways<ValidContextWithNonIdentiableDependentEntity>());

			using (var context = new ValidContextWithNonIdentiableDependentEntity())
			{
				var root = new AggregateRootWithNonIdentifiableReference
				{
					NonIdentifiable = new NonIdentifable(),
				};

				context.Persist(root);
				Assert.Throws<NotSupportedException>(() => context.SaveChanges());
			}
		}

		[Fact]
		public void WhenSavingNonIdentifiableReferenceCollectionEntry_ThenThrowsNotSupportedException()
		{
			System.Data.Entity.Database.SetInitializer<ValidContextWithNonIdentiableDependentEntity>(
				new System.Data.Entity.DropCreateDatabaseAlways<ValidContextWithNonIdentiableDependentEntity>());

			using (var context = new ValidContextWithNonIdentiableDependentEntity())
			{
				var root = new AggregateRootWithNonIdentifiableReference
				{
					NonIdentifiables = 
					{
						new NonIdentifable(),
					}
				};

				context.Persist(root);
				Assert.Throws<NotSupportedException>(() => context.SaveChanges());
			}
		}
	}

	public class AggregateRoot : IAggregateRoot<long>
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public virtual long Id { get; set; }

		public virtual bool IsDeleted { get; set; }
	}

	internal class TestContext : DomainContext<TestContext, long>
	{
		public TestContext()
			: base("EntityContextSpec")
		{

		}

		protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
			base.OnModelCreating(modelBuilder);
		}

		public virtual System.Data.Entity.DbSet<AggregateRootContextAccessor> ContextConsumer { get; set; }
		public virtual System.Data.Entity.DbSet<AggregateRootWithRequiredProperty> RequiredProperties { get; set; }
		public virtual System.Data.Entity.DbSet<Foo> Foos { get; set; }
		public virtual System.Data.Entity.DbSet<Baz> Baz { get; set; }
		public virtual System.Data.Entity.DbSet<User> Users { get; set; }
	}

	internal class InvalidContextRoot : DomainContext<InvalidContextRoot, long>
	{
		public System.Data.Entity.DbSet<NonAggregateRoot> NonRoot { get; set; }
	}

	internal class ValidContextWithNonIdentiableDependentEntity : DomainContext<ValidContextWithNonIdentiableDependentEntity, long>
	{
		public System.Data.Entity.DbSet<AggregateRootWithNonIdentifiableReference> Root { get; set; }
	}

	internal class ContextWithInvalidIdType : DomainContext<ContextWithInvalidIdType, ContextWithInvalidIdType.InvalidKeyType>
	{
		public class InvalidKeyType : IComparable
		{
			public int CompareTo(object obj)
			{
				return 0;
			}
		}
	}

	public class AggregateRootWithRequiredProperty : AggregateRoot
	{
		[Required]
		public virtual string Name { get; set; }
	}

	public class AggregateRootWithNonIdentifiableReference : IAggregateRoot<long>
	{
		public AggregateRootWithNonIdentifiableReference()
		{
			this.NonIdentifiables = new HashSet<NonIdentifable>();
		}

		public virtual bool IsDeleted { get; set; }
		public virtual long Id { get; set; }

		public virtual NonIdentifable NonIdentifiable { get; set; }

		public virtual ICollection<NonIdentifable> NonIdentifiables { get; set; }
	}

	public class NonAggregateRoot
	{
	}

	public class NonIdentifable
	{
		public virtual int Id { get; set; }
		public virtual string Title { get; set; }
	}
}