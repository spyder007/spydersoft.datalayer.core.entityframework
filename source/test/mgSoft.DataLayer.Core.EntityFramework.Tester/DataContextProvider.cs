using mgSoft.DataLayer.Core;
using mgSoft.DataLayer.Core.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mgSoft.DataLayer.Core.EntityFramework.Tester
{
    public class DataContext : mgSoft.DataLayer.Core.EntityFramework.DynamicDbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        protected override void MapTableObjects(ModelBuilder modelBuilder)
        {
            MapTableToObject<Beverage>(modelBuilder);
        }

        public override bool ContextSupports<T>(T item)
        {
            var itemType = item.GetType();

            if (itemType == typeof(Beverage))
            {
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Class DataContextProvider.
    /// </summary>
    /// <seealso cref="IDataContextProvider" />
    /// <seealso cref="System.IDisposable" />
    /// TODO Edit XML Comment Template for DataContextProvider
    public class DataContextProvider : IDataContextProvider, IDisposable
    {
        #region Private Members

        /// <summary>
        /// The disposed value
        /// </summary>
        /// TODO Edit XML Comment Template for disposedValue
        private bool disposedValue = false; // To detect redundant calls

        private DataContext _dbContext;

        #endregion Private Members

        public DataContextProvider(DataContext dataContext)
        {
            _dbContext = dataContext;
        }


        #region IDataContextProvider Implementation

        /// <summary>
        /// Gets the database context.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>DbContext.</returns>
        /// <exception cref="NotSupportedException">Items of type {item.GetType()}</exception>
        public DbContext GetDbContext(IDataItem item)
        {

            if (_dbContext.ContextSupports(item))
            {
                return _dbContext;
            }

            throw new NotSupportedException($"Items of type {item.GetType()} are not supported by this DataContextProvider");
        }

        /// <summary>
        /// Gets the database context.
        /// </summary>
        /// <typeparam name="TDataItem">The type of the t data item.</typeparam>
        /// <returns>DbContext.</returns>
        /// TODO Edit XML Comment Template for GetDbContext`1
        public DbContext GetDbContext<TDataItem>() where TDataItem : class, IDataItem, new()
        {
            TDataItem item = new TDataItem();
            return GetDbContext(item);
        }

        /// <summary>
        /// Initializes the specified data store path.
        /// </summary>
        /// <param name="dataStorePath">The data store path.</param>
        /// TODO Edit XML Comment Template for Initialize
        public void Initialize()
        {
            //_hbnDbContext = dataContext;
        }

        /// <summary>
        /// Verifies the context.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// TODO Edit XML Comment Template for VerifyContext
        public bool VerifyContext()
        {
            var contextValid = (_dbContext != null && _dbContext.Database != null);

            return (contextValid);
        }

        #endregion IDataContextProvider Implementation

        #region IDisposable Support


        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// TODO Edit XML Comment Template for Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// TODO Edit XML Comment Template for Dispose
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}
