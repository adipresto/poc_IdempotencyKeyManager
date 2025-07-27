# Idempotent Key Manager Proof of Concepts
This project demonstrates how to utilize a `Mutex` in C# together with the `Semaphore` class to control access to a pool of resources. It uses `ConcurrentDictionary` to ensure thread-safe access to data structures and leverages the native `MemoryCache` for caching. The implementation guarantees that the value retrieved from the key storage pool remains consistent with its original value.

## Features
- In-memory key-value storage with a configurable lifetime in milliseconds.
- Mutex-based `TryLock` and `ReleaseLock` mechanism with retry logic for lock acquisition.
- Idempotency key generation and management.


## File Structures
- `IdempotentKeyManager0XA/InMemoryKeyStorage.cs`  
  Implementation of key storage using `MemoryCache` for in-memory caching and `Semaphore` as a mutex.
- `IdempotentKeyManager0XA/IdempotencyService.cs`  
  Idempotency service implementation that asynchronously locks a key with retry logic, checks if a result already exists, and either retrieves the cached value or executes the provided operation and stores its result.
- `MainAplikasi.Test/IdempotencyServiceTest.cs`  
  Unit tests covering key locking, result retrieval, and idempotency behavior under concurrent execution.

## Getting Started
### Prerequisites
- .NET 6
- `Microsoft.Extensions.Caching.Memory`
- Moq
- xUnit
