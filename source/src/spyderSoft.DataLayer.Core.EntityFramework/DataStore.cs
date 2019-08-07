using spyderSoft.DataLayer.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using spyderSoft.DataLayer.Core;
using Microsoft.EntityFrameworkCore;

namespace spyderSoft.DataLayer.Core.EntityFramework
{
    /// <summary>
    /// <see cref="IDataStore"/> implementation which utilizes the EntityFramework for data storage and retrieval
    /// </summary>
    public class DataStore : IDataStore
    {
        #region Private Members

        private IDataContextProvider _contextProvider;

        #endregion Private Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStore" /> class.
        /// </summary>
        /// <param name="dataContextProvider">The <see cref="IDataContextProvider"/> used to provide the <see cref="DbContext"/> for various items.</param>
        /// <exception cref="System.Exception">Database does not exist.</exception>
        public DataStore(IDataContextProvider dataContextProvider)
        {
            _contextProvider = dataContextProvider;
            DataStorePath = string.Empty;

            _contextProvider.Initialize();
            if (!dataContextProvider.VerifyContext())
            {
                throw new Exception("Database does not exist.");
            }
        }

        #endregion Constructor

        #region IDataStore Implementation

        /// <summary>
        /// Gets or sets the data store path.
        /// </summary>
        /// <value>The data store path.</value>
        public string DataStorePath { get; set; }

        /// <summary>
        /// Fixes the dynamic expression.  LINQ to Entity has issues with expressions
        /// built via PredicateBuilder, so we serialize and deserialize the predicate to make it usable.
        /// </summary>
        /// <typeparam name="TDataContract">The type of the data contract.</typeparam>
        /// <param name="predicate">The predicate to be fixed.</param>
        /// <returns>Expression&lt;Func&lt;TDataContract, System.Boolean&gt;&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        public Expression<Func<TDataContract, bool>> FixDynamicExpression<TDataContract>(Expression<Func<TDataContract, bool>> predicate)
            where TDataContract : class, IDataItem, new()
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            // Since the dynamic expression built fails evaluation in Entity Framework, it needed to be 'Fixed'.
            // This was accomplished by serializing and deserializing, which evaluates the variable values and yields a valid expression.
            //var serializer = new ExpressionSerializer(new JsonSerializer());
            //string pred = serializer.SerializeText(predicate);
            //var expr = (Expression<Func<TDataContract, bool>>)serializer.DeserializeText(pred);
            return predicate;
        }

        /// <summary>
        /// Deletes a record from the database.
        /// </summary>
        /// <param name="itemToDelete">The item to delete.</param>
        /// <exception cref="System.ArgumentNullException">itemToDelete</exception>
        public void DeleteItem(IDataItem itemToDelete)
        {
            if (itemToDelete == null)
            {
                throw new ArgumentNullException(nameof(itemToDelete));
            }

            ProcessDeleteItem(itemToDelete);
        }

        /// <summary>
        /// Deletes multiple records from the database.
        /// </summary>
        /// <param name="itemsToDelete">The items to delete.</param>
        /// <exception cref="System.ArgumentNullException">itemsToDelete</exception>
        public void DeleteItems(IEnumerable<IDataItem> itemsToDelete)
        {
            if (itemsToDelete == null)
            {
                throw new ArgumentNullException(nameof(itemsToDelete));
            }

            ProcessDeleteItems(itemsToDelete);
        }

        /// <summary>
        /// Gets a record from the database.
        /// </summary>
        /// <typeparam name="T">The Data Contract type, which identifies the table to get from.</typeparam>
        /// <param name="key">The primary key of the record to get.</param>
        /// <returns>The record as a data contract for the record.</returns>
        public T GetItem<T>(long key) where T : class, IDataItem, new()
        {
            return ProcessGetItem<T>(key);
        }

