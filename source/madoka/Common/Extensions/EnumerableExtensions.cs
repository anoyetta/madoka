using System;
using System.Collections.Generic;

namespace madoka.Common
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Walk<T>(
            this IEnumerable<T> e,
            Action<T> action)
        {
            foreach (var item in e)
            {
                action(item);
            }

            return e;
        }
    }
}
