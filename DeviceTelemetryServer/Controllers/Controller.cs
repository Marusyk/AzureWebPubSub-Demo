using Azure.Messaging.WebPubSub;
using Microsoft.AspNetCore.Mvc;

namespace DeviceTelemetryServer.Controllers;

[ApiController]
[Route("[controller]")]
public class Controller : ControllerBase
{
    private readonly ILogger<Controller> _logger;
    private readonly WebPubSubServiceClient _webPubSubServiceClient;
    private readonly MemoryStorage _memoryStorage;
    private readonly TimeSpan _expiresAfter = TimeSpan.FromDays(1);

    public Controller(ILogger<Controller> logger, WebPubSubServiceClient webPubSubServiceClient, MemoryStorage memoryStorage)
    {
        _logger = logger;
        _webPubSubServiceClient = webPubSubServiceClient;
        _memoryStorage = memoryStorage;
    }

    [HttpPost("subscriptions/{clientId}/devices/{deviceId}")]
    public async Task<Response> Subscribe(
        [FromRoute] string clientId,
        [FromRoute] string deviceId,
        [FromBody] Request request)
    {
        _logger.LogInformation("Client '{ClientId}' subscribes for {DeviceId} telemetry indicators: {Indicators}",
            clientId, deviceId, string.Join(",", request.Indicators));

        var subscription = new Subscription(deviceId, request.Indicators);
        _memoryStorage.AddSubscription(clientId, subscription);

        var url = await _webPubSubServiceClient.GetClientAccessUriAsync(_expiresAfter, clientId);
        return new Response(url.AbsoluteUri);
    }

    // [HttpPost("send")]
    // public async Task Publish(Publish request)
    // {
    //     _logger.LogInformation("Send value '{Value}' to group '{Group}'",
    //         request.Value, request.Group);
    //
    //     await _webPubSubServiceClient.SendToAllAsync(request.Group, request.Value);
    // }
}

public record Request(IEnumerable<string> Indicators);
public record Response(string Url);
public record Publish(string DeviceId, string Indicator, string Value)
{
    public string Group => $"{DeviceId}_{Indicator}";
}