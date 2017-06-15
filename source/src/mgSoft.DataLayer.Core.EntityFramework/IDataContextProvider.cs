using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace mgSoft.DataLayer.Core.EntityFramework
{
    /// <summary>
    /// Interface IDataContextProvider.  Implementations of this interface must be injected in to <see cref="DataStore"/> via the constructor
    /// to provide it with the appropriate <see cref="DbContext"/> implementations.
    /// </summary>
    /// <remarks>
    /// Implementations of this class provide the flexibility to create multiple <see cref="DbContext"/> 
    /// (or, as suggested, <see cref="DynamicDbContext"/> objects which split the context based on functional areas.
    /// </remarks>
    public interface IDataContextProvider
    {
        /// <summary>
        /// Initializes the specified data store path.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Verifies the current instance of this context provider is valid.
        /// </summary>
        /// <returns><c>true</c> if this instance of the context provider is valid, <c>false</c> otherwise.</returns>
        bool VerifyContext();

        /// <summary>
        /// Gets the database context.
        /// </summary>
        /// <returns>System.Data.Entity.DbContext.</returns>
        DbContext GetDbContext<TDataItem>() where TDataItem : class, IDataItem, new();

        /// <summary>
        /// Gets the database context.
        /// </summary>
        /// <returns>System.Data.Entity.DbContext.</returns>
        DbContext GetDbContext(IDataItem item);
    }
}
