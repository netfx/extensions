#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion

namespace NetFx.Patterns.Adapter
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using FromTo = System.Tuple<System.Type, System.Type>;

    /// <summary>
    /// Service implementation that provides pluggable adaptation of types and their adapter 
    /// registrations.
    /// </summary>
    partial class AdapterService : IAdapterService, IAdapterRegistry
    {
        private static readonly MethodInfo AdaptExpressionGenerator = typeof(AdapterService).GetMethod("GetAdaptExpression", BindingFlags.NonPublic | BindingFlags.Static);

        private ConcurrentDictionary<Type, IEnumerable<TypeInheritance>> cachedOrderedTypeHierarchies = new ConcurrentDictionary<Type, IEnumerable<TypeInheritance>>();
        private ConcurrentDictionary<FromTo, Func<IAdapter, object, object>> cachedAdaptMethods = new ConcurrentDictionary<FromTo, Func<IAdapter, object, object>>();
        private ConcurrentDictionary<Type, IEnumerable<AdapterEntry>> cachedCompatibleAdapters = new ConcurrentDictionary<Type, IEnumerable<AdapterEntry>>();

        private List<AdapterEntry> adapters = new List<AdapterEntry>();

        /// <summary>
        /// Registers an adapter to be used by the <see cref="IAdapterService"/>.
        /// </summary>
        /// <param name="adapter">Adapter to register.</param>
        public virtual void Register(IAdapter adapter)
        {
            Guard.NotNull(() => adapter, adapter);

            // Must clear the cached pre-calculated compatible adapters 
            // whenever a new adapter is provided.
            this.cachedCompatibleAdapters.Clear();
            this.adapters.AddRange(adapter.GetType()
                .GetInterfaces()
                // For each implemented IAdapter<TFrom, TTo>, add an entry to the available adapters.
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAdapter<,>))
                .Select(i => i.GetGenericArguments())
                .Select(args => new AdapterEntry
                {
                    Adapter = adapter,
                    Supports = new AdapterSupport
                    {
                        From = args[0],
                        To = args[1],
                    }
                }));
        }

        /// <summary>
        /// Tries to adapt the given <paramref name="instance"/> to the requested type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert the instance to.</typeparam>
        /// <param name="instance">The instance to convert.</param>
        /// <returns>
        /// The adapted object if an adapter for the source could be found; <see langword="null"/> otherwise.
        /// </returns>
        public virtual T To<T>(object instance)
            where T : class
        {
            if (instance == null)
                return default(T);

            // TODO: we should be able to cache all this based on the 
            // concrete instance type, so that the discovery of 
            // adapters is done only once.

            var instanceType = instance.GetType();

            if (typeof(T).IsAssignableFrom(instanceType))
                return (T)instance;

            var compatibleAdapters = cachedCompatibleAdapters.GetOrAdd(
                instanceType,
                targetType =>
                {
                    var tree = GetInheritance(targetType);
                    // Search by proximity.
                    return this.adapters
                        // Filter out all adapters that cannot convert to the specific return type.
                        // TODO: add flexible inheritance priority also on the T side.
                        .Where(x => x.Supports.To == typeof(T))
                        // Locate the mapping to a type inheritance, if any, which allows ordering by distance (implicit priority)
                        .Select(x => new { Adapter = x, Inheritance = tree.FirstOrDefault(t => x.Supports.From == t.Type) })
                        .Where(x => x.Inheritance != null && x.Adapter.Supports.From.IsAssignableFrom(instanceType))
                        // Priority order
                        .OrderBy(x => x.Inheritance.Distance)
                        .Select(x => x.Adapter)
                        .ToList();
                });

            foreach (var compatibleAdapter in compatibleAdapters)
            {
                var adaptMethod = GetAdaptMethod(compatibleAdapter.Supports);
                var result = adaptMethod(compatibleAdapter.Adapter, instance) as T;

                if (result != default(T))
                    return result;
            }

            return default(T);
        }

        private Func<IAdapter, object, object> GetAdaptMethod(AdapterSupport metadata)
        {
            return this.cachedAdaptMethods.GetOrAdd(
                new FromTo(metadata.From, metadata.To),
                (FromTo fromTo) => ((Expression<Func<IAdapter, object, object>>)
                    AdaptExpressionGenerator.MakeGenericMethod(fromTo.Item1, fromTo.Item2).Invoke(null, null))
                    .Compile());
        }

        private static Expression<Func<IAdapter, object, object>> GetAdaptExpression<TFrom, TTo>()
        {
            return (adapter, source) => ((IAdapter<TFrom, TTo>)adapter).Adapt((TFrom)source);
        }

        private IEnumerable<TypeInheritance> GetInheritance(Type sourceType)
        {
            return cachedOrderedTypeHierarchies.GetOrAdd(
                sourceType,
                type => type.GetInheritanceTree()
                    .Inheritance
                    .Traverse(TraverseKind.BreadthFirst, t => t.Inheritance)
                    .Concat(new[] { new TypeInheritance(sourceType, 0) })
                    .OrderBy(t => t.Distance)
                    .Distinct());
        }

        private class AdapterEntry
        {
            public IAdapter Adapter { get; set; }
            public AdapterSupport Supports { get; set; }
        }

        partial class AdapterSupport
        {
            public Type From { get; set; }
            public Type To { get; set; }
        }

    }
}
