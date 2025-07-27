using IdemoptencyKeyManager0XA.Interfaces;
using IdemoptencyKeyManager0XA;
using Moq;

namespace IdempotencyKeyService.Tests
{
    public class IdempotencyServiceTest
    {
        [Fact]
        public async Task ExecuteAsync_FirstTime_ShouldRunOperationAndStoreResult()
        {
            // Arrange

            // Generate GUID-based key with timestamp
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var guid = Guid.NewGuid().ToString("N");

            var key = $"idem_{timestamp}_{guid}";
            var expectedResult = "OK";

            var storageMock = new Mock<IIdempotencyKeyStorage>();
            storageMock.Setup(s => s.ExistsAsync(key))
                .ReturnsAsync(false);
            storageMock.Setup(s => s.TryLockAsync(key, It.IsAny<TimeSpan>()))
                .ReturnsAsync(true);

            var service = new IdempotencyService(storageMock.Object); // implementasi kamu sendiri

            // Act
            var result = await service.ExecuteAsync(key, async () =>
            {
                await Task.Delay(10); // simulasi kerja async
                return expectedResult;
            });

            // Assert
            Assert.Equal(expectedResult, result);
            storageMock.Verify(s => s.StoreAsync(key, expectedResult, It.IsAny<TimeSpan?>()), Times.Once);
            storageMock.Verify(s => s.ReleaseLockAsync(key), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_KeyAlreadyProcessed_ShouldReturnCachedResult()
        {
            // Arrange
            // Generate GUID-based key with timestamp
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var guid = Guid.NewGuid().ToString("N");

            var key = $"idem_{timestamp}_{guid}";
            var cachedResult = "CACHED";

            var storageMock = new Mock<IIdempotencyKeyStorage>();
            storageMock.Setup(s => s.ExistsAsync(key))
                .ReturnsAsync(true);
            storageMock.Setup(s => s.GetResultAsync<string>(key))
                .ReturnsAsync(cachedResult);

            var service = new IdempotencyService(storageMock.Object);

            // Act
            var result = await service.ExecuteAsync(key, () => Task.FromResult("NEW"));

            // Assert
            Assert.Equal(cachedResult, result);
            storageMock.Verify(s => s.StoreAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()), Times.Never);
            storageMock.Verify(s => s.ReleaseLockAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ConcurrentAccess_ShouldRunOnlyOnce()
        {
            // Arrange
            // Generate GUID-based key with timestamp
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var guid = Guid.NewGuid().ToString("N");

            var key = $"idem_{timestamp}_{guid}";
            var expectedResult = "RESULT";

            var storageMock = new Mock<IIdempotencyKeyStorage>();
            storageMock.SetupSequence(s => s.ExistsAsync(key))
                .ReturnsAsync(false) // thread 1: belum ada
                .ReturnsAsync(true); // thread 2: sudah ada

            storageMock.Setup(s => s.TryLockAsync(key, It.IsAny<TimeSpan>()))
                .ReturnsAsync(true);
            storageMock.Setup(s => s.GetResultAsync<string>(key))
                .ReturnsAsync(expectedResult);

            var service = new IdempotencyService(storageMock.Object);

            // Act
            var t1 = service.ExecuteAsync(key, () => Task.FromResult(expectedResult));
            var t2 = service.ExecuteAsync(key, () => Task.FromResult("SHOULD_NOT_RUN"));

            var results = await Task.WhenAll(t1, t2);

            // Assert
            Assert.Equal(expectedResult, results[0]);
            Assert.Equal(expectedResult, results[1]);
            storageMock.Verify(s => s.StoreAsync(key, expectedResult, It.IsAny<TimeSpan?>()), Times.Once);
        }
    }
}