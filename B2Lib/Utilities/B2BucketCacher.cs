using System.Collections.Generic;
using System.Linq;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2BucketCacher
    {
        private readonly List<B2BucketObject> _cache;

        public B2BucketCacher()
        {
            _cache = new List<B2BucketObject>();
        }

        public List<B2BucketObject> GetState()
        {
            lock (_cache)
                return new List<B2BucketObject>(_cache);
        }

        public void LoadState(List<B2BucketObject> state)
        {
            lock (_cache)
                _cache.AddRange(state);
        }

        public B2BucketObject GetById(string id)
        {
            return GetById(id, false);
        }

        private B2BucketObject GetById(string id, bool create)
        {
            lock (_cache)
            {
                B2BucketObject item = _cache.SingleOrDefault(s => s.BucketId == id);
                if (item == null && create)
                    _cache.Add(item = new B2BucketObject { BucketId = id });
                return item;
            }
        }

        public B2BucketObject GetByName(string name)
        {
            lock (_cache)
                return _cache.SingleOrDefault(s => s.BucketName == name);
        }

        public void RecordBucket(IEnumerable<B2BucketObject> buckets)
        {
            foreach (B2BucketObject bucket in buckets)
                RecordBucket(bucket);
        }

        public B2BucketObject RecordBucket(B2BucketObject bucket)
        {
            B2BucketObject item = GetById(bucket.BucketId, true);

            item.BucketId = bucket.BucketId;
            item.BucketName = bucket.BucketName;

            return item;
        }

        public void RemoveBucket(B2BucketObject bucket)
        {
            lock (_cache)
                _cache.RemoveAll(s => s.BucketId == bucket.BucketId);
        }
    }
}