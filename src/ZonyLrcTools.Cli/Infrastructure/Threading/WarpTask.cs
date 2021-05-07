using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZonyLrcTools.Cli.Infrastructure.Threading
{
    public class WarpTask : IDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly SemaphoreSlim _semaphore;
        private readonly int _maxDegreeOfParallelism;

        public WarpTask(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            }

            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
        }

        public async Task RunAsync(Func<Task> taskFactory, CancellationToken cancellationToken = default)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token))
            {
                await _semaphore.WaitAsync(cts.Token);
                try
                {
                    await taskFactory().ConfigureAwait(false);
                }
                finally
                {
                    _semaphore.Release(1);
                }
            }
        }

        public async Task<T> RunAsync<T>(Func<Task<T>> taskFactory, CancellationToken cancellationToken = default)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token))
            {
                await _semaphore.WaitAsync(cts.Token);
                try
                {
                    return await taskFactory().ConfigureAwait(false);
                }
                finally
                {
                    _semaphore.Release(1);
                }
            }
        }

        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cts.Cancel();
                    for (int i = 0; i < _maxDegreeOfParallelism; i++)
                    {
                        _semaphore.WaitAsync().GetAwaiter().GetResult();
                    }

                    _semaphore.Dispose();
                    _cts.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WarpTask()
        {
            Dispose(false);
        }
    }
}