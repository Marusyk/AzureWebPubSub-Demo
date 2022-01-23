using Microsoft.Azure.WebPubSub.AspNetCore;
using Microsoft.Azure.WebPubSub.Common;

namespace DeviceTelemetryServer;

public class TelemetryHub : WebPubSubHub
{
    private readonly MemoryStorage _memoryStorage;
    private readonly ILogger<TelemetryHub> _logger;

    public TelemetryHub(MemoryStorage memoryStorage, ILogger<TelemetryHub> logger)
    {
        _memoryStorage = memoryStorage;
        _logger = logger;
    }

    public override async ValueTask<ConnectEventResponse> OnConnectAsync(ConnectEventRequest request, CancellationToken cancellationToken)
    {
        string clientId = request.ConnectionContext.UserId;
        Subscription? subscription = await _memoryStorage.GetSubscription(clientId);
        if (subscription is null)
        {
            throw new InvalidOperationException($"No subscriptions found for client '{clientId}'. Connection is denied.");
        }

        _logger.LogInformation("Client {ClientId} connected", clientId);

        return new ConnectEventResponse
        {
            UserId = clientId,
            Groups = subscription.Groups.ToArray()
        };
    }

    public override async Task OnDisconnectedAsync(DisconnectedEventRequest request)
    {
        string clientId = request.ConnectionContext.UserId;

        _logger.LogInformation("Client '{UseId}' disconnected", clientId);

        await _memoryStorage.Remove(clientId);
    }
}