        /// <summary>
        /// Gets records from the database.
        /// </summary>
        /// <typeparam name="T">The Data Contract type, which identifies the table to get from.</typeparam>
        /// <param name="predicate">The predicate, or query, to filter data.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <param name="take">The number of records to take.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">take;Take must be greater than zero.</exception>
        public IEnumerable<T> GetItems<T>(Expression<Func<T, bool>> predicate, int skip, int take) where T : class, IDataItem, new()
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (take < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take), take, "Take must be greater than zero.");
            }

            return ProcessGetItems<T>(predicate, skip, take);
        }

        /// <summary>
        /// Gets records from the database.
        /// </summary>
        /// <typeparam name="T">The Data Contract type, which identifies the table to get from.</typeparam>
        /// <param name="predicate">The predicate, or query, to filter data.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        public IEnumerable<T> GetItems<T>(Expression<Func<T, bool>> predicate) where T : class, IDataItem, new()
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ProcessGetItems<T>(predicate);
        }

        /// <summary>
        /// Gets records from the database.
        /// </summary>
        /// <typeparam name="T">The Data Contract type, which identifies the table to get from.</typeparam>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        public IEnumerable<T> GetItems<T>() where T : class, IDataItem, new()
        {
            return ProcessGetItems<T>();
        }

        /// <summary>
        /// Gets records from the database.
        /// </summary>
        /// <typeparam name="T">The Data Contract type, which identifies the table to get from.</typeparam>
        /// <param name="skip">The number of records to skip.</param>
        /// <param name="take">The number of records to take.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">take;Take must be greater than zero.</exception>
        public IEnumerable<T> GetItems<T>(int skip, int take) where T : class, IDataItem, new()
        {
            if (take < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take), take, "Take must be greater than zero.");
            }

            return ProcessGetItems<T>(skip, take);
        }

        /// <summary>
        /// Gets the items queryable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>IQueryable&lt;T&gt;.</returns>
        /// TODO Edit XML Comment Template for GetItemsQueryable`1
        public IQueryable<T> GetItemsQueryable<T>() where T : class, IDataItem, new()
        {
            var dbContext = _contextProvider.GetDbContext<T>();
            return dbContext.Set<T>();
        }

        /// <summary>
        /// Gets the count or records based on the query predicate.
        /// </summary>
        /// <typeparam name="T">The Data Contract type, which identifies the table to get from.</typeparam>
        /// <param name="predicate">The predicate, or query, to filter by.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        public int GetItemsCount<T>(Expression<Func<T, bool>> predicate) where T : class, IDataItem, new()
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ProcessGetItemsCount<T>(predicate);
        }

        /// <summary>
        /// Gets the count or records for the type/table specified.
        /// </summary>
        /// <typeparam name="T">The Data Contract type, which identifies the table to get from.</typeparam>
        /// <returns>System.Int32.</returns>
        public int GetItemsCount<T>() where T : class, IDataItem, new()
        {
            return ProcessGetItemsCount<T>();
        }

        /// <summary>
        /// Saves a record to the database.
        /// </summary>
        /// <typeparam name="T">The Data Contract type, which identifies the table to save to.</typeparam>
        /// <param name="itemToSave">The data contract to save to the database.</param>
        /// <returns>T.</returns>
        /// <exception cref="System.ArgumentNullException">itemToSave</exception>
        public T SaveItem<T>(T itemToSave) where T : class, IDataItem, new()
        {
            if (itemToSave == null)
            {
                throw new ArgumentNullException(nameof(itemToSave));
            }

            return ProcessSaveItem<T>(itemToSave);
        }

        /// <summary>
        /// Saves m ultiple records to the database.
        /// </summary>
        /// <typeparam name="T">The Data Contract type, which identifies the table to save to.</typeparam>
        /// <param name="itemsToSave">The items to save to the database.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">itemsToSave</exception>
        public IEnumerable<T> SaveItems<T>(IEnumerable<T> itemsToSave) where T : class, IDataItem, new()
        {
            if (itemsToSave == null)
            {
                throw new ArgumentNullException(nameof(itemsToSave));
            }

            return ProcessSaveItems(itemsToSave);
        }

        #endregion IDataStore Implementation

        #region Private Methods

        /// <summary>
        /// Processes saving multiple items.  It will create an db context then call save for
        /// each item passing in that same context.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="itemsToSave">The items to save.</param>
        /// <returns>IEnumerable&lt;TDataContract&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">itemsToSave</exception>
        private IEnumerable<TDataContract> ProcessSaveItems<TDataContract>(IEnumerable<TDataContract> itemsToSave)
            where TDataContract : class, IDataItem, new()
        {
            var dbContext = _contextProvider.GetDbContext<TDataContract>();

            if (itemsToSave == null)
            {
                throw new ArgumentNullException(nameof(itemsToSave));
            }

            var items = itemsToSave as IList<TDataContract> ?? itemsToSave.ToList();
            foreach (var itemToSave in items)
            {
                ProcessSaveItem<TDataContract>(itemToSave, dbContext);
            }

            return items;
        }

        /// <summary>
        /// Processes saving an item to the database.
        /// It creates a instance of a db Context and then saves using that context.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="itemToSave">The item to save.</param>
        /// <returns>TDataContract.</returns>
        /// <exception cref="System.ArgumentNullException">itemToSave</exception>
        private TDataContract ProcessSaveItem<TDataContract>(TDataContract itemToSave) where TDataContract : class, IDataItem, new()
        {
            if (itemToSave == null)
            {
                throw new ArgumentNullException(nameof(itemToSave));
            }

            var dbContext = _contextProvider.GetDbContext<TDataContract>();
            return ProcessSaveItem<TDataContract>(itemToSave, dbContext);
        }

        /// <summary>
        /// Processes saving an item to the database in the provided db context.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="itemToSave">The item to save.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>TDataContract.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// itemToSave
        /// or
        /// dbContext
        /// </exception>
        private TDataContract ProcessSaveItem<TDataContract>(TDataContract itemToSave, DbContext dbContext)
            where TDataContract : class, IDataItem, new()
        {
            if (itemToSave == null)
            {
                throw new ArgumentNullException(nameof(itemToSave));
            }

            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (itemToSave.Id > 0)
            {
                //UPDATE
                var data = ProcessGetItem<TDataContract>(itemToSave.Id, dbContext);
                if (data == null)
                {
                    dbContext.Set<TDataContract>().Add(itemToSave);
                }
                else
                {
                    //(dbContext as IObjectContextAdapter).ObjectContext.Detach(data);
                    dbContext.Entry(itemToSave).State = EntityState.Modified;
                }
            }
            else
            {
                //INSERT
                dbContext.Set<TDataContract>().Add(itemToSave);
            }

            dbContext.SaveChanges();

            return itemToSave;
        }

        /// <summary>
        /// Processes deleting an item from the database.
        /// </summary>
        /// <param name="itemToDelete">The item to delete.</param>
        /// <exception cref="System.ArgumentNullException">itemToDelete</exception>
        private void ProcessDeleteItem(IDataItem itemToDelete)
        {
            if (itemToDelete == null)
            {
                throw new ArgumentNullException(nameof(itemToDelete));
            }

            var dbContext = _contextProvider.GetDbContext(itemToDelete);
            ProcessDeleteItem(itemToDelete, dbContext);
        }

        /// <summary>
        /// Processes deleting an item from the database in the provided db context.
        /// </summary>
        /// <param name="itemToDelete">The item to delete.</param>
        /// <param name="dbContext">The database context.</param>
        /// <exception cref="System.ArgumentNullException">
        /// itemToDelete
        /// or
        /// dbContext
        /// </exception>
        private void ProcessDeleteItem<TDataItem>(TDataItem itemToDelete, DbContext dbContext) where TDataItem : class, IDataItem
        {
            if (itemToDelete == null)
            {
                throw new ArgumentNullException(nameof(itemToDelete));
            }

            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            Type dataItemType = itemToDelete.GetType();
            var entry = dbContext.Entry(itemToDelete);
            if (entry.State == EntityState.Detached)
            {
                var getItemMethodInfo = GetType().GetGenericMethod("ProcessGetItem",
                                                                    new Type[] { typeof(long), typeof(DbContext) });

                // What we have is a pointer to a specific 'generic' method, but we still need to turn it into an
                // instance of the particular generic method we want to invoke.
                var genericMethodInfo = getItemMethodInfo.MakeGenericMethod(new[] { dataItemType });

                object data = genericMethodInfo.Invoke(this, new object[] { itemToDelete.Id, dbContext });
                if (data == null)
                {
                    // no record in the data store for the ID. Nothing to delete
                    return;
                }

                itemToDelete = data as TDataItem;
            }

            if (itemToDelete != null)
            {
                dbContext.Remove(itemToDelete);
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Processes deleting multiple items from the database.
        /// </summary>
        /// <param name="itemsToDelete">The items to delete.</param>
        /// <exception cref="System.ArgumentNullException">itemsToDelete</exception>
        private void ProcessDeleteItems(IEnumerable<IDataItem> itemsToDelete)
        {
            if (itemsToDelete == null)
            {
                throw new ArgumentNullException(nameof(itemsToDelete));
            }

            var dbContext = _contextProvider.GetDbContext(itemsToDelete.FirstOrDefault());
            foreach (var itemToDelete in itemsToDelete)
            {
                ProcessDeleteItem(itemToDelete, dbContext);
            }
        }

        /// <summary>
        /// Processes getting an item from the database.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>TDataContract.</returns>
        private TDataContract ProcessGetItem<TDataContract>(long key) where TDataContract : class, IDataItem, new()
        {
            var dbContext = _contextProvider.GetDbContext<TDataContract>();
            return ProcessGetItem<TDataContract>(key, dbContext);
        }

        /// <summary>
        /// Processes getting an item from the database in the provided db context.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>TDataContract.</returns>
        /// <exception cref="System.ArgumentNullException">dbContext</exception>
        private static TDataContract ProcessGetItem<TDataContract>(long key, DbContext dbContext)
            where TDataContract : class, IDataItem, new()
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }
            var temp = dbContext.Set<TDataContract>().Find(key);
            return temp;
        }

        /// <summary>
        /// Processes getting items from the database with a query/predicate.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="skip">The number of items to skip.</param>
        /// <param name="take">The number of items to take.</param>
        /// <returns>IEnumerable&lt;TDataContract&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">take;Take cannot be less than zero.</exception>
        private IEnumerable<TDataContract> ProcessGetItems<TDataContract>(
            Expression<Func<TDataContract, bool>> predicate, int skip, int take) where TDataContract : class, IDataItem, new()
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (take < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take), take, "Take cannot be less than zero.");
            }

            var dbContext = _contextProvider.GetDbContext<TDataContract>();
            return ProcessGetItems(predicate, skip, take, dbContext);
        }

        /// <summary>
        /// Processes getting items from the database with a query/predicate in the provided db context.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="skip">The number of items to skip.</param>
        /// <param name="take">The number of items to take.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>IEnumerable&lt;TDataContract&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// predicate
        /// or
        /// dbContext
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">take;take must be greater than 0.</exception>
        private IEnumerable<TDataContract> ProcessGetItems<TDataContract>(
            Expression<Func<TDataContract, bool>> predicate, int skip, int take, DbContext dbContext)
            where TDataContract : class, IDataItem, new()
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (take < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take), take, "take must be greater than 0.");
            }

            var data =
                dbContext.Set<TDataContract>()
                    .Where(predicate)
                    .OrderBy(GetKeyAsExpression<TDataContract>())
                    .Skip(skip)
                    .Take(take)
                    .ToList();
            return (data.Count == 0) ? null : data;
        }

        /// <summary>
        /// Processes getting items from the database with a query/predicate.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <returns>IEnumerable&lt;TDataContract&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        private IEnumerable<TDataContract> ProcessGetItems<TDataContract>(
            Expression<Func<TDataContract, bool>> predicate)
            where TDataContract : class, IDataItem, new()
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var dbContext = _contextProvider.GetDbContext<TDataContract>();
            return ProcessGetItems(predicate, dbContext);
        }

        /// <summary>
        /// Processes getting items from the database with a query/predicate in the provided db context.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>IEnumerable&lt;TDataContract&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// predicate
        /// or
        /// dbContext
        /// </exception>
        private IEnumerable<TDataContract> ProcessGetItems<TDataContract>(
            Expression<Func<TDataContract, bool>> predicate, DbContext dbContext)
            where TDataContract : class, IDataItem, new()
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }
            

            var data = dbContext.Set<TDataContract>().Where(predicate).ToList();
            return (data.Count == 0) ? null : data;
        }

        /// <summary>
        /// Processes getting itesm from the database.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <returns>IEnumerable&lt;TDataContract&gt;.</returns>
        private IEnumerable<TDataContract> ProcessGetItems<TDataContract>() where TDataContract : class, IDataItem, new()
        {
            var dbContext = _contextProvider.GetDbContext<TDataContract>();
            return ProcessGetItems<TDataContract>(dbContext);
        }

        /// <summary>
        /// Processes getting items from the database in the provided db context.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="dbContext">The database context.</param>
        /// <returns>IEnumerable&lt;TDataContract&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">dbContext</exception>
        private IEnumerable<TDataContract> ProcessGetItems<TDataContract>(DbContext dbContext)
            where TDataContract : class, IDataItem, new()
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            var data = (from c in dbContext.Set<TDataContract>() select c).ToList();
            return (data.Count == 0) ? null : data;
        }

        /// <summary>
        /// Processes getting items from the database.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="skip">The number of items to skip.</param>
        /// <param name="take">The number of items to take.</param>
        /// <returns>IEnumerable&lt;TDataContract&gt;.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">take;Take must be greater than zero.</exception>
        private IEnumerable<TDataContract> ProcessGetItems<TDataContract>(int skip, int take)
            where TDataContract : class, IDataItem, new()
        {
            if (take < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take), take, "Take must be greater than zero.");
            }

            var dbContext = _contextProvider.GetDbContext<TDataContract>();
            return ProcessGetItems<TDataContract>(skip, take, dbContext);
        }

        /// <summary>
        /// Processes getting items from the database in the provided db context.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="skip">The number of items to skip.</param>
        /// <param name="take">The number of items to take.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>IEnumerable&lt;TDataContract&gt;.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">take;Take must be greater than zero.</exception>
        /// <exception cref="System.ArgumentNullException">dbContext</exception>
        private IEnumerable<TDataContract> ProcessGetItems<TDataContract>(int skip, int take, DbContext dbContext)
            where TDataContract : class, IDataItem, new()
        {
            if (take < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take), take, "Take must be greater than zero.");
            }

            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            var data = new List<TDataContract>();

            if (dbContext.Set<TDataContract>().Any())
            {
                data =
                    dbContext.Set<TDataContract>()
                        .OrderBy(GetKeyAsExpression<TDataContract>())
                        .Skip(skip)
                        .Take(take)
                        .ToList();
            }

            return (data.Count == 0) ? null : data;
        }

        /// <summary>
        /// Processes getting a count of items in the database with a query/predicate.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        private int ProcessGetItemsCount<TDataContract>(Expression<Func<TDataContract, bool>> predicate)
            where TDataContract : class, IDataItem, new()
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var dbContext = _contextProvider.GetDbContext<TDataContract>();
            return ProcessGetItemsCount<TDataContract>(predicate, dbContext);
        }

        /// <summary>
        /// Processes getting a count of items in the database with a query/predicate in the provided db context.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// predicate
        /// or
        /// dbContext
        /// </exception>
        private int ProcessGetItemsCount<TDataContract>(Expression<Func<TDataContract, bool>> predicate,
            DbContext dbContext)
            where TDataContract : class, IDataItem, new()
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            return dbContext.Set<TDataContract>().Where(predicate).Count();
        }

        /// <summary>
        /// Processes getting a count of items in the database.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <returns>System.Int32.</returns>
        private int ProcessGetItemsCount<TDataContract>() where TDataContract : class, IDataItem, new()
        {
            var dbContext = _contextProvider.GetDbContext<TDataContract>();
            return ProcessGetItemsCount<TDataContract>(dbContext);
        }

        /// <summary>
        /// Processes getting a count ot items in the database in the provide db context.
        /// </summary>
        /// <typeparam name="TDataContract">The Data Contract type, which identifies the table.</typeparam>
        /// <param name="dbContext">The database context.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.ArgumentNullException">dbContext</exception>
        private int ProcessGetItemsCount<TDataContract>(DbContext dbContext) where TDataContract : class, IDataItem, new()
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            return dbContext.Set<TDataContract>().Count();
        }

        /// <summary>
        /// Gets the primary key of the provided contract so it can be used as an expression in LINQ.
        /// </summary>
        /// <typeparam name="T">The Data Contract type, which identifies the table.</typeparam>
        /// <returns>Expression&lt;Func&lt;T, System.Int32&gt;&gt;.</returns>
        private static Expression<Func<T, long>> GetKeyAsExpression<T>()
        {
            var type = typeof(T);
            var pe = Expression.Parameter(type);
            var idProperty =
                type.GetTypeInfo().GetProperties().FirstOrDefault(t => t.GetCustomAttributes(typeof(KeyAttribute), false).Any());
            string propertyName = "Id";
            if (idProperty != null)
            {
                propertyName = idProperty.Name;
            }

            var expr = Expression.Lambda<Func<T, long>>(Expression.PropertyOrField(pe, propertyName), pe);
            return expr;
        }

        #endregion Private Methods
    }
}