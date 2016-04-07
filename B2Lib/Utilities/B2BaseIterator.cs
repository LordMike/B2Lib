using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace B2Lib.Utilities
{
    public abstract class B2BaseIterator<T> : IEnumerable<T> where T : class
    {
        protected B2Communicator Communicator { get; }

        public int PageSize { get; set; } = 1000;

        protected B2BaseIterator(B2Communicator communicator)
        {
            Communicator = communicator;
        }

        protected abstract List<T> GetNextPage(out bool isDone);

        public IEnumerator<T> GetEnumerator()
        {
            bool isDone;
            do
            {
                List<T> page = GetNextPage(out isDone);

                foreach (T item in page)
                    yield return item;

                if (!page.Any())
                    yield break;
            } while (!isDone);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}