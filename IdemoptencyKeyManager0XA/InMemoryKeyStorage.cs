using IdemoptencyKeyManager0XA.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace IdemoptencyKeyManager0XA
{
    public class InMemoryKeyStorage : IIdempotencyKeyStorage, IDisposable
    {
        private readonly IMemoryCache _memoryCache;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        private bool _strictMode = false;

        public InMemoryKeyStorage(bool strictMode = false)
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _strictMode = strictMode;
        }

        public Task StoreAsync(string key, object result, TimeSpan? expiration = null)
        {
            if (_strictMode)
                if (_memoryCache.Get(key) != null)
                    throw new InvalidOperationException($"Key {key} already exists");

            var options = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
                options.AbsoluteExpirationRelativeToNow = expiration.Value;

            _memoryCache.Set(key, result, options);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out var value));
        }

        public Task<T> GetResultAsync<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out var value) && value is T typedValue)
            {
                return Task.FromResult(typedValue);
            }

            return Task.FromResult(default(T));
        }

        public Task ReleaseLockAsync(string key)
        {
            if (_locks.TryGetValue(key, out var semaphore))
            {
                semaphore.Release();
            }
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }

        public async Task<bool> TryLockAsync(string key, TimeSpan lockDuration)
        {
            var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            return await semaphore.WaitAsync(lockDuration);
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }
    }
}
