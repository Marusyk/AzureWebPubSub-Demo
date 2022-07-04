using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Azure.WebPubSub.Common;
using Microsoft.Extensions.Logging;

namespace DeviceTelemetry.Server.Functions;

internal class WebPubSubFunctions
{
    private readonly MemoryStorage _memoryStorage;
    private readonly ILogger<WebPubSubFunctions> _logger;

    public WebPubSubFunctions(MemoryStorage memoryStorage, ILogger<WebPubSubFunctions> logger)
    {
        _memoryStorage = memoryStorage;
        _logger = logger;
    }

    [FunctionName("wps-connect")]
    public async Task<ConnectEventResponse> Connect(
        [WebPubSubTrigger("TelemetryHub", WebPubSubEventType.System, "Connect")] ConnectEventRequest request)
    {
        string clientId = request.ConnectionContext.UserId;
        var subscription = await _memoryStorage.GetSubscription(clientId);
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

    [FunctionName("wps-disconnect")]
    public async Task Disconnected(
        [WebPubSubTrigger("TelemetryHub", WebPubSubEventType.System, "Disconnected")] DisconnectedEventRequest request,
        [WebPubSub(Hub = "TelemetryHub")] IAsyncCollector<WebPubSubAction> action)
    {
        string clientId = request.ConnectionContext.UserId;

        _logger.LogInformation("Client '{UseId}' disconnected", clientId);

        await _memoryStorage.Remove(clientId);
        await action.AddAsync(WebPubSubAction.CreateRemoveUserFromAllGroupsAction(clientId));
    }
}