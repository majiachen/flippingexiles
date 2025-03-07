using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class RateLimiter : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly LinkedList<DateTime> _requestTimes = new();
    private readonly int _windowSeconds;
    private bool _disposed;

    public RateLimiter(int maxRequests, int windowSeconds)
    {
        _semaphore = new SemaphoreSlim(maxRequests, maxRequests);
        _windowSeconds = windowSeconds;
    }

    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        while (!_disposed)
        {
            lock (_requestTimes)
            {
                ClearExpiredRequests();
                if (_requestTimes.Count < _semaphore.CurrentCount)
                {
                    _requestTimes.AddLast(DateTime.UtcNow);
                    return;
                }
            }

            await Task.Delay(CalculateDelay(), cancellationToken);
        }
    }

    private void ClearExpiredRequests()
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-_windowSeconds);
        while (_requestTimes.First?.Value < cutoff)
        {
            _requestTimes.RemoveFirst();
        }
    }

    private int CalculateDelay()
    {
        if (_requestTimes.First is null) return 0;
        var oldest = _requestTimes.First.Value;
        return (int)(_windowSeconds - (DateTime.UtcNow - oldest).TotalSeconds) * 1000;
    }

    public void Dispose()
    {
        _disposed = true;
        _semaphore.Dispose();
    }
}