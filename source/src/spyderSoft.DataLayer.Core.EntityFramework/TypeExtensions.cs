using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace spyderSoft.DataLayer.Core.EntityFramework
{
    /// <summary>
    /// Class TypeExtensions.  This class is a helper class to assist in reflection against generic methods, which
    /// the .NET Type.GetMethod does not handle well.
    /// </summary>
    /// <remarks>
    /// If this class is needed somewhere other than the EntityFramework project, please discuss a common location
    /// with the rest of the team.
    /// </remarks>
    public static class TypeExtensions
    {
        #region Public Methods

        /// <summary>
        /// This extension method gets a particular generic method based on the calling type
        /// and the name and parameter types of the desired variant of the method.
        /// </summary>
        /// <param name="genericType">The <see cref="Type"/> of the generic method being looked for.</param>
        /// <param name="name">The name of the generic method being looked for.</param>
        /// <param name="parameterTypes">An array of the parameter types that are parameters to the particular
        /// generic method that is being looked for.</param>
        /// <returns>A <see cref="MethodInfo"/> object representing the method that was found, or null
        /// if the method was not found on the <paramref name="genericType"/> that was given.</returns>
        public static MethodInfo GetGenericMethod(this Type genericType, string name, Type[] parameterTypes)
        {
            var methods = genericType.GetTypeInfo().GetMethods(BindingFlags.Instance |
                                                 BindingFlags.NonPublic |
                                                 BindingFlags.Public | 
                                                 BindingFlags.Static);

            // We basically have all of the methods that are available on the type, select only the
            // ones that match on name and sift through them and find the one with the right parameter set.
            foreach (var method in methods.Where(m => m.Name == name))
            {
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

                if (methodParameterTypes.SequenceEqual(parameterTypes, new SimpleTypeComparer()))
                {
                    return method;
                }
            }

            return null;
        }

        #endregion Public Methods

        #region Private SimpleTypeComparer Class

        /// <summary>
        /// Embedded Class SimpleTypeComparer.
        /// </summary>
        private class SimpleTypeComparer : IEqualityComparer<Type>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object of <see cref="Type"/> to compare.</param>
            /// <param name="y">The second object of <see cref="Type"/> to compare.</param>
            /// <returns>true if the specified objects are equal; otherwise, false.</returns>
            public bool Equals(Type x, Type y)
            {
                return (x.AssemblyQualifiedName == y.AssemblyQualifiedName) &&
                       (x.Namespace == y.Namespace) &&
                       (x.Name == y.Name);
            }

            /// <summary>
            /// Returns a hash code for this instance.  Not really needed but required to
            /// be present.
            /// </summary>
            /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
            /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
            /// <exception cref="System.NotImplementedException"></exception>
            public int GetHashCode(Type obj)
            {
                throw new NotImplementedException();
            }
        }

        #endregion Private SimpleTypeComparer Class
    }
}