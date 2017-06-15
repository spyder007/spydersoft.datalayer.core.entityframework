using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using mgSoft.DataLayer.Core;
using mgSoft.DataLayer.Core.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace mgSoft.DataLayer.Core.EntityFramework
{
    /// <summary>
    /// Abstract class DynamicDbContext.  Implementations of this class are required to use the <see cref="DataStore"/> class.
    /// </summary>
    /// <remarks>
    /// This class should be created in a library or application that has access to all of the <see cref="IDataItem"/> implementations that you wish to store.
    /// Override the abstract <see cref="MapTableObjects(ModelBuilder)"/> method and call <see cref="MapTableToObject{TDataItem}(ModelBuilder)"/>, passing each
    /// <see cref="IDataItem"/> that will be accessed by this context.
    /// </remarks>
    public abstract class DynamicDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDbContext"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        protected DynamicDbContext(DbContextOptions options) : base(options)
        {
        }

        #region DynamicDbContext Interface

        /// <summary>
        /// This method must be implemented in an extending class in order to map decorated
        ///  POCO objects to data tables based on their attribute values.
        /// </summary>
        /// <remarks>
        /// Extended classes will be very basic, with the contents of this function simply calling
        ///     <code>MapTableToObject&lt;IDataItem&gt;(modelBuilder);</code>
        /// for each data contract they wish to have in this context.  It is advisable to create multiple DynamicDbContexts
        /// based on functional area and utilize a smart implementation of <see cref="T:IDataContextProvider"/> to switch
        /// between context's for a given database.
        /// </remarks>
        /// <param name="modelBuilder">The model builder.</param>
        protected abstract void MapTableObjects(ModelBuilder modelBuilder);

        /// <summary>
        /// Checks if the context supports the given type.
        /// </summary>
        /// <typeparam name="T">Any <see cref="IDataItem"/> implementation.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if this class supports the given item, <c>false</c> otherwise.</returns>
        public virtual bool ContextSupports<T>(T item) where T : IDataItem
        {
            return false;
        }

        #endregion DynamicDbContext Interface

        #region DbContext Implementation

        /// <summary>
        /// This method is called when the model for a derived context has been initialized, but
        /// before the model has been locked down and used to initialize the context.  The default
        /// implementation of this method does nothing, but it can be overridden in a derived class
        /// such that the model can be further configured before it is locked down.
        /// </summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        /// <remarks>Typically, this method is called only once when the first instance of a derived context
        /// is created.  The model for that context is then cached and is for all further instances of
        /// the context in the app domain.  This caching can be disabled by setting the ModelCaching
        /// property on the given ModelBuilder, but note that this can seriously degrade performance.
        /// More control over caching is provided through use of the ModelBuilder and DbContextFactory
        /// classes directly.</remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            MapTableObjects(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        #endregion DbContext Implementation

        #region Protected Methods

        /// <summary>
        /// Maps the table to object.
        /// </summary>
        /// <typeparam name="TDataItem"></typeparam>
        /// <param name="modelBuilder">The model builder.</param>
        protected void MapTableToObject<TDataItem>(ModelBuilder modelBuilder) where TDataItem : class, IDataItem, new()
        {
            Type type = typeof(TDataItem);
            var typeInfo = type.GetTypeInfo();
            
            ParameterExpression pe = Expression.Parameter(type);

            //Map the table name
            var tableAttribute = typeInfo.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() as TableAttribute;
            string tableName = tableAttribute.Name;
            var entityConfiguration = modelBuilder.Entity<TDataItem>().ToTable(tableName);

            // Map the Primary Key
            var idProperty = typeInfo.GetProperties().FirstOrDefault(t => t.GetCustomAttributes(typeof(KeyAttribute), false).Any());
            entityConfiguration.HasKey(idProperty.Name);

            var columnProperties =
                typeInfo.GetProperties()
                    .Where(t => t.GetCustomAttributes(typeof(ColumnNameAttribute), false).Any())
                    .ToList();

            foreach (var columnProp in columnProperties)
            {
                var attr = columnProp.GetCustomAttribute<ColumnNameAttribute>();
                entityConfiguration.Property(columnProp.Name).HasColumnName(attr.Name);
            }
            
            //Ignore any properties marked as such
            var ignoreProperties = typeInfo.GetProperties()
                                       .Where(t => t.GetCustomAttributes(typeof(IgnorePropertyAttribute), false).Any())
                                       .ToList();
            // Loop thought the list of properties to process.
            foreach (var expression in ignoreProperties.Select(ignoreProperty => CreatePropSelectorExpression<TDataItem>(ignoreProperty.Name)))
            {
                entityConfiguration.Ignore(expression);
            }

        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Creates a property expression (t => t.PropertyName)
        /// </summary>
        /// <typeparam name="TContract">The type of the data contract</typeparam>
        /// <param name="propertyName">The name of the property to be looked for.</param>
        /// <returns></returns>
        private static Expression<Func<TContract, object>> CreatePropSelectorExpression<TContract>(string propertyName)
        {
            var param = Expression.Parameter(typeof(TContract));
            var body = Expression.Convert(Expression.PropertyOrField(param, propertyName), typeof(object));
            return Expression.Lambda<Func<TContract, object>>(body, param);
        }

        /// <summary>
        /// Creates the Property method of the Entity Configuration so that
        /// we can easily set attributes on columns based upon decorations in the contract
        /// </summary>
        /// <typeparam name="T">The type of the Data Item</typeparam>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="returnType">The type of the property in the data contract</param>
        /// <param name="pe">An expression parameter for the Data Item</param>
        /// <param name="entityConfiguration">The Entity Framework Configuration for the model being beuilt</param>
        /// <param name="propertyName">The name of the property that is updated based on an attribute</param>
        /// <returns>System.Object.</returns>
        private object BuildEntityPropertyMethod<T>(string methodName, Type returnType, ParameterExpression pe, EntityTypeBuilder<T> entityConfiguration, string propertyName) where T : class, IDataItem
        {
            //Get the type of the DataItem
            Type type = typeof(T);

            //Build up a Lambda expression that selects the property
            //Similar to typing (t => t.ID)
            MemberExpression expressionProperty = Expression.PropertyOrField(pe, propertyName);
            var delegateType = typeof(Func<,>).MakeGenericType(type, returnType);
            var expr = Expression.Lambda(delegateType, expressionProperty, pe);

            if (returnType != typeof(string))
            {
                //Get the property method on the configuration that takes a generic as a parameter.
                var method = entityConfiguration
                    .GetType().GetTypeInfo()
                    .GetMethods()
                    .FirstOrDefault(m => m.Name == methodName && m.IsGenericMethod);
                //Create the Property method and return it.
                var prop = method.MakeGenericMethod(new Type[] { returnType }).Invoke(entityConfiguration, new object[] { expr });

                return prop;
            }
            else
            {
                var method = entityConfiguration
                    .GetType().GetTypeInfo()
                    .GetMethods()
                    .FirstOrDefault(m => m.Name == methodName && m.ReturnType == typeof(string));

                var prop = method.Invoke(entityConfiguration, new object[] { expr });
                return prop;
            }
        }

        #endregion Private Methods
    }
}