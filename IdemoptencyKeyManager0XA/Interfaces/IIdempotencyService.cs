using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdemoptencyKeyManager0XA.Interfaces
{
    /// <summary>
    /// Menangani operasi idempoten, mencegah eksekusi ganda pada proses yang sama dengan kunci tertentu.
    /// </summary>
    public interface IIdempotencyService
    {
        /// <summary>
        /// Mengeksekusi operasi dengan dukungan idempoten.
        /// Jika kunci sudah pernah diproses, akan mengembalikan hasil yang tersimpan.
        /// </summary>
        /// <typeparam name="T">Tipe hasil yang dikembalikan oleh operasi.</typeparam>
        /// <param name="idempotencyKey">Kunci idempoten untuk operasi ini.</param>
        /// <param name="operation">Fungsi asinkron yang akan dijalankan jika kunci belum diproses.</param>
        /// <returns>Hasil dari operasi, baik dari eksekusi baru maupun dari cache idempoten.</returns>
        Task<T> ExecuteAsync<T>(string idempotencyKey, Func<Task<T>> operation);

        /// <summary>
        /// Mengecek apakah kunci idempoten sudah pernah diproses.
        /// </summary>
        /// <param name="idempotencyKey">Kunci idempoten yang akan dicek.</param>
        /// <returns>True jika kunci sudah diproses; false jika belum.</returns>
        Task<bool> IsProcessedAsync(string idempotencyKey);

        /// <summary>
        /// Menghasilkan kunci idempoten baru.
        /// </summary>
        /// <returns>String kunci idempoten yang unik.</returns>
        string GenerateKey();

        /// <summary>
        /// Memvalidasi format atau nilai kunci idempoten.
        /// </summary>
        /// <param name="key">Kunci idempoten yang akan divalidasi.</param>
        /// <returns>True jika kunci valid; false jika tidak.</returns>
        bool ValidateKey(string key);
    }

}
