using System.Collections.Concurrent;

namespace B2Lib.Utilities
{
    /// <summary>
    /// Helper class to lock a single key
    /// </summary>
    internal static class KeyLocker
    {
        private static ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public static object GetLockObject(string key)
        {
            return _locks.GetOrAdd(key, s => new object());
        }
    }
}