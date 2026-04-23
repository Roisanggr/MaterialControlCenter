using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace MaterialControlCenter.Service
{
    public static class AppCache
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;

        public static async Task<T> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> factory,
            int minutes
        )
        {
            var lazy = new Lazy<Task<T>>(factory, true);

            var existing = Cache.AddOrGetExisting(
                key,
                lazy,
                new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(minutes)
                }
            ) as Lazy<Task<T>>;

            try
            {
                return await (existing ?? lazy).Value;
            }
            catch
            {
                Cache.Remove(key);
                throw;
            }
        }

        public static T GetOrSet<T>(
            string key,
            Func<T> factory,
            int minutes
        )
        {
            var lazy = new Lazy<T>(factory, true);

            var existing = Cache.AddOrGetExisting(
                key,
                lazy,
                new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(minutes)
                }
            ) as Lazy<T>;

            try
            {
                return (existing ?? lazy).Value;
            }
            catch
            {
                Cache.Remove(key);
                throw;
            }
        }
    }
}
