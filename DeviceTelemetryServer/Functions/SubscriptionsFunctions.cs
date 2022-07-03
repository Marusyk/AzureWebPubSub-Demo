using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Extensions.Logging;

namespace DeviceTelemetryServer.Functions;

public class SubscriptionsFunctions
{
    private readonly MemoryStorage _memoryStorage;
    private readonly ILogger<SubscriptionsFunctions> _logger;

    public SubscriptionsFunctions(MemoryStorage memoryStorage, ILogger<SubscriptionsFunctions> logger)
    {
        _memoryStorage = memoryStorage;
        _logger = logger;
    }

    [FunctionName("client-subscribe")]
    public Response Subscribe(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscriptions/{clientId}/devices/{deviceId}")] Request request,
        string clientId, string deviceId,
        [WebPubSubConnection(Hub = "TelemetryHub", UserId = "{clientId}")] WebPubSubConnection connection)
    {
        _logger.LogInformation("Client '{ClientId}' subscribes for {DeviceId} telemetry indicators: {Indicators}",
            clientId, deviceId, string.Join(",", request.Indicators));

        var subscription = new Subscription(deviceId, request.Indicators);
        _memoryStorage.AddSubscription(clientId, subscription);

        return new Response(connection.Uri.AbsoluteUri);
    }
}

public record Response(string Url);
public record Request(IEnumerable<string> Indicators);