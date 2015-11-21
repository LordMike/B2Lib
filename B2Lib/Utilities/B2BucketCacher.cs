using System.Collections.Generic;
using System.Linq;
using B2Lib.Objects;

namespace B2Lib.Utilities
{
    public class B2BucketCacher
    {
        private List<B2BucketCache> _cache;

        public B2BucketCacher()
        {
            _cache = new List<B2BucketCache>();
        }

        public List<B2BucketCache> GetState()
        {
            return new List<B2BucketCache>(_cache.Select(s => s.CreateCopy()));
        }

        public void LoadState(List<B2BucketCache> state)
        {
            lock (_cache)
                _cache.AddRange(state);
        }

        public B2BucketCache GetById(string id)
        {
            return GetById(id, false);
        }

        private B2BucketCache GetById(string id, bool create)
        {
            lock (_cache)
            {
                B2BucketCache item = _cache.SingleOrDefault(s => s.BucketId == id);
                if (item == null && create)
                    _cache.Add(item = new B2BucketCache { BucketId = id });
                return item;
            }
        }

        public B2BucketCache GetByName(string name)
        {
            lock (_cache)
                return _cache.SingleOrDefault(s => s.BucketName == name);
        }

        public void RecordBucket(IEnumerable<B2Bucket> buckets)
        {
            foreach (B2Bucket bucket in buckets)
                RecordBucket(bucket);
        }

        public B2BucketCache RecordBucket(B2Bucket bucket)
        {
            B2BucketCache item = GetById(bucket.BucketId, true);

            item.BucketId = bucket.BucketId;
            item.BucketName = bucket.BucketName;

            return item;
        }

        public B2BucketCache RecordBucket(B2UploadConfiguration config)
        {
            B2BucketCache item = GetById(config.BucketId, true);

            item.UploadAuthorizationToken = config.AuthorizationToken;
            item.BucketId = config.BucketId;
            item.UploadUri = config.UploadUrl;

            return item;
        }

        public void RemoveBucket(B2Bucket bucket)
        {
            lock (_cache)
                _cache.RemoveAll(s => s.BucketId == bucket.BucketId);
        }
    }
}