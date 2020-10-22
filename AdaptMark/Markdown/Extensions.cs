// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;

namespace AdaptMark.Parsers.Markdown
{
    public static class Extensions
    {
        public static IReadOnlyList<T> AsReadonly<T>(this IList<T> list)
        {
            return new ReadonlyList<T>(list);
        }

        private class ReadonlyList<T> : IReadOnlyList<T>
        {
            private readonly IList<T> list;

            public ReadonlyList(IList<T> list)
            {
                this.list = list;
            }

            public T this[int index] => this.list[index];

            public int Count => this.list.Count;

            public IEnumerator<T> GetEnumerator()
            {
                return this.list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.list.GetEnumerator();
            }
        }
    }
}