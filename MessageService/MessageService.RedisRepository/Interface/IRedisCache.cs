using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageService.RedisRepository.Interface
{
    public interface IRedisCache
    {
        bool Set<T>(string key, T value);

        bool Set<T>(string key, T value, TimeSpan timeout);

        T Get<T>(string key);

        bool Remove(string key);

        bool IsKeyExist(string key);

        Task<T> GetAsync<T>(string key);

        Task<IEnumerable<T>> GetListAsync<T>(string key);

        Task<bool> IsKeyExistAsync(string key);

        Task<bool> RemoveAsync(string key);

        Task<bool> SetAsync<T>(string key, T value);

        Task<bool> SetAsync<T>(string key, T value, TimeSpan expiredTimeSpan);
    }
}
