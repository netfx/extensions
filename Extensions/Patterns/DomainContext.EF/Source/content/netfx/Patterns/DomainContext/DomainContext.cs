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
using System.Data.Entity;
using System.Diagnostics;
using System.Reflection;
using System.Data.Objects;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Collections.Concurrent;
using System.Data.Entity.Validation;
using System.Collections;
using System.Linq.Expressions;
using System.IO;
using System.CodeDom.Compiler;
using System.Dynamic;

/// <summary>
/// Base class for domain contexts. 
/// </summary>
/// <typeparam name="TContextInterface">The type of the context interface, used to determine and 
/// validate the aggregate roots exposed by the context.</typeparam>
/// <typeparam name="TId">The type of identifiers used by this context aggregate roots 
/// (it is be the same for all aggregate roots). Must be a value type or a string.</typeparam>
/// <remarks>
/// <para>
/// This context is smart enough to automatically process insert or updates of entire 
/// object graphs from aggregate roots. The deeper the graphs and the relationships, 
/// the more expensive save operations are, but the behavior is consistent and transparent 
/// to the user, and makes modeling domain behavior much easier than dealing with explicit 
/// updates of collections and references.
/// </para>
/// <para>
/// The <typeparamref name="TContextInterface"/> is used to determine what type are 
/// the aggregate roots for the context. All public <see cref="IQueryable{T}"/> properties 
/// of this interface are considered aggregate roots. All such properties whose <c>T</c> 
/// type parameter does not implement the <see cref="IAggregateRoot{TId}"/> interface compatible 
/// with the context <typeparamref name="TId"/> parameter cause an exception that is logged 
/// at static initialization time.
/// </para>
/// <para>
/// All entity types tracked by this context must implement <see cref="IIdentifiable{T}"/>, unless 
/// they are complex types. This ensures proper updating/deleting behavior for object graphs.
/// </para>
/// <para>
/// A domain context is configured by default with the following properties:
/// <code>
/// this.Configuration.AutoDetectChangesEnabled = true;
/// this.Configuration.LazyLoadingEnabled = true;
/// this.Configuration.ProxyCreationEnabled = true;
/// this.Configuration.ValidateOnSaveEnabled = true;
/// </code>
/// This can be changed in a derived class constructor.
/// </para>
/// </remarks>
/// <nuget id="netfx-Patterns.DomainContext.EF" />
public abstract partial class DomainContext<TContextInterface, TId> : DbContext, IDomainContext<TId>
	where TContextInterface : class
	where TId : IComparable
{
	private static MethodInfo SaveOrUpdateMethod;
	private static MethodInfo ValidateEntityMethod;
	private static MethodInfo DeleteStoredEntityMethod;
	private static MethodInfo CastDbEntryMethod;
	private static HashSet<Type> AggregateRoots;

	/// <summary>
	/// Entity types that have been already been validated for IIdentifiable recursively.
	/// </summary>
	private Dictionary<Type, Type> validatedTypes = new Dictionary<Type, Type>();
	private ITraceSource tracer;

	/// <summary>
	/// Validates the domain context, so that runtime behavior can be 
	/// guaranteed to succeed.
	/// </summary>
	static DomainContext()
	{
		var tracer = Tracer.GetSourceFor<DomainContext<TContextInterface, TId>>();
		if (!typeof(TId).IsValueType && typeof(TId) != typeof(string))
		{
			tracer.TraceError("TId can only be a value type or a string.");
			throw new NotSupportedException("TId can only be a value type or a string.");
		}

		var queryables = typeof(TContextInterface)
			// All public properties of IDomainContext
			.GetProperties()
			// That are of type IQueryable<T>
			.Where(prop => prop.PropertyType.IsGenericType && (IsQueryable(prop.PropertyType) || IsDbSet(prop.PropertyType)))
			.Select(prop => new { Property = prop, EntityType = TypeSystem.GetElementType(prop.PropertyType) });

		// Throw for queryables that aren't even identifiables.
		var errors = string.Join(Environment.NewLine, queryables
			.Where(x => !x.EntityType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IIdentifiable<>)))
			.Select(property => string.Format("The context has queryable property '{0}' which exposes an entity that does not implement required IIdentifiable interface.",
				property.Property.Name, property.EntityType.Name)));

		if (errors.Length > 0)
		{
			tracer.TraceError(errors);
			throw new NotSupportedException(errors);
		}

		SaveOrUpdateMethod = typeof(DomainContext<TContextInterface, TId>).GetMethod("SaveOrUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
		ValidateEntityMethod = typeof(DomainContext<TContextInterface, TId>).GetMethod("ValidateIdentifiableEntity", BindingFlags.NonPublic | BindingFlags.Instance);
		DeleteStoredEntityMethod = typeof(DomainContext<TContextInterface, TId>).GetMethod("DeleteStoredTypedImpl", BindingFlags.NonPublic | BindingFlags.Instance);
		CastDbEntryMethod = typeof(DbEntityEntry).GetMethod("Cast", BindingFlags.Public | BindingFlags.Instance);

		AggregateRoots = new HashSet<Type>(queryables
			// Only those that are aggregate roots, 'cause we might expose a non-aggregate root for querying purposes only.
			.Where(prop => typeof(IAggregateRoot<TId>).IsAssignableFrom(prop.EntityType))
			.Select(x => x.EntityType));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainContext&lt;TContextInterface, TId&gt;"/> class 
	/// with the default connection name.
	/// </summary>
	protected DomainContext()
	{
		Initialize();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainContext&lt;TContextInterface, TId&gt;"/> class 
	/// with the given connection string or name.
	/// </summary>
	protected DomainContext(string nameOrConnectionString)
		: base(nameOrConnectionString)
	{
		Initialize();
	}

	void IDomainContext<TId>.SaveChanges()
	{
		this.SaveChanges();
	}

	/// <summary>
	/// Saves all changes made in this context to the underlying database.
	/// </summary>
	/// <returns>
	/// The number of objects written to the underlying database.
	/// </returns>
	/// <exception cref="InvalidOperationException">Thrown if the context has been disposed.</exception>
	/// <exception cref="DbEntityValidationException">Thrown if validation fails for any of the entities 
	/// with pending changes in the context.</exception>
	public override int SaveChanges()
	{
		var errors = this.GetValidationErrors();
		if (errors.Any())
			throw new DbFormattedValidationException(errors);

		OnContextSavingChanges();
		var count = base.SaveChanges();
		OnContextSavedChanges();

		return count;
	}

	/// <summary>
	/// Creates a new instance of an aggregate root.
	/// </summary>
	/// <typeparam name="T">Type of aggregate root to instantiate.</typeparam>
	public T New<T>(Action<T> initializer = null)
		where T : class, IAggregateRoot<TId>
	{
		var entity = ((IObjectContextAdapter)this).ObjectContext.CreateObject<T>();

		InitializeDomainContextAccessor(entity);
		OnEntityCreated(entity);

		if (initializer != null)
			initializer(entity);

		return entity;
	}

	/// <summary>
	/// Finds the aggregate root with the specified id.
	/// </summary>
	public T Find<T>(TId id)
		where T : class, IAggregateRoot<TId>
	{
		return this.Set<T>().Find(id);
	}

	/// <summary>
	/// Inserts or updates the specified aggregate root.
	/// </summary>
	public void Save<T>(T entity)
		where T : class, IAggregateRoot<TId>
	{
		SaveOrUpdate<T>(entity, new ConcurrentDictionary<Type, HashSet<object>>());
	}

	/// <summary>
	/// Logically deletes the specified aggregate root.
	/// </summary>
	public void Delete<T>(T entity)
		where T : class, IAggregateRoot<TId>
	{
		var saved = this.Set<T>().Find(entity.Id);

		saved.IsDeleted = true;
	}

	/// <summary>
	/// Logically deletes the aggregate root with the specified identifier.
	/// </summary>
	public void Delete<T>(TId id)
		where T : class, IAggregateRoot<TId>
	{
		var saved = this.Set<T>().Find(id);

		saved.IsDeleted = true;
	}

	/// <summary>
	/// Called when the context is created.
	/// </summary>
	partial void OnContextCreated();

	/// <summary>
	/// Called when an entity has been created either by calling 
	/// <see cref="New"/> or by loading it from the underlying 
	/// storage.
	/// </summary>
	partial void OnEntityCreated(object entity);

	/// <summary>
	/// Called when pending changes have been saved.
	/// </summary>
	partial void OnContextSavedChanges();

	/// <summary>
	/// Called before saving changes in the context.
	/// </summary>
	partial void OnContextSavingChanges();

	/// <summary>
	/// Called before saving an entity in the context. This occurs 
	/// before the context changes have been saved and persisted 
	/// to the underlying storage.
	/// </summary>
	partial void OnEntitySaving<T>(T entity);

	/// <summary>
	/// Called after saving an entity in the context. This occurs 
	/// before the context changes have been saved and persisted 
	/// to the underlying storage.
	/// </summary>
	partial void OnEntitySaved<T>(T entity);

	/// <summary>
	/// Initializes default settings for the context.
	/// </summary>
	private void Initialize()
	{
		this.tracer = Tracer.GetSourceFor(this.GetType());
		this.Configuration.AutoDetectChangesEnabled = true;
		this.Configuration.LazyLoadingEnabled = true;
		this.Configuration.ProxyCreationEnabled = true;
		this.Configuration.ValidateOnSaveEnabled = true;

		OnContextCreated();

		((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized += OnObjectMaterialized;
	}

	/// <summary>
	/// Hook into the underlying EF materialization so that we can provide our own 
	/// extensibility hook for partial classes (i.e. DomainEvents).
	/// </summary>
	private void OnObjectMaterialized(object sender, ObjectMaterializedEventArgs e)
	{
		InitializeDomainContextAccessor(e.Entity);
		OnEntityCreated(e.Entity);
	}

	/// <summary>
	/// Sets the entity DomainContext property if the <see cref="IDomainContextAccessor{T}"/> 
	/// interface is implemented by it.
	/// </summary>
	private void InitializeDomainContextAccessor(object entity)
	{
		var contextConsumer = entity as IDomainContextAccessor<TContextInterface>;
		if (contextConsumer != null)
			contextConsumer.Context = this as TContextInterface;
	}

	/// <summary>
	/// Gets the object context.
	/// </summary>
	private ObjectContext ObjectContext
	{
		get { return ((IObjectContextAdapter)this).ObjectContext; }
	}

	/// <summary>
	/// Validates the entity.
	/// </summary>
	protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entry, IDictionary<object, object> items)
	{
		var result = base.ValidateEntity(entry, items);

		Type entityType = GetEntityType(entry);

		if (!validatedTypes.ContainsKey(entityType))
		{
			ThrowIfNotIdentifiable(entityType);

			// Build cache of generic methods for faster calls?
			var validateMethod = ValidateEntityMethod.MakeGenericMethod(new Type[] { entityType });
			var castMethod = CastDbEntryMethod.MakeGenericMethod(new Type[] { entityType });

			Invoke(() => validateMethod.Invoke(this, new[] { castMethod.Invoke(entry, null), result }));
		}

		return result;
	}

	/// <summary>
	/// Validates that all properties that are references are identifiable.
	/// </summary>
	private void ValidateIdentifiableEntity<TEntity>(DbEntityEntry<TEntity> entry, DbEntityValidationResult result)
		where TEntity : class
	{
		var entity = (dynamic)entry.Entity;
		foreach (var property in typeof(TEntity).GetProperties()
			.Where(x => x.CanRead && x.CanWrite))
		{
			var memberEntry = entry.Member(property.Name);
			if (memberEntry is DbCollectionEntry)
			{
				ThrowIfNotIdentifiable(property.PropertyType);
				var elementType = TypeSystem.GetElementType(property.PropertyType);
				// We only validate first entry as we just care about the types.
				var collection = (IEnumerable)property.GetValue(entry.Entity, new object[0]);
				if (collection != null)
				{
					var reference = collection.Cast<object>().FirstOrDefault();
					if (reference != null)
						ValidateEntity(this.Entry(reference), null);
				}
			}
			else if (memberEntry is DbReferenceEntry)
			{
				ThrowIfNotIdentifiable(property.PropertyType);

				var reference = property.GetValue(entity, new object[0]);
				// Validate recursively the structure.
				if (reference != null)
					ValidateEntity(this.Entry(reference), null);
			}
		}
	}

	/// <summary>
	/// Processes the entire entity graph by comparing it with the persisted state, 
	/// including all references so that collections and references can be automatically 
	/// updated.
	/// </summary>
	private void SaveOrUpdate<TEntity>(TEntity entity, ConcurrentDictionary<Type, HashSet<object>> processedIds)
		where TEntity : class
	{
		OnEntitySaving(entity);

		var idType = GetIdType(typeof(TEntity));
		IComparable defaultId = idType.IsValueType ? (IComparable)Activator.CreateInstance(idType) : (IComparable)null;
		IComparable entityId = ((dynamic)entity).Id;

		if (entityId.CompareTo(defaultId) == 0)
		{
			this.Entry(entity).State = EntityState.Added;
		}
		else if (!processedIds.GetOrAdd(typeof(TEntity), type => new HashSet<object>()).Contains(entityId))
		{
			processedIds[typeof(TEntity)].Add(entityId);

			var stored = QueryIncludingAllReferencesAsNoTracking(entity).SingleOrDefault(BuildEquals<TEntity>(entityId, idType));
			if (stored == null)
			{
				this.Entry(entity).State = EntityState.Added;
			}
			else
			{
				var entry = this.Entry(entity);
				entry.State = EntityState.Modified;
				entry.OriginalValues.SetValues(entry.GetDatabaseValues());

				UpdateReferences(entity, stored, processedIds);
			}
		}

		OnEntitySaved(entity);
	}

	/// <summary>
	/// Updates the collections and references reached by the entity, by comparing them 
	/// with the persisted state.
	/// </summary>
	private void UpdateReferences<TEntity>(TEntity entity, TEntity stored, ConcurrentDictionary<Type, HashSet<object>> processedIds)
		where TEntity : class
	{
		var entry = this.Entry(entity);

		var properties = typeof(TEntity).GetProperties().Where(x => x.CanRead);
		foreach (var property in properties)
		{
			var memberEntry = entry.Member(property.Name);
			if (memberEntry is DbCollectionEntry)
			{
				ThrowIfNotIdentifiable(property.PropertyType);

				var collectionEntry = (DbCollectionEntry)memberEntry;
				var collection = (IEnumerable)memberEntry.CurrentValue ?? Enumerable.Empty<object>();
				var references = collection.Cast<dynamic>().ToArray();
				var referenceEntityType = TypeSystem.GetElementType(property.PropertyType);
				var typedUpdateMethod = SaveOrUpdateMethod.MakeGenericMethod(new Type[] { referenceEntityType });

				// Recursively save or update each referenced entity
				foreach (var reference in references)
				{
					Invoke(() => typedUpdateMethod.Invoke(this, new object[] { reference, processedIds }));
				}

				var storedCollection = (IEnumerable)property.GetValue(stored, new object[0]) ?? Enumerable.Empty<object>();
				var storedReferences = storedCollection.Cast<dynamic>();

				if (IsAggregateRoot(referenceEntityType))
				{
					// For aggregate roots, we just delete the relationship.
					foreach (var storedReference in storedReferences.Where(x => !references.Any(r => r.Id == x.Id)).ToArray())
					{
						DeleteRelationship<TEntity>(entity, property, referenceEntityType, storedReference.Id);
					}
				}
				else
				{
					// Delete previously existing collection items that don't exist 
					// anymore on the entity relationship if the referenced entities 
					// are not aggregate roots.
					foreach (var storedReference in storedReferences.Where(x => !references.Any(r => r.Id == x.Id)).ToArray())
					{
						DeleteStoredEntity(referenceEntityType, entity, (object)storedReference.Id);
					}
				}
			}
			else if (memberEntry is DbReferenceEntry)
			{
				ThrowIfNotIdentifiable(property.PropertyType);

				var referenceEntityType = TypeSystem.GetElementType(property.PropertyType);
				var reference = memberEntry.CurrentValue;
				if (reference != null)
				{
					Invoke(() => SaveOrUpdateMethod.MakeGenericMethod(new Type[] { referenceEntityType }).Invoke(this, new object[] { reference, processedIds }));
					if (this.Entry(entity).State != EntityState.Added)
					{
						// If we don't refresh, the state manager things there are no changes to save and 
						// fails with a concurrency exception :S
						this.ObjectContext.Refresh(RefreshMode.ClientWins, entity);
					}
				}

				// Need to determine if we need to delete a relationship.
				var storedReference = property.GetValue(stored, new object[0]);
				if (storedReference != null)
				{
					IComparable storedId = ((dynamic)storedReference).Id;
					// If there was an existing stored reference, 
					// and the new value is null or it has changed
					// We need to either delete the old subsidiary 
					// entity or remove the existing relationship 
					// with the old aggregate root.
					if (reference == null ||
						((IComparable)((dynamic)reference).Id).CompareTo(storedId) != 0)
					{
						if (IsAggregateRoot(property.PropertyType))
						{
							DeleteRelationship<TEntity>(entity, property, referenceEntityType, storedId);
						}
						else
						{
							// For non-aggregate roots, EF already takes care of 
							// deleting the null referenced entity.
							DeleteStoredEntity(referenceEntityType, entity, storedId);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// This is needed because apparently EF can't build this expression properly.
	/// </summary>
	private Expression<Func<TEntity, bool>> BuildEquals<TEntity>(object id, Type idType)
		where TEntity : class
	{
		// We know there is an Id property because the entity type must be IIdentifiable<TId>.
		var property = typeof(TEntity).GetProperty("Id");
		var entity = Expression.Parameter(typeof(TEntity));
		var lambda = Expression.Lambda<Func<TEntity, bool>>(
			Expression.Equal(
				Expression.MakeMemberAccess(entity, property),
				Expression.Constant(id, idType)), entity);

		return lambda;
	}

	/// <summary>
	/// Includes all direct references as no tracking for the purpose of comparing 
	/// with the saved state.
	/// </summary>
	private IQueryable<TEntity> QueryIncludingAllReferencesAsNoTracking<TEntity>(TEntity entity)
		where TEntity : class
	{
		var entry = this.Entry(entity);
		var dbSet = this.Set<TEntity>();

		var result = dbSet.AsNoTracking<TEntity>();

		var properties = typeof(TEntity).GetProperties().Where(x => x.CanRead);
		foreach (var property in properties)
		{
			var memberEntry = entry.Member(property.Name);
			if (memberEntry != null)
			{
				if (memberEntry is DbCollectionEntry || memberEntry is DbReferenceEntry)
				{
					result = result.Include(property.Name);
				}
			}
		}

		return result;
	}

	/// <summary>
	/// Deletes a relationship to an aggregate root.
	/// </summary>
	private void DeleteRelationship<TEntity>(TEntity entity, PropertyInfo property, Type referenceEntityType, object storedId) where TEntity : class
	{
		// For aggregate roots, we just delete the relationship.
		var referencedEntity = this.Set(referenceEntityType).Find(storedId);
		this.ObjectContext.ObjectStateManager.ChangeRelationshipState(
			entity, referencedEntity, property.Name, EntityState.Deleted);
	}

	/// <summary>
	/// Deletes an entity.
	/// </summary>
	private void DeleteStoredEntity(Type referenceEntityType, object owningEntity, object referenceId)
	{
		Invoke(() => DeleteStoredEntityMethod.MakeGenericMethod(referenceEntityType)
			.Invoke(this, new[] { owningEntity, referenceId }));
	}

	private void DeleteStoredTypedImpl<TEntity>(object owningEntity, object referenceId)
		where TEntity : class
	{
		var referenceEntityType = typeof(TEntity);
		// Load it again with tracking to mark deleted.
		var referencedSet = this.Set<TEntity>();
		var referencedEntity = referencedSet.Find(referenceId);
		var deletedEntry = this.Entry(referencedEntity);

		// Note that rather than doing logical delete, for non-aggregate 
		// roots we directly delete the entity.

		var properties = referenceEntityType.GetProperties().Where(x => x.CanRead);
		foreach (var property in properties)
		{
			var memberEntry = deletedEntry.Member(property.Name);
			if (memberEntry is DbCollectionEntry)
			{
				ThrowIfNotIdentifiable(property.PropertyType);

				var deletedEntityType = TypeSystem.GetElementType(property.PropertyType);

				// Aggregate root references dissapear automatically as we're deleting 
				// the referencing entity too.
				if (!IsAggregateRoot(deletedEntityType))
				{
					var collectionEntry = (DbCollectionEntry)memberEntry;
					var references = ((IEnumerable)memberEntry.CurrentValue).Cast<dynamic>().ToArray();

					foreach (var reference in references)
					{
						DeleteStoredEntity(deletedEntityType, (object)reference, (object)reference.Id);
					}
				}
			}
			else if (memberEntry is DbReferenceEntry)
			{
				ThrowIfNotIdentifiable(property.PropertyType);

				var deletedEntityType = TypeSystem.GetElementType(property.PropertyType);
				var reference = memberEntry.CurrentValue;
				if (reference != null && !IsAggregateRoot(property.PropertyType))
					DeleteStoredEntity(deletedEntityType, reference, (object)((dynamic)reference).Id);
			}
		}

		// Loading the entity for deleting causes the in-memory 
		// reference to be set again, so we refresh with the 
		// client value that was set to null.
		deletedEntry.State = EntityState.Deleted;
		this.ObjectContext.Refresh(RefreshMode.ClientWins, owningEntity);
	}

	/// <summary>
	/// Determines whether the given type is an aggregate root..
	/// </summary>
	private bool IsAggregateRoot(Type type)
	{
		return AggregateRoots.Contains(type);
	}

	/// <summary>
	/// Determines whether the given type is a DbSet.
	/// </summary>
	private static bool IsDbSet(Type type)
	{
		return type.GetGenericTypeDefinition() == typeof(DbSet<>) ||
			type.GetGenericTypeDefinition() == typeof(IDbSet<>);
	}

	/// <summary>
	/// Determines whether the given type is an <see cref="IQueryable"/>.
	/// </summary>
	private static bool IsQueryable(Type type)
	{
		return type.GetGenericTypeDefinition() == typeof(IQueryable<>);
	}

	/// <summary>
	/// Gets the type of identifier from the <see cref="IIdentifiable{T}"/> 
	/// interface implemented by the given type.
	/// </summary>
	private Type GetIdType(Type type)
	{
		return TypeSystem.GetElementType(type)
			.GetInterfaces()
			.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IIdentifiable<>))
			.Select(x => x.GetGenericArguments()[0])
			.First();
	}

	/// <summary>
	/// Throws if the given entity type does not implement <see cref="IIdentifiable{T}"/>.
	/// </summary>
	private void ThrowIfNotIdentifiable(Type entityType)
	{
		if (!TypeSystem.GetElementType(entityType)
			.GetInterfaces()
			.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IIdentifiable<>)))
			throw new NotSupportedException(string.Format(
				"Domain context requires all entities to implement IIdentifiable<TId>. Invalid entity type {0}.",
				entityType.Name));
	}

	/// <summary>
	/// Invokes the specified action and rethrows the inner exception if a
	/// reflection target invocation exception happens.
	/// </summary>
	private void Invoke(Action invoker)
	{
		try
		{
			invoker();
		}
		catch (TargetInvocationException tie)
		{
			throw tie.InnerException;
		}
	}

	private static Type GetEntityType(DbEntityEntry entry)
	{
		dynamic entity = entry.AsDynamicReflection();

		return entity.InternalEntry.EntityType;
	}

	/// <summary>
	/// Provides human-readable validation errors.
	/// </summary>
	private class DbFormattedValidationException : DbEntityValidationException
	{
		public DbFormattedValidationException(IEnumerable<DbEntityValidationResult> validationResults)
			: base(BuildMessage(validationResults), validationResults)
		{
		}

		private static string BuildMessage(IEnumerable<DbEntityValidationResult> validationResults)
		{
			var result = new StringWriter();
			var writer = new IndentedTextWriter(result, "\t");

			foreach (var entityError in validationResults)
			{
				var entity = (dynamic)entityError.Entry.Entity;
				writer.Indent++;
				writer.WriteLine("{0} Id = {1}", entityError.Entry.Entity, entity.Id);

				writer.Indent++;

				foreach (var validationError in entityError.ValidationErrors)
				{
					writer.WriteLine("{0}.{1}: {2}",
						GetEntityType(entityError.Entry),
						validationError.PropertyName,
						validationError.ErrorMessage);
				}

				writer.Indent--;
				writer.Indent--;
			}

			writer.Flush();

			return result.ToString();
		}
	}

	/// <summary>
	/// This is basically what everyone else (OData, MSDN samples, etc.) 
	/// use for determining the element type given a sequence type (i.e. IEnumerable{T}).
	/// </summary>
	private static class TypeSystem
	{
		internal static Type GetElementType(Type seqType)
		{
			Type ienum = FindIEnumerable(seqType);
			if (ienum == null) return seqType;
			return ienum.GetGenericArguments()[0];
		}

		private static Type FindIEnumerable(Type seqType)
		{
			if (seqType == null || seqType == typeof(string))
				return null;

			if (seqType.IsArray)
				return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

			if (seqType.IsGenericType)
			{
				foreach (Type arg in seqType.GetGenericArguments())
				{
					Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
					if (ienum.IsAssignableFrom(seqType))
					{
						return ienum;
					}
				}
			}

			Type[] ifaces = seqType.GetInterfaces();
			if (ifaces != null && ifaces.Length > 0)
			{
				foreach (Type iface in ifaces)
				{
					Type ienum = FindIEnumerable(iface);
					if (ienum != null) return ienum;
				}
			}

			if (seqType.BaseType != null && seqType.BaseType != typeof(object))
			{
				return FindIEnumerable(seqType.BaseType);
			}

			return null;
		}
	}
}