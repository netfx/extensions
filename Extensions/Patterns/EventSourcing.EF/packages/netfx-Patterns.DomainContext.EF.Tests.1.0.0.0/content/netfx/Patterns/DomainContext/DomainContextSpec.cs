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

namespace Tests
{
	public class DomainContextSpec
	{
		public DomainContextSpec()
		{
			Database.SetInitializer<TestContext>(new DropCreateDatabaseAlways<TestContext>());
			using (var db = new TestContext())
			{
				db.Database.Initialize(true);
			}
		}

		[Fact]
		public void WhenDeletingAggregateRootById_ThenMarksDeleted()
		{
			var foo = new Foo { Name = "Foo" };
			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				db.Delete<Foo>(foo.Id);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name);
				Assert.True(saved.IsDeleted);
			}
		}

		[Fact]
		public void WhenDeletingAggregateRootByEntity_ThenMarksDeleted()
		{
			var foo = new Foo { Name = "Foo" };
			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				db.Delete(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name);
				Assert.True(saved.IsDeleted);
			}
		}

		[Fact]
		public void WhenSavingAndUpdating_ThenReconnectsEntity()
		{
			var foo = new Foo { Name = "Foo" };
			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);
			}

			foo.Name = "Bar";

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);
			}
		}

		[Fact]
		public void WhenSavingInvalidEntity_ThenThrowsValidationException()
		{
			var foo = new AggregateRootWithRequiredProperty();
			using (var db = new TestContext())
			{
				db.Save(foo);
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
		public void WhenNewEntity_ThenCanInitializeAndFindIt()
		{
			var id = default(long);
			using (var db = new TestContext())
			{
				var foo = db.New<Foo>(x => x.Name = "foo");

				db.Save(foo);
				db.SaveChanges();

				id = foo.Id;
			}

			using (var db = new TestContext())
			{
				var saved = db.Find<Foo>(id);
				Assert.NotNull(saved);
				Assert.Equal("foo", saved.Name);
			}
		}

		[Fact]
		public void WhenRemovingElementsFromCollection_ThenDeletesForNonAggregateRoot()
		{
			var foo = new Foo
			{
				Name = "Foo",
				Bars =
				{
					new Bar(), 
					new Bar(),
				}
			};

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.Include(x => x.Bars).FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);
				Assert.Equal(2, saved.Bars.Count);
			}

			foo.Bars.RemoveAt(0);

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.Include(x => x.Bars).FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);
				Assert.Equal(1, saved.Bars.Count);

				Assert.Equal(1, db.Set<Bar>().Count());
			}
		}

		[Fact]
		public void WhenRemovingElementsFromCollection_ThenDoesNotDeleteEntityForAggregateRoot()
		{
			var baz1 = new Baz();
			var baz2 = new Baz();
			var foo = new Foo
			{
				Name = "Foo",
				Baz = { baz1, baz2 }
			};

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.Include(x => x.Baz).FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);
				Assert.Equal(2, saved.Baz.Count);
			}

			foo.Baz.RemoveAt(0);

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.Include(x => x.Baz).FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);
				Assert.Equal(1, saved.Baz.Count);

				// Original instance remains though.
				Assert.Equal(2, db.Set<Baz>().Count());
			}
		}

		[Fact]
		public void WhenChangingReference_ThenDoesNotDeleteEntityForAggregateRoot()
		{
			var user1 = new User();
			var user2 = new User();
			var foo = new Foo
			{
				Name = "Foo",
				User = user1,
			};

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.Include(x => x.User).FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);
				Assert.NotNull(saved.User);
			}

			foo.User = user2;

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.Include(x => x.User).FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);

				Assert.Equal(user2.Id, saved.User.Id);

				// Original instance remains though.
				Assert.Equal(2, db.Set<User>().Count());
			}
		}

		[Fact]
		public void WhenRemovingReference_ThenDoesNotDeleteEntityForAggregateRoot()
		{
			var user1 = new User();
			var foo = new Foo
			{
				Name = "Foo",
				User = user1,
			};

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.Include(x => x.User).FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);
				Assert.NotNull(saved.User);
			}

			foo.User = null;

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.Include(x => x.User).FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);

				Assert.Null(saved.User);

				// Original instance remains though.
				Assert.Equal(1, db.Set<User>().Count());
			}
		}

		[Fact]
		public void WhenSavingAndUpdatingDependentDifferentIdType_ThenReconnectsEntity()
		{
			var foo = new Foo { Name = "Foo", Content = new Content { Payload = "Hello" } };
			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name && x.Content.Payload == "Hello");
				Assert.NotNull(saved);
			}

			foo.Name = "Bar";
			foo.Content.Payload = "World";

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			Assert.Equal(foo.Name, "Bar");
			Assert.Equal(foo.Content.Payload, "World");

			using (var db = new TestContext())
			{
				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name && x.Content.Payload == foo.Content.Payload);
				Assert.NotNull(saved);
			}
		}

		[Fact]
		public void WhenSavingAndDisconnectingDependentDifferentIdType_ThenDeletesOldDependentEntity()
		{
			var foo = new Foo { Name = "Foo", Content = new Content { Payload = "Hello" } };
			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name && x.Content.Payload == "Hello");
				Assert.NotNull(saved);
			}

			foo.Name = "Bar";
			foo.Content = new Content { Payload = "World" };

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			Assert.Equal(foo.Name, "Bar");
			Assert.Equal(foo.Content.Payload, "World");

			using (var db = new TestContext())
			{
				Assert.Equal(1, db.Set<Content>().Count());

				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name);
				Assert.Equal(saved.Name, "Bar");
				Assert.Equal(saved.Content.Payload, "World");
			}
		}

		[Fact]
		public void WhenSavingAndRemovingDependentDifferentIdType_ThenDeletesOldDependentEntity()
		{
			var foo = new Foo { Name = "Foo", Content = new Content { Payload = "Hello" } };
			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name && x.Content.Payload == "Hello");
				Assert.NotNull(saved);
			}

			foo.Name = "Bar";
			foo.Content = null;

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			Assert.Equal(foo.Name, "Bar");
			Assert.Null(foo.Content);

			using (var db = new TestContext())
			{
				Assert.Equal(0, db.Set<Content>().Count());

				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name);
				Assert.Equal(saved.Name, "Bar");
				Assert.Null(foo.Content);
			}
		}

		[Fact]
		public void WhenSavingAndRemovingDependentGeneratedId_ThenDeletesOldDependentEntity()
		{
			var foo = new Foo
			{
				Name = "Foo",
				Content = new Content
				{
					Payload = "Hello",
					Image = new Image { File = "Foo.png" }
				}
			};
			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name && x.Content.Payload == "Hello");
				Assert.NotNull(saved);
			}

			foo.Name = "Bar";
			foo.Content = null;

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			Assert.Equal(foo.Name, "Bar");
			Assert.Null(foo.Content);

			using (var db = new TestContext())
			{
				Assert.Equal(0, db.Set<Content>().Count());
				Assert.Equal(0, db.Set<Image>().Count());

				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name);
				Assert.Equal(saved.Name, "Bar");
				Assert.Null(foo.Content);
			}
		}

		[Fact]
		public void WhenSavingAndRemovingDependentGeneratedIds_ThenDeletesOldDependentEntities()
		{
			var foo = new Foo
			{
				Name = "Foo",
				Content = new Content
				{
					Payload = "Hello",
					OtherImages = new List<Image>
					{
						new Image { File = "Foo.png" },
					}
				}
			};
			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name && x.Content.Payload == "Hello");
				Assert.NotNull(saved);
			}

			foo.Name = "Bar";
			foo.Content = null;

			using (var db = new TestContext())
			{
				db.Save(foo);
				db.SaveChanges();
			}

			Assert.Equal(foo.Name, "Bar");
			Assert.Null(foo.Content);

			using (var db = new TestContext())
			{
				Assert.Equal(0, db.Set<Content>().Count());
				Assert.Equal(0, db.Set<Image>().Count());

				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name);
				Assert.Equal(saved.Name, "Bar");
				Assert.Null(foo.Content);
			}
		}

		[Fact]
		public void WhenContextContainsNonIndentifiableQueryable_ThenThrowsNotSupportedException()
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
		public void WhenContextWithIdType_ThenSucceeds()
		{
			var context = new ContextWithStringIdType();
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

				context.Save(root);
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

				context.Save(root);
				Assert.Throws<NotSupportedException>(() => context.SaveChanges());
			}
		}

		[Fact]
		public void WhenDependentIdIsGenerated_ThenCanSaveRoot()
		{
			System.Data.Entity.Database.SetInitializer(
				new System.Data.Entity.DropCreateDatabaseAlways<ContextWithRootWithGeneratedIdDepedent>());

			var originalDependentId = 0L;

			using (var context = new ContextWithRootWithGeneratedIdDepedent())
			{
				var root = new RootWithGeneratedIdDependent
				{
					Name = "root",
					Dependent = new GeneratedIdDependent
					{
						Name = "dep",
						Blob = new Blob { File = "file.txt" },
					},
				};

				context.Save(root);
				context.SaveChanges();

				originalDependentId = root.Dependent.Id;
			}

			using (var context = new ContextWithRootWithGeneratedIdDepedent())
			{
				var root = context.Roots.Include(x => x.Dependent).First(x => x.Name == "root");

				Assert.Equal(originalDependentId, root.Dependent.Id);

				Assert.Equal("dep", root.Dependent.Name);
				Assert.Equal("file.txt", root.Dependent.Blob.File);

				root.Dependent = new GeneratedIdDependent
				{
					Name = "dep2",
					Blob = new Blob { File = "file2.txt" },
				};

				context.Save(root);
				context.SaveChanges();
			}

			using (var context = new ContextWithRootWithGeneratedIdDepedent())
			{
				var root = context.Roots.First(x => x.Name == "root" && x.Dependent.Name == "dep2" && x.Dependent.Blob.File == "file2.txt");

				Assert.NotNull(root);

				Assert.NotEqual(0, root.Dependent.Id);
				Assert.NotEqual(originalDependentId, root.Dependent.Id);

				Assert.Equal(1, context.Set<GeneratedIdDependent>().Count());
				Assert.Equal(1, context.Set<Blob>().Count());
			}
		}

		[Fact]
		public void WhenNewOrSavedContextConsumer_ThenSetsContextProperty()
		{
			using (var db = new TestContext())
			{
				var root = db.New<AggregateRootContextAccessor>();

				Assert.NotNull(root.Context);
				Assert.Same(db, root.Context);

				db.Save(root);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.ContextConsumer.First();
				Assert.NotNull(saved);
				Assert.NotNull(saved.Context);
				Assert.Same(db, saved.Context);
			}
		}

	}

	public class AggregateRoot : IAggregateRoot<long>
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public virtual long Id { get; set; }

		public virtual bool IsDeleted { get; set; }
	}

	public class TestContext : DomainContext<TestContext, long>
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

	public class AggregateRootContextAccessor : AggregateRoot, IDomainContextAccessor<TestContext>
	{
		[NotMapped]
		public TestContext Context { get; set; }
	}

	public class InvalidContextRoot : DomainContext<InvalidContextRoot, long>
	{
		public System.Data.Entity.DbSet<NonAggregateRoot> NonRoot { get; set; }
	}

	public class ValidContextWithNonIdentiableDependentEntity : DomainContext<ValidContextWithNonIdentiableDependentEntity, long>
	{
		public System.Data.Entity.DbSet<AggregateRootWithNonIdentifiableReference> Root { get; set; }
	}

	public class ContextWithStringIdType : DomainContext<ContextWithStringIdType, string>
	{
	}

	public class ContextWithInvalidIdType : DomainContext<ContextWithInvalidIdType, ContextWithInvalidIdType.InvalidKeyType>
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

	public class Foo : AggregateRoot
	{
		public Foo()
		{
			this.Bars = new List<Bar>();
			this.Baz = new List<Baz>();
			this.Customers = new List<User>();
		}
		public virtual string Name { get; set; }
		public virtual List<Bar> Bars { get; set; }
		public virtual List<Baz> Baz { get; set; }

		public virtual User User { get; set; }

		[ReadOnly(true)]
		public virtual User Owner { get; set; }

		[ReadOnly(true)]
		public virtual ICollection<User> Customers { get; set; }

		public virtual Content Content { get; set; }
	}

	public class Bar : AggregateRoot
	{
	}

	public class Baz : AggregateRoot
	{
	}

	public class Content : IIdentifiable<Guid>
	{
		public Content()
		{
			this.Id = Guid.NewGuid();
		}

		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public virtual Guid Id { get; set; }

		public virtual string Payload { get; set; }

		public virtual Image Image { get; set; }

		public virtual ICollection<Image> OtherImages { get; set; }
	}

	public class Image : IIdentifiable<long>
	{
		public virtual long Id { get; set; }

		public virtual string File { get; set; }
	}

	public class User : AggregateRoot
	{
		public User()
		{
			this.BirthDate = DateTime.Parse(DateTime.UtcNow.ToString());
		}

		[ReadOnly(true)]
		public virtual DateTime BirthDate { get; set; }
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

	public class ContextWithRootWithGeneratedIdDepedent : DomainContext<ContextWithStringIdType, long>
	{
		public virtual System.Data.Entity.DbSet<RootWithGeneratedIdDependent> Roots { get; set; }
	}

	public class RootWithGeneratedIdDependent : AggregateRoot
	{
		public virtual string Name { get; set; }
		//[Required]
		public virtual GeneratedIdDependent Dependent { get; set; }
	}

	public class Entity : IIdentifiable<long>
	{
		public Entity()
		{
			//this.Id = Guid.NewGuid();
		}

		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public virtual long Id { get; set; }
	}

	public class GeneratedIdDependent : Entity
	{
		public virtual string Name { get; set; }
		[Required]
		public virtual Blob Blob { get; set; }
	}

	public class Blob : Entity
	{
		public virtual string File { get; set; }
	}
}