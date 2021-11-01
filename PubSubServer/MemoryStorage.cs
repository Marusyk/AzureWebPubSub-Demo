using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PubSubServer
{
    public class MemoryStorage
    {
        private readonly ConcurrentDictionary<string, Subscription> _data = new();

        public void Add(string clientId, Subscription subscription)
        {
            _data.AddOrUpdate(clientId, subscription, (s, subscription1) => subscription);
        }

        public IEnumerable<string> GetGroups(string clientId)
        {
            if (!_data.TryGetValue(clientId, out var subscription))
            {
                throw new Exception("Client has not found");
            }
            return subscription.Groups;
        }

        public void Remove(string clientId)
        {
            _data.TryRemove(clientId, out _);
        }
    }

    public record Subscription(string EquipmentNumber, IEnumerable<string> Fields)
    {
        public IEnumerable<string> Groups => Fields.Select(field => $"{EquipmentNumber}_{field}");
    }
}