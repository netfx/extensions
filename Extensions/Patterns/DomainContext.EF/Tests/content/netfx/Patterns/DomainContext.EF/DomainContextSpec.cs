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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);
			}
		}

		[Fact]
		public void WhenSavingWithCircularReference_ThenSucceeds()
		{
			var foo = new Foo { Name = "Foo" };
			var bar = new Bar { Parent = foo };
			foo.Bars.Add(bar);
			using (var db = new TestContext())
			{
				db.Persist(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				var saved = db.Foos.FirstOrDefault(x => x.Name == foo.Name);
				Assert.Equal(1, saved.Bars.Count);
				Assert.Same(saved, saved.Bars[0].Parent);
			}
		}

		[Fact]
		public void WhenIncludingDependent_ThenSucceeds()
		{
			var foo = new Foo { Name = "Foo", Baz = { new Baz() } };
			using (var db = new TestContext())
			{
				db.Persist(foo);
				db.SaveChanges();
			}

			using (var db = new TestContext())
			{
				IQueryable<Foo> foos = db.Foos;
				var saved = foos.Include(x => x.Baz).FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);
				Assert.True(saved.Baz.Any());
			}
		}

		[Fact(Skip = "Can't figure out how to use string paths :S")]
		public void WhenIncludingDependentAsString_ThenSucceeds()
		{
			var foo = new Foo { Name = "Foo", Bars = { new Bar { Baz = { new Baz() } } } };
			using (var db = new TestContext())
			{
				db.Persist(foo);
				db.SaveChanges();
			}
			
			using (var db = new TestContext())
			{
				IQueryable<Foo> foos = db.Foos;
				var saved = foos.Include(x => "Bars").FirstOrDefault(x => x.Name == foo.Name);
				Assert.NotNull(saved);
				Assert.True(saved.Bars.Any());
			}
		}

		[Fact]
		public void WhenNewEntity_ThenCanInitializeAndFindIt()
		{
			var id = default(long);
			using (var db = new TestContext())
			{
				var foo = db.New<Foo>(x => x.Name = "foo");

				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
				db.Persist(foo);
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
		public void WhenContextWithIdType_ThenSucceeds()
		{
			var context = new ContextWithStringIdType();
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

				context.Persist(root);
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

				context.Persist(root);
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

				db.Persist(root);
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
		public virtual System.Data.Entity.DbSet<Foo> Foos { get; set; }
		public virtual System.Data.Entity.DbSet<Baz> Baz { get; set; }
		public virtual System.Data.Entity.DbSet<User> Users { get; set; }
	}

	internal class AggregateRootContextAccessor : AggregateRoot, IDomainContextAccessor<TestContext>
	{
		[NotMapped]
		public TestContext Context { get; set; }
	}

	internal class ContextWithStringIdType : DomainContext<ContextWithStringIdType, string>
	{
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
		public virtual IList<Bar> Bars { get; set; }
		public virtual IList<Baz> Baz { get; set; }

		public virtual User User { get; set; }

		[ReadOnly(true)]
		public virtual User Owner { get; set; }

		[ReadOnly(true)]
		public virtual IList<User> Customers { get; set; }

		public virtual Content Content { get; set; }
	}

	public class Bar : AggregateRoot
	{
		public Bar()
		{
			this.Baz = new List<Baz>();
		}

		public virtual IList<Baz> Baz { get; set; }
		public virtual Foo Parent { get; set; }
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

	internal class ContextWithRootWithGeneratedIdDepedent : DomainContext<ContextWithStringIdType, long>
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