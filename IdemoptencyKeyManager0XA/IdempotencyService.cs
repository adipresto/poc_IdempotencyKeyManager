using IdemoptencyKeyManager0XA.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace IdemoptencyKeyManager0XA
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly IIdempotencyKeyStorage _storage;
        private readonly TimeSpan _lockTimeout;
        private readonly TimeSpan _resultExpiration;

        public IdempotencyService(IIdempotencyKeyStorage storage,
                                TimeSpan? lockTimeout = null,
                                TimeSpan? resultExpiration = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _lockTimeout = lockTimeout ?? TimeSpan.FromMinutes(5);
            _resultExpiration = resultExpiration ?? TimeSpan.FromHours(24);
        }

        public async Task<T> ExecuteAsync<T>(string idempotencyKey, Func<Task<T>> operation)
        {
            bool lockAcquired = false;
            short retryCount = 0;
            short maxRetries = 3;

            if (string.IsNullOrEmpty(idempotencyKey))
                throw new ArgumentException("Idempotency key cannot be null or empty", nameof(idempotencyKey));

            if (!ValidateKey(idempotencyKey))
                throw new ArgumentException("Invalid idempotency key format", nameof(idempotencyKey));

            // Coba ambil lock dulu, kalau gagal coba lagi (retry logic)
            while (!lockAcquired && retryCount < maxRetries)
            {
                lockAcquired = await _storage.TryLockAsync(idempotencyKey, _lockTimeout);

                if (!lockAcquired)
                {
                    retryCount++;

                    // Tidak dapat lock → request lain sedang proses,
                    // ambil hasil kalau request lain sudah selesai
                    if (await _storage.ExistsAsync(idempotencyKey))
                    {
                        return await _storage.GetResultAsync<T>(idempotencyKey);
                    }

                    // Tiap percobaan dikali 100ms untuk mencoba lagi
                    await Task.Delay(100 * retryCount); // 100ms, 200ms, 300ms
                }
            }

            if (!lockAcquired)
            {
                throw new InvalidOperationException($"Unable to acquire lock after {maxRetries} retries");
            }

            try
            {
                // Double-check setelah lock
                if (await _storage.ExistsAsync(idempotencyKey))
                {
                    return await _storage.GetResultAsync<T>(idempotencyKey);
                }

                // Jalankan operasi utama
                var result = await operation();

                // Simpan hasil
                await _storage.StoreAsync(idempotencyKey, result, _resultExpiration);

                return result;
            }
            finally
            {
                // Selalu release lock
                await _storage.ReleaseLockAsync(idempotencyKey);
            }
        }


        public string GenerateKey()
        {
            // Generate GUID-based key with timestamp
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var guid = Guid.NewGuid().ToString("N");
            return $"idem_{timestamp}_{guid}";
        }

        public async Task<bool> IsProcessedAsync(string idempotencyKey)
        {
            if (string.IsNullOrEmpty(idempotencyKey))
                return false;

            return await _storage.ExistsAsync(idempotencyKey);
        }

        public bool ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            // Basic validation rules
            if (key.Length < 10 || key.Length > 255)
                return false;

            // Should contain only alphanumeric, hyphens, underscores
            foreach (char c in key)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                    return false;
            }

            return true;
        }
    }
}
