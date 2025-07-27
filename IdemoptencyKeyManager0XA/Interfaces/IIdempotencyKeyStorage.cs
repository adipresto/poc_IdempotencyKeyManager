using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdemoptencyKeyManager0XA.Interfaces
{
    /// <summary>
    /// Menyimpan dan mengelola kunci idempoten beserta hasil yang terkait.
    /// </summary>
    public interface IIdempotencyKeyStorage
    {
        /// <summary>
        /// Mengecek apakah sebuah kunci sudah ada di penyimpanan.
        /// </summary>
        /// <param name="key">Kunci idempoten yang akan dicek.</param>
        /// <returns>True jika kunci ada, false jika tidak.</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Menyimpan hasil yang terkait dengan kunci idempoten.
        /// </summary>
        /// <param name="key">Kunci idempoten yang akan disimpan.</param>
        /// <param name="result">Objek hasil yang akan disimpan.</param>
        /// <param name="expiration">Opsional: waktu kedaluwarsa untuk data yang disimpan.</param>
        Task StoreAsync(string key, object result, TimeSpan? expiration = null);

        /// <summary>
        /// Mengambil hasil yang terkait dengan kunci idempoten.
        /// </summary>
        /// <typeparam name="T">Tipe objek hasil.</typeparam>
        /// <param name="key">Kunci idempoten yang akan diambil hasilnya.</param>
        /// <returns>Objek hasil jika ditemukan; jika tidak, nilai default dari T.</returns>
        Task<T> GetResultAsync<T>(string key);

        /// <summary>
        /// Menghapus kunci idempoten dan data yang terkait dari penyimpanan.
        /// </summary>
        /// <param name="key">Kunci idempoten yang akan dihapus.</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// Mencoba mengambil kunci penguncian (lock) untuk mencegah pemrosesan bersamaan pada kunci tertentu.
        /// </summary>
        /// <param name="key">Kunci idempoten yang akan dikunci.</param>
        /// <param name="lockDuration">Durasi penguncian.</param>
        /// <returns>True jika penguncian berhasil; false jika tidak.</returns>
        Task<bool> TryLockAsync(string key, TimeSpan lockDuration);

        /// <summary>
        /// Melepas kunci penguncian (lock) yang sebelumnya diambil untuk kunci tertentu.
        /// </summary>
        /// <param name="key">Kunci idempoten yang akan dibuka kuncinya.</param>
        Task ReleaseLockAsync(string key);
    }

}
