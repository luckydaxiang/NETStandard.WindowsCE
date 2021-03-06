﻿using System.Collections;
using System.Collections.Generic;

#if NET35_CF
namespace System.Collections.Generic
#else
namespace Mock.System.Collections.Generic
#endif
{
    public interface ISet<T> : ICollection<T>, IEnumerable, IEnumerable<T>
    {
        new bool Add(T item);
        void ExceptWith(IEnumerable<T> other);
        void IntersectWith(IEnumerable<T> other);
        bool IsProperSubsetOf(IEnumerable<T> other);
        bool IsProperSupersetOf(IEnumerable<T> other);
        bool IsSubsetOf(IEnumerable<T> other);
        bool IsSupersetOf(IEnumerable<T> other);
        bool Overlaps(IEnumerable<T> other);
        bool SetEquals(IEnumerable<T> other);
        void SymmetricExceptWith(IEnumerable<T> other);
        void UnionWith(IEnumerable<T> other);
    }
}
