using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageService.RedisRepository.Interface;
using MessageService.RedisRepository.Utility;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MessageService.RedisRepository.Implementation
{
    public class RedisCache : IRedisCache
    {
        public static IDatabase Cache = RedisConnector.RedisConnection.GetDatabase();

        public T Get<T>(string key)
        {
            T result = default(T);
            RedisValue redisValue = Cache.StringGet(key);
            if (redisValue.HasValue && !redisValue.IsNullOrEmpty)
            {
                result = JsonConvert.DeserializeObject<T>(redisValue);
            }
            return result;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            T result = default(T);
            RedisValue redisValue = await Cache.StringGetAsync(key).ConfigureAwait(false);
            if (redisValue.HasValue && !redisValue.IsNullOrEmpty)
            {
                result = JsonConvert.DeserializeObject<T>(redisValue);
            }
            return result;
        }

        public async Task<IEnumerable<T>> GetListAsync<T>(string key)
        {
            IEnumerable<T> result = default(IEnumerable<T>);
            RedisValue redisValue = await Cache.StringGetAsync(key).ConfigureAwait(false);
            if (redisValue.HasValue && !redisValue.IsNullOrEmpty)
            {
                result = JsonConvert.DeserializeObject<IEnumerable<T>>(redisValue);
            }
            return result;
        }

        public async Task<List<T>> GetAsync<T>(string[] keys)
        {
            List<T> result = new List<T>();
            try
            {
                var redisKeys = keys.Select(_ => new RedisKey().Append(_)).ToArray();
                var redisValue = await Cache.StringGetAsync(redisKeys).ConfigureAwait(false);
                if (redisValue != null && redisValue.Any())
                {
                    foreach (var val in redisValue)
                    {
                        result.Add(JsonConvert.DeserializeObject<T>(val));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }

            return result;
        }

        public bool IsKeyExist(string key)
        {
            return Cache.KeyExists(key);
        }

        public async Task<bool> IsKeyExistAsync(string key)
        {
            return await Cache.KeyExistsAsync(key).ConfigureAwait(false);
        }

        public bool Remove(string key)
        {
            return Cache.KeyDelete(key);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await Cache.KeyDeleteAsync(key);
        }

        public bool Set<T>(string key, T value)
        {
            var str = JsonConvert.SerializeObject(value);
            return Cache.StringSet(key, str);
        }

        public async Task<bool> SetAsync<T>(string key, T value)
        {
            var str = JsonConvert.SerializeObject(value);
            return await Cache.StringSetAsync(key, str);
        }

        public bool Set<T>(string key, T value, TimeSpan expiredTimeSpan)
        {
            var str = JsonConvert.SerializeObject(value);
            return Cache.StringSet(key, str, expiredTimeSpan);
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan expiredTimeSpan)
        {
            var str = JsonConvert.SerializeObject(value);
            return await Cache.StringSetAsync(key, str, expiredTimeSpan);
        }
    }
}
