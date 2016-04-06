using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace B2Lib.Utilities
{
    public abstract class B2BaseIterator<T> : IEnumerable<T> where T : class
    {
        protected B2Communicator Communicator { get; }
        protected Uri ApiUri { get; }

        public int PageSize { get; set; } = 1000;

        protected B2BaseIterator(B2Communicator communicator, Uri apiUri)
        {
            Communicator = communicator;
            ApiUri = apiUri;
        }

        protected abstract List<T> GetNextPage();

        protected abstract void PreProcessItem(T item);

        public IEnumerator<T> GetEnumerator()
        {
            while (true)
            {
                List<T> page = GetNextPage();

                foreach (T item in page)
                {
                    PreProcessItem(item);

                    yield return item;
                }

                if (!page.Any())
                    yield break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}