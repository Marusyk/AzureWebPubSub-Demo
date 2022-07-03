using System.Collections.Concurrent;

namespace DeviceTelemetryServer;

public class MemoryStorage
{
    private readonly ConcurrentDictionary<string, Subscription> _data = new();

    public void AddSubscription(string clientId, Subscription subscription)
    {
        _data.AddOrUpdate(clientId, subscription, (s, subscription1) => subscription);
    }

    public Task<Subscription> GetSubscription(string clientId)
    {
        if (!_data.TryGetValue(clientId, out var subscription))
        {
            return Task.FromResult<Subscription>(null);
        }
        return Task.FromResult<Subscription>(subscription);
    }

    public Task Remove(string clientId)
    {
        _data.TryRemove(clientId, out _);
        return Task.CompletedTask;
    }
}

public record Subscription(string DeviceId, IEnumerable<string> Indicators)
{
    public IEnumerable<string> Groups => Indicators.Select(indicator => $"{DeviceId}_{indicator}".ToLowerInvariant());
}
