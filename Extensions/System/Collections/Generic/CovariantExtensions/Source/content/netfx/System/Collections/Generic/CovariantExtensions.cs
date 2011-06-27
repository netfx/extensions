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
using System.Collections;

/* Credits go to the Umbrella (http://codeplex.com/umbrella) project */

/// <summary>
/// Provides extension methods that allow to treat collections, enumerables and lists as 
/// covariant of a generic type other than their constructed type (i.e. for down-casting collections).
/// </summary>
internal static class CovariantExtensions
{
    /// <summary>
    /// Allows for covariance of generic ICollections. Adapts a collection of type
    /// <typeparam name="T" /> into a collection of type <typeparam name="U" />
    /// </summary>
	/// <nuget packageId="netfx-System.Collections.Generic.CovariantExtensions"></nuget>
	/// <param name="source" this="true">The collection where covariant will be applied</param>
    public static ICollection<U> ToCovariant<T, U>(this ICollection<T> source)
        where T : U
    {
        return new CollectionInterfaceAdapter<T, U>(source);
    }

    /// <summary>
    /// Allows for covariance of generic ILists. Adapts a collection of type
    /// <typeparam name="T" /> into a collection of type <typeparam name="U" />
    /// </summary>
	/// <nuget packageId="netfx-System.Collections.Generic.CovariantExtensions"></nuget>
	/// <param name="source" this="true">The list where covariant will be applied</param>
    public static IList<U> ToCovariant<T, U>(this IList<T> source)
        where T : U
    {
        return new ListInterfaceAdapter<T, U>(source);
    }

    /// <summary>
    /// Allows for covariance of generic IEnumerables. Adapts a collection of type
    /// <typeparam name="T" /> into a collection of type <typeparam name="U" />
    /// </summary>
	/// <nuget packageId="netfx-System.Collections.Generic.CovariantExtensions"></nuget>
	/// <param name="source" this="true">The enumerable where covariant will be applied</param>
    public static IEnumerable<U> ToCovariant<T, U>(this IEnumerable<T> source)
        where T : U
    {
        return new EnumerableInterfaceAdapter<T, U>(source);
    }

    /// <summary>
    /// Allows for covariance of generic ICollections. Adapts a collection of type
    /// <typeparam name="T" /> into a collection of type <typeparam name="U" />
    /// </summary>
    class CollectionInterfaceAdapter<T, U>
        : EnumerableInterfaceAdapter<T, U>, ICollection<U> where T : U
    {
        new ICollection<T> Target { get; set; }

        public void Add(U item)
        {
            Target.Add((T)item);
        }

        public void Clear()
        {
            Target.Clear();
        }

        public bool Contains(U item)
        {
            return Target.Contains((T)item);
        }

        public void CopyTo(U[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < Target.Count; i++)
            {
                array[i] = Target.ElementAt(i);
            }
        }

        public bool Remove(U item)
        {
            return Target.Remove((T)item);
        }

        public int Count
        {
            get { return Target.Count; }
        }

        public bool IsReadOnly
        {
            get { return Target.IsReadOnly; }
        }

        public CollectionInterfaceAdapter(ICollection<T> target)
            : base(target)
        {
        }
    }

    /// <summary>
    /// Allows for covariance of generic IEnumerables. Adapts a collection of type
    /// <typeparam name="T" /> into a collection of type <typeparam name="U" />
    /// </summary>
    class EnumerableInterfaceAdapter<T, U> : IEnumerable<U> where T : U
    {
        public IEnumerable<T> Target { get; set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<U> GetEnumerator()
        {
            foreach (T item in Target)
                yield return item;
        }

        public EnumerableInterfaceAdapter(IEnumerable<T> target)
        {
            Target = target;
        }
    }

    /// <summary>
    /// Allows for covariance of generic ILists. Adapts a collection of type
    /// <typeparam name="T" /> into a collection of type <typeparam name="U" />
    /// </summary>
    class ListInterfaceAdapter<T, U> : CollectionInterfaceAdapter<T, U>, IList<U>
        where T : U
    {
        new IList<T> Target { get; set; }

        public int IndexOf(U item)
        {
            return Target.IndexOf((T)item);
        }

        public void Insert(int index, U item)
        {
            Target.Insert(index, (T)item);
        }

        public void RemoveAt(int index)
        {
            Target.RemoveAt(index);
        }

        public U this[int index]
        {
            get { return Target[index]; }
            set { Target[index] = (T)value; }
        }

        public ListInterfaceAdapter(IList<T> target)
            : base(target)
        {
        }
    }
}
