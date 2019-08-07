## `DataStore`

`spyderSoft.DataLayer.Core.IDataStore` implementation which utilizes the EntityFramework for data storage and retrieval
```csharp
public class spyderSoft.DataLayer.Core.EntityFramework.DataStore
    : IDataStore

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | DataStorePath | Gets or sets the data store path. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | DeleteItem(`IDataItem` itemToDelete) | Deletes a record from the database. | 
| `void` | DeleteItems(`IEnumerable<IDataItem>` itemsToDelete) | Deletes multiple records from the database. | 
| `Expression<Func<TDataContract, Boolean>>` | FixDynamicExpression(`Expression<Func<TDataContract, Boolean>>` predicate) | Fixes the dynamic expression.  LINQ to Entity has issues with expressions  built via PredicateBuilder, so we serialize and deserialize the predicate to make it usable. | 
| `T` | GetItem(`Int64` key) | Gets a record from the database. | 
| `IEnumerable<T>` | GetItems(`Expression<Func<T, Boolean>>` predicate, `Int32` skip, `Int32` take) | Gets records from the database. | 
| `IEnumerable<T>` | GetItems(`Expression<Func<T, Boolean>>` predicate) | Gets records from the database. | 
| `IEnumerable<T>` | GetItems() | Gets records from the database. | 
| `IEnumerable<T>` | GetItems(`Int32` skip, `Int32` take) | Gets records from the database. | 
| `Int32` | GetItemsCount(`Expression<Func<T, Boolean>>` predicate) | Gets the count or records based on the query predicate. | 
| `Int32` | GetItemsCount() | Gets the count or records based on the query predicate. | 
| `IQueryable<T>` | GetItemsQueryable() | Gets the items queryable. | 
| `T` | SaveItem(`T` itemToSave) | Saves a record to the database. | 
| `IEnumerable<T>` | SaveItems(`IEnumerable<T>` itemsToSave) | Saves m ultiple records to the database. | 


## `DynamicDbContext`

Abstract class DynamicDbContext.  Implementations of this class are required to use the [spyderSoft.DataLayer.Core.EntityFramework.DataStore](spyderSoft.DataLayer.Core.EntityFramework.md#datastore) class.
```csharp
public abstract class spyderSoft.DataLayer.Core.EntityFramework.DynamicDbContext
    : DbContext, IDisposable, IInfrastructure<IServiceProvider>, IDbContextDependencies, IDbSetCache, IDbQueryCache, IDbContextPoolable

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | ContextSupports(`T` item) | Checks if the context supports the given type. | 
| `void` | MapTableObjects(`ModelBuilder` modelBuilder) | This method must be implemented in an extending class in order to map decorated  POCO objects to data tables based on their attribute values. | 
| `void` | MapTableToObject(`ModelBuilder` modelBuilder) | Maps the table to object. | 
| `void` | OnModelCreating(`ModelBuilder` modelBuilder) | This method is called when the model for a derived context has been initialized, but  before the model has been locked down and used to initialize the context.  The default  implementation of this method does nothing, but it can be overridden in a derived class  such that the model can be further configured before it is locked down. | 


## `IDataContextProvider`

Interface IDataContextProvider.  Implementations of this interface must be injected in to [spyderSoft.DataLayer.Core.EntityFramework.DataStore](spyderSoft.DataLayer.Core.EntityFramework.md#datastore) via the constructor  to provide it with the appropriate `Microsoft.EntityFrameworkCore.DbContext` implementations.
```csharp
public interface spyderSoft.DataLayer.Core.EntityFramework.IDataContextProvider

```

Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `DbContext` | GetDbContext() | Gets the database context. | 
| `DbContext` | GetDbContext(`IDataItem` item) | Gets the database context. | 
| `void` | Initialize() | Initializes the specified data store path. | 
| `Boolean` | VerifyContext() | Verifies the current instance of this context provider is valid. | 


## `TypeExtensions`

Class TypeExtensions.  This class is a helper class to assist in reflection against generic methods, which  the .NET Type.GetMethod does not handle well.
```csharp
public static class spyderSoft.DataLayer.Core.EntityFramework.TypeExtensions

```

Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `MethodInfo` | GetGenericMethod(this `Type` genericType, `String` name, `Type[]` parameterTypes) | This extension method gets a particular generic method based on the calling type  and the name and parameter types of the desired variant of the method. | 


