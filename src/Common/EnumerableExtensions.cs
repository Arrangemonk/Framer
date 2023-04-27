using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace framer.Common
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> objs, Action<T,int> action)
        {
            int index = 0;
            foreach (var obj in objs)
            {
                action(obj,index);
                index++;
            }
            return objs;
        }

        public static bool Any<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            int index = 0;
            foreach (T item in source)
            {
                if (predicate(item, index))
                {
                    return true;
                }
                index++;
            }
            return false;
        }

        public static IEnumerable<Tuple<T,T>> ConvertToPairs<T>(this IEnumerable<T> objs)
        {
            var enumerable = objs as T[] ?? objs.ToArray();
            if (enumerable.IsNullOrEmpty())
                yield break;
            T old = enumerable.First();
            foreach (var current in enumerable.Skip(1))
            {

                yield return Tuple.Create(old,current);
                old = current;
            }
        }

        public static List<T> Splice<T>(this List<T> source, int index, int count, params T[] remainer)
        {
            remainer = remainer ?? Array.Empty<T>();
            var items = source.GetRange(index, count);
            source.RemoveRange(index, count);
            source.AddRange(remainer);
            return items;
        }

        public static bool All<T>(this IEnumerable<T> objs, Func<T, int,bool> action)
        {
            int index = 0;
            bool result = true;
            foreach (var obj in objs)
            {
                result &= action(obj, index);
                if (!result)
                    return false;
            }
            return result;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> objs, Action<T> action)
        {
            foreach (var obj in objs)
                action(obj);
            return objs;
        }

        public static IEnumerable<T> ForEach<T>(Action<T> action, params T[] objs)
        {
            foreach (var obj in objs)
                action(obj);
            return objs;
        }

        public static IEnumerable<T> OmitNulls<T>(this IEnumerable<T> objs)
        {
            return objs.Where(obj => obj != null);
        }

        public static IEnumerable<T> Omit<T>(this IEnumerable<T> objs, Func<T,bool> elemnttoOmit)
        {
            return objs.Where(obj => obj != null && !elemnttoOmit(obj));
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? input)
        {
            return input == null || input.All(elem => elem == null);
        }

        public static IEnumerable<T> FlatMap<T>(this IEnumerable<T> input, Func<T,IEnumerable<T>> selector)
        {
            foreach(var obj in input)
            {
                foreach (var result in selector(obj))
                    yield return result;

            }
        }

        private class FuncEqualityComparer<T> : IEqualityComparer<T>
        {
            private Func<T, T, bool> equality;
            public FuncEqualityComparer(Func<T,T,bool> equality)
            {
                this.equality = equality;
            }
            public bool Equals(T? x, T? y)
            {
                if (x == null || y == null)
                    return false;
                return equality(x, y);
            }

            public int GetHashCode([DisallowNull] T obj)
            {
               return obj.GetHashCode();
            }
        }

        public static IEnumerable<T> UniqeWith<T>(this IEnumerable<T> input, Func<T,T,bool> equality)
        {
            HashSet<T> set = new HashSet<T>(new FuncEqualityComparer<T>(equality));
            foreach(var elem in input)
            {
                set.Add(elem);
            }
            return set;
        }

    }
}